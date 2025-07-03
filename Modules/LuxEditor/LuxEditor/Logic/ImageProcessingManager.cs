using SkiaSharp;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace LuxEditor.Logic
{
    public static class ImageProcessingManager
    {
        private static GRContext? _grContext = CreateGpuContext();

        /// <summary>
        /// Creates a color filter that combines all the filters.
        /// </summary>
        /// <param name="filters"></param>
        /// <returns></returns>
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

            var tone = CreateToneCurveFilter(filters);
            if (tone != null) cf = SKColorFilter.CreateCompose(tone, cf);

            return cf;
        }

        /// <summary>
        /// Creates a texture filter that combines all the filters.
        /// </summary>
        /// <param name="a"></param>
        /// <param name="b"></param>
        /// <returns></returns>
        private static SKImageFilter? ComposeImageFilters(SKImageFilter? a, SKImageFilter? b)
        {
            if (a == null) return b;
            if (b == null) return a;
            return SKImageFilter.CreateCompose(a, b);
        }

        /// <summary>
        /// Applies the filters to the source bitmap and returns a new bitmap.
        /// </summary>
        /// <param name="source"></param>
        /// <param name="filters"></param>
        /// <returns></returns>
        public static Task<SKBitmap> ApplyFiltersAsync(
            SKBitmap source,
            Dictionary<string, object> filters,
            CancellationToken ct = default)
        {
            return Task.Run(() =>
            {
                ct.ThrowIfCancellationRequested();

                var target = new SKBitmap(source.Width, source.Height,
                                          source.ColorType, source.AlphaType);

                using var surface = SKSurface.Create(target.Info);
                using var canvas = surface.Canvas;
                canvas.Clear(SKColors.Transparent);

                var paint = new SKPaint
                {
                    ColorFilter = CreateCombinedColorFilter(filters),
                    ImageFilter = CreateTextureFilter(filters)
                };

                canvas.DrawBitmap(BlurBackground(source, filters), 0, 0, paint);
                canvas.Flush();

                ct.ThrowIfCancellationRequested();

                surface.ReadPixels(target.Info, target.GetPixels(),
                                   target.RowBytes, 0, 0);

                return target;
            }, ct);
        }

        /*
        private static SKBitmap BlurBackground(SKBitmap source, Dictionary<string, object> filters)
        {
            Dictionary<string, object>? blurSettings = filters.TryGetValue("Blur", out var blur) ? (Dictionary<string, object> ?)blur : null;

            if (blurSettings == null || (bool)blurSettings["State"] == false)
                return source;

            SKBitmap? mask = blurSettings.TryGetValue("Mask", out var tm) ? (SKBitmap)tm : null;

            if (mask == null || mask.Width != source.Width || mask.Height != source.Height)
                return source;

            float sigma = blurSettings.TryGetValue("Sigma", out var s) ? (float)s : 5f;

            int w = source.Width;
            int h = source.Height;
            var blurred = new SKBitmap(w, h, source.ColorType, source.AlphaType);
            using (var canvas = new SKCanvas(blurred))
            using (var paint = new SKPaint
            {
                IsAntialias = true,
                ImageFilter = SKImageFilter.CreateBlur(sigma, sigma)
            })
            {
                canvas.Clear();
                canvas.DrawBitmap(source, new SKRect(0, 0, w, h), paint);
            }

            var result = new SKBitmap(w, h, source.ColorType, source.AlphaType);
            for (int y = 0; y < h; y++)
            {
                for (int x = 0; x < w; x++)
                {
                    // mask is foreground in white, background in black
                    var m = mask.GetPixel(x, y);
                    bool isFore = m.Red > 128;
                    result.SetPixel(x, y,
                        isFore
                          ? source.GetPixel(x, y)
                          : blurred.GetPixel(x, y)
                    );
                }
            }

            return result;
        }
        */

        /// <summary>
        /// Applies a blur-effect on the background, and keeps the foreground intact.
        /// </summary>
        /// <param name="source">Bitmap source to be blurred</param>
        /// <param name="filters">Filters dictionnary</param>
        /// <returns>The original bitmap with a blurred background</returns>
        /// <exception cref="ArgumentNullException">If source is null</exception>
        private static SKBitmap BlurBackground(SKBitmap source, Dictionary<string, object> filters)
        {
            if (source == null) throw new ArgumentNullException(nameof(source));
            if (filters == null) return source;

            // Extract blur settings
            if (!filters.TryGetValue("Blur", out var blurObj) || blurObj is not Dictionary<string, object> blurSettings)
                return source;

            if (!blurSettings.TryGetValue("State", out var stateObj) || !(stateObj is bool state) || !state)
                return source;

            if (!blurSettings.TryGetValue("Mask", out var maskObj) || maskObj is not SKBitmap mask)
                return source;

            if (mask.Width != source.Width || mask.Height != source.Height)
                return source;

            float sigma = 5f;
            if (blurSettings.TryGetValue("Sigma", out var sigmaObj))
            {
                try { sigma = Convert.ToSingle(sigmaObj); } catch { }
            }

            int width = source.Width;
            int height = source.Height;

            var blurred = new SKBitmap(width, height, source.ColorType, source.AlphaType);
            using (var canvas = new SKCanvas(blurred))
            using (var paint = new SKPaint
            {
                IsAntialias = true,
                ImageFilter = SKImageFilter.CreateBlur(sigma, sigma)
            })
            {
                canvas.Clear();
                canvas.DrawBitmap(source, 0, 0, paint);
            }

            var result = new SKBitmap(width, height, source.ColorType, source.AlphaType);

            var sourceSpan = source.PeekPixels().GetPixelSpan<SKColor>();
            var blurredSpan = blurred.PeekPixels().GetPixelSpan<SKColor>();
            var maskSpan = mask.PeekPixels().GetPixelSpan<SKColor>();
            var resultSpan = result.PeekPixels().GetPixelSpan<SKColor>();

            for (int i = 0; i < sourceSpan.Length; i++)
            {
                var maskPixel = maskSpan[i];
                bool isForeground = maskPixel.Red > 128;
                resultSpan[i] = isForeground ? sourceSpan[i] : blurredSpan[i];
            }

            return result;
        }


        /// <summary>
        /// Creates a base color matrix for the image processing.
        /// </summary>
        /// <param name="filters"></param>
        /// <returns></returns>
        private static float[] CreateBaseMatrix(Dictionary<string, object> filters)
        {
            float exposure = filters.TryGetValue("Exposure", out var exp) ? (float)exp : 0f;
            float contrast = filters.TryGetValue("Contrast", out var con) ? (float)con : 0f;
            float saturation = filters.TryGetValue("Saturation", out var sat) ? (float)sat : 0f;
            float temperature = filters.TryGetValue("Temperature", out var temp) ? (float)temp : 6500f;
            float tint = filters.TryGetValue("Tint", out var ti) ? (float)ti : 0f;

            if (contrast < 0f)
            {
                exposure += contrast * 5;
            }

            var exposureGain = MathF.Pow(2, exposure / 4);

            // Temperature
            var (redShift, greenShift, blueShift) = CreateWhiteBalanceMatrix(temperature, tint);

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
        /// Creates a color filter for shadows and highlights.
        /// </summary>
        /// <param name="filters"></param>
        /// <returns></returns>
        private static SKColorFilter? CreateShadowsHighlightsFilter(Dictionary<string, Object> filters)
        {
            filters.TryGetValue("Shadows", out var rawSh);
            filters.TryGetValue("Highlights", out var rawHi);
            float shadows = (float)rawSh / 100f;
            float highlights = (float)rawHi / 100f;

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

        /// <summary>
        /// Create a color filter for blacks and whites
        /// </summary>
        /// <param name="filters"></param>
        /// <returns></returns>
        static SKColorFilter? CreateBlacksWhitesLUT(Dictionary<string, Object> filters)
        {
            filters.TryGetValue("Blacks", out var rawB);
            filters.TryGetValue("Whites", out var rawW);
            float blacks = Math.Clamp((float)rawB / 100f, -1f, 1f);
            float whites = Math.Clamp((float)rawW / 100f, -1f, 1f);
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

        /// <summary>
        /// Create a color filter for dehaze
        /// </summary>
        /// <param name="filters"></param>
        /// <returns></returns>
        static SKColorFilter? CreateDehazeFilter(Dictionary<string, Object> filters)
        {
            filters.TryGetValue("Dehaze", out var raw);
            float h = Math.Clamp((float)raw / 200f, -1f, 1f);

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

        /// <summary>
        /// Creates a texture filter for the image.
        /// </summary>
        /// <param name="filters"></param>
        /// <returns></returns>
        private static SKImageFilter? CreateTextureFilter(Dictionary<string, object> filters)
        {
            if (!filters.TryGetValue("Texture", out var raw))
                return null;

            float t = Math.Clamp(Convert.ToSingle(raw) / 100f, -1f, 1f);
            if (Math.Abs(t) < 1e-6f)
                return null;

            if (t > 0f)
            {
                float a = t;
                float center = 1f + 4f * a;
                float neighbor = -a;

                var kernel = new float[]
                {
                    0,        neighbor, 0,
                    neighbor, center,   neighbor,
                    0,        neighbor, 0
                };

                return SKImageFilter.CreateMatrixConvolution(
                    kernelSize: new SKSizeI(3, 3),
                    kernel: kernel,
                    gain: 1f,
                    bias: 0f,
                    kernelOffset: new SKPointI(1, 1),
                    tileMode: SKShaderTileMode.Clamp,
                    convolveAlpha: false);
            }
            else
            {
                float sigma = Math.Abs(t) * 3.0f;
                return SKImageFilter.CreateBlur(sigma, sigma);
            }
        }

        /// <summary>
        /// Creates a vibrance filter for the image.
        /// </summary>
        /// <param name="filters"></param>
        /// <returns></returns>
        /// <exception cref="InvalidOperationException"></exception>
        private static SKColorFilter? CreateVibranceFilter(Dictionary<string, Object> filters)
        {
            if (!filters.TryGetValue("Vibrance", out var raw)) return null;
            float vibrance = Math.Clamp((float)raw / 100f, -1f, 1f);
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

            string errorText;
            using var effect = SKRuntimeEffect.CreateColorFilter(sksl, out errorText);
            if (!string.IsNullOrEmpty(errorText))
                throw new InvalidOperationException(errorText);

            var uniforms = new SKRuntimeEffectUniforms(effect)
            {
                ["vibrance"] = vibrance
            };

            return effect.ToColorFilter(uniforms);
        }

        /// <summary>
        /// Creates a white balance matrix for the image.
        /// </summary>
        /// <param name="temperature"></param>
        /// <param name="tint"></param>
        /// <returns></returns>
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

        /// <summary>
        /// Resizes the bitmap to the specified width and height.
        /// </summary>
        /// <param name="source"></param>
        /// <param name="width"></param>
        /// <param name="height"></param>
        /// <returns></returns>
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

        /// <summary>
        /// Generates a preview bitmap with the specified height while maintaining the aspect ratio.
        /// </summary>
        /// <param name="source"></param>
        /// <param name="targetHeight"></param>
        /// <returns></returns>
        public static SKBitmap GeneratePreview(SKBitmap source, int targetHeight)
        {
            float aspectRatio = (float)source.Width / source.Height;
            int targetWidth = (int)(targetHeight * aspectRatio);
            return ResizeBitmap(source, targetWidth, targetHeight);
        }

        /// <summary>
        /// Generates a medium resolution bitmap with the specified maximum height while maintaining the aspect ratio.
        /// </summary>
        /// <param name="source"></param>
        /// <param name="maxHeight"></param>
        /// <returns></returns>
        public static SKBitmap GenerateMediumResolution(SKBitmap source, int maxHeight = 600)
        {
            if (source.Height <= maxHeight)
                return source;

            float aspectRatio = (float)source.Width / source.Height;
            int targetHeight = maxHeight;
            int targetWidth = (int)(targetHeight * aspectRatio);
            return ResizeBitmap(source, targetWidth, targetHeight);
        }

        /// <summary>
        /// Creates a GPU context for image processing.
        /// </summary>
        /// <returns></returns>
        private static GRContext? CreateGpuContext()
        {
            try { return GRContext.CreateGl(); }
            catch { return null; }
        }

        /// <summary>
        /// Resizes the bitmap using the CPU.
        /// </summary>
        /// <param name="src"></param>
        /// <param name="w"></param>
        /// <param name="h"></param>
        /// <param name="draft"></param>
        /// <returns></returns>
        private static SKBitmap ResizeOnCpu(SKBitmap src, int h, bool draft)
        {
            float ratio = (float)src.Width / src.Height;
            int w = (int)(h * ratio);
            var dstInfo = new SKImageInfo(w, h, src.ColorType, src.AlphaType);
            var dst = new SKBitmap(dstInfo);
            using var s = SKSurface.Create(dstInfo);
            var canvas = s.Canvas;

            var sampling = new SKSamplingOptions(
                draft ? SKFilterMode.Nearest : SKFilterMode.Linear,
                SKMipmapMode.None);

            canvas.Clear();
            canvas.DrawImage(SKImage.FromBitmap(src),
                             new SKRect(0, 0, w, h),
                             sampling);
            canvas.Flush();
            s.Snapshot().ReadPixels(dstInfo, dst.GetPixels(), dst.RowBytes, 0, 0);
            return dst;
        }

        /// <summary>
        /// Upscales the bitmap to the specified height using the GPU if available.
        /// </summary>
        /// <param name="src"></param>
        /// <param name="targetHeight"></param>
        /// <param name="draft"></param>
        /// <returns></returns>
        public static SKImage Upscale(
            SKBitmap src, int targetHeight, bool draft = true)
        {
            if (_grContext == null)
                return SKImage.FromBitmap(ResizeOnCpu(src, targetHeight, draft));

            float ratio = (float)src.Width / src.Height;
            int targetWidth = (int)(targetHeight * ratio);

            var info = new SKImageInfo(targetWidth, targetHeight,
                                       src.ColorType, src.AlphaType);

            using var surface = SKSurface.Create(_grContext, true, info);
            var canvas = surface.Canvas;

            var sampling = new SKSamplingOptions(
                draft ? SKFilterMode.Nearest : SKFilterMode.Linear,
                SKMipmapMode.None);

            canvas.Clear();
            canvas.DrawImage(SKImage.FromBitmap(src),
                             new SKRect(0, 0, targetWidth, targetHeight),
                             sampling);
            canvas.Flush();

            return surface.Snapshot();
        }

        /// <summary>
        /// Applies the filters to the source bitmap and returns a new bitmap.
        /// </summary>
        /// <param name="source"></param>
        /// <param name="filters"></param>
        /// <returns></returns>
        public static Task<SKBitmap> ApplyFiltersAsync(
            SKBitmap source, Dictionary<string, object> filters)
            => ApplyFiltersAsync(source, filters, CancellationToken.None);

        /// <summary>
        /// Creates a tone curve filter based on the provided parameters.
        /// </summary>
        /// <param name="f"></param>
        /// <returns></returns>
        private static SKColorFilter? CreateToneCurveFilter(Dictionary<string, object> f)
        {
            Span<byte> id = stackalloc byte[256];
            for (int i = 0; i < 256; i++) id[i] = (byte)i;

            byte[] p = f.TryGetValue("ToneCurve_Parametric", out var o1) ? (byte[])o1 : id.ToArray();
            byte[] pt = f.TryGetValue("ToneCurve_Point", out var o2) ? (byte[])o2 : id.ToArray();
            byte[] rCh = f.TryGetValue("ToneCurve_Red", out var o3) ? (byte[])o3 : id.ToArray();
            byte[] gCh = f.TryGetValue("ToneCurve_Green", out var o4) ? (byte[])o4 : id.ToArray();
            byte[] bCh = f.TryGetValue("ToneCurve_Blue", out var o5) ? (byte[])o5 : id.ToArray();

            var lutR = new byte[256];
            var lutG = new byte[256];
            var lutB = new byte[256];

            bool changed = false;
            for (int i = 0; i < 256; i++)
            {
                int v = i;
                v = p[v];
                v = pt[v];
                lutR[i] = rCh[v];
                lutG[i] = gCh[v];
                lutB[i] = bCh[v];

                changed |= lutR[i] != i || lutG[i] != i || lutB[i] != i;
            }
            return changed ? SKColorFilter.CreateTable(id.ToArray(), lutR, lutG, lutB) : null;
        }

        /// <summary>
        /// Applies the filters to the source bitmap and returns a new bitmap synchronously.
        /// </summary>
        /// <param name="source"></param>
        /// <param name="filters"></param>
        /// <param name="ct"></param>
        /// <returns></returns>
        public static SKBitmap ApplyFilters(SKBitmap source, Dictionary<string, object> filters, CancellationToken ct = default)
        {
            return ApplyFiltersAsync(source, filters, ct).Result;
        }
    }
}
