using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Logic_Revolver.Engine
{
    public abstract class Scene
    {
        public abstract void Load();   // Chạy khi mới vào màn
        public abstract void Update(); // Chạy liên tục mỗi khung hình
        public abstract void Draw(Panel view); // Vẽ giao diện
    }
}
