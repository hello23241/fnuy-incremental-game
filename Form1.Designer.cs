namespace WinFormsApp1
{
    partial class MainForm
    {
        /// <summary>
        ///  Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        ///  Clean up any resources being used.
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
        ///  Required method for Designer support - do not modify
        ///  the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            button1 = new Button();
            labelPoint = new Label();
            buttonUpgrade = new Button();
            labelUpgradeCost = new Label();
            buttonPrestige = new Button();
            labelPrestigeCost = new Label();
            labelUpgradeInfo = new Label();
            buttonAscend = new Button();
            labelAscendCost = new Label();
            buttonOpenAscensionShop = new Button();
            labelPrestigeInfo = new Label();
            buttonGenerator = new Button();
            labelUpgradeNote = new Label();
            labelGeneratorInfo = new Label();
            labelSoftCap = new Label();
            labelPointGain = new Label();
            labelPointsPerSecond = new Label();
            panelTitleBar = new Panel();
            buttonTranscend = new Button();
            labelTranscendCost = new Label();
            labelCooldown = new Label();
            SuspendLayout();
            // 
            // button1
            // 
            button1.Location = new Point(19, 107);
            button1.Name = "button1";
            button1.Size = new Size(139, 45);
            button1.TabIndex = 1;
            button1.Text = "Click Me";
            button1.UseVisualStyleBackColor = true;
            button1.Click += Button1_Click;
            // 
            // labelPoint
            // 
            labelPoint.Font = new Font("Segoe UI", 20.25F, FontStyle.Regular, GraphicsUnit.Point, 0);
            labelPoint.ForeColor = Color.White;
            labelPoint.Location = new Point(409, 42);
            labelPoint.Name = "labelPoint";
            labelPoint.RightToLeft = RightToLeft.No;
            labelPoint.Size = new Size(248, 58);
            labelPoint.TabIndex = 2;
            labelPoint.Text = "Points: 10000000";
            labelPoint.TextAlign = ContentAlignment.MiddleCenter;
            labelPoint.Click += labelPoint_Click;
            // 
            // buttonUpgrade
            // 
            buttonUpgrade.Location = new Point(19, 202);
            buttonUpgrade.Name = "buttonUpgrade";
            buttonUpgrade.Size = new Size(100, 30);
            buttonUpgrade.TabIndex = 3;
            buttonUpgrade.Text = "Upgrade";
            buttonUpgrade.UseVisualStyleBackColor = true;
            buttonUpgrade.Click += buttonUpgrade_Click;
            // 
            // labelUpgradeCost
            // 
            labelUpgradeCost.ForeColor = Color.White;
            labelUpgradeCost.Location = new Point(125, 202);
            labelUpgradeCost.Name = "labelUpgradeCost";
            labelUpgradeCost.Size = new Size(142, 30);
            labelUpgradeCost.TabIndex = 4;
            labelUpgradeCost.Text = "Upgrade Cost: 10000000";
            labelUpgradeCost.TextAlign = ContentAlignment.MiddleLeft;
            // 
            // buttonPrestige
            // 
            buttonPrestige.Location = new Point(19, 238);
            buttonPrestige.Name = "buttonPrestige";
            buttonPrestige.Size = new Size(100, 30);
            buttonPrestige.TabIndex = 6;
            buttonPrestige.Text = "Prestige";
            buttonPrestige.UseVisualStyleBackColor = true;
            buttonPrestige.Click += buttonPrestige_Click;
            // 
            // labelPrestigeCost
            // 
            labelPrestigeCost.ForeColor = Color.White;
            labelPrestigeCost.Location = new Point(125, 238);
            labelPrestigeCost.Name = "labelPrestigeCost";
            labelPrestigeCost.Size = new Size(142, 30);
            labelPrestigeCost.TabIndex = 7;
            labelPrestigeCost.Text = "Prestige Cost: 10000000";
            labelPrestigeCost.TextAlign = ContentAlignment.MiddleLeft;
            labelPrestigeCost.Click += labelPrestigeCost_Click;
            // 
            // labelUpgradeInfo
            // 
            labelUpgradeInfo.ForeColor = Color.White;
            labelUpgradeInfo.Location = new Point(273, 202);
            labelUpgradeInfo.Name = "labelUpgradeInfo";
            labelUpgradeInfo.Size = new Size(272, 30);
            labelUpgradeInfo.TabIndex = 5;
            labelUpgradeInfo.Text = "improve click gain by 1 + x*(1.01+[prestige*0.02])";
            labelUpgradeInfo.TextAlign = ContentAlignment.MiddleLeft;
            // 
            // buttonAscend
            // 
            buttonAscend.Location = new Point(19, 274);
            buttonAscend.Name = "buttonAscend";
            buttonAscend.Size = new Size(100, 30);
            buttonAscend.TabIndex = 9;
            buttonAscend.Text = "Ascend";
            buttonAscend.UseVisualStyleBackColor = true;
            buttonAscend.Click += buttonAscend_Click;
            // 
            // labelAscendCost
            // 
            labelAscendCost.ForeColor = Color.White;
            labelAscendCost.Location = new Point(125, 274);
            labelAscendCost.Name = "labelAscendCost";
            labelAscendCost.Size = new Size(142, 30);
            labelAscendCost.TabIndex = 10;
            labelAscendCost.Text = "Ascend Cost: 10000000";
            labelAscendCost.TextAlign = ContentAlignment.MiddleLeft;
            // 
            // buttonOpenAscensionShop
            // 
            buttonOpenAscensionShop.Location = new Point(273, 274);
            buttonOpenAscensionShop.Name = "buttonOpenAscensionShop";
            buttonOpenAscensionShop.Size = new Size(180, 30);
            buttonOpenAscensionShop.TabIndex = 11;
            buttonOpenAscensionShop.Text = "Open Ascension Window";
            buttonOpenAscensionShop.UseVisualStyleBackColor = true;
            buttonOpenAscensionShop.Click += buttonOpenAscensionShop_Click;
            // 
            // labelPrestigeInfo
            // 
            labelPrestigeInfo.ForeColor = Color.White;
            labelPrestigeInfo.Location = new Point(273, 238);
            labelPrestigeInfo.Name = "labelPrestigeInfo";
            labelPrestigeInfo.Size = new Size(332, 30);
            labelPrestigeInfo.TabIndex = 13;
            labelPrestigeInfo.Text = "increases the factor of upgrade by 0.02";
            labelPrestigeInfo.TextAlign = ContentAlignment.MiddleLeft;
            labelPrestigeInfo.Click += labelPrestigeInfo_Click;
            // 
            // buttonGenerator
            // 
            buttonGenerator.Location = new Point(251, 122);
            buttonGenerator.Name = "buttonGenerator";
            buttonGenerator.Size = new Size(120, 30);
            buttonGenerator.TabIndex = 14;
            buttonGenerator.Text = "Buy Generator";
            buttonGenerator.UseVisualStyleBackColor = true;
            buttonGenerator.Click += buttonGenerator_Click;
            // 
            // labelUpgradeNote
            // 
            labelUpgradeNote.ForeColor = Color.White;
            labelUpgradeNote.Location = new Point(551, 202);
            labelUpgradeNote.Name = "labelUpgradeNote";
            labelUpgradeNote.Size = new Size(200, 30);
            labelUpgradeNote.TabIndex = 12;
            labelUpgradeNote.Text = "Half the cooldown on first purchase";
            labelUpgradeNote.TextAlign = ContentAlignment.MiddleLeft;
            labelUpgradeNote.Click += labelUpgradeNote_Click;
            // 
            // labelGeneratorInfo
            // 
            labelGeneratorInfo.ForeColor = Color.White;
            labelGeneratorInfo.Location = new Point(377, 122);
            labelGeneratorInfo.Name = "labelGeneratorInfo";
            labelGeneratorInfo.Size = new Size(515, 30);
            labelGeneratorInfo.TabIndex = 15;
            labelGeneratorInfo.Text = "Generators: 10 | Cost: 10000000 | Every generators 10x your current passive gain after the first";
            labelGeneratorInfo.TextAlign = ContentAlignment.MiddleLeft;
            labelGeneratorInfo.Click += labelGeneratorInfo_Click;
            // 
            // labelSoftCap
            // 
            labelSoftCap.Font = new Font("Segoe UI", 11.25F, FontStyle.Regular, GraphicsUnit.Point, 0);
            labelSoftCap.ForeColor = Color.Red;
            labelSoftCap.Location = new Point(19, 166);
            labelSoftCap.Name = "labelSoftCap";
            labelSoftCap.Size = new Size(429, 20);
            labelSoftCap.TabIndex = 0;
            labelSoftCap.Text = "Current points is over 10000000, gain is divided by 10000000";
            labelSoftCap.Click += labelSoftCap_Click;
            // 
            // labelPointGain
            // 
            labelPointGain.AutoSize = true;
            labelPointGain.Font = new Font("Segoe UI", 11.25F, FontStyle.Regular, GraphicsUnit.Point, 0);
            labelPointGain.ForeColor = Color.Teal;
            labelPointGain.Location = new Point(19, 42);
            labelPointGain.Name = "labelPointGain";
            labelPointGain.Size = new Size(147, 20);
            labelPointGain.TabIndex = 99;
            labelPointGain.Text = "Point Gain: 10000000";
            labelPointGain.Click += labelPointGain_Click;
            // 
            // labelPointsPerSecond
            // 
            labelPointsPerSecond.ForeColor = Color.White;
            labelPointsPerSecond.Location = new Point(680, 63);
            labelPointsPerSecond.Name = "labelPointsPerSecond";
            labelPointsPerSecond.Size = new Size(139, 30);
            labelPointsPerSecond.TabIndex = 16;
            labelPointsPerSecond.Text = "Points/second: 1000000";
            labelPointsPerSecond.TextAlign = ContentAlignment.MiddleLeft;
            labelPointsPerSecond.Click += labelPointsPerSecond_Click;
            // 
            // panelTitleBar
            // 
            panelTitleBar.BackColor = Color.FromArgb(24, 24, 24);
            panelTitleBar.Location = new Point(0, 0);
            panelTitleBar.Name = "panelTitleBar";
            panelTitleBar.Size = new Size(1063, 28);
            panelTitleBar.TabIndex = 17;
            // 
            // buttonTranscend
            // 
            buttonTranscend.BackColor = Color.DarkViolet;
            buttonTranscend.Enabled = false;
            buttonTranscend.ForeColor = Color.White;
            buttonTranscend.Location = new Point(19, 310);
            buttonTranscend.Name = "buttonTranscend";
            buttonTranscend.Size = new Size(100, 30);
            buttonTranscend.TabIndex = 100;
            buttonTranscend.Text = "Transcend";
            buttonTranscend.UseVisualStyleBackColor = false;
            buttonTranscend.Click += buttonTranscend_Click;
            // 
            // labelTranscendCost
            // 
            labelTranscendCost.ForeColor = Color.White;
            labelTranscendCost.Location = new Point(125, 310);
            labelTranscendCost.Name = "labelTranscendCost";
            labelTranscendCost.Size = new Size(180, 30);
            labelTranscendCost.TabIndex = 101;
            labelTranscendCost.Text = "Transcend Cost: 1,000,000,000";
            labelTranscendCost.TextAlign = ContentAlignment.MiddleLeft;
            // 
            // labelCooldown
            // 
            labelCooldown.ForeColor = Color.Orange;
            labelCooldown.Location = new Point(19, 80);
            labelCooldown.Name = "labelCooldown";
            labelCooldown.Size = new Size(120, 20);
            labelCooldown.TabIndex = 0;
            labelCooldown.Text = "";
            // 
            // MainForm
            // 
            AutoScaleDimensions = new SizeF(7F, 15F);
            AutoScaleMode = AutoScaleMode.Font;
            BackColor = Color.FromArgb(32, 32, 32);
            ClientSize = new Size(1063, 574);
            Controls.Add(labelCooldown);
            Controls.Add(buttonTranscend);
            Controls.Add(labelTranscendCost);
            Controls.Add(panelTitleBar);
            Controls.Add(labelPointGain);
            Controls.Add(labelSoftCap);
            Controls.Add(labelGeneratorInfo);
            Controls.Add(buttonGenerator);
            Controls.Add(button1);
            Controls.Add(labelPoint);
            Controls.Add(buttonUpgrade);
            Controls.Add(labelUpgradeCost);
            Controls.Add(labelUpgradeInfo);
            Controls.Add(labelUpgradeNote);
            Controls.Add(buttonPrestige);
            Controls.Add(labelPrestigeCost);
            Controls.Add(labelPrestigeInfo);
            Controls.Add(buttonAscend);
            Controls.Add(labelAscendCost);
            Controls.Add(buttonOpenAscensionShop);
            Controls.Add(labelPointsPerSecond);
            FormBorderStyle = FormBorderStyle.FixedSingle;
            MaximizeBox = false;
            Name = "MainForm";
            Text = "MainForm";
            ResumeLayout(false);
            PerformLayout();
        }

        #endregion

        private System.Windows.Forms.Button button1;
        private System.Windows.Forms.Label labelPoint;
        private System.Windows.Forms.Button buttonUpgrade;
        private System.Windows.Forms.Label labelUpgradeCost;
        private System.Windows.Forms.Button buttonPrestige;
        private System.Windows.Forms.Label labelPrestigeCost;
        private System.Windows.Forms.Label labelPrestigeInfo;
        private System.Windows.Forms.Button buttonGenerator;
        private System.Windows.Forms.Label labelGeneratorInfo;
        private System.Windows.Forms.Label labelUpgradeInfo;
        private System.Windows.Forms.Label labelUpgradeNote;
        private System.Windows.Forms.Button buttonAscend;
        private System.Windows.Forms.Label labelAscendCost;
        private System.Windows.Forms.Button buttonOpenAscensionShop;
        private System.Windows.Forms.Label labelSoftCap;
        private System.Windows.Forms.Label labelPointGain;
        private System.Windows.Forms.Label labelPointsPerSecond;
        private System.Windows.Forms.Button buttonTranscend;
        private System.Windows.Forms.Label labelTranscendCost;
        private System.Windows.Forms.Label labelCooldown;
#if DEBUG
        private System.Windows.Forms.Button buttonDebug;
#endif

    }
}