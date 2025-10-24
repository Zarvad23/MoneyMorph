using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;

namespace MoneyMorph
{
    public class NeonPanel : Panel
    {
        private float _phase; // Текущая фаза анимации
        private readonly Color[] _palette =
        {
            Color.FromArgb(255, 120, 109, 245),
            Color.FromArgb(255, 78, 205, 196),
            Color.FromArgb(255, 255, 94, 205),
            Color.FromArgb(255, 0, 199, 255)
        };

        public NeonPanel()
        {
            DoubleBuffered = true; // Уменьшает мерцание при перерисовке
            ResizeRedraw = true; // Перерисовывает панель при изменении размеров
        }

        public void AdvancePhase(float delta)
        {
            _phase = (_phase + delta) % 1f;
            if (_phase < 0f)
            {
                _phase += 1f;
            }

            Invalidate(); // Запрашивает перерисовку для отображения обновлённой анимации
        }

        protected override void OnPaintBackground(PaintEventArgs pevent)
        {
            // Фоновая отрисовка выполняется в OnPaint, поэтому базовую реализацию не вызываем
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            Rectangle rect = ClientRectangle;
            if (rect.Width <= 0 || rect.Height <= 0)
            {
                base.OnPaint(e);
                return;
            }

            e.Graphics.SmoothingMode = SmoothingMode.AntiAlias;

            float angle = 35f + (float)Math.Sin(_phase * Math.PI * 2f) * 45f; // Угол градиента плавно меняется
            Color start = Interpolate(_palette[0], _palette[1], SmoothWave(_phase + 0.05f));
            Color middle = Interpolate(_palette[1], _palette[2], SmoothWave(_phase + 0.35f));
            Color end = Interpolate(_palette[2], _palette[3], SmoothWave(_phase + 0.65f));

            using LinearGradientBrush brush = new LinearGradientBrush(rect, start, end, angle);
            ColorBlend blend = new ColorBlend
            {
                Colors = new[]
                {
                    Color.FromArgb(220, start),
                    Color.FromArgb(210, middle),
                    Color.FromArgb(220, end)
                },
                Positions = new[] { 0f, 0.5f, 1f }
            };
            brush.InterpolationColors = blend;
            e.Graphics.FillRectangle(brush, rect);

            Rectangle glowRect = new Rectangle(rect.Width / -4, rect.Height / 4, rect.Width * 3 / 2, rect.Height);
            using GraphicsPath path = new GraphicsPath();
            path.AddEllipse(glowRect);
            using PathGradientBrush glowBrush = new PathGradientBrush(path)
            {
                CenterColor = Color.FromArgb(96, Color.White),
                SurroundColors = new[] { Color.FromArgb(0, Color.White) }
            };
            e.Graphics.FillPath(glowBrush, path);

            base.OnPaint(e); // Отрисовывает дочерние элементы поверх градиента
        }

        private static float SmoothWave(float value)
        {
            float t = value % 1f;
            if (t < 0f)
            {
                t += 1f;
            }

            return 0.5f - 0.5f * (float)Math.Cos(t * Math.PI * 2f);
        }

        private static Color Interpolate(Color from, Color to, float amount)
        {
            amount = Math.Clamp(amount, 0f, 1f);
            int a = (int)(from.A + (to.A - from.A) * amount);
            int r = (int)(from.R + (to.R - from.R) * amount);
            int g = (int)(from.G + (to.G - from.G) * amount);
            int b = (int)(from.B + (to.B - from.B) * amount);
            return Color.FromArgb(a, r, g, b);
        }
    }
}
