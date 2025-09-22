namespace WinFormsApp1
{
    partial class AscensionShop
    {
        private System.Windows.Forms.Panel panelTitleBar;
        private System.Windows.Forms.Label labelTitle;
        private System.Windows.Forms.Button buttonClose;
        private System.Windows.Forms.TabControl tabControl;
        private System.Windows.Forms.TabPage tabBoosts;
        private System.Windows.Forms.TabPage tabChallenges;
        private System.Windows.Forms.Label labelBoosts;
        private System.Windows.Forms.Label labelChallenges;
        private System.Windows.Forms.PictureBox pictureBoxChallengeIcon0;
        private System.Windows.Forms.PictureBox pictureBoxChallengeIcon1;
        private System.Windows.Forms.PictureBox pictureBoxChallengeIcon2;
        private System.Windows.Forms.PictureBox pictureBoxChallengeIcon3;
        private System.Windows.Forms.RichTextBox richTextBoxChallengeInfo;
        private System.Windows.Forms.Button buttonChallengeAction;
        private System.Windows.Forms.Label labelChallengeReward;
        private void InitializeComponent()
        {
            panelTitleBar = new Panel();
            labelTitle = new Label();
            buttonClose = new Button();
            tabControl = new TabControl();
            tabBoosts = new TabPage();
            tabChallenges = new TabPage();
            labelBoosts = new Label();
            labelChallenges = new Label();
            pictureBoxChallengeIcon0 = new PictureBox();
            pictureBoxChallengeIcon1 = new PictureBox();
            pictureBoxChallengeIcon2 = new PictureBox();
            pictureBoxChallengeIcon3 = new PictureBox();
            richTextBoxChallengeInfo = new RichTextBox();
            buttonChallengeAction = new Button();

            panelTitleBar.SuspendLayout();
            panelTitleBar.Size = new Size(579, 28);
            tabControl.SuspendLayout();
            tabBoosts.SuspendLayout();
            tabChallenges.SuspendLayout();
            SuspendLayout();
            this.Resize += (s, e) =>
            {
                panelTitleBar.Width = this.ClientSize.Width;
                buttonClose.Location = new Point(panelTitleBar.Width - buttonClose.Width, 0);
            };
            // 
            // pictureBoxChallengeIcon0
            // 
            pictureBoxChallengeIcon0.Location = new Point(40, 60);
            pictureBoxChallengeIcon0.Size = new Size(64, 64);
            pictureBoxChallengeIcon0.SizeMode = PictureBoxSizeMode.Zoom;
            pictureBoxChallengeIcon0.Image = Properties.Resources.icon1; // Add an icon to your resources named "ChallengeIcon"
            pictureBoxChallengeIcon0.Cursor = Cursors.Hand;
            pictureBoxChallengeIcon0.Click += (s, e) => PictureBoxChallengeIcon_Click(0);
            // 
            // pictureBoxChallengeIcon1
            // 
            pictureBoxChallengeIcon1.Location = new Point(120, 60);
            pictureBoxChallengeIcon1.Size = new Size(64, 64);
            pictureBoxChallengeIcon1.SizeMode = PictureBoxSizeMode.Zoom;
            pictureBoxChallengeIcon1.Image = Properties.Resources.icon2;
            pictureBoxChallengeIcon1.Cursor = Cursors.Hand;
            pictureBoxChallengeIcon1.Click += (s, e) => PictureBoxChallengeIcon_Click(1);
            // 
            // pictureBoxChallengeIcon2
            // 
            pictureBoxChallengeIcon2.Location = new Point(200, 60);
            pictureBoxChallengeIcon2.Size = new Size(64, 64);
            pictureBoxChallengeIcon2.SizeMode = PictureBoxSizeMode.Zoom;
            pictureBoxChallengeIcon2.Image = Properties.Resources.icon3;
            pictureBoxChallengeIcon2.Cursor = Cursors.Hand;
            pictureBoxChallengeIcon2.Click += (s, e) => PictureBoxChallengeIcon_Click(2);
            // 
            // pictureBoxChallengeIcon3
            // 
            pictureBoxChallengeIcon3.Location = new Point(280, 60);
            pictureBoxChallengeIcon3.Size = new Size(64, 64);
            pictureBoxChallengeIcon3.SizeMode = PictureBoxSizeMode.Zoom;
            pictureBoxChallengeIcon3.Image = Properties.Resources.icon4;
            pictureBoxChallengeIcon3.Cursor = Cursors.Hand;
            pictureBoxChallengeIcon3.Click += (s, e) => PictureBoxChallengeIcon_Click(3);
            // 
            // richTextBoxChallengeInfo
            // 
            richTextBoxChallengeInfo = new RichTextBox();
            richTextBoxChallengeInfo.Location = new Point(40, 180);
            richTextBoxChallengeInfo.Size = new Size(350, 100);
            richTextBoxChallengeInfo.Font = new Font("Segoe UI", 11F);
            richTextBoxChallengeInfo.ForeColor = Color.White;
            richTextBoxChallengeInfo.BackColor = Color.FromArgb(40, 80, 160);
            richTextBoxChallengeInfo.BorderStyle = BorderStyle.None;
            richTextBoxChallengeInfo.ReadOnly = true;
            richTextBoxChallengeInfo.ScrollBars = RichTextBoxScrollBars.None;
            richTextBoxChallengeInfo.Visible = false;
            tabChallenges.Controls.Add(richTextBoxChallengeInfo);
            // 
            // buttonChallengeAction
            // 
            buttonChallengeAction.Location = new Point(400, 285);
            buttonChallengeAction.Size = new Size(100, 32);
            buttonChallengeAction.Text = "Start";
            buttonChallengeAction.Visible = false;
            buttonChallengeAction.Click += buttonChallengeAction_Click;
            //
            // labelChallengeReward
            //
            labelChallengeReward = new Label();
            labelChallengeReward.Location = new Point(400, 180);
            labelChallengeReward.Size = new Size(160, 56);
            labelChallengeReward.Font = new Font("Segoe UI", 10F, FontStyle.Bold);
            labelChallengeReward.ForeColor = Color.Gold;
            labelChallengeReward.BackColor = Color.Transparent;
            labelChallengeReward.TextAlign = ContentAlignment.MiddleLeft;
            labelChallengeReward.Visible = false;
            tabChallenges.Controls.Add(labelChallengeReward);
            // Add controls to tabChallenges
            tabChallenges.Controls.Add(pictureBoxChallengeIcon0);
            tabChallenges.Controls.Add(pictureBoxChallengeIcon1);
            tabChallenges.Controls.Add(pictureBoxChallengeIcon2);
            tabChallenges.Controls.Add(pictureBoxChallengeIcon3);
            tabChallenges.Controls.Add(richTextBoxChallengeInfo);
            tabChallenges.Controls.Add(buttonChallengeAction);
            // 
            // panelTitleBar
            // 
            panelTitleBar.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
            panelTitleBar.BackColor = Color.FromArgb(24, 48, 96);
            panelTitleBar.Controls.Add(labelTitle);
            panelTitleBar.Controls.Add(buttonClose);
            panelTitleBar.Location = new Point(0, 0);
            panelTitleBar.Name = "panelTitleBar";
            panelTitleBar.Size = new Size(463, 28);
            panelTitleBar.TabIndex = 0;
            buttonClose.Anchor = AnchorStyles.Top | AnchorStyles.Right;
            buttonClose.Location = new Point(panelTitleBar.Width - buttonClose.Width, 0);
            panelTitleBar.Resize += (s, e) =>
            {
                buttonClose.Location = new Point(panelTitleBar.Width - buttonClose.Width, 0);
            };
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
            // buttonClose
            //
            buttonClose.BackColor = Color.FromArgb(32, 64, 128);
            buttonClose.FlatAppearance.BorderSize = 0;
            buttonClose.FlatStyle = FlatStyle.Flat;
            buttonClose.ForeColor = Color.Gainsboro;
            buttonClose.Name = "buttonClose";
            buttonClose.Size = new Size(40, 28);
            buttonClose.TabIndex = 2;
            buttonClose.Text = "X";
            buttonClose.UseVisualStyleBackColor = false;
            buttonClose.Anchor = AnchorStyles.Top | AnchorStyles.Right;
            buttonClose.Location = new Point(panelTitleBar.Width - buttonClose.Width, 0);
            // 
            // tabControl
            // 
            tabControl.Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;
            tabControl.Controls.Add(tabBoosts);
            tabControl.Controls.Add(tabChallenges);
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
            // tabChallenges
            // 
            tabChallenges.BackColor = Color.FromArgb(40, 80, 160);
            tabChallenges.Controls.Add(labelChallenges);
            tabChallenges.Location = new Point(4, 24);
            tabChallenges.Name = "tabChallenges";
            tabChallenges.Size = new Size(559, 337);
            tabChallenges.TabIndex = 1;
            tabChallenges.Text = "Challenges";
            // 
            // labelChallenges
            // 
            labelChallenges.Font = new Font("Segoe UI", 11F);
            labelChallenges.ForeColor = Color.White;
            labelChallenges.Location = new Point(20, 20);
            labelChallenges.Name = "labelChallenges";
            labelChallenges.Size = new Size(500, 180);
            labelChallenges.TabIndex = 0;
            labelChallenges.Text = "";
            // 
            // AscensionShop
            // 
            BackColor = Color.FromArgb(30, 60, 120);
            ClientSize = new Size(579, 405);
            Controls.Add(panelTitleBar);
            Controls.Add(tabControl);
            FormBorderStyle = FormBorderStyle.None;
            Name = "AscensionShop";
            Text = "Ascension Window";
            panelTitleBar.ResumeLayout(false);
            panelTitleBar.PerformLayout();
            tabControl.ResumeLayout(false);
            tabBoosts.ResumeLayout(false);
            tabChallenges.ResumeLayout(false);
            ResumeLayout(false);
        }
    }
}