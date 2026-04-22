using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Logic_Revolver.Game.UI
{
    public class CowboyButton : Button
    {
        private Color _baseColor; 
        private Color _hoverColor; 
        private Color _clickColor;

        public CowboyButton()
        {
            this.FlatStyle = FlatStyle.Flat;
            this.FlatAppearance.BorderSize = 2;
            this.Font = new Font("Georgia", 12, FontStyle.Bold); 
            this.Cursor = Cursors.Hand;
            this.Size = new Size(130, 50); 
            this.ForeColor = Color.WhiteSmoke;

            SetColors(Color.SaddleBrown);

            this.MouseEnter += (s, e) => this.BackColor = _hoverColor;
            this.MouseLeave += (s, e) => this.BackColor = _baseColor;
            this.MouseDown += (s, e) => this.BackColor = _clickColor;
            this.MouseUp += (s, e) => this.BackColor = _hoverColor;
        }

        public void SetColors(Color baseColor)
        {
            _baseColor = baseColor;
            // Tạo màu hover sáng hơn, màu click tối hơn
            _hoverColor = ControlPaint.Light(baseColor, 0.2f);
            _clickColor = ControlPaint.Dark(baseColor, 0.2f);

            this.BackColor = _baseColor;
            this.FlatAppearance.BorderColor = ControlPaint.Dark(_baseColor, 0.5f); // Viền tối màu
        }
    }

    // 2. GROUP BOX CAO BỒI (Chữ to, rõ)
    public class CowboyGroupBox : GroupBox
    {
        public CowboyGroupBox()
        {
            this.ForeColor = Color.FromArgb(101, 67, 33);
            this.Font = new Font("Georgia", 12, FontStyle.Bold);
        }
        protected override void OnPaint(PaintEventArgs e)
        {
            base.OnPaint(e);
            ControlPaint.DrawBorder(e.Graphics, this.ClientRectangle,
                Color.FromArgb(101, 67, 33), ButtonBorderStyle.Solid);
        }
    }

    // 3. LOG BẢNG TIN (Kiểu giấy cũ)
    public class CowboyLog : RichTextBox
    {
        public CowboyLog()
        {
            this.BackColor = Color.FromArgb(255, 248, 220);
            this.ForeColor = Color.Black;
            this.Font = new Font("Courier New", 10, FontStyle.Regular);
            this.BorderStyle = BorderStyle.Fixed3D;
            this.ReadOnly = true; 
            this.Cursor = Cursors.Default; 
        }
    }
}
