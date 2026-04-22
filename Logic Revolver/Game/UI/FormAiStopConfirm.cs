using System;
using System.Drawing;
using System.Windows.Forms;
using Logic_Revolver.Properties;

namespace Logic_Revolver
{
    public class FormAiStopConfirm : Form
    {
        private Label lblTitle;
        private Label lblMessage;
        private CowboyButton btnYes;
        private CowboyButton btnNo;
        private Panel panelMain;

        public FormAiStopConfirm()
        {
            InitializeUI();
        }

        private void InitializeUI()
        {
            this.Text = "AI Hỗ Trợ";
            this.StartPosition = FormStartPosition.CenterParent;
            this.FormBorderStyle = FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.ShowInTaskbar = false;
            this.ClientSize = new Size(430, 230);
            this.BackColor = Color.FromArgb(101, 67, 33);

            panelMain = new Panel
            {
                Dock = DockStyle.Fill,
                BackgroundImage = Resources.Brown,
                BackgroundImageLayout = ImageLayout.Stretch
            };
            this.Controls.Add(panelMain);

            lblTitle = new Label
            {
                Text = "TẮT AI TỰ CHƠI",
                AutoSize = false,
                Size = new Size(360, 40),
                Location = new Point(35, 20),
                TextAlign = ContentAlignment.MiddleCenter,
                Font = new Font("Georgia", 18, FontStyle.Bold),
                ForeColor = Color.Bisque,
                BackColor = Color.Transparent
            };

            lblMessage = new Label
            {
                Text = "Bạn muốn tắt AI tự chơi?",
                AutoSize = false,
                Size = new Size(360, 50),
                Location = new Point(35, 78),
                TextAlign = ContentAlignment.MiddleCenter,
                Font = new Font("Georgia", 12, FontStyle.Bold),
                ForeColor = Color.WhiteSmoke,
                BackColor = Color.Transparent
            };

            btnYes = new CowboyButton
            {
                Text = "CÓ",
                Size = new Size(120, 45),
                Location = new Point(75, 155),
                Font = new Font("Georgia", 11, FontStyle.Bold)
            };
            btnYes.SetColors(Color.DarkRed);
            btnYes.Click += (s, e) =>
            {
                this.DialogResult = DialogResult.Yes;
                this.Close();
            };

            btnNo = new CowboyButton
            {
                Text = "KHÔNG",
                Size = new Size(120, 45),
                Location = new Point(235, 155),
                Font = new Font("Georgia", 11, FontStyle.Bold)
            };
            btnNo.SetColors(Color.FromArgb(70, 40, 20));
            btnNo.Click += (s, e) =>
            {
                this.DialogResult = DialogResult.No;
                this.Close();
            };

            panelMain.Controls.Add(lblTitle);
            panelMain.Controls.Add(lblMessage);
            panelMain.Controls.Add(btnYes);
            panelMain.Controls.Add(btnNo);
        }
    }
}