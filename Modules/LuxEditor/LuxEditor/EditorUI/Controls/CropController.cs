using System;
using SkiaSharp;

namespace LuxEditor.EditorUI.Controls
{
    public sealed class CropController
    {
        public enum Interaction
        {
            None, Move, Rotate,
            ResizeNW, ResizeNE, ResizeSW, ResizeSE
        }

        public struct CropBox { public float X, Y, Width, Height, Angle; }

        public CropBox Box => _box;
        public bool LockAspectRatio { get => _lockedRatio.HasValue; set => _lockedRatio = value ? _box.Width / _box.Height : null; }
        public Interaction ActiveInteraction { get; private set; } = Interaction.None;
        public Interaction HoverInteraction { get; private set; } = Interaction.None;

        public event Action? BoxChanged;

        private CropBox _box;
        private float? _lockedRatio;
        private float _canvasW, _canvasH;

        private SKPoint _pStart;          
        private CropBox _startBox;
        private float _startAngle; 
        private const float HANDLE = 12f;

        public CropController(float w, float h) => ResizeCanvas(w, h);

        public void ResizeCanvas(float w, float h)
        {
            _canvasW = w; _canvasH = h;
            if (_box.Width < 1 || _box.Height < 1)
                Reset();
            Clamp();
        }

        public void Reset()
        {
            _box = new() { X = 0, Y = 0, Width = _canvasW, Height = _canvasH, Angle = 0 };
            _lockedRatio = null;
            BoxChanged?.Invoke();
        }

        public void Load(CropBox saved)
        {
            _box = saved;
            _lockedRatio = saved.Width > 0 && saved.Height > 0 ? saved.Width / saved.Height : null;
            Clamp();
        }

        public void ApplyPresetRatio(float ratio)
        {
            _lockedRatio = ratio;
            SetSize(_box.Width, _box.Width / ratio);
        }

        public void SetAngle(float deg)
        {
            _box.Angle = ((deg % 360) + 360) % 360;
            BoxChanged?.Invoke();
        }

        public void SetSize(float w, float h)
        {
            if (_lockedRatio.HasValue) h = w / _lockedRatio.Value;
            var cx = _box.X + _box.Width * .5f;
            var cy = _box.Y + _box.Height * .5f;
            _box.Width = w; _box.Height = h;
            _box.X = cx - w * .5f; _box.Y = cy - h * .5f;
            Clamp();
        }

        public void OnPointerPressed(double x, double y)
        {
            HoverInteraction = HitTest((float)x, (float)y);
            ActiveInteraction = HoverInteraction;
            _pStart = new((float)x, (float)y);
            _startBox = _box;

            if (ActiveInteraction == Interaction.Rotate)
            {
                var c = Centre();
                _startAngle = MathF.Atan2(_pStart.Y - c.Y, _pStart.X - c.X) * 180f / MathF.PI - _box.Angle;
            }
        }

        public void OnPointerMoved(double x, double y)
        {
            float dx = (float)x - _pStart.X;
            float dy = (float)y - _pStart.Y;

            switch (ActiveInteraction)
            {
                case Interaction.Move:
                    _box.X = _startBox.X + dx;
                    _box.Y = _startBox.Y + dy;
                    break;

                case Interaction.ResizeNW: ResizeFromCorner(+dx, +dy, true, true); break;
                case Interaction.ResizeNE: ResizeFromCorner(+dx, -dy, false, true); break;
                case Interaction.ResizeSW: ResizeFromCorner(+dx, -dy, true, false); break;
                case Interaction.ResizeSE: ResizeFromCorner(+dx, +dy, false, false); break;

                case Interaction.Rotate:
                    var c = Centre();
                    var aNow = MathF.Atan2((float)y - c.Y, (float)x - c.X) * 180f / MathF.PI;
                    _box.Angle = (aNow - _startAngle + 360f) % 360f;
                    break;

                default:
                    HoverInteraction = HitTest((float)x, (float)y);
                    break;
            }

            Clamp();
        }

        public void OnPointerReleased()
        {
            ActiveInteraction = Interaction.None;
        }

        public void UpdateHover(double x, double y) => HoverInteraction = HitTest((float)x, (float)y);


        public void Draw(SKCanvas c)
        {
            using var p = new SKPaint { Style = SKPaintStyle.Stroke, Color = SKColors.White, StrokeWidth = 2 };
            c.Save();
            var ctr = Centre();
            c.Translate(ctr.X, ctr.Y); c.RotateDegrees(_box.Angle);

            using var guide = new SKPaint { Style = SKPaintStyle.Stroke, Color = SKColors.White.WithAlpha(80), StrokeWidth = 1 };
            for (int i = 1; i <= 2; i++)
            {
                float x = -_box.Width * .5f + _box.Width * i / 3f;
                float y = -_box.Height * .5f + _box.Height * i / 3f;
                c.DrawLine(x, -_box.Height * .5f, x, _box.Height * .5f, guide);
                c.DrawLine(-_box.Width * .5f, y, _box.Width * .5f, y, guide);
            }

            c.DrawRect(-_box.Width * .5f, -_box.Height * .5f, _box.Width, _box.Height, p);

            DrawHandle(c, -_box.Width * .5f, -_box.Height * .5f, Interaction.ResizeNW);
            DrawHandle(c, _box.Width * .5f, -_box.Height * .5f, Interaction.ResizeNE);
            DrawHandle(c, -_box.Width * .5f, _box.Height * .5f, Interaction.ResizeSW);
            DrawHandle(c, _box.Width * .5f, _box.Height * .5f, Interaction.ResizeSE);

            using var centreP = new SKPaint { Color = SKColors.Yellow, StrokeWidth = 1 };
            c.DrawLine(-8, 0, 8, 0, centreP);
            c.DrawLine(0, -8, 0, 8, centreP);

            c.Restore();
        }

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
            c.DrawRect(x - size * .5f, y - size * .5f, size, size, p);
        }

        private Interaction HitTest(float x, float y)
        {
            var ctr = Centre();
            float dx = x - ctr.X, dy = y - ctr.Y;
            float rad = -_box.Angle * MathF.PI / 180f;
            float rx = dx * MathF.Cos(rad) - dy * MathF.Sin(rad);
            float ry = dx * MathF.Sin(rad) + dy * MathF.Cos(rad);

            if (IsNear(rx, ry, -_box.Width * .5f, -_box.Height * .5f)) return Interaction.ResizeNW;
            if (IsNear(rx, ry, _box.Width * .5f, -_box.Height * .5f)) return Interaction.ResizeNE;
            if (IsNear(rx, ry, -_box.Width * .5f, _box.Height * .5f)) return Interaction.ResizeSW;
            if (IsNear(rx, ry, _box.Width * .5f, _box.Height * .5f)) return Interaction.ResizeSE;

            float dist = MathF.Min(
                MathF.Abs(rx) - _box.Width * .5f,
                MathF.Abs(ry) - _box.Height * .5f);
            if (dist > 4 && dist < 18) return Interaction.Rotate;

            if (MathF.Abs(rx) < _box.Width * .5f && MathF.Abs(ry) < _box.Height * .5f)
                return Interaction.Move;

            return Interaction.None;
        }

        private static bool IsNear(float x, float y, float targetX, float targetY)
            => MathF.Abs(x - targetX) <= HANDLE && MathF.Abs(y - targetY) <= HANDLE;

        private SKPoint Centre() => new(_box.X + _box.Width * .5f,
                                        _box.Y + _box.Height * .5f);

        private void ResizeFromCorner(float dxG, float dyG, bool left, bool top)
        {
            float a = -_startBox.Angle * MathF.PI / 180f;
            float cos = MathF.Cos(a), sin = MathF.Sin(a);
            float lx = dxG * cos - dyG * sin;
            float ly = dxG * sin + dyG * cos;

            float newW = _startBox.Width + (left ? -lx : lx);
            float newH = _startBox.Height + (top ? -ly : ly);

            if (_lockedRatio.HasValue)
                newH = newW / _lockedRatio.Value;

            newW = Math.Max(newW, 32);
            newH = Math.Max(newH, 32);

            _box.X = left ? _startBox.X + (_startBox.Width - newW) : _startBox.X;
            _box.Y = top ? _startBox.Y + (_startBox.Height - newH) : _startBox.Y;
            _box.Width = newW;
            _box.Height = newH;
        }

        private void Clamp()
        {
            if (_canvasW <= 1 || _canvasH <= 1) return;
            _box.Width = Math.Clamp(_box.Width, 32, _canvasW);
            _box.Height = Math.Clamp(_box.Height, 32, _canvasH);
            _box.X = Math.Clamp(_box.X, 0, _canvasW - _box.Width);
            _box.Y = Math.Clamp(_box.Y, 0, _canvasH - _box.Height);
            BoxChanged?.Invoke();
        }
    }
}
