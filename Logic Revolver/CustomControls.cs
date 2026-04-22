using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;

namespace Logic_Revolver
{
    public class CowboyButton : Button
    {
        public Color BaseColor { get; set; } = Color.FromArgb(101, 67, 33);
        public Color HoverColor { get; set; } = Color.FromArgb(130, 85, 45);
        public Color PressColor { get; set; } = Color.FromArgb(70, 40, 20);

        private bool isHovering = false;
        private bool isPressed = false;

        public CowboyButton()
        {
            FlatStyle = FlatStyle.Flat;
            FlatAppearance.BorderSize = 0;
            FlatAppearance.MouseDownBackColor = Color.Transparent;
            FlatAppearance.MouseOverBackColor = Color.Transparent;
            BackColor = Color.FromArgb(101, 67, 33);
            ForeColor = Color.Bisque;
            Font = new Font("Georgia", 11, FontStyle.Bold);
            Cursor = Cursors.Hand;

            SetStyle(
                ControlStyles.AllPaintingInWmPaint |
                ControlStyles.UserPaint |
                ControlStyles.OptimizedDoubleBuffer |
                ControlStyles.ResizeRedraw,
                true
            );
        }

        public void SetColors(Color baseColor)
        {
            BaseColor = baseColor;
            HoverColor = ControlPaint.Light(baseColor, 0.2f);
            PressColor = ControlPaint.Dark(baseColor, 0.2f);
            BackColor = baseColor;
            Invalidate();
        }

        protected override void OnMouseEnter(EventArgs e)
        {
            isHovering = true;
            Invalidate();
            base.OnMouseEnter(e);
        }

        protected override void OnMouseLeave(EventArgs e)
        {
            isHovering = false;
            isPressed = false;
            Invalidate();
            base.OnMouseLeave(e);
        }

        protected override void OnMouseDown(MouseEventArgs mevent)
        {
            isPressed = true;
            Invalidate();
            base.OnMouseDown(mevent);
        }

        protected override void OnMouseUp(MouseEventArgs mevent)
        {
            isPressed = false;
            Invalidate();
            base.OnMouseUp(mevent);
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            Rectangle rect = ClientRectangle;
            rect.Width -= 1;
            rect.Height -= 1;

            Color fill = BaseColor;
            if (isPressed) fill = PressColor;
            else if (isHovering) fill = HoverColor;

            using (SolidBrush brush = new SolidBrush(fill))
            using (Pen pen = new Pen(Color.FromArgb(220, 230, 200, 160), 2f))
            {
                e.Graphics.FillRectangle(brush, rect);
                e.Graphics.DrawRectangle(pen, rect);
            }

            TextRenderer.DrawText(
                e.Graphics,
                Text,
                Font,
                rect,
                ForeColor,
                TextFormatFlags.HorizontalCenter | TextFormatFlags.VerticalCenter
            );
        }
    }

    public class CowboyToggle : Control
    {
        private bool isChecked;

        public bool Checked
        {
            get => isChecked;
            set
            {
                if (isChecked != value)
                {
                    isChecked = value;
                    Invalidate();
                    CheckedChanged?.Invoke(this, EventArgs.Empty);
                }
            }
        }

        public event EventHandler CheckedChanged;

        public CowboyToggle()
        {
            Size = new Size(74, 34);
            Cursor = Cursors.Hand;

            SetStyle(
                ControlStyles.AllPaintingInWmPaint |
                ControlStyles.UserPaint |
                ControlStyles.OptimizedDoubleBuffer |
                ControlStyles.ResizeRedraw |
                ControlStyles.SupportsTransparentBackColor,
                true
            );

            BackColor = Color.Transparent;
        }

        protected override void OnClick(EventArgs e)
        {
            Checked = !Checked;
            base.OnClick(e);
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            e.Graphics.SmoothingMode = SmoothingMode.AntiAlias;

            Rectangle rect = new Rectangle(0, 0, Width - 1, Height - 1);
            int radius = Height / 2;
            int knobSize = Height - 8;
            int knobX = Checked ? Width - knobSize - 4 : 4;
            Color bg = Checked ? Color.FromArgb(55, 140, 80) : Color.FromArgb(90, 55, 35);

            using (GraphicsPath path = RoundedRect(rect, radius))
            using (SolidBrush brush = new SolidBrush(bg))
            using (Pen pen = new Pen(Color.FromArgb(180, 230, 200, 160), 1.2f))
            {
                e.Graphics.FillPath(brush, path);
                e.Graphics.DrawPath(pen, path);
            }

            Rectangle knob = new Rectangle(knobX, 4, knobSize, knobSize);
            using (SolidBrush brush = new SolidBrush(Color.Bisque))
            {
                e.Graphics.FillEllipse(brush, knob);
            }

            string text = Checked ? "ON" : "OFF";
            TextRenderer.DrawText(
                e.Graphics,
                text,
                new Font("Georgia", 8.5f, FontStyle.Bold),
                rect,
                Color.WhiteSmoke,
                TextFormatFlags.HorizontalCenter | TextFormatFlags.VerticalCenter
            );
        }

        private GraphicsPath RoundedRect(Rectangle bounds, int radius)
        {
            int d = radius * 2;
            GraphicsPath path = new GraphicsPath();

            path.AddArc(bounds.X, bounds.Y, d, d, 180, 90);
            path.AddArc(bounds.Right - d, bounds.Y, d, d, 270, 90);
            path.AddArc(bounds.Right - d, bounds.Bottom - d, d, d, 0, 90);
            path.AddArc(bounds.X, bounds.Bottom - d, d, d, 90, 90);
            path.CloseFigure();

            return path;
        }
    }

    public class CowboySlider : Control
    {
        private int minimum = 0;
        private int maximum = 100;
        private int currentValue = 70;
        private bool dragging = false;

        public int Minimum
        {
            get => minimum;
            set
            {
                minimum = value;
                if (currentValue < minimum) currentValue = minimum;
                Invalidate();
            }
        }

        public int Maximum
        {
            get => maximum;
            set
            {
                maximum = value;
                if (currentValue > maximum) currentValue = maximum;
                Invalidate();
            }
        }

        public int Value
        {
            get => currentValue;
            set
            {
                int newValue = Math.Max(Minimum, Math.Min(Maximum, value));
                if (currentValue != newValue)
                {
                    currentValue = newValue;
                    Invalidate();
                    ValueChanged?.Invoke(this, EventArgs.Empty);
                }
            }
        }

        public event EventHandler ValueChanged;

        public CowboySlider()
        {
            Size = new Size(190, 40);
            Cursor = Cursors.Hand;

            SetStyle(
                ControlStyles.AllPaintingInWmPaint |
                ControlStyles.UserPaint |
                ControlStyles.OptimizedDoubleBuffer |
                ControlStyles.ResizeRedraw |
                ControlStyles.SupportsTransparentBackColor,
                true
            );

            BackColor = Color.Transparent;
        }

        protected override void OnMouseDown(MouseEventArgs e)
        {
            dragging = true;
            UpdateValueFromX(e.X);
            base.OnMouseDown(e);
        }

        protected override void OnMouseMove(MouseEventArgs e)
        {
            if (dragging)
                UpdateValueFromX(e.X);

            base.OnMouseMove(e);
        }

        protected override void OnMouseUp(MouseEventArgs e)
        {
            dragging = false;
            base.OnMouseUp(e);
        }

        private void UpdateValueFromX(int x)
        {
            int left = 16;
            int right = Width - 16;
            float percent = (float)(x - left) / Math.Max(1, right - left);
            percent = Math.Max(0f, Math.Min(1f, percent));

            Value = Minimum + (int)Math.Round(percent * (Maximum - Minimum));
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            e.Graphics.SmoothingMode = SmoothingMode.AntiAlias;

            int left = 16;
            int right = Width - 16;
            int y = Height / 2;

            using (Pen backPen = new Pen(Color.FromArgb(120, 70, 40, 20), 8))
            using (Pen fillPen = new Pen(Color.Bisque, 8))
            {
                e.Graphics.DrawLine(backPen, left, y, right, y);

                float percent = (float)(Value - Minimum) / Math.Max(1, Maximum - Minimum);
                int fillX = left + (int)((right - left) * percent);

                e.Graphics.DrawLine(fillPen, left, y, fillX, y);

                for (int i = 0; i <= 10; i++)
                {
                    int tx = left + (right - left) * i / 10;
                    e.Graphics.DrawLine(Pens.BurlyWood, tx, y + 8, tx, y + 12);
                }

                Rectangle knob = new Rectangle(fillX - 10, y - 10, 20, 20);
                using (SolidBrush brush = new SolidBrush(Color.FromArgb(101, 67, 33)))
                using (Pen pen = new Pen(Color.Bisque, 2))
                {
                    e.Graphics.FillEllipse(brush, knob);
                    e.Graphics.DrawEllipse(pen, knob);
                }
            }
        }
    }
}