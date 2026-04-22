using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Drawing;

namespace SharedCore
{
    internal class ImageLoader
    {
        public static Image Load(string fileName)
        {
            string path = Path.Combine(
                AppDomain.CurrentDomain.BaseDirectory,
                "Image_share",
                fileName
            );

            if (!File.Exists(path))
                throw new FileNotFoundException($"Image not found: {path}");

            return Image.FromFile(path);
        }
    }
}
