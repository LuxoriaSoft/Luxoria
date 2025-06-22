using System;
using SkiaSharp;

namespace LuxEditor.EditorUI.Controls
{
    public sealed class CropController
    {
        public enum Interaction
        {
            None,
            Move,
            Rotate,
            ResizeNW,
            ResizeNE,
            ResizeSW,
            ResizeSE
        }

        public struct CropBox
        {
            public float X;
            public float Y;
            public float Width;
            public float Height;
            public float Angle;
        }

        public CropBox Box { get => _box; set => _box = value; }
        public bool LockAspectRatio { get => _lockedRatio.HasValue; set => _lockedRatio = value ? _box.Width / _box.Height : null; }
        public Interaction ActiveInteraction { get; private set; } = Interaction.None;
        public Interaction HoverInteraction { get; private set; } = Interaction.None;

        public event Action? BoxChanged;

        private CropBox _box;
        private float? _lockedRatio;
        private float _canvasW;
        private float _canvasH;

        private SKPoint _pStart;
        private CropBox _startBox;
        private float _startAngle;

        private const float HANDLE = 12f;

        /// <summary>Creates a new controller for the specified canvas size.</summary>
        public CropController(float canvasWidth, float canvasHeight) => ResizeCanvas(canvasWidth, canvasHeight);

        /// <summary>Resizes the canvas and clamps the current crop box.</summary>
        public void ResizeCanvas(float w, float h)
        {
            _canvasW = w;
            _canvasH = h;
            if (_box.Width < 1 || _box.Height < 1) Reset();
            Clamp();
        }

        /// <summary>Resets the crop box to cover the whole canvas.</summary>
        public void Reset()
        {
            _box = new CropBox { X = 0, Y = 0, Width = _canvasW, Height = _canvasH, Angle = 0 };
            _lockedRatio = null;
            BoxChanged?.Invoke();
        }

        /// <summary>Loads a previously saved crop box.</summary>
        public void Load(CropBox saved)
        {
            _box = saved;
            _lockedRatio = saved.Width > 0 && saved.Height > 0 ? saved.Width / saved.Height : null;
            Clamp();
        }

        /// <summary>Applies a preset aspect ratio to the crop box.</summary>
        public void ApplyPresetRatio(float ratio)
        {
            _lockedRatio = ratio;
            SetSize(_box.Width, _box.Width / ratio);
        }

        /// <summary>Sets the absolute angle of the crop box.</summary>
        public void SetAngle(float deg)
        {
            _box.Angle = ((deg % 360f) + 360f) % 360f;
            Clamp();
            BoxChanged?.Invoke();
        }

        /// <summary>Sets the size of the crop box while keeping its center fixed.</summary>
        public void SetSize(float w, float h)
        {
            if (_lockedRatio.HasValue) h = w / _lockedRatio.Value;
            var cx = _box.X + _box.Width * 0.5f;
            var cy = _box.Y + _box.Height * 0.5f;
            _box.Width = w;
            _box.Height = h;
            _box.X = cx - w * 0.5f;
            _box.Y = cy - h * 0.5f;
            Clamp();
        }

        /// <summary>Starts an interaction when the pointer is pressed.</summary>
        public void OnPointerPressed(double x, double y)
        {
            HoverInteraction = HitTest((float)x, (float)y);
            ActiveInteraction = HoverInteraction;
            _pStart = new SKPoint((float)x, (float)y);
            _startBox = _box;

            if (ActiveInteraction == Interaction.Rotate)
            {
                var c = Centre();
                _startAngle = MathF.Atan2(_pStart.Y - c.Y, _pStart.X - c.X) * 180f / MathF.PI - _box.Angle;
            }
        }

        /// <summary>Updates the crop box while the pointer is moved.</summary>
        public void OnPointerMoved(double x, double y)
        {
            var dx = (float)x - _pStart.X;
            var dy = (float)y - _pStart.Y;

            switch (ActiveInteraction)
            {
                case Interaction.Move:
                    _box.X = _startBox.X + dx;
                    _box.Y = _startBox.Y + dy;
                    break;

                case Interaction.ResizeNW:
                    ResizeFromCorner(dx, dy, true, true);
                    break;
                case Interaction.ResizeNE:
                    ResizeFromCorner(dx, dy, false, true);
                    break;
                case Interaction.ResizeSW:
                    ResizeFromCorner(dx, dy, true, false);
                    break;
                case Interaction.ResizeSE:
                    ResizeFromCorner(dx, dy, false, false);
                    break;

                case Interaction.Rotate:
                    var c = Centre();
                    var aNow = MathF.Atan2((float)y - c.Y, (float)x - c.X) * 180f / MathF.PI;
                    _box.Angle = ((aNow - _startAngle) % 360f + 360f) % 360f;
                    break;

                default:
                    HoverInteraction = HitTest((float)x, (float)y);
                    break;
            }

            Clamp();
        }

        /// <summary>Ends the current interaction.</summary>
        public void OnPointerReleased() => ActiveInteraction = Interaction.None;

        /// <summary>Updates the hover state without starting an interaction.</summary>
        public void UpdateHover(double x, double y) => HoverInteraction = HitTest((float)x, (float)y);

        /// <summary>Draws the crop box and its handles onto the provided canvas.</summary>
        public void Draw(SKCanvas c)
        {
            using var p = new SKPaint { Style = SKPaintStyle.Stroke, Color = SKColors.White, StrokeWidth = 2 };
            c.Save();
            var ctr = Centre();
            c.Translate(ctr.X, ctr.Y);
            c.RotateDegrees(_box.Angle);

            using var guide = new SKPaint { Style = SKPaintStyle.Stroke, Color = SKColors.White.WithAlpha(80), StrokeWidth = 1 };
            for (int i = 1; i <= 2; i++)
            {
                var x = -_box.Width * 0.5f + _box.Width * i / 3f;
                var y = -_box.Height * 0.5f + _box.Height * i / 3f;
                c.DrawLine(x, -_box.Height * 0.5f, x, _box.Height * 0.5f, guide);
                c.DrawLine(-_box.Width * 0.5f, y, _box.Width * 0.5f, y, guide);
            }

            c.DrawRect(-_box.Width * 0.5f, -_box.Height * 0.5f, _box.Width, _box.Height, p);

            DrawHandle(c, -_box.Width * 0.5f, -_box.Height * 0.5f, Interaction.ResizeNW);
            DrawHandle(c, _box.Width * 0.5f, -_box.Height * 0.5f, Interaction.ResizeNE);
            DrawHandle(c, -_box.Width * 0.5f, _box.Height * 0.5f, Interaction.ResizeSW);
            DrawHandle(c, _box.Width * 0.5f, _box.Height * 0.5f, Interaction.ResizeSE);

            using var centreP = new SKPaint { Color = SKColors.Yellow, StrokeWidth = 1 };
            c.DrawLine(-8, 0, 8, 0, centreP);
            c.DrawLine(0, -8, 0, 8, centreP);

            c.Restore();
        }

        /// <summary>Returns the centre point of the crop box.</summary>
        private SKPoint Centre() => new(_box.X + _box.Width * 0.5f, _box.Y + _box.Height * 0.5f);

        /// <summary>Draws a single handle at the specified local position.</summary>
        private void DrawHandle(SKCanvas c, float x, float y, Interaction id)
        {
            var size = HANDLE;
            var act = ActiveInteraction == id;
            var hov = HoverInteraction == id;
            using var p = new SKPaint
            {
                Color = act ? SKColors.Orange : (hov ? SKColors.Lime : SKColors.White),
                Style = SKPaintStyle.Fill
            };
            c.DrawRect(x - size * 0.5f, y - size * 0.5f, size, size, p);
        }

        /// <summary>Performs hit-testing to determine which part of the box is under the pointer.</summary>
        private Interaction HitTest(float x, float y)
        {
            var ctr = Centre();
            var dx = x - ctr.X;
            var dy = y - ctr.Y;

            var rad = -_box.Angle * MathF.PI / 180f;
            var cos = MathF.Cos(rad);
            var sin = MathF.Sin(rad);

            var rx = dx * cos - dy * sin;
            var ry = dx * sin + dy * cos;

            if (IsNear(rx, ry, -_box.Width * 0.5f, -_box.Height * 0.5f)) return Interaction.ResizeNW;
            if (IsNear(rx, ry, _box.Width * 0.5f, -_box.Height * 0.5f)) return Interaction.ResizeNE;
            if (IsNear(rx, ry, -_box.Width * 0.5f, _box.Height * 0.5f)) return Interaction.ResizeSW;
            if (IsNear(rx, ry, _box.Width * 0.5f, _box.Height * 0.5f)) return Interaction.ResizeSE;

            var dist = MathF.Min(MathF.Abs(rx) - _box.Width * 0.5f, MathF.Abs(ry) - _box.Height * 0.5f);
            if (dist > 4 && dist < 18) return Interaction.Rotate;

            if (MathF.Abs(rx) < _box.Width * 0.5f && MathF.Abs(ry) < _box.Height * 0.5f) return Interaction.Move;

            return Interaction.None;
        }

        /// <summary>Checks whether a point is within the handle area of a corner.</summary>
        private static bool IsNear(float x, float y, float targetX, float targetY) =>
            MathF.Abs(x - targetX) <= HANDLE && MathF.Abs(y - targetY) <= HANDLE;

        /// <summary>Resizes the crop box from a corner with full rotation support.</summary>
        private void ResizeFromCorner(float dxG, float dyG, bool leftEdge, bool topEdge)
        {
            var rad = _startBox.Angle * MathF.PI / 180f;
            var cos = MathF.Cos(rad);
            var sin = MathF.Sin(rad);

            var dirX = new SKPoint(cos, sin);
            var dirY = new SKPoint(-sin, cos);

            var dW = dxG * dirX.X + dyG * dirX.Y;
            var dH = dxG * dirY.X + dyG * dirY.Y;

            if (leftEdge) dW = -dW;
            if (topEdge) dH = -dH;

            var newW = MathF.Max(_startBox.Width + dW, 32f);
            var newH = MathF.Max(_startBox.Height + dH, 32f);

            if (_lockedRatio.HasValue)
            {
                if (MathF.Abs(dW) >= MathF.Abs(dH))
                    newH = newW / _lockedRatio.Value;
                else
                    newW = newH * _lockedRatio.Value;
            }

            var deltaW = newW - _startBox.Width;
            var deltaH = newH - _startBox.Height;

            var centreShift = new SKPoint();
            if (MathF.Abs(deltaW) > 0.01f)
            {
                var sign = leftEdge ? -1f : 1f;
                centreShift.X += dirX.X * deltaW * sign * 0.5f;
                centreShift.Y += dirX.Y * deltaW * sign * 0.5f;
            }

            if (MathF.Abs(deltaH) > 0.01f)
            {
                var sign = topEdge ? -1f : 1f;
                centreShift.X += dirY.X * deltaH * sign * 0.5f;
                centreShift.Y += dirY.Y * deltaH * sign * 0.5f;
            }

            var centre = new SKPoint(_startBox.X + _startBox.Width * 0.5f, _startBox.Y + _startBox.Height * 0.5f);
            centre = new SKPoint(centre.X + centreShift.X, centre.Y + centreShift.Y);

            _box.Width = newW;
            _box.Height = newH;
            _box.X = centre.X - newW * 0.5f;
            _box.Y = centre.Y - newH * 0.5f;
        }


        private void Clamp()
        {
            if (_canvasW < 32f || _canvasH < 32f) return;

            static void GetBounds(in CropBox box, float canvasW, float canvasH,
                                  out float minX, out float minY,
                                  out float maxX, out float maxY)
            {
                var ctr = new SKPoint(box.X + box.Width * .5f, box.Y + box.Height * .5f);

                var rad = box.Angle * MathF.PI / 180f;
                var cos = MathF.Cos(rad);
                var sin = MathF.Sin(rad);

                var hw = box.Width * .5f;
                var hh = box.Height * .5f;

                minX = minY = float.MaxValue;
                maxX = maxY = float.MinValue;

                Span<SKPoint> local = stackalloc SKPoint[4]
                {
            new(-hw, -hh), new(hw, -hh), new(hw, hh), new(-hw, hh)
        };

                foreach (var p in local)
                {
                    var gx = p.X * cos - p.Y * sin + ctr.X;
                    var gy = p.X * sin + p.Y * cos + ctr.Y;

                    if (gx < minX) minX = gx;
                    if (gx > maxX) maxX = gx;
                    if (gy < minY) minY = gy;
                    if (gy > maxY) maxY = gy;
                }
            }

            GetBounds(_box, _canvasW, _canvasH, out var minX, out var minY, out var maxX, out var maxY);

            var bboxW = maxX - minX;
            var bboxH = maxY - minY;

            var scale = MathF.Min(1f, MathF.Min(_canvasW / bboxW, _canvasH / bboxH));
            if (scale < 0.999f)
            {
                _box.Width = MathF.Max(_box.Width * scale, 32f);
                _box.Height = MathF.Max(_box.Height * scale, 32f);

                GetBounds(_box, _canvasW, _canvasH, out minX, out minY, out maxX, out maxY);
            }

            var shiftX = 0f;
            var shiftY = 0f;

            if (minX < 0f) shiftX = -minX;
            else if (maxX > _canvasW) shiftX = _canvasW - maxX;

            if (minY < 0f) shiftY = -minY;
            else if (maxY > _canvasH) shiftY = _canvasH - maxY;

            if (MathF.Abs(shiftX) > 0.001f || MathF.Abs(shiftY) > 0.001f)
            {
                _box.X += shiftX;
                _box.Y += shiftY;
            }
        }


    }
}
