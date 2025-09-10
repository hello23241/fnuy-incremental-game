namespace WinFormsApp1
{
    public partial class Form1 : Form
    {
        private int point = 0;
        private double pointMultiplier = 1.0;
        private double upgradeCost = 5.0;
        private double prestigeBonus = 0.0;
        private double prestigeCost = 1000.0;

        public Form1()
        {
            InitializeComponent();
            labelPoint.Text = point.ToString();
            labelUpgradeCost.Text = $"Upgrade Cost: {upgradeCost:F0}";
            button1.Text = $"+{(int)pointMultiplier} points";
            UpdateUpgradeInfoLabel();
        }

        private void button1_Click(object sender, EventArgs e)
        {
            point += (int)pointMultiplier;
            labelPoint.Text = point.ToString();
            button1.Text = $"+{(int)pointMultiplier} points";
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
    }
}
