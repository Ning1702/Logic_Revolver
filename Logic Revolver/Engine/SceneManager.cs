using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Logic_Revolver.Engine
{
    public static class SceneManager
    {
        private static Scene currentScene;
        private static Panel mainView; // Cái Panel chính của Form

        public static void SetDisplay(Panel panel)
        {
            mainView = panel;
        }

        public static void LoadScene(Scene newScene)
        {
            if (mainView == null) return;

            mainView.Controls.Clear(); // Xóa sạch giao diện màn cũ
            currentScene = newScene;
            currentScene.Load();       // Tải màn mới
            currentScene.Draw(mainView); // Vẽ màn mới
        }
    }
}
