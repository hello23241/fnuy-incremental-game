namespace WinFormsApp1
{
    partial class AscensionWindow
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
            labelBoosts = new Label();
            tabChallenges = new TabPage();
            labelChallenges = new Label();
            pictureBoxChallengeIcon0 = new PictureBox();
            pictureBoxChallengeIcon1 = new PictureBox();
            pictureBoxChallengeIcon2 = new PictureBox();
            pictureBoxChallengeIcon3 = new PictureBox();
            richTextBoxChallengeInfo = new RichTextBox();
            buttonChallengeAction = new Button();
            labelChallengeReward = new Label();
            panelTitleBar.SuspendLayout();
            tabControl.SuspendLayout();
            tabBoosts.SuspendLayout();
            tabChallenges.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)pictureBoxChallengeIcon0).BeginInit();
            ((System.ComponentModel.ISupportInitialize)pictureBoxChallengeIcon1).BeginInit();
            ((System.ComponentModel.ISupportInitialize)pictureBoxChallengeIcon2).BeginInit();
            ((System.ComponentModel.ISupportInitialize)pictureBoxChallengeIcon3).BeginInit();
            SuspendLayout();
            // 
            // panelTitleBar
            // 
            panelTitleBar.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
            panelTitleBar.BackColor = Color.FromArgb(24, 48, 96);
            panelTitleBar.Controls.Add(labelTitle);
            panelTitleBar.Controls.Add(buttonClose);
            panelTitleBar.Location = new Point(0, 0);
            panelTitleBar.Name = "panelTitleBar";
            panelTitleBar.Size = new Size(579, 28);
            panelTitleBar.TabIndex = 0;
            panelTitleBar.Resize += panelTitleBar_Resize;
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
            buttonClose.Anchor = AnchorStyles.Top | AnchorStyles.Right;
            buttonClose.BackColor = Color.FromArgb(32, 64, 128);
            buttonClose.FlatAppearance.BorderSize = 0;
            buttonClose.FlatStyle = FlatStyle.Flat;
            buttonClose.ForeColor = Color.Gainsboro;
            buttonClose.Location = new Point(539, 0);
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
            tabControl.Controls.Add(tabChallenges);
            tabControl.Location = new Point(0, 28);
            tabControl.Name = "tabControl";
            tabControl.SelectedIndex = 0;
            tabControl.Size = new Size(579, 377);
            tabControl.TabIndex = 1;
            // 
            // tabBoosts
            // 
            tabBoosts.BackColor = Color.FromArgb(40, 80, 160);
            tabBoosts.Controls.Add(labelBoosts);
            tabBoosts.Location = new Point(4, 24);
            tabBoosts.Name = "tabBoosts";
            tabBoosts.Size = new Size(571, 349);
            tabBoosts.TabIndex = 0;
            tabBoosts.Text = "Ascension Boosts";
            // 
            // labelBoosts
            // 
            labelBoosts.Font = new Font("Segoe UI", 11F);
            labelBoosts.ForeColor = Color.White;
            labelBoosts.Location = new Point(20, 20);
            labelBoosts.Name = "labelBoosts";
            labelBoosts.Size = new Size(543, 293);
            labelBoosts.TabIndex = 0;
            labelBoosts.Text = "Ascension Count Boosts:\nYou have X ascensions.\n\n1. [Active] Point multiplier: x1.10 (1.1x per ascension)\n2. [Locked] Button cooldown reduction & hold: Unlocks at 2 ascensions";
            // 
            // tabChallenges
            // 
            tabChallenges.BackColor = Color.FromArgb(40, 80, 160);
            tabChallenges.Controls.Add(labelChallenges);
            tabChallenges.Controls.Add(pictureBoxChallengeIcon0);
            tabChallenges.Controls.Add(pictureBoxChallengeIcon1);
            tabChallenges.Controls.Add(pictureBoxChallengeIcon2);
            tabChallenges.Controls.Add(pictureBoxChallengeIcon3);
            tabChallenges.Controls.Add(richTextBoxChallengeInfo);
            tabChallenges.Controls.Add(buttonChallengeAction);
            tabChallenges.Controls.Add(labelChallengeReward);
            tabChallenges.Location = new Point(4, 24);
            tabChallenges.Name = "tabChallenges";
            tabChallenges.Size = new Size(571, 349);
            tabChallenges.TabIndex = 1;
            tabChallenges.Text = "Challenges";
            // 
            // labelChallenges
            // 
            labelChallenges.Font = new Font("Segoe UI", 11F);
            labelChallenges.ForeColor = Color.White;
            labelChallenges.Location = new Point(20, 20);
            labelChallenges.Name = "labelChallenges";
            labelChallenges.Size = new Size(500, 24);
            labelChallenges.TabIndex = 0;
            // 
            // pictureBoxChallengeIcon0
            // 
            pictureBoxChallengeIcon0.Location = new Point(40, 60);
            pictureBoxChallengeIcon0.Name = "pictureBoxChallengeIcon0";
            pictureBoxChallengeIcon0.Size = new Size(64, 64);
            pictureBoxChallengeIcon0.SizeMode = PictureBoxSizeMode.Zoom;
            pictureBoxChallengeIcon0.Image = global::WinFormsApp1.Properties.Resources.icon1;
            pictureBoxChallengeIcon0.TabIndex = 1;
            pictureBoxChallengeIcon0.TabStop = false;
            pictureBoxChallengeIcon0.Click += pictureBoxChallengeIcon0_Click;
            // 
            // pictureBoxChallengeIcon1
            // 
            pictureBoxChallengeIcon1.Location = new Point(120, 60);
            pictureBoxChallengeIcon1.Name = "pictureBoxChallengeIcon1";
            pictureBoxChallengeIcon1.Size = new Size(64, 64);
            pictureBoxChallengeIcon1.SizeMode = PictureBoxSizeMode.Zoom;
            pictureBoxChallengeIcon1.Image = global::WinFormsApp1.Properties.Resources.icon2;
            pictureBoxChallengeIcon1.TabIndex = 2;
            pictureBoxChallengeIcon1.TabStop = false;
            pictureBoxChallengeIcon1.Click += pictureBoxChallengeIcon1_Click;
            // 
            // pictureBoxChallengeIcon2
            // 
            pictureBoxChallengeIcon2.Location = new Point(200, 60);
            pictureBoxChallengeIcon2.Name = "pictureBoxChallengeIcon2";
            pictureBoxChallengeIcon2.Size = new Size(64, 64);
            pictureBoxChallengeIcon2.SizeMode = PictureBoxSizeMode.Zoom;
            pictureBoxChallengeIcon2.Image = global::WinFormsApp1.Properties.Resources.icon3;
            pictureBoxChallengeIcon2.TabIndex = 3;
            pictureBoxChallengeIcon2.TabStop = false;
            pictureBoxChallengeIcon2.Click += pictureBoxChallengeIcon2_Click;
            // 
            // pictureBoxChallengeIcon3
            // 
            pictureBoxChallengeIcon3.Location = new Point(280, 60);
            pictureBoxChallengeIcon3.Name = "pictureBoxChallengeIcon3";
            pictureBoxChallengeIcon3.Size = new Size(64, 64);
            pictureBoxChallengeIcon3.SizeMode = PictureBoxSizeMode.Zoom;
            pictureBoxChallengeIcon3.Image = global::WinFormsApp1.Properties.Resources.icon4;
            pictureBoxChallengeIcon3.TabIndex = 4;
            pictureBoxChallengeIcon3.TabStop = false;
            pictureBoxChallengeIcon3.Click += pictureBoxChallengeIcon3_Click;
            // 
            // richTextBoxChallengeInfo
            // 
            richTextBoxChallengeInfo.BackColor = Color.FromArgb(40, 80, 160);
            richTextBoxChallengeInfo.BorderStyle = BorderStyle.None;
            richTextBoxChallengeInfo.Font = new Font("Segoe UI", 11F);
            richTextBoxChallengeInfo.ForeColor = Color.White;
            richTextBoxChallengeInfo.Location = new Point(40, 180);
            richTextBoxChallengeInfo.Name = "richTextBoxChallengeInfo";
            richTextBoxChallengeInfo.ReadOnly = true;
            richTextBoxChallengeInfo.ScrollBars = RichTextBoxScrollBars.None;
            richTextBoxChallengeInfo.Size = new Size(350, 100);
            richTextBoxChallengeInfo.TabIndex = 5;
            richTextBoxChallengeInfo.Text = "";
            richTextBoxChallengeInfo.Visible = false;
            // 
            // buttonChallengeAction
            // 
            buttonChallengeAction.Location = new Point(400, 285);
            buttonChallengeAction.Name = "buttonChallengeAction";
            buttonChallengeAction.Size = new Size(100, 32);
            buttonChallengeAction.TabIndex = 6;
            buttonChallengeAction.Text = "Start";
            buttonChallengeAction.Visible = false;
            buttonChallengeAction.Click += buttonChallengeAction_Click;
            // 
            // labelChallengeReward
            // 
            labelChallengeReward.BackColor = Color.Transparent;
            labelChallengeReward.Font = new Font("Segoe UI", 10F, FontStyle.Bold);
            labelChallengeReward.ForeColor = Color.Gold;
            labelChallengeReward.Location = new Point(400, 180);
            labelChallengeReward.Name = "labelChallengeReward";
            labelChallengeReward.Size = new Size(160, 56);
            labelChallengeReward.TabIndex = 7;
            labelChallengeReward.TextAlign = ContentAlignment.MiddleLeft;
            labelChallengeReward.Visible = false;
            // 
            // AscensionWindow
            // 
            AutoScaleDimensions = new SizeF(7F, 15F);
            AutoScaleMode = AutoScaleMode.Font;
            BackColor = Color.FromArgb(30, 60, 120);
            ClientSize = new Size(579, 405);
            Controls.Add(panelTitleBar);
            Controls.Add(tabControl);
            FormBorderStyle = FormBorderStyle.None;
            Name = "AscensionWindow";
            Text = "Ascension Window";
            Resize += AscensionWindow_Resize;
            panelTitleBar.ResumeLayout(false);
            panelTitleBar.PerformLayout();
            tabControl.ResumeLayout(false);
            tabBoosts.ResumeLayout(false);
            tabChallenges.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)pictureBoxChallengeIcon0).EndInit();
            ((System.ComponentModel.ISupportInitialize)pictureBoxChallengeIcon1).EndInit();
            ((System.ComponentModel.ISupportInitialize)pictureBoxChallengeIcon2).EndInit();
            ((System.ComponentModel.ISupportInitialize)pictureBoxChallengeIcon3).EndInit();
            ResumeLayout(false);
        }
    }
}