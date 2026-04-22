namespace Logic_Revolver
{
    partial class FormMenu
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.lblTitle = new System.Windows.Forms.Label();
            this.lblStart = new System.Windows.Forms.Label();
            this.lblOptions = new System.Windows.Forms.Label();
            this.lblRules = new System.Windows.Forms.Label();
            this.lblExit = new System.Windows.Forms.Label();
            this.panelTitle = new System.Windows.Forms.Panel();
            this.panelMenu = new System.Windows.Forms.Panel();
            this.panelMainMenu = new System.Windows.Forms.Panel();
            this.panelTitle.SuspendLayout();
            this.panelMenu.SuspendLayout();
            this.panelMainMenu.SuspendLayout();
            this.SuspendLayout();
            // 
            // lblTitle
            // 
            this.lblTitle.AutoSize = true;
            this.lblTitle.BackColor = System.Drawing.Color.Transparent;
            this.lblTitle.Font = new System.Drawing.Font("Impact", 27.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lblTitle.ForeColor = System.Drawing.Color.Bisque;
            this.lblTitle.Location = new System.Drawing.Point(261, 31);
            this.lblTitle.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.lblTitle.Name = "lblTitle";
            this.lblTitle.Size = new System.Drawing.Size(304, 59);
            this.lblTitle.TabIndex = 0;
            this.lblTitle.Text = "Logic Revolver";
            this.lblTitle.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // lblStart
            // 
            this.lblStart.AutoSize = true;
            this.lblStart.BackColor = System.Drawing.Color.Transparent;
            this.lblStart.Font = new System.Drawing.Font("Arial", 19.8F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lblStart.ForeColor = System.Drawing.Color.Bisque;
            this.lblStart.Location = new System.Drawing.Point(57, 21);
            this.lblStart.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.lblStart.Name = "lblStart";
            this.lblStart.Size = new System.Drawing.Size(92, 38);
            this.lblStart.TabIndex = 5;
            this.lblStart.Text = "Start";
            this.lblStart.Click += new System.EventHandler(this.lblStart_Click);
            this.lblStart.MouseEnter += new System.EventHandler(this.Label_MouseEnter);
            this.lblStart.MouseLeave += new System.EventHandler(this.Label_MouseLeave);
            // 
            // lblOptions
            // 
            this.lblOptions.AutoSize = true;
            this.lblOptions.BackColor = System.Drawing.Color.Transparent;
            this.lblOptions.Font = new System.Drawing.Font("Arial", 19.8F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lblOptions.ForeColor = System.Drawing.Color.Bisque;
            this.lblOptions.Location = new System.Drawing.Point(44, 79);
            this.lblOptions.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.lblOptions.Name = "lblOptions";
            this.lblOptions.Size = new System.Drawing.Size(119, 38);
            this.lblOptions.TabIndex = 6;
            this.lblOptions.Text = "Sound";
            this.lblOptions.Click += new System.EventHandler(this.lblOptions_Click);
            this.lblOptions.MouseEnter += new System.EventHandler(this.Label_MouseEnter);
            this.lblOptions.MouseLeave += new System.EventHandler(this.Label_MouseLeave);
            // 
            // lblRules
            // 
            this.lblRules.AutoSize = true;
            this.lblRules.BackColor = System.Drawing.Color.Transparent;
            this.lblRules.Font = new System.Drawing.Font("Arial", 19.8F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lblRules.ForeColor = System.Drawing.Color.Bisque;
            this.lblRules.Location = new System.Drawing.Point(57, 135);
            this.lblRules.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.lblRules.Name = "lblRules";
            this.lblRules.Size = new System.Drawing.Size(106, 38);
            this.lblRules.TabIndex = 7;
            this.lblRules.Text = "Rules";
            this.lblRules.Click += new System.EventHandler(this.lblRules_Click);
            this.lblRules.MouseEnter += new System.EventHandler(this.Label_MouseEnter);
            this.lblRules.MouseLeave += new System.EventHandler(this.Label_MouseLeave);
            // 
            // lblExit
            // 
            this.lblExit.AutoSize = true;
            this.lblExit.BackColor = System.Drawing.Color.Transparent;
            this.lblExit.Font = new System.Drawing.Font("Arial", 19.8F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lblExit.ForeColor = System.Drawing.Color.Bisque;
            this.lblExit.Location = new System.Drawing.Point(57, 190);
            this.lblExit.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.lblExit.Name = "lblExit";
            this.lblExit.Size = new System.Drawing.Size(77, 38);
            this.lblExit.TabIndex = 8;
            this.lblExit.Text = "Exit";
            this.lblExit.Click += new System.EventHandler(this.lblExit_Click);
            this.lblExit.MouseEnter += new System.EventHandler(this.Label_MouseEnter);
            this.lblExit.MouseLeave += new System.EventHandler(this.Label_MouseLeave);
            // 
            // panelTitle
            // 
            this.panelTitle.AutoSize = true;
            this.panelTitle.BackColor = System.Drawing.Color.Transparent;
            this.panelTitle.Controls.Add(this.lblTitle);
            this.panelTitle.Dock = System.Windows.Forms.DockStyle.Top;
            this.panelTitle.Location = new System.Drawing.Point(0, 0);
            this.panelTitle.Margin = new System.Windows.Forms.Padding(3, 2, 3, 2);
            this.panelTitle.Name = "panelTitle";
            this.panelTitle.Size = new System.Drawing.Size(821, 90);
            this.panelTitle.TabIndex = 9;
            // 
            // panelMenu
            // 
            this.panelMenu.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.panelMenu.AutoSize = true;
            this.panelMenu.BackColor = System.Drawing.Color.Transparent;
            this.panelMenu.Controls.Add(this.lblOptions);
            this.panelMenu.Controls.Add(this.lblStart);
            this.panelMenu.Controls.Add(this.lblRules);
            this.panelMenu.Controls.Add(this.lblExit);
            this.panelMenu.Location = new System.Drawing.Point(3, 336);
            this.panelMenu.Margin = new System.Windows.Forms.Padding(3, 2, 3, 2);
            this.panelMenu.Name = "panelMenu";
            this.panelMenu.Size = new System.Drawing.Size(228, 238);
            this.panelMenu.TabIndex = 10;
            // 
            // panelMainMenu
            // 
            this.panelMainMenu.BackColor = System.Drawing.Color.Transparent;
            this.panelMainMenu.Controls.Add(this.panelTitle);
            this.panelMainMenu.Controls.Add(this.panelMenu);
            this.panelMainMenu.Dock = System.Windows.Forms.DockStyle.Fill;
            this.panelMainMenu.Location = new System.Drawing.Point(0, 0);
            this.panelMainMenu.Margin = new System.Windows.Forms.Padding(3, 2, 3, 2);
            this.panelMainMenu.Name = "panelMainMenu";
            this.panelMainMenu.Size = new System.Drawing.Size(821, 601);
            this.panelMainMenu.TabIndex = 11;
            // 
            // FormMenu
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(8F, 16F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackColor = System.Drawing.SystemColors.InactiveCaptionText;
            this.BackgroundImage = global::Logic_Revolver.Properties.Resources.Background;
            this.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Stretch;
            this.ClientSize = new System.Drawing.Size(821, 601);
            this.Controls.Add(this.panelMainMenu);
            this.DoubleBuffered = true;
            this.Margin = new System.Windows.Forms.Padding(4, 4, 4, 4);
            this.MaximizeBox = false;
            this.Name = "FormMenu";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "FormMenu";
            this.panelTitle.ResumeLayout(false);
            this.panelTitle.PerformLayout();
            this.panelMenu.ResumeLayout(false);
            this.panelMenu.PerformLayout();
            this.panelMainMenu.ResumeLayout(false);
            this.panelMainMenu.PerformLayout();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.Label lblTitle;
        private System.Windows.Forms.Label lblStart;
        private System.Windows.Forms.Label lblOptions;
        private System.Windows.Forms.Label lblRules;
        private System.Windows.Forms.Label lblExit;
        private System.Windows.Forms.Panel panelTitle;
        private System.Windows.Forms.Panel panelMenu;
        private System.Windows.Forms.Panel panelMainMenu;
    }
}