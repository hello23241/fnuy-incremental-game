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
            buttonDebug = new Button();
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
            labelUpgradeCost.Text = "Upgrade Cost: 5";
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
            labelUpgradeInfo.Size = new Size(300, 30);
            labelUpgradeInfo.TabIndex = 5;
            labelUpgradeInfo.Text = "improve click gain by 1 + x*(1.01+[prestige*0.01])";
            labelUpgradeInfo.TextAlign = ContentAlignment.MiddleLeft;
            labelUpgradeInfo.Click += labelUpgradeInfo_Click;
            // buttonAscend
            buttonAscend = new Button();
            buttonAscend.Location = new Point(30, 182); // Adjust as needed
            buttonAscend.Name = "buttonAscend";
            buttonAscend.Size = new Size(100, 30);
            buttonAscend.TabIndex = 9;
            buttonAscend.Text = "Ascend";
            buttonAscend.UseVisualStyleBackColor = true;
            buttonAscend.Click += buttonAscend_Click;
            // labelAscendCost
            labelAscendCost = new Label();
            labelAscendCost.Location = new Point(136, 182);
            labelAscendCost.Name = "labelAscendCost";
            labelAscendCost.Size = new Size(120, 30);
            labelAscendCost.TabIndex = 10;
            labelAscendCost.Text = "Ascend Cost: 1000000";
            labelAscendCost.TextAlign = ContentAlignment.MiddleLeft;

            // buttonOpenAscensionShop
            buttonOpenAscensionShop = new Button();
            buttonOpenAscensionShop.Location = new Point(30, 218);
            buttonOpenAscensionShop.Name = "buttonOpenAscensionShop";
            buttonOpenAscensionShop.Size = new Size(180, 30);
            buttonOpenAscensionShop.TabIndex = 11;
            buttonOpenAscensionShop.Text = "Open Ascension Shop";
            buttonOpenAscensionShop.UseVisualStyleBackColor = true;
            buttonOpenAscensionShop.Click += buttonOpenAscensionShop_Click;
            // 
            // buttonDebug
            // 
            buttonDebug.Location = new Point(670, 390);
            buttonDebug.Name = "buttonDebug";
            buttonDebug.Size = new Size(100, 40);
            buttonDebug.TabIndex = 8;
            buttonDebug.Text = "Debug x10";
            buttonDebug.UseVisualStyleBackColor = true;
            buttonDebug.Click += buttonDebug_Click;
            // 
            // Form1
            // 
            AutoScaleDimensions = new SizeF(7F, 15F);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(800, 450);
            Controls.Add(button1);
            Controls.Add(labelPoint);
            Controls.Add(buttonUpgrade);
            Controls.Add(labelUpgradeCost);
            Controls.Add(labelUpgradeInfo);
            Controls.Add(buttonPrestige);
            Controls.Add(labelPrestigeCost);
            Controls.Add(buttonAscend);
            Controls.Add(buttonDebug);
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
        private System.Windows.Forms.Label labelUpgradeInfo;
        private System.Windows.Forms.Button buttonDebug;
        private System.Windows.Forms.Button buttonAscend;
        private System.Windows.Forms.Label labelAscendCost;
        private System.Windows.Forms.Button buttonOpenAscensionShop;


    }
}