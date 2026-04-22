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

            SceneManager.SetDisplay(this.mainPanel);
        }

        protected override void OnShown(EventArgs e)
        {
            base.OnShown(e);

            // Lúc này cửa sổ đã hiện, an toàn để chạy game
            SceneManager.LoadScene(new GameplayScene());
        }
    }
}