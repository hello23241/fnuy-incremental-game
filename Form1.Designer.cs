namespace WinFormsApp1
{
    partial class Form1
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
            SuspendLayout();
            // 
            // button1
            // 
            button1.Location = new Point(30, 30);
            button1.Name = "button1";
            button1.Size = new Size(100, 30);
            button1.TabIndex = 1;
            button1.Text = "Click Me";
            button1.UseVisualStyleBackColor = true;
            button1.Click += button1_Click;
            // 
            // labelPoint
            // 
            labelPoint.Location = new Point(140, 30);
            labelPoint.Name = "labelPoint";
            labelPoint.Size = new Size(80, 30);
            labelPoint.TabIndex = 2;
            labelPoint.Text = "0";
            labelPoint.TextAlign = ContentAlignment.MiddleLeft;
            // 
            // buttonUpgrade
            // 
            buttonUpgrade.Location = new Point(30, 110);
            buttonUpgrade.Name = "buttonUpgrade";
            buttonUpgrade.Size = new Size(100, 30);
            buttonUpgrade.TabIndex = 3;
            buttonUpgrade.Text = "Upgrade";
            buttonUpgrade.UseVisualStyleBackColor = true;
            buttonUpgrade.Click += buttonUpgrade_Click;
            // 
            // labelUpgradeCost
            // 
            labelUpgradeCost.Location = new Point(136, 110);
            labelUpgradeCost.Name = "labelUpgradeCost";
            labelUpgradeCost.Size = new Size(120, 30);
            labelUpgradeCost.TabIndex = 4;
            labelUpgradeCost.Text = "Upgrade Cost: 10";
            labelUpgradeCost.TextAlign = ContentAlignment.MiddleLeft;
            // 
            // buttonPrestige
            // 
            buttonPrestige.Location = new Point(30, 146);
            buttonPrestige.Name = "buttonPrestige";
            buttonPrestige.Size = new Size(100, 30);
            buttonPrestige.TabIndex = 6;
            buttonPrestige.Text = "Prestige";
            buttonPrestige.UseVisualStyleBackColor = true;
            buttonPrestige.Click += buttonPrestige_Click;
            // 
            // labelPrestigeCost
            // 
            labelPrestigeCost.Location = new Point(136, 146);
            labelPrestigeCost.Name = "labelPrestigeCost";
            labelPrestigeCost.Size = new Size(120, 30);
            labelPrestigeCost.TabIndex = 7;
            labelPrestigeCost.Text = "Prestige Cost: 1000";
            labelPrestigeCost.TextAlign = ContentAlignment.MiddleLeft;
            // 
            // labelUpgradeInfo
            // 
            labelUpgradeInfo.Location = new Point(262, 110);
            labelUpgradeInfo.Name = "labelUpgradeInfo";
            labelUpgradeInfo.Size = new Size(272, 30);
            labelUpgradeInfo.TabIndex = 5;
            labelUpgradeInfo.Text = "improve click gain by 1 + x*(1.01+[prestige*0.02])";
            labelUpgradeInfo.TextAlign = ContentAlignment.MiddleLeft;
            labelUpgradeInfo.Click += labelUpgradeInfo_Click;
            // 
            // buttonAscend
            // 
            buttonAscend.Location = new Point(30, 182);
            buttonAscend.Name = "buttonAscend";
            buttonAscend.Size = new Size(100, 30);
            buttonAscend.TabIndex = 9;
            buttonAscend.Text = "Ascend";
            buttonAscend.UseVisualStyleBackColor = true;
            buttonAscend.Click += buttonAscend_Click;
            // 
            // labelAscendCost
            // 
            labelAscendCost.Location = new Point(136, 182);
            labelAscendCost.Name = "labelAscendCost";
            labelAscendCost.Size = new Size(142, 30);
            labelAscendCost.TabIndex = 10;
            labelAscendCost.Text = "Ascend Cost: 1000000";
            labelAscendCost.TextAlign = ContentAlignment.MiddleLeft;
            // 
            // buttonOpenAscensionShop
            // 
            buttonOpenAscensionShop.Location = new Point(30, 218);
            buttonOpenAscensionShop.Name = "buttonOpenAscensionShop";
            buttonOpenAscensionShop.Size = new Size(180, 30);
            buttonOpenAscensionShop.TabIndex = 11;
            buttonOpenAscensionShop.Text = "Open Ascension Shop";
            buttonOpenAscensionShop.UseVisualStyleBackColor = true;
            buttonOpenAscensionShop.Click += buttonOpenAscensionShop_Click;
            // 
            // labelPrestigeInfo
            // 
            labelPrestigeInfo.Location = new Point(262, 146);
            labelPrestigeInfo.Name = "labelPrestigeInfo";
            labelPrestigeInfo.Size = new Size(221, 30);
            labelPrestigeInfo.TabIndex = 13;
            labelPrestigeInfo.Text = "increases the factor of upgrade by 0.02";
            labelPrestigeInfo.TextAlign = ContentAlignment.MiddleLeft;
            // 
            // buttonGenerator
            // 
            buttonGenerator.Location = new Point(230, 30);
            buttonGenerator.Name = "buttonGenerator";
            buttonGenerator.Size = new Size(120, 30);
            buttonGenerator.TabIndex = 14;
            buttonGenerator.Text = "Buy Generator";
            buttonGenerator.UseVisualStyleBackColor = true;
            buttonGenerator.Click += buttonGenerator_Click;
            // 
            // labelUpgradeNote
            // 
            labelUpgradeNote.Location = new Point(540, 110);
            labelUpgradeNote.Name = "labelUpgradeNote";
            labelUpgradeNote.Size = new Size(200, 30);
            labelUpgradeNote.TabIndex = 12;
            labelUpgradeNote.Text = "Half the cooldown on first purchase";
            labelUpgradeNote.TextAlign = ContentAlignment.MiddleLeft;
            // 
            // labelGeneratorInfo
            // 
            labelGeneratorInfo.Location = new Point(360, 30);
            labelGeneratorInfo.Name = "labelGeneratorInfo";
            labelGeneratorInfo.Size = new Size(200, 30);
            labelGeneratorInfo.TabIndex = 15;
            labelGeneratorInfo.Text = "Generators: 0 | Cost: 100 | Points/s: 0";
            labelGeneratorInfo.TextAlign = ContentAlignment.MiddleLeft;
            // 
            // Form1
            // 
            AutoScaleDimensions = new SizeF(7F, 15F);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(800, 450);
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
            Name = "Form1";
            Text = "Form1";
            ResumeLayout(false);
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
#if DEBUG
private System.Windows.Forms.Button buttonDebug;
#endif

    }
}