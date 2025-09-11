using BreakInfinity;
using System.IO;
using System.Text.Json;

namespace WinFormsApp1
{
    public partial class Form1 : Form
    {
        private BigDouble point = new BigDouble(0);
        private BigDouble pointMultiplier = new BigDouble(1.0);
        private BigDouble upgradeCost = new BigDouble(10.0);
        private BigDouble prestigeBonus = new BigDouble(0.0);
        private BigDouble prestigeCost = new BigDouble(1000.0);
        private BigDouble generatorCost = new BigDouble(100.0);
        private BigDouble ascendCost = new BigDouble(1000000.0);
        private BigDouble ascensionPoints = 0;
        private int generatorCount = 0;
        private System.Windows.Forms.Timer generatorTimer;
        private System.Windows.Forms.Timer cooldownTimer;
        private int cooldownDuration = 1000; // milliseconds
        private int cooldownElapsed = 0;
        private bool isCooldown = false;
        private const double PrestigeIncrement = 0.02;
        //constructor
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
            labelGeneratorInfo.Visible = false;
            buttonGenerator.Visible = false;
            // This is necessary
            UpdateButtonStates();
        }
        private void UpdateUI()
        {
            UpdateButtonStates();
            UpdateUpgradeInfoLabel();
            UpdateGeneratorInfo();
        }
        private void UpdateButtonStates()
        {
            // Check if player has enough points for each action
            buttonUpgrade.Enabled = point >= (BigDouble)upgradeCost;
            buttonPrestige.Enabled = point >= (BigDouble)prestigeCost;
            buttonAscend.Enabled = point >= (BigDouble)ascendCost;
            buttonGenerator.Enabled = point >= (BigDouble)generatorCost;

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

            point += (BigDouble)pointMultiplier;
            labelPoint.Text = point.ToString("F1");
            button1.Text = $"+{((BigDouble)pointMultiplier).ToString("F1")} points";
            UpdateUI();

            // Start cooldown
            isCooldown = true;
            button1.Enabled = false;
            cooldownElapsed = 0;
            cooldownTimer.Start();
        }


        private void buttonUpgrade_Click(object sender, EventArgs e)
        {
            if (point >= (BigDouble)upgradeCost)
            {
                point -= (BigDouble)upgradeCost;
                pointMultiplier = 1 + pointMultiplier * (1.01 + prestigeBonus);
                upgradeCost = BigDouble.Pow(upgradeCost, 1.05);
                labelPoint.Text = point.ToString("F1");
                labelUpgradeCost.Text = $"Upgrade Cost: {upgradeCost.ToString("F1")}";
                button1.Text = $"+{((BigDouble)pointMultiplier).ToString("F1")} points";
                UpdateUI();

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
            if (point >= (BigDouble)prestigeCost)
            {
                point = 0;
                prestigeBonus += PrestigeIncrement;
                pointMultiplier = 1.0 + prestigeBonus;
                upgradeCost = 5.0;
                prestigeCost = BigDouble.Pow(prestigeCost, 1.05);
                labelPrestigeCost.Text = $"Prestige Cost: {prestigeCost:F1}";
                labelPoint.Text = point.ToString("F1");
                labelUpgradeCost.Text = $"Upgrade Cost: {upgradeCost:F1}";
                button1.Text = $"+{((BigDouble)pointMultiplier).ToString("F1")} points";
                buttonAscend.Visible = true;
                labelAscendCost.Visible = true;
                labelPrestigeInfo.Visible = true;
                labelGeneratorInfo.Visible = true;
                buttonGenerator.Visible = true;
                UpdateUI();
            }
        }
        private void UpdateGeneratorInfo()
        {
            if (generatorCount == 0)
            { return; }
            BigDouble pps = Math.Pow(10,generatorCount) * 0.01 * pointMultiplier;
            labelGeneratorInfo.Text = $"Generators: {generatorCount} | Cost: {generatorCost:F0} | Point/second: {pps:F1}";
        }

        private void buttonGenerator_Click(object sender, EventArgs e)
        {
            if (point >= (BigDouble)generatorCost)
            {
                point -= (BigDouble)generatorCost;
                generatorCount++;
                generatorCost = BigDouble.Pow(generatorCost, 2);
                labelPoint.Text = point.ToString("E3"); // e.g., "1.23E+12"
                UpdateUI();
            }
        }
        private void GeneratorTimer_Tick(object sender, EventArgs e)
        {
            BigDouble passiveGain = generatorCount * 0.1 * pointMultiplier;
            point += passiveGain;
            labelPoint.Text = point.ToString("F1");
            UpdateUI();
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
                ascendCost = BigDouble.Pow(ascendCost, 1.1); // Optional scaling
                ascensionPoints += 1;

                labelPoint.Text = point.ToString("F1");
                labelUpgradeCost.Text = $"Upgrade Cost: {upgradeCost:F1}";
                labelPrestigeCost.Text = $"Prestige Cost: {prestigeCost:F1}";
                labelAscendCost.Text = $"Ascend Cost: {ascendCost:F1}";
                labelPoint.Text = $"Points: {point:F1}";
                buttonOpenAscensionShop.Visible = true;
                UpdateUI();
            }
        }
        //offline progress
        private void SaveGame()
        {
            var state = new GameState
            {
                Point = point,
                PointMultiplier = pointMultiplier,
                UpgradeCost = upgradeCost,
                PrestigeBonus = prestigeBonus,
                PrestigeCost = prestigeCost,
                GeneratorCost = generatorCost,
                GeneratorCount = generatorCount,
                AscendCost = ascendCost,
                AscensionPoints = ascensionPoints,
                LastSavedTime = DateTime.Now
            };

            string json = JsonSerializer.Serialize(state);
            File.WriteAllText("savegame.json", json);
        }

        private void LoadGame()
        {
            if (!File.Exists("savegame.json")) return;

            string json = File.ReadAllText("savegame.json");
            var state = JsonSerializer.Deserialize<GameState>(json);

            point = state.Point;
            pointMultiplier = state.PointMultiplier;
            upgradeCost = state.UpgradeCost;
            prestigeBonus = state.PrestigeBonus;
            prestigeCost = state.PrestigeCost;
            generatorCost = state.GeneratorCost;
            generatorCount = state.GeneratorCount;
            ascendCost = state.AscendCost;
            ascensionPoints = state.AscensionPoints;

            ApplyOfflineProgress(state.LastSavedTime);
            UpdateUI();
        }
        private void ApplyOfflineProgress(DateTime lastSaved)
        {
            TimeSpan offlineTime = DateTime.Now - lastSaved;
            BigDouble seconds = offlineTime.TotalSeconds;

            BigDouble passiveGain = generatorCount * 0.1 * pointMultiplier * seconds;
            point += (BigDouble)passiveGain;

            MessageBox.Show($"Welcome back! You earned {(BigDouble)passiveGain} points while you were away.");
        }

#if DEBUG
        private void buttonDebug_Click(object sender, EventArgs e)
        {
            pointMultiplier *= 10;
            button1.Text = $"+{((BigDouble)pointMultiplier).ToString("F1")} points";
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
            BigDouble multiplier = 1.01 + prestigeBonus;
            labelUpgradeInfo.Text = $"improve click gain by 1 + x*({multiplier:F2})";
        }

        private void labelUpgradeInfo_Click(object sender, EventArgs e)
        {

        }
    }
}
