using SkiaSharp;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace LuxEditor.Logic
{
    public static class ImageProcessingManager
    {
        private static SKColorFilter CreateCombinedColorFilter(Dictionary<string, object> filters)
        {
            var cf = SKColorFilter.CreateColorMatrix(CreateBaseMatrix(filters));

            var sh = CreateShadowsHighlightsFilter(filters);
            if (sh != null) cf = SKColorFilter.CreateCompose(sh, cf);

            var bw = CreateBlacksWhitesLUT(filters);
            if (bw != null) cf = SKColorFilter.CreateCompose(bw, cf);

            var dz = CreateDehazeFilter(filters);
            if (dz != null) cf = SKColorFilter.CreateCompose(dz, cf);

            var vib = CreateVibranceFilter(filters);
            if (vib != null) cf = SKColorFilter.CreateCompose(vib, cf);

            return cf;
        }

        private static SKImageFilter? ComposeImageFilters(SKImageFilter? a, SKImageFilter? b)
        {
            if (a == null) return b;
            if (b == null) return a;
            return SKImageFilter.CreateCompose(a, b);
        }

        public static async Task<SKBitmap> ApplyFiltersAsync(SKBitmap source, Dictionary<string, object> filters)
        {
            return await Task.Run(() =>
            {
                var target = new SKBitmap(source.Width, source.Height, source.ColorType, source.AlphaType);
                using var surface = SKSurface.Create(target.Info);
                using var canvas = surface.Canvas;
                canvas.Clear(SKColors.Transparent);

                var paint = new SKPaint
                {
                    ColorFilter = CreateCombinedColorFilter(filters),
                    ImageFilter = ComposeImageFilters(
                        CreateTextureFilter(filters),
                        CreateClarityFilter(filters)
                    )
                };

                canvas.DrawBitmap(source, 0, 0, paint);
                canvas.Flush();
                surface.ReadPixels(target.Info, target.GetPixels(), target.RowBytes, 0, 0);

                return target;
            });
        }


        /// <summary>
        /// Constructs a color matrix applying exposure, contrast, saturation, temperature, and tint.
        /// </summary>
        private static float[] CreateBaseMatrix(Dictionary<string, object> filters)
        {
            float exposure = filters.TryGetValue("Exposure", out var exp) ? (float) exp : 0f;
            float contrast = filters.TryGetValue("Contrast", out var con) ? (float) con : 0f;
            float saturation = filters.TryGetValue("Saturation", out var sat) ? (float) sat : 0f;
            float temperature = filters.TryGetValue("Temperature", out var temp) ? (float) temp : 6500f;
            float tint = filters.TryGetValue("Tint", out var ti) ? (float) ti : 0f;

            var exposureGain = MathF.Pow(2, (exposure / 5));

            // Temperature
            var (redShift,  greenShift, blueShift) = CreateWhiteBalanceMatrix(temperature, tint);

            // Saturation
            const float lumR = 0.2126f;
            const float lumG = 0.7152f;
            const float lumB = 0.0722f;
            float satFactor = 1f + (saturation / 100f);
            float rSat = lumR * (1f - satFactor);
            float gSat = lumG * (1f - satFactor);
            float bSat = lumB * (1f - satFactor);

            // Contrast
            float contrastFactor = 1f + (contrast / 500f);
            float translate = 128f * (1f - contrastFactor);

            return new float[]
            {
                (rSat + satFactor) * exposureGain * contrastFactor * redShift,
                gSat * exposureGain * contrastFactor * redShift,
                bSat * exposureGain * contrastFactor * redShift,
                0,
                translate,

                rSat * exposureGain * contrastFactor * greenShift,
                (gSat + satFactor) * exposureGain * contrastFactor * greenShift,
                bSat * exposureGain * contrastFactor * greenShift,
                0,
                translate,

                rSat * exposureGain * contrastFactor * blueShift,
                gSat * exposureGain * contrastFactor * blueShift,
                (bSat + satFactor) * exposureGain * contrastFactor * blueShift,
                0,
                translate,

                0, 0, 0, 1, 0
            };

        }

        /// <summary>
        /// Builds a table‐based filter that adjusts shadows and highlights.
        /// Shadows/Highlights values are in –100…100.
        /// </summary>
        private static SKColorFilter? CreateShadowsHighlightsFilter(Dictionary<string, Object> filters)
        {
            filters.TryGetValue("Shadows", out var rawSh);
            filters.TryGetValue("Highlights", out var rawHi);
            float shadows = (float) rawSh / 100f;
            float highlights = (float) rawHi / 100f;

            if (MathF.Abs(shadows) < 1e-6 && MathF.Abs(highlights) < 1e-6)
                return null;

            var table = new byte[256];
            for (int i = 0; i < 256; i++)
            {
                float v = i / 255f;
                float v2;
                if (v < 0.25f)
                {
                    v2 = v + (0.25f - v) * shadows;
                }
                else if (v > 0.75f)
                {
                    v2 = v + (v - 0.75f) * highlights;
                }
                else
                {
                    v2 = v;
                }
                table[i] = (byte)(Math.Clamp(v2, 0f, 1f) * 255);
            }

            return SKColorFilter.CreateTable(table);
        }

        static SKColorFilter? CreateBlacksWhitesLUT(Dictionary<string, Object> filters)
        {
            filters.TryGetValue("Blacks", out var rawB);
            filters.TryGetValue("Whites", out var rawW);
            float blacks = Math.Clamp((float) rawB / 100f, -1f, 1f);
            float whites = Math.Clamp((float) rawW / 100f, -1f, 1f);
            if (Math.Abs(blacks) < 1e-6f && Math.Abs(whites) < 1e-6f)
                return null;

            var table = new byte[256];
            for (int i = 0; i < 256; i++)
            {
                float v = i / 255f;
                float v2 = v < 0.5f
                    ? v + (0.5f - v) * blacks
                    : v + (v - 0.5f) * whites;
                table[i] = (byte)(Math.Clamp(v2, 0f, 1f) * 255);
            }

            return SKColorFilter.CreateTable(table);
        }

        static SKColorFilter? CreateDehazeFilter(Dictionary<string, Object> filters)
        {
            filters.TryGetValue("Dehaze", out var raw);
            float h = Math.Clamp((float) raw / 100f, 0f, 1f);
            if (h <= 0f) return null;

            var table = new byte[256];
            for (int i = 0; i < 256; i++)
            {
                float v = i / 255f;
                float v2 = (v - h) / (1f - h);
                // gentle gamma to preserve midtones
                v2 = MathF.Pow(Math.Clamp(v2, 0f, 1f), 1f / (1f + 0.25f * h));
                table[i] = (byte)(v2 * 255f);
            }
            return SKColorFilter.CreateTable(table);
        }

        static SKImageFilter? CreateTextureFilter(Dictionary<string, Object> filters)
        {
            filters.TryGetValue("Texture", out var raw);
            float t = Math.Clamp((float) raw / 100f, 0f, 1f);
            if (t <= 0f) return null;

            float amt = t;
            float center = 1f + 8f * amt;
            float neighbor = -amt;

            float[] kernel = new float[]
            {
        0,        neighbor,      0,
        neighbor, center,        neighbor,
        0,        neighbor,      0
            };

            return SKImageFilter.CreateMatrixConvolution(
                new SKSizeI(3, 3),
                kernel,
                gain: 1f,
                bias: 0f,
                new SKPointI(1, 1),
                SKShaderTileMode.Clamp,
                convolveAlpha: false
            );
        }

        static SKImageFilter? CreateClarityFilter(Dictionary<string, Object> filters)
        {
            filters.TryGetValue("Clarity", out var raw);
            float c = Math.Clamp((float) raw / 100f, 0f, 1f);
            if (c <= 0f) return null;

            // 5x5 unsharp-mask for mid-frequency boost
            float amt = c;
            float center = 1f + 24f * amt;
            float surround = -amt;

            var kernel = new float[25];
            for (int y = 0; y < 5; y++)
                for (int x = 0; x < 5; x++)
                {
                    int idx = y * 5 + x;
                    kernel[idx] = (x == 2 && y == 2) ? center : surround;
                }

            return SKImageFilter.CreateMatrixConvolution(
                new SKSizeI(5, 5),
                kernel,
                gain: 1f,
                bias: 0f,
                new SKPointI(2, 2),
                SKShaderTileMode.Clamp,
                convolveAlpha: false
            );
        }


        private static SKColorFilter? CreateVibranceFilter(Dictionary<string, Object> filters)
        {
            if (!filters.TryGetValue("Vibrance", out var raw)) return null;
            float vibrance = Math.Clamp((float) raw / 100f, -1f, 1f);
            if (Math.Abs(vibrance) < 1e-6f) return null;

            const string sksl = @"
                uniform half vibrance;
                half4 main(half4 inColor) {
                    half maxv = max(inColor.r, max(inColor.g, inColor.b));
                    half minv = min(inColor.r, min(inColor.g, inColor.b));
                    half sat  = (maxv - minv) / (maxv + 1e-5);
                    half f    = vibrance * (1 - sat);
                    half gray = (inColor.r + inColor.g + inColor.b) * (1.0/3.0);
                    half3 rgb = mix(half3(gray), inColor.rgb, 1 + f);
                    return half4(rgb, inColor.a);
                }";

            // compile the SkSL
            string errorText;
            using var effect = SKRuntimeEffect.CreateColorFilter(sksl, out errorText);
            if (!string.IsNullOrEmpty(errorText))
                throw new InvalidOperationException(errorText);
            // :contentReference[oaicite:0]{index=0}

            // set up the single uniform
            var uniforms = new SKRuntimeEffectUniforms(effect)
            {
                ["vibrance"] = vibrance
            };
            // :contentReference[oaicite:1]{index=1}

            // no child shaders needed for this color-only effect
            return effect.ToColorFilter(uniforms);
        }

        private static (float r, float g, float b) CreateWhiteBalanceMatrix(float temperature, float tint)
        {
            temperature = Math.Clamp(temperature, 2000f, 50000f);
            float kelvinRef = 6500f;

            float temperatureRatio = (float)Math.Log(temperature / kelvinRef, 2.0);

            float redShift = 1f + 0.2f * temperatureRatio;
            float blueShift = 1f - 0.2f * temperatureRatio;

            float greenShift = 1f - (tint / 100f);
            
            redShift = Math.Clamp(redShift, 0.5f, 2.5f);
            greenShift = Math.Clamp(greenShift, 0.5f, 2.5f);
            blueShift = Math.Clamp(blueShift, 0.5f, 2.5f);

            return (redShift, greenShift, blueShift);
        }

        public static SKBitmap ResizeBitmap(SKBitmap source, int width, int height)
        {
            SKBitmap resized = new SKBitmap(width, height);

            using var surface = SKSurface.Create(new SKImageInfo(width, height));
            var canvas = surface.Canvas;
            canvas.Clear();

            var sampling = new SKSamplingOptions(SKFilterMode.Linear, SKMipmapMode.Linear);
            canvas.DrawImage(SKImage.FromBitmap(source), new SKRect(0, 0, width, height), sampling);
            canvas.Flush();
            surface.Snapshot().ReadPixels(resized.Info, resized.GetPixels(), resized.RowBytes, 0, 0);

            return resized;
        }

        public static SKBitmap GeneratePreview(SKBitmap source, int targetHeight)
        {
            float aspectRatio = (float)source.Width / source.Height;
            int targetWidth = (int)(targetHeight * aspectRatio);
            return ResizeBitmap(source, targetWidth, targetHeight);
        }

        public static SKBitmap GenerateMediumResolution(SKBitmap source, int maxHeight = 600)
        {
            if (source.Height <= maxHeight)
                return source;

            float aspectRatio = (float)source.Width / source.Height;
            int targetHeight = maxHeight;
            int targetWidth = (int)(targetHeight * aspectRatio);
            return ResizeBitmap(source, targetWidth, targetHeight);
        }
    }
}
