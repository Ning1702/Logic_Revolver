using Logic_Revolver.Engine;
using Logic_Revolver.Game.Scenes;
using System;
using System.Windows.Forms;

namespace Logic_Revolver
{
    public partial class FormMain : Form
    {
        public FormMain()
        {
            InitializeComponent();

            // Giữ dữ liệu cũ của bạn
            SceneManager.SetDisplay(this.mainPanel);

            // Bổ sung để form scale/resize được
            this.FormBorderStyle = FormBorderStyle.Sizable;
            this.MaximizeBox = true;
            this.MinimizeBox = true;
            this.StartPosition = FormStartPosition.CenterScreen;

            this.mainPanel.Dock = DockStyle.Fill;

            this.Resize += FormMain_Resize;
        }

        protected override void OnShown(EventArgs e)
        {
            base.OnShown(e);

            // Lúc này cửa sổ đã hiện, an toàn để chạy game
            SceneManager.LoadScene(new GameplayScene());
        }

        private void FormMain_Resize(object sender, EventArgs e)
        {
            // Khi đổi kích thước form thì vẽ lại scene theo size mới
            if (mainPanel.Width > 0 && mainPanel.Height > 0)
            {
                SceneManager.CurrentScene?.Draw(mainPanel);
            }
        }

        protected override void OnFormClosing(FormClosingEventArgs e)
        {
            base.OnFormClosing(e);

            // Chỉ khi người dùng bấm nút X mới thoát hẳn chương trình
            if (e.CloseReason == CloseReason.UserClosing)
            {
                Application.Exit();
            }
        }
    }
}