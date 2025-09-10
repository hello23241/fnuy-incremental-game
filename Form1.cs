namespace WinFormsApp1
{
    public partial class Form1 : Form
    {
        private int point = 0;
        private double pointMultiplier = 1.0;
        private double upgradeCost = 5.0;
        private double prestigeBonus = 0.0;
        private double prestigeCost = 1000.0;
        private System.Windows.Forms.Timer cooldownTimer;
        private int cooldownDuration = 1000; // milliseconds
        private int cooldownElapsed = 0;
        private bool isCooldown = false;


        public Form1()
        {
            InitializeComponent();
            
            progressBarCooldown.Maximum = cooldownDuration; // Now it uses the correct value
            progressBarCooldown.Step = cooldownTimer.Interval; // Optional: makes stepping smoother

            cooldownTimer = new System.Windows.Forms.Timer();
            cooldownTimer.Interval = 50;
            cooldownTimer.Tick += CooldownTimer_Tick;
            
        }

        private void CooldownTimer_Tick(object sender, EventArgs e)
        {
            cooldownElapsed += cooldownTimer.Interval;
            progressBarCooldown.Value = Math.Min(progressBarCooldown.Maximum, cooldownElapsed);

            if (cooldownElapsed >= cooldownDuration)
            {
                cooldownTimer.Stop();
                isCooldown = false;
                button1.Enabled = true;
                progressBarCooldown.Value = 0;
            }
        }

        private void button1_Click(object sender, EventArgs e)
        {
            if (isCooldown) return;

            point += (int)pointMultiplier;
            labelPoint.Text = point.ToString();
            button1.Text = $"+{(int)pointMultiplier} points";

            // Start cooldown
            isCooldown = true;
            button1.Enabled = false;
            cooldownElapsed = 0;
            progressBarCooldown.Value = 0;
            cooldownTimer.Start();
        }


        private void buttonUpgrade_Click(object sender, EventArgs e)
        {
            if (point >= (int)upgradeCost)
            {
                point -= (int)upgradeCost;
                pointMultiplier = 1 + pointMultiplier * (1.01 + prestigeBonus);
                upgradeCost = Math.Pow(upgradeCost, 1.05);
                labelPoint.Text = point.ToString();
                labelUpgradeCost.Text = $"Upgrade Cost: {upgradeCost:F0}";
                button1.Text = $"+{(int)pointMultiplier} points";
                UpdateUpgradeInfoLabel();
            }
        }

        private void buttonPrestige_Click(object sender, EventArgs e)
        {
            if (point >= (int)prestigeCost)
            {
                point = 0;
                prestigeBonus += 0.02;
                pointMultiplier = 1.0 + prestigeBonus;
                upgradeCost = 5.0;
                prestigeCost = Math.Pow(prestigeCost, 1.05);
                labelPrestigeCost.Text = $"Prestige Cost: {prestigeCost:F0}";
                labelPoint.Text = point.ToString();
                labelUpgradeCost.Text = $"Upgrade Cost: {upgradeCost:F0}";
                button1.Text = $"+{(int)pointMultiplier} points";
                UpdateUpgradeInfoLabel();
            }
        }

        private void buttonDebug_Click(object sender, EventArgs e)
        {
            pointMultiplier *= 10;
            button1.Text = $"+{(int)pointMultiplier} points";
            UpdateUpgradeInfoLabel();
        }

        private void UpdateUpgradeInfoLabel()
        {
            double multiplier = 1.01 + prestigeBonus;
            labelUpgradeInfo.Text = $"improve click gain by 1 + x*({multiplier:F2})";
        }

        private void labelUpgradeInfo_Click(object sender, EventArgs e)
        {

        }
    }
}
