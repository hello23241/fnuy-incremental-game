namespace WinFormsApp1
{
    public partial class Form1 : Form
    {
        private int point = 1000;
        private double pointMultiplier = 1.0;
        private double upgradeCost = 10.0;
        private double prestigeBonus = 0.0;
        private double prestigeCost = 1000.0;
        private double generatorCost = 100.0;
        private int generatorCount = 0;
        private System.Windows.Forms.Timer generatorTimer;
        private System.Windows.Forms.Timer cooldownTimer;
        private int cooldownDuration = 1000; // milliseconds
        private int cooldownElapsed = 0;
        private bool isCooldown = false;
        private double ascendCost = 1000000.0;
        private int ascensionPoints = 0;
        private const double PrestigeIncrement = 0.02;
        public Form1()
        {
            InitializeComponent();
            //cooldown on "click me"
            cooldownTimer = new System.Windows.Forms.Timer();
            cooldownTimer.Interval = 50;
            cooldownTimer.Tick += CooldownTimer_Tick;
            //timer for the point generator
            generatorTimer = new System.Windows.Forms.Timer();
            generatorTimer.Interval = 1000; // 1 second
            generatorTimer.Tick += GeneratorTimer_Tick;
            generatorTimer.Start();
            // Hiding buttons at the start
            buttonAscend.Visible = false;
            labelAscendCost.Visible = false;
            buttonOpenAscensionShop.Visible = false;
            buttonPrestige.Visible = false;
            labelPrestigeCost.Visible = false;
            labelPrestigeInfo.Visible = false;
            // This is necessary
            UpdateButtonStates();
        }
        private void UpdateButtonStates()
        {
            // Check if player has enough points for each action
            buttonUpgrade.Enabled = point >= (int)upgradeCost;
            buttonPrestige.Enabled = point >= (int)prestigeCost;
            buttonAscend.Enabled = point >= (int)ascendCost;
            buttonGenerator.Enabled = point >= (int)generatorCost;

            //Change button colors for visual feedback
            buttonUpgrade.BackColor = buttonUpgrade.Enabled ? Color.LightGreen : Color.Gray;
            buttonPrestige.BackColor = buttonPrestige.Enabled ? Color.LightBlue : Color.Gray;
            buttonAscend.BackColor = buttonAscend.Enabled ? Color.MediumPurple : Color.Gray;
            buttonGenerator.BackColor = buttonGenerator.Enabled ? Color.LightGreen : Color.Gray;
        }

        private void CooldownTimer_Tick(object sender, EventArgs e)
        {
            cooldownElapsed += cooldownTimer.Interval;

            if (cooldownElapsed >= cooldownDuration)
            {
                cooldownTimer.Stop();
                isCooldown = false;
                button1.Enabled = true;
                cooldownElapsed = 0;
            }
        }

        private void button1_Click(object sender, EventArgs e)
        {
            if (isCooldown) return;

            point += (int)pointMultiplier;
            labelPoint.Text = point.ToString();
            button1.Text = $"+{(int)pointMultiplier} points";
            UpdateButtonStates();

            // Start cooldown
            isCooldown = true;
            button1.Enabled = false;
            cooldownElapsed = 0;
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
                UpdateButtonStates();

                UpdateUpgradeInfoLabel();
                if (cooldownDuration == 1000)
                {
                    cooldownDuration = 500;
                    buttonPrestige.Visible = true;
                    labelPrestigeCost.Visible = true;
                    labelUpgradeNote.Visible = false;
                    labelPrestigeInfo.Text = $"increases the factor of upgrade by {PrestigeIncrement}";

                }
            }
        }

        private void buttonPrestige_Click(object sender, EventArgs e)
        {
            if (point >= (int)prestigeCost)
            {
                point = 0;
                prestigeBonus += PrestigeIncrement;
                pointMultiplier = 1.0 + prestigeBonus;
                upgradeCost = 5.0;
                prestigeCost = Math.Pow(prestigeCost, 1.05);
                labelPrestigeCost.Text = $"Prestige Cost: {prestigeCost:F0}";
                labelPoint.Text = point.ToString();
                labelUpgradeCost.Text = $"Upgrade Cost: {upgradeCost:F0}";
                button1.Text = $"+{(int)pointMultiplier} points";
                buttonAscend.Visible = true;
                labelAscendCost.Visible = true;
                labelPrestigeInfo.Visible = true;
                UpdateUpgradeInfoLabel();
                UpdateButtonStates();
            }
        }
            private void buttonGenerator_Click(object sender, EventArgs e)
        {
            if (point >= (int)generatorCost)
            {
                point -= (int)generatorCost;
                generatorCount++;
                generatorCost = Math.Pow(generatorCost, 2);
                labelPoint.Text = point.ToString();
                UpdateButtonStates();
            }
        }
        private void GeneratorTimer_Tick(object sender, EventArgs e)
        {
            double passiveGain = generatorCount * 0.1 * pointMultiplier;
            point += (int)passiveGain;
            labelPoint.Text = point.ToString();
            UpdateButtonStates();
        }
        private void buttonAscend_Click(object sender, EventArgs e)
        {
            if (point >= ascendCost)
            {
                point = 0;
                pointMultiplier = 1.0;
                upgradeCost = 5.0;
                prestigeBonus = 0.0;
                prestigeCost = 1000.0;
                ascendCost = Math.Pow(ascendCost, 1.1); // Optional scaling
                ascensionPoints += 1;

                labelPoint.Text = point.ToString();
                labelUpgradeCost.Text = $"Upgrade Cost: {upgradeCost:F0}";
                labelPrestigeCost.Text = $"Prestige Cost: {prestigeCost:F0}";
                labelAscendCost.Text = $"Ascend Cost: {ascendCost:F0}";
                button1.Text = $"+{(int)pointMultiplier} points";
                buttonOpenAscensionShop.Visible = true;
                UpdateUpgradeInfoLabel();
                UpdateButtonStates();
            }
        }

#if DEBUG
        private void buttonDebug_Click(object sender, EventArgs e)
        {
            pointMultiplier *= 10;
            button1.Text = $"+{(int)pointMultiplier} points";
            UpdateUpgradeInfoLabel();
        }
#endif

        private void buttonOpenAscensionShop_Click(object sender, EventArgs e)
        {
            // Pass current ascension points to the shop
            AscensionShop shop = new AscensionShop(ascensionPoints);
            shop.ShowDialog(); // Opens the shop as a modal window
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
