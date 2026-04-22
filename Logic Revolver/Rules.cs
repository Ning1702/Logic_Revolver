using System;
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;
using Logic_Revolver.Properties;

namespace Logic_Revolver
{
    public partial class Rules : Form
    {
        private Size baseFormSize;

        private float baseTitleFontSize;
        private float baseBodyFontSize;
        private float baseButtonFontSize;

        private Dictionary<Control, Rectangle> baseBounds = new Dictionary<Control, Rectangle>();

        public Rules()
        {
            InitializeComponent();

            this.AutoScaleMode = AutoScaleMode.None;

            baseFormSize = this.ClientSize;

            baseTitleFontSize = label5.Font.Size;
            baseBodyFontSize = label2.Font.Size;
            baseButtonFontSize = btnQuaylai.Font.Size;

            SaveControlBounds(this);

            this.Load += Rules_Load;
            this.Resize += Rules_Resize;
        }

        private void Rules_Load(object sender, EventArgs e)
        {
            panelRules.BackgroundImage = Resources.Brown;
            panelRules.BackgroundImageLayout = ImageLayout.Stretch;

            ApplyButtonStyle(btnQuaylai);
            ApplyButtonStyle(btnChitiet);

            btnQuaylai.Text = "BACK";
            btnChitiet.Text = "ITEMS";

            Rules_Resize(this, EventArgs.Empty);
        }

        private void ApplyButtonStyle(Button btn)
        {
            btn.BackColor = Color.FromArgb(70, 40, 20);
            btn.ForeColor = Color.Bisque;
            btn.FlatStyle = FlatStyle.Flat;
            btn.FlatAppearance.BorderColor = Color.Bisque;
            btn.FlatAppearance.BorderSize = 1;
            btn.FlatAppearance.MouseOverBackColor = Color.FromArgb(95, 55, 30);
            btn.FlatAppearance.MouseDownBackColor = Color.FromArgb(55, 30, 15);
            btn.Font = new Font("Georgia", 11, FontStyle.Bold);
            btn.Cursor = Cursors.Hand;
            btn.UseVisualStyleBackColor = false;
        }

        private void SaveControlBounds(Control parent)
        {
            foreach (Control ctrl in parent.Controls)
            {
                baseBounds[ctrl] = ctrl.Bounds;

                if (ctrl.Controls.Count > 0)
                    SaveControlBounds(ctrl);
            }
        }

        private void ScaleControls(Control parent, float scaleX, float scaleY)
        {
            foreach (Control ctrl in parent.Controls)
            {
                if (baseBounds.ContainsKey(ctrl))
                {
                    Rectangle r = baseBounds[ctrl];

                    ctrl.Left = (int)(r.Left * scaleX);
                    ctrl.Top = (int)(r.Top * scaleY);
                    ctrl.Width = (int)(r.Width * scaleX);
                    ctrl.Height = (int)(r.Height * scaleY);
                }

                if (ctrl.Controls.Count > 0)
                    ScaleControls(ctrl, scaleX, scaleY);
            }
        }

        private void Rules_Resize(object sender, EventArgs e)
        {
            if (baseFormSize.Width == 0 || baseFormSize.Height == 0) return;

            float scaleX = (float)this.ClientSize.Width / baseFormSize.Width;
            float scaleY = (float)this.ClientSize.Height / baseFormSize.Height;
            float scale = Math.Min(scaleX, scaleY);

            ScaleControls(this, scaleX, scaleY);

            label5.Font = new Font(
                label5.Font.FontFamily,
                baseTitleFontSize * scale,
                label5.Font.Style
            );

            label5.Left = (panel4.ClientSize.Width - label5.Width) / 2;
            label5.Top = Math.Max(10, (panel4.ClientSize.Height - label5.Height) / 2);

            label2.Font = new Font(
                label2.Font.FontFamily,
                baseBodyFontSize * scale,
                label2.Font.Style
            );

            btnQuaylai.Font = new Font(
                btnQuaylai.Font.FontFamily,
                baseButtonFontSize * scale,
                btnQuaylai.Font.Style
            );

            btnChitiet.Font = new Font(
                btnChitiet.Font.FontFamily,
                baseButtonFontSize * scale,
                btnChitiet.Font.Style
            );
        }

        private void btnQuaylai_Click(object sender, EventArgs e)
        {
            this.Close();
        }

        private void btnChitiet_Click(object sender, EventArgs e)
        {
            Item f = new Item();
            f.Show();
        }
    }
}