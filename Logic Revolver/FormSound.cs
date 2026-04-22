using System;
using System.Drawing;
using System.Windows.Forms;
using Logic_Revolver.Properties;
using Logic_Revolver.Engine;

namespace Logic_Revolver
{
    public partial class FormSound : Form
    {
        public bool MusicEnabled { get; private set; } = true;
        public bool SfxEnabled { get; private set; } = true;
        public int MusicVolume { get; private set; } = 70;
        public int SfxVolume { get; private set; } = 70;

        public FormSound()
        {
            InitializeComponent();
            ApplyStyle();
            LoadCurrentValues();
            WireEvents();
        }

        private void ApplyStyle()
        {
            this.BackgroundImage = Resources.Brown;
            this.BackgroundImageLayout = ImageLayout.Stretch;
            this.StartPosition = FormStartPosition.CenterParent;
            this.FormBorderStyle = FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.DoubleBuffered = true;
            this.Text = "Sound";
            this.ClientSize = new Size(560, 390);

            lblTitle.Text = "SOUND";
            lblTitle.BackColor = Color.Transparent;
            lblTitle.ForeColor = Color.Bisque;
            lblTitle.Font = new Font("Georgia", 22, FontStyle.Bold);

            lblMusic.BackColor = Color.Transparent;
            lblMusic.ForeColor = Color.Bisque;
            lblMusic.Font = new Font("Georgia", 14, FontStyle.Bold);

            lblSfx.BackColor = Color.Transparent;
            lblSfx.ForeColor = Color.Bisque;
            lblSfx.Font = new Font("Georgia", 14, FontStyle.Bold);

            lblMusicValue.BackColor = Color.Transparent;
            lblMusicValue.ForeColor = Color.WhiteSmoke;
            lblMusicValue.Font = new Font("Georgia", 11, FontStyle.Bold);

            lblSfxValue.BackColor = Color.Transparent;
            lblSfxValue.ForeColor = Color.WhiteSmoke;
            lblSfxValue.Font = new Font("Georgia", 11, FontStyle.Bold);

            pnlMusic.BackColor = Color.FromArgb(120, 35, 18, 8);
            pnlSfx.BackColor = Color.FromArgb(120, 35, 18, 8);

            btnBack.SetColors(Color.FromArgb(70, 40, 20));
            btnBack.Text = "BACK";

        }

        private void LoadCurrentValues()
        {
            MusicEnabled = AudioManager.MusicEnabled;
            SfxEnabled = AudioManager.SfxEnabled;
            MusicVolume = AudioManager.MusicVolume;
            SfxVolume = AudioManager.SfxVolume;

            tgMusic.Checked = MusicEnabled;
            tgSfx.Checked = SfxEnabled;

            sldMusic.Minimum = 0;
            sldMusic.Maximum = 100;
            sldMusic.Value = MusicVolume;

            sldSfx.Minimum = 0;
            sldSfx.Maximum = 100;
            sldSfx.Value = SfxVolume;

            lblMusicValue.Text = $"{MusicVolume}%";
            lblSfxValue.Text = $"{SfxVolume}%";
        }

        private void WireEvents()
        {
            sldMusic.ValueChanged += (s, e) =>
            {
                lblMusicValue.Text = $"{sldMusic.Value}%";
                MusicVolume = sldMusic.Value;

                // Realtime: kéo tới đâu chỉnh nhạc tới đó
                AudioManager.SetMusicVolume(MusicVolume);
            };

            sldSfx.ValueChanged += (s, e) =>
            {
                lblSfxValue.Text = $"{sldSfx.Value}%";
                SfxVolume = sldSfx.Value;

                // Realtime: kéo tới đâu chỉnh hiệu ứng tới đó
                AudioManager.SetSfxVolume(SfxVolume);
            };

            tgMusic.CheckedChanged += (s, e) =>
            {
                MusicEnabled = tgMusic.Checked;

                // Realtime: bật tắt nhạc ngay
                AudioManager.SetMusicEnabled(MusicEnabled);
            };

            tgSfx.CheckedChanged += (s, e) =>
            {
                SfxEnabled = tgSfx.Checked;

                // Realtime: bật tắt hiệu ứng ngay
                AudioManager.SetSfxEnabled(SfxEnabled);
            };

            btnBack.Click += btnBack_Click;
           
        }

        private void btnBack_Click(object sender, EventArgs e)
        {
            MusicEnabled = tgMusic.Checked;
            SfxEnabled = tgSfx.Checked;
            MusicVolume = sldMusic.Value;
            SfxVolume = sldSfx.Value;

            this.Close();
        }

        
    }
}