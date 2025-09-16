namespace WinFormsApp1
{
    partial class AscensionShop
    {
        private System.Windows.Forms.Panel panelTitleBar;
        private System.Windows.Forms.Label labelTitle;
        private System.Windows.Forms.Button buttonMinimize;
        private System.Windows.Forms.Button buttonClose;
        private System.Windows.Forms.TabControl tabControl;
        private System.Windows.Forms.TabPage tabBoosts;
        private System.Windows.Forms.TabPage tabSpend;
        private System.Windows.Forms.Label labelBoosts;
        private System.Windows.Forms.Label labelPoints;
        private System.Windows.Forms.Panel upgradePanel1;
        private System.Windows.Forms.Panel upgradePanel2;
        private System.Windows.Forms.Panel upgradePanel3;
        private System.Windows.Forms.Panel upgradePanel4;
        private System.Windows.Forms.PictureBox icon1;
        private System.Windows.Forms.PictureBox icon2;
        private System.Windows.Forms.PictureBox icon3;
        private System.Windows.Forms.PictureBox icon4;
        private System.Windows.Forms.Panel detailsPanel;
        private System.Windows.Forms.Label labelEffect;
        private System.Windows.Forms.Label labelCost;
        private System.Windows.Forms.Button buttonBuy;

        private void InitializeComponent()
        {
            panelTitleBar = new Panel();
            labelTitle = new Label();
            buttonMinimize = new Button();
            buttonClose = new Button();
            tabControl = new TabControl();
            tabBoosts = new TabPage();
            labelBoosts = new Label();
            tabSpend = new TabPage();
            labelPoints = new Label();
            upgradePanel1 = new Panel { Size = new Size(48, 48), Location = new Point(30, 70), BackColor = Color.Gray, Cursor = Cursors.Hand };
            upgradePanel2 = new Panel { Size = new Size(48, 48), Location = new Point(90, 70), BackColor = Color.Gray, Cursor = Cursors.Hand };
            upgradePanel3 = new Panel { Size = new Size(48, 48), Location = new Point(150, 70), BackColor = Color.Gray, Cursor = Cursors.Hand };
            upgradePanel4 = new Panel { Size = new Size(48, 48), Location = new Point(210, 70), BackColor = Color.Gray, Cursor = Cursors.Hand };
            icon1 = new PictureBox { Dock = DockStyle.Fill, SizeMode = PictureBoxSizeMode.Zoom };
            icon2 = new PictureBox { Dock = DockStyle.Fill, SizeMode = PictureBoxSizeMode.Zoom };
            icon3 = new PictureBox { Dock = DockStyle.Fill, SizeMode = PictureBoxSizeMode.Zoom };
            icon4 = new PictureBox { Dock = DockStyle.Fill, SizeMode = PictureBoxSizeMode.Zoom };
            detailsPanel = new Panel { Size = new Size(300, 110), Location = new Point(30, 130), BackColor = Color.FromArgb(50, 50, 80), Visible = false };
            labelEffect = new Label { Location = new Point(10, 10), Size = new Size(280, 40), ForeColor = Color.White, Font = new Font("Segoe UI", 10F) };
            labelCost = new Label { Location = new Point(10, 60), Size = new Size(180, 20), ForeColor = Color.White, Font = new Font("Segoe UI", 10F) };
            buttonBuy = new Button { Location = new Point(200, 60), Size = new Size(80, 30), Text = "Buy" };
            panelTitleBar.SuspendLayout();
            tabControl.SuspendLayout();
            tabBoosts.SuspendLayout();
            tabSpend.SuspendLayout();
            SuspendLayout();
            // 
            // panelTitleBar
            // 
            panelTitleBar.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
            panelTitleBar.BackColor = Color.FromArgb(24, 48, 96);
            panelTitleBar.Controls.Add(labelTitle);
            panelTitleBar.Controls.Add(buttonMinimize);
            panelTitleBar.Controls.Add(buttonClose);
            panelTitleBar.Location = new Point(0, 0);
            panelTitleBar.Name = "panelTitleBar";
            panelTitleBar.Size = new Size(463, 28);
            panelTitleBar.TabIndex = 0;
            // 
            // labelTitle
            // 
            labelTitle.AutoSize = true;
            labelTitle.Font = new Font("Segoe UI", 12F, FontStyle.Bold);
            labelTitle.ForeColor = Color.Gainsboro;
            labelTitle.Location = new Point(10, 4);
            labelTitle.Name = "labelTitle";
            labelTitle.Size = new Size(155, 21);
            labelTitle.TabIndex = 0;
            labelTitle.Text = "Ascension Window";
            // 
            // buttonMinimize
            // 
            buttonMinimize.BackColor = Color.FromArgb(32, 64, 128);
            buttonMinimize.FlatAppearance.BorderSize = 0;
            buttonMinimize.FlatStyle = FlatStyle.Flat;
            buttonMinimize.ForeColor = Color.Gainsboro;
            buttonMinimize.Location = new Point(284, 0);
            buttonMinimize.Name = "buttonMinimize";
            buttonMinimize.Size = new Size(40, 28);
            buttonMinimize.TabIndex = 1;
            buttonMinimize.Text = "—";
            buttonMinimize.UseVisualStyleBackColor = false;
            // 
            // buttonClose
            // 
            buttonClose.BackColor = Color.FromArgb(32, 64, 128);
            buttonClose.FlatAppearance.BorderSize = 0;
            buttonClose.FlatStyle = FlatStyle.Flat;
            buttonClose.ForeColor = Color.Gainsboro;
            buttonClose.Location = new Point(284, 0);
            buttonClose.Name = "buttonClose";
            buttonClose.Size = new Size(40, 28);
            buttonClose.TabIndex = 2;
            buttonClose.Text = "X";
            buttonClose.UseVisualStyleBackColor = false;
            // 
            // tabControl
            // 
            tabControl.Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;
            tabControl.Controls.Add(tabBoosts);
            tabControl.Controls.Add(tabSpend);
            tabControl.Location = new Point(0, 28);
            tabControl.Name = "tabControl";
            tabControl.SelectedIndex = 0;
            tabControl.Size = new Size(567, 365);
            tabControl.TabIndex = 1;
            // 
            // tabBoosts
            // 
            tabBoosts.BackColor = Color.FromArgb(40, 80, 160);
            tabBoosts.Controls.Add(labelBoosts);
            tabBoosts.Location = new Point(4, 24);
            tabBoosts.Name = "tabBoosts";
            tabBoosts.Size = new Size(559, 337);
            tabBoosts.TabIndex = 0;
            tabBoosts.Text = "Ascension Boosts";
            // 
            // labelBoosts
            // 
            labelBoosts.Font = new Font("Segoe UI", 11F);
            labelBoosts.ForeColor = Color.White;
            labelBoosts.Location = new Point(20, 20);
            labelBoosts.Name = "labelBoosts";
            labelBoosts.Size = new Size(236, 180);
            labelBoosts.TabIndex = 0;
            labelBoosts.Text = "Ascension Count Boosts:\nYou have X ascensions.\n\n1. [Active] Point multiplier: x1.10 (1.1x per ascension)\n2. [Locked] Button cooldown reduction & hold: Unlocks at 2 ascensions";
            // 
            // tabSpend
            // 
            tabSpend.BackColor = Color.FromArgb(40, 80, 160);
            tabSpend.Controls.Add(labelPoints);
            tabSpend.Controls.Add(upgradePanel1);
            tabSpend.Controls.Add(upgradePanel2);
            tabSpend.Controls.Add(upgradePanel3);
            tabSpend.Controls.Add(upgradePanel4);
            tabSpend.Controls.Add(detailsPanel);
            tabSpend.Location = new Point(4, 24);
            tabSpend.Name = "tabSpend";
            tabSpend.Size = new Size(276, 233);
            tabSpend.TabIndex = 1;
            tabSpend.Text = "Spend Points";
            // 
            // labelPoints
            // 
            labelPoints.Font = new Font("Segoe UI", 11F);
            labelPoints.ForeColor = Color.White;
            labelPoints.Location = new Point(20, 20);
            labelPoints.Name = "labelPoints";
            labelPoints.Size = new Size(236, 30);
            labelPoints.TabIndex = 0;
            labelPoints.Text = "Ascension Points: X";
            // 
            // icon1
            // 
            icon1.Location = new Point(30, 70);
            icon1.Name = "icon1";
            icon1.Size = new Size(64, 64);
            icon1.SizeMode = PictureBoxSizeMode.StretchImage;
            icon1.TabIndex = 6;
            icon1.TabStop = false;
            // 
            // icon2
            // 
            icon2.Location = new Point(150, 70);
            icon2.Name = "icon2";
            icon2.Size = new Size(64, 64);
            icon2.SizeMode = PictureBoxSizeMode.StretchImage;
            icon2.TabIndex = 7;
            icon2.TabStop = false;
            // 
            // icon3
            // 
            icon3.Location = new Point(270, 70);
            icon3.Name = "icon3";
            icon3.Size = new Size(64, 64);
            icon3.SizeMode = PictureBoxSizeMode.StretchImage;
            icon3.TabIndex = 8;
            icon3.TabStop = false;
            // 
            // icon4
            // 
            icon4.Location = new Point(390, 70);
            icon4.Name = "icon4";
            icon4.Size = new Size(64, 64);
            icon4.SizeMode = PictureBoxSizeMode.StretchImage;
            icon4.TabIndex = 9;
            icon4.TabStop = false;
            // 
            // detailsPanel
            // 
            detailsPanel.BackColor = Color.FromArgb(60, 120, 240);
            detailsPanel.Location = new Point(20, 180);
            detailsPanel.Name = "detailsPanel";
            detailsPanel.Size = new Size(460, 150);
            detailsPanel.TabIndex = 10;
            // 
            // labelEffect
            // 
            labelEffect.AutoSize = true;
            labelEffect.Font = new Font("Segoe UI", 10F);
            labelEffect.ForeColor = Color.White;
            labelEffect.Location = new Point(10, 10);
            labelEffect.Name = "labelEffect";
            labelEffect.Size = new Size(83, 19);
            labelEffect.TabIndex = 11;
            labelEffect.Text = "Effect: None";
            // 
            // labelCost
            // 
            labelCost.AutoSize = true;
            labelCost.Font = new Font("Segoe UI", 10F);
            labelCost.ForeColor = Color.White;
            labelCost.Location = new Point(10, 60);
            labelCost.Name = "labelCost";
            labelCost.Size = new Size(44, 19);
            labelCost.TabIndex = 12;
            labelCost.Text = "Cost: 0";
            // 
            // buttonBuy
            // 
            buttonBuy.BackColor = Color.FromArgb(255, 165, 0);
            buttonBuy.FlatAppearance.BorderSize = 0;
            buttonBuy.FlatStyle = FlatStyle.Flat;
            buttonBuy.Font = new Font("Segoe UI", 10F, FontStyle.Bold);
            buttonBuy.ForeColor = Color.Black;
            buttonBuy.Location = new Point(200, 60);
            buttonBuy.Name = "buttonBuy";
            buttonBuy.Size = new Size(80, 30);
            buttonBuy.TabIndex = 13;
            buttonBuy.Text = "Buy";
            buttonBuy.UseVisualStyleBackColor = false;
            // 
            // AscensionShop
            // 
            BackColor = Color.FromArgb(30, 60, 120);
            ClientSize = new Size(579, 405);
            Controls.Add(panelTitleBar);
            Controls.Add(tabControl);
            // After initializing detailsPanel, labelEffect, labelCost, buttonBuy:
            detailsPanel.Controls.Add(labelEffect);
            detailsPanel.Controls.Add(labelCost);
            detailsPanel.Controls.Add(buttonBuy);
            FormBorderStyle = FormBorderStyle.None;
            Name = "AscensionShop";
            Text = "Ascension Window";
            panelTitleBar.ResumeLayout(false);
            panelTitleBar.PerformLayout();
            tabControl.ResumeLayout(false);
            tabBoosts.ResumeLayout(false);
            tabSpend.ResumeLayout(false);
            ResumeLayout(false);
        }
    }
}