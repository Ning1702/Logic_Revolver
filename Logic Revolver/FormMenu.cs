using System;
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;
using Logic_Revolver.Engine;
using Logic_Revolver.Properties;

namespace Logic_Revolver
{
    public partial class FormMenu : Form
    {
        Size baseFormSize;

        float baseMenuFontSize;
        float baseTitleFontSize;

        List<Label> menuLabels;

        // Lưu style + màu GỐC của từng label
        Dictionary<Label, FontStyle> baseStyles = new Dictionary<Label, FontStyle>();
        Dictionary<Label, Color> baseColors = new Dictionary<Label, Color>();

        public FormMenu()
        {
            InitializeComponent();

            // ❗ Không cho WinForms auto scale
            this.AutoScaleMode = AutoScaleMode.None;

            // Size form gốc
            baseFormSize = this.ClientSize;

            // Size font gốc
            baseMenuFontSize = lblStart.Font.Size;   // 19.8
            baseTitleFontSize = lblTitle.Font.Size;

            // Gom menu
            menuLabels = new List<Label>
            {
                lblStart,
                lblOptions,
                lblRules,
                lblExit
            };

            // Lưu trạng thái ban đầu + gắn hover
            foreach (var lbl in menuLabels)
            {
                lbl.Cursor = Cursors.Hand;

                baseStyles[lbl] = lbl.Font.Style;      // Bold
                baseColors[lbl] = lbl.ForeColor;       // Bisque

                lbl.MouseEnter += Label_MouseEnter;
                lbl.MouseLeave += Label_MouseLeave;
            }

            this.Resize += Form1_Resize;

            // Gọi 1 lần để scale đúng lúc mở form
            Form1_Resize(this, EventArgs.Empty);

            // --- 2. GỌI NHẠC NỀN MENU TẠI ĐÂY ---
            if (Resources.bgm_menu != null)
            {
                AudioManager.PlayBGM(Resources.bgm_menu);
            }
        }

        private void Label_MouseEnter(object sender, EventArgs e)
        {
            if (sender is Label lbl)
            {
                // Giữ style gốc (Bold) + thêm Underline
                FontStyle hoverStyle = baseStyles[lbl] | FontStyle.Underline;

                lbl.Font = new Font(
                    lbl.Font.FontFamily,
                    lbl.Font.Size,          // GIỮ size đã scale
                    hoverStyle
                );

                // Màu khi hover (tuỳ bạn đổi)
                lbl.ForeColor = Color.Orange;
            }
        }

        private void Label_MouseLeave(object sender, EventArgs e)
        {
            if (sender is Label lbl)
            {
                // Trả về style & màu GỐC, giữ size
                lbl.Font = new Font(
                    lbl.Font.FontFamily,
                    lbl.Font.Size,
                    baseStyles[lbl]          // Bold gốc
                );

                lbl.ForeColor = baseColors[lbl]; // Bisque gốc
            }
        }

        private void Form1_Resize(object sender, EventArgs e)
        {
            float scaleX = (float)this.ClientSize.Width / baseFormSize.Width;
            float scaleY = (float)this.ClientSize.Height / baseFormSize.Height;

            float scale = Math.Min(scaleX, scaleY);

            // Scale TITLE (giữ style hiện có)
            lblTitle.Font = new Font(
                lblTitle.Font.FontFamily,
                baseTitleFontSize * scale,
                lblTitle.Font.Style
            );

            // CĂN TITLE RA GIỮA SAU KHI SCALE
            lblTitle.Left = (panelTitle.ClientSize.Width - lblTitle.Width) / 2;

            // Scale MENU (giữ style hiện có: Bold / Hover)
            foreach (var lbl in menuLabels)
            {
                lbl.Font = new Font(
                    lbl.Font.FontFamily,
                    baseMenuFontSize * scale,
                    lbl.Font.Style
                );
            }
        }

        private void lblExit_Click(object sender, EventArgs e)
        {
            using (FormExitConfirm frm = new FormExitConfirm())
            {
                if (frm.ShowDialog(this) == DialogResult.Yes)
                {
                    Application.Exit();
                }
            }
        }

        private void lblRules_Click(object sender, EventArgs e)
        {
            using (var f = new Rules())
            {
                f.StartPosition = FormStartPosition.CenterParent;
                f.ShowDialog(this); // <-- truyền this làm owner
            }
        }

        private void lblStart_Click(object sender, EventArgs e)
        {
            // --- 3. DỌN DẸP ÂM THANH MENU TRƯỚC KHI VÀO GAME ---
            AudioManager.StopAll();

            FormMain main = new FormMain();
            main.Show();

            this.Hide();
        }

        private void lblOptions_Click(object sender, EventArgs e)
        {
            using (FormSound frm = new FormSound())
            {
                frm.StartPosition = FormStartPosition.CenterParent;
                frm.ShowDialog(this);
            }
        }
    }
}