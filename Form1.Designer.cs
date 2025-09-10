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
            this.components = new System.ComponentModel.Container();
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(800, 450);
            this.Text = "Form1";

            this.button1 = new System.Windows.Forms.Button();
            this.labelPoint = new System.Windows.Forms.Label();
            this.buttonUpgrade = new System.Windows.Forms.Button();
            this.labelUpgradeCost = new System.Windows.Forms.Label();
            this.buttonPrestige = new System.Windows.Forms.Button();
            this.labelPrestigeCost = new System.Windows.Forms.Label();
            this.labelUpgradeInfo = new System.Windows.Forms.Label();
            this.buttonDebug = new System.Windows.Forms.Button();
            this.SuspendLayout();
            // 
            // button1
            // 
            this.button1.Location = new System.Drawing.Point(30, 30);
            this.button1.Name = "button1";
            this.button1.Size = new System.Drawing.Size(100, 30);
            this.button1.Text = "Click Me";
            this.button1.UseVisualStyleBackColor = true;
            this.button1.Click += new System.EventHandler(this.button1_Click);
            // 
            // labelPoint
            // 
            this.labelPoint.Location = new System.Drawing.Point(140, 30);
            this.labelPoint.Name = "labelPoint";
            this.labelPoint.Size = new System.Drawing.Size(80, 30);
            this.labelPoint.Text = "0";
            this.labelPoint.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // buttonUpgrade
            // 
            this.buttonUpgrade.Location = new System.Drawing.Point(30, 70);
            this.buttonUpgrade.Name = "buttonUpgrade";
            this.buttonUpgrade.Size = new System.Drawing.Size(100, 30);
            this.buttonUpgrade.Text = "Upgrade";
            this.buttonUpgrade.UseVisualStyleBackColor = true;
            this.buttonUpgrade.Click += new System.EventHandler(this.buttonUpgrade_Click);
            // 
            // labelUpgradeCost
            // 
            this.labelUpgradeCost.Location = new System.Drawing.Point(140, 70);
            this.labelUpgradeCost.Name = "labelUpgradeCost";
            this.labelUpgradeCost.Size = new System.Drawing.Size(120, 30);
            this.labelUpgradeCost.Text = "Upgrade Cost: 5";
            this.labelUpgradeCost.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // labelUpgradeInfo
            // 
            this.labelUpgradeInfo.Location = new System.Drawing.Point(270, 70);
            this.labelUpgradeInfo.Name = "labelUpgradeInfo";
            this.labelUpgradeInfo.Size = new System.Drawing.Size(300, 30);
            this.labelUpgradeInfo.Text = "improve click gain by 1 + x*(1.01+[prestige*0.01])";
            this.labelUpgradeInfo.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // buttonPrestige
            // 
            this.buttonPrestige.Location = new System.Drawing.Point(30, 110);
            this.buttonPrestige.Name = "buttonPrestige";
            this.buttonPrestige.Size = new System.Drawing.Size(100, 30);
            this.buttonPrestige.Text = "Prestige";
            this.buttonPrestige.UseVisualStyleBackColor = true;
            this.buttonPrestige.Click += new System.EventHandler(this.buttonPrestige_Click);
            // 
            // labelPrestigeCost
            // 
            this.labelPrestigeCost.Location = new System.Drawing.Point(140, 110);
            this.labelPrestigeCost.Name = "labelPrestigeCost";
            this.labelPrestigeCost.Size = new System.Drawing.Size(120, 30);
            this.labelPrestigeCost.Text = "Prestige Cost: 1000";
            this.labelPrestigeCost.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // buttonDebug
            // 
            this.buttonDebug.Location = new System.Drawing.Point(670, 390);
            this.buttonDebug.Name = "buttonDebug";
            this.buttonDebug.Size = new System.Drawing.Size(100, 40);
            this.buttonDebug.Text = "Debug x10";
            this.buttonDebug.UseVisualStyleBackColor = true;
            this.buttonDebug.Click += new System.EventHandler(this.buttonDebug_Click);
            // 
            // Form1
            // 
            this.Controls.Add(this.button1);
            this.Controls.Add(this.labelPoint);
            this.Controls.Add(this.buttonUpgrade);
            this.Controls.Add(this.labelUpgradeCost);
            this.Controls.Add(this.labelUpgradeInfo);
            this.Controls.Add(this.buttonPrestige);
            this.Controls.Add(this.labelPrestigeCost);
            this.Controls.Add(this.buttonDebug);
            this.ResumeLayout(false);
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
    }
}