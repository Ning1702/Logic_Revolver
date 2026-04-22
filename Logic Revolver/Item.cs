using Logic_Revolver.Properties;   // dùng Resources từ SharedCore
using System;
using System.Drawing;
using System.Windows.Forms;

namespace Logic_Revolver
{
    public partial class Item : Form
    {
        public Item()
        {
            InitializeComponent();
        }

        public class ItemData
        {
            public string Name { get; set; }
            public string Desc { get; set; }
            public Image Icon { get; set; }
        }

        private PictureBox _selected;

        private void Item_Click(object sender, EventArgs e)
        {
            PictureBox pb = sender as PictureBox;
            if (pb == null) return;

            ItemData data = pb.Tag as ItemData;
            if (data == null) return;

            // Update khu chi tiết bên phải
            lblName.Text = data.Name;
            rtbDesc.Text = data.Desc;

            picDetail.Image = data.Icon;
            picDetail.SizeMode = PictureBoxSizeMode.Zoom;

            // Highlight item đang chọn
            if (_selected != null)
            {
                _selected.BackColor = Color.Transparent;
                _selected.Padding = new Padding(0);
            }

            _selected = pb;
            _selected.BackColor = Color.FromArgb(50, 255, 255, 255);
            _selected.Padding = new Padding(3);
        }


        private void Item_Load(object sender, EventArgs e)
        {
            // tránh Designer crash
            if (DesignMode) return;

            this.BackgroundImage = Resources.Brown;
            this.BackgroundImageLayout = ImageLayout.Stretch;

            // Setup chung cho các icon bên trái
            PictureBox[] icons =
            {
                picBeer, picBullet, picMagnifier, picCigarette, picSaw, pictHandcuff
            };

            foreach (var pb in icons)
            {
                pb.Cursor = Cursors.Hand;
                pb.SizeMode = PictureBoxSizeMode.Zoom;
                pb.BackColor = Color.Transparent;
                pb.Click -= Item_Click;      // tránh bị gắn nhiều lần
                pb.Click += Item_Click;
            }

            // Setup khu chi tiết bên phải
            rtbDesc.ReadOnly = true;
            rtbDesc.BorderStyle = BorderStyle.None;
            rtbDesc.Font = new Font("Segoe UI", 15F, FontStyle.Regular);
            rtbDesc.ForeColor = Color.Black;
            rtbDesc.BackColor = Color.Peru;

            lblName.Font = new Font("Georgia", 20F, FontStyle.Bold);
            lblName.ForeColor = Color.Black;
            lblName.AutoSize = true;

            picDetail.SizeMode = PictureBoxSizeMode.Zoom;

            // Gán ảnh hiển thị cho PictureBox (bên trái)
            picBeer.Image = Resources.Beer;
            picBullet.Image = Resources.Bullet;
            picMagnifier.Image = Resources.Magnifier;
            picCigarette.Image = Resources.Tobacco;
            picSaw.Image = Resources.Saw;
            pictHandcuff.Image = Resources.Handcuff;

            // ===== GÁN DATA (Tag) =====
            picBeer.Tag = new ItemData
            {
                Name = "Bia",
                Desc = "• Hồi phục 1 máu.\n• Gợi ý: Dùng khi bạn đang thấp máu trước lượt nguy hiểm.",
                Icon = Resources.Beer
            };

            picBullet.Tag = new ItemData
            {
                Name = "Đạn",
                Desc = "• Mỗi viên đạn thật sẽ mất 1 máu, đạn rỗng sẽ không mất máu\n• Gợi ý: Có cả đạn rỗng và đạn thật, tính toán kỹ trước khi bắn.",
                Icon = Resources.Bullet
            };

            picMagnifier.Tag = new ItemData
            {
                Name = "Kính lúp",
                Desc = "• Soi thông tin viên đạn kế tiếp .\n• Gợi ý: Giúp quyết định bắn Dealer hay bắn mình.",
                Icon = Resources.Magnifier
            };

            picCigarette.Tag = new ItemData
            {
                Name = "Thuốc lá",
                Desc = "• Hồi 1 máu mỗi lần sử dụng.\n• Gợi ý: Nên sử dụng khi yếu máu để giảm rủi ro bị hạ sớm.",
                Icon = Resources.Tobacco
            };

            picSaw.Tag = new ItemData
            {
                Name = "Cưa",
                Desc = "• Cưa nòng súng và lần bắn tiếp theo sẽ gây x2 sát thương.\n• Gợi ý: Sử dung khi biết chắc viên tiếp theo là đạn thật để tối ưu sát thương.",
                Icon = Resources.Saw
            };

            pictHandcuff.Tag = new ItemData
            {
                Name = "Còng tay",
                Desc = "• Khống chế đối thủ trong 1 lượt.\n• Gợi ý: Dùng khi muốn chặn đối thủ phản công.",
                Icon = Resources.Handcuff
            };

            // Auto chọn item đầu tiên
            Item_Click(picBeer, EventArgs.Empty);
        }
    }
}
