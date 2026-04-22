namespace Logic_Revolver
{
    partial class FormSound
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        private void InitializeComponent()
        {
            this.lblTitle = new System.Windows.Forms.Label();
            this.pnlMusic = new System.Windows.Forms.Panel();
            this.lblMusic = new System.Windows.Forms.Label();
            this.sldMusic = new Logic_Revolver.CowboySlider();
            this.tgMusic = new Logic_Revolver.CowboyToggle();
            this.lblMusicValue = new System.Windows.Forms.Label();
            this.pnlSfx = new System.Windows.Forms.Panel();
            this.lblSfx = new System.Windows.Forms.Label();
            this.sldSfx = new Logic_Revolver.CowboySlider();
            this.tgSfx = new Logic_Revolver.CowboyToggle();
            this.lblSfxValue = new System.Windows.Forms.Label();
            this.btnBack = new Logic_Revolver.CowboyButton();
            this.pnlMusic.SuspendLayout();
            this.pnlSfx.SuspendLayout();
            this.SuspendLayout();
            // 
            // lblTitle
            // 
            this.lblTitle.AutoSize = true;
            this.lblTitle.Location = new System.Drawing.Point(194, 28);
            this.lblTitle.Name = "lblTitle";
            this.lblTitle.Size = new System.Drawing.Size(77, 13);
            this.lblTitle.TabIndex = 0;
            this.lblTitle.Text = "SETTING";
            // 
            // pnlMusic
            // 
            this.pnlMusic.Controls.Add(this.lblMusic);
            this.pnlMusic.Controls.Add(this.sldMusic);
            this.pnlMusic.Controls.Add(this.tgMusic);
            this.pnlMusic.Controls.Add(this.lblMusicValue);
            this.pnlMusic.Location = new System.Drawing.Point(35, 82);
            this.pnlMusic.Name = "pnlMusic";
            this.pnlMusic.Size = new System.Drawing.Size(490, 90);
            this.pnlMusic.TabIndex = 1;
            // 
            // lblMusic
            // 
            this.lblMusic.AutoSize = true;
            this.lblMusic.Location = new System.Drawing.Point(24, 34);
            this.lblMusic.Name = "lblMusic";
            this.lblMusic.Size = new System.Drawing.Size(42, 13);
            this.lblMusic.TabIndex = 0;
            this.lblMusic.Text = "Music";
            // 
            // sldMusic
            // 
            this.sldMusic.Location = new System.Drawing.Point(109, 25);
            this.sldMusic.Name = "sldMusic";
            this.sldMusic.Size = new System.Drawing.Size(190, 40);
            this.sldMusic.TabIndex = 1;
            // 
            // tgMusic
            // 
            this.tgMusic.Location = new System.Drawing.Point(390, 24);
            this.tgMusic.Name = "tgMusic";
            this.tgMusic.Size = new System.Drawing.Size(74, 34);
            this.tgMusic.TabIndex = 2;
            // 
            // lblMusicValue
            // 
            this.lblMusicValue.AutoSize = true;
            this.lblMusicValue.Location = new System.Drawing.Point(318, 34);
            this.lblMusicValue.Name = "lblMusicValue";
            this.lblMusicValue.Size = new System.Drawing.Size(31, 13);
            this.lblMusicValue.TabIndex = 3;
            this.lblMusicValue.Text = "70%";
            // 
            // pnlSfx
            // 
            this.pnlSfx.Controls.Add(this.lblSfx);
            this.pnlSfx.Controls.Add(this.sldSfx);
            this.pnlSfx.Controls.Add(this.tgSfx);
            this.pnlSfx.Controls.Add(this.lblSfxValue);
            this.pnlSfx.Location = new System.Drawing.Point(35, 198);
            this.pnlSfx.Name = "pnlSfx";
            this.pnlSfx.Size = new System.Drawing.Size(490, 90);
            this.pnlSfx.TabIndex = 2;
            // 
            // lblSfx
            // 
            this.lblSfx.AutoSize = true;
            this.lblSfx.Location = new System.Drawing.Point(24, 34);
            this.lblSfx.Name = "lblSfx";
            this.lblSfx.Size = new System.Drawing.Size(30, 13);
            this.lblSfx.TabIndex = 0;
            this.lblSfx.Text = "SFX";
            // 
            // sldSfx
            // 
            this.sldSfx.Location = new System.Drawing.Point(109, 25);
            this.sldSfx.Name = "sldSfx";
            this.sldSfx.Size = new System.Drawing.Size(190, 40);
            this.sldSfx.TabIndex = 1;
            // 
            // tgSfx
            // 
            this.tgSfx.Location = new System.Drawing.Point(390, 24);
            this.tgSfx.Name = "tgSfx";
            this.tgSfx.Size = new System.Drawing.Size(74, 34);
            this.tgSfx.TabIndex = 2;
            // 
            // lblSfxValue
            // 
            this.lblSfxValue.AutoSize = true;
            this.lblSfxValue.Location = new System.Drawing.Point(318, 34);
            this.lblSfxValue.Name = "lblSfxValue";
            this.lblSfxValue.Size = new System.Drawing.Size(31, 13);
            this.lblSfxValue.TabIndex = 3;
            this.lblSfxValue.Text = "70%";
            // 
            // btnBack
            // 
            this.btnBack.Location = new System.Drawing.Point(215, 321);
            this.btnBack.Name = "btnBack";
            this.btnBack.Size = new System.Drawing.Size(130, 42);
            this.btnBack.TabIndex = 3;
            this.btnBack.Text = "BACK";
            // 
            // FormSetting
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(560, 390);
            this.Controls.Add(this.btnBack);
            this.Controls.Add(this.pnlSfx);
            this.Controls.Add(this.pnlMusic);
            this.Controls.Add(this.lblTitle);
            this.Name = "FormSound";
            this.Text = "FormSound";
            this.pnlMusic.ResumeLayout(false);
            this.pnlMusic.PerformLayout();
            this.pnlSfx.ResumeLayout(false);
            this.pnlSfx.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();
        }

        #endregion

        private System.Windows.Forms.Label lblTitle;
        private System.Windows.Forms.Panel pnlMusic;
        private System.Windows.Forms.Label lblMusic;
        private CowboySlider sldMusic;
        private CowboyToggle tgMusic;
        private System.Windows.Forms.Label lblMusicValue;
        private System.Windows.Forms.Panel pnlSfx;
        private System.Windows.Forms.Label lblSfx;
        private CowboySlider sldSfx;
        private CowboyToggle tgSfx;
        private System.Windows.Forms.Label lblSfxValue;
        private CowboyButton btnBack;
    }
}