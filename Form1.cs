using BreakInfinity;
using System.IO;
using Newtonsoft.Json;

namespace WinFormsApp1
{
    public partial class Form1 : Form
    {
        private BigDouble point = new BigDouble(0);
        private BigDouble pointMultiplier = new BigDouble(1);
        private BigDouble upgradeCost = new BigDouble(10);
        private BigDouble prestigeBonus = new BigDouble(0);
        private BigDouble prestigeCost = new BigDouble(1000);
        private BigDouble generatorCost = new BigDouble(100);
        private BigDouble ascendCost = new BigDouble(1000000);
        private BigDouble ascensionPoints = new BigDouble(0);
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
            labelSoftCap.Visible = false;
            // This is necessary
            LoadGame();
            UpdateUI();
        }
        private readonly string savePath = Path.Combine(
        Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
        "FnuyIncrementalGame",
        "savegame.json"
);

        private void UpdateUI()
        {
            UpdateButtonStates();
            UpdateUpgradeInfoLabel();
            UpdateGeneratorInfo();
            UpdateSoftCapLabel();
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

            BigDouble divisor = GetSoftCapDivisor(point);
            BigDouble gain = pointMultiplier / divisor;
            point += gain;

            labelPoint.Text = FormatNumbers(point);
            button1.Text = $"+{FormatNumbers(gain)} points";

            UpdateUI();

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
                pointMultiplier = 1 + pointMultiplier * prestigeBonus;
                upgradeCost = BigDouble.Pow(upgradeCost, 1.05);
                labelPoint.Text = FormatNumbers(point);
                labelUpgradeCost.Text = $"Upgrade Cost: {FormatNumbers(upgradeCost)}";
                button1.Text = $"+{FormatNumbers(pointMultiplier)} points";
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
                upgradeCost = 10.0;
                prestigeCost = BigDouble.Pow(prestigeCost, 1.05);
                labelPrestigeCost.Text = $"Prestige Cost: {FormatNumbers(prestigeCost)}";
                labelPoint.Text = point.ToString("F1");
                labelUpgradeCost.Text = $"Upgrade Cost: {FormatNumbers(upgradeCost)}";
                button1.Text = $"+{((BigDouble)pointMultiplier).ToString("F1")} points";
                buttonAscend.Visible = true;
                labelAscendCost.Visible = true;
                labelPrestigeInfo.Visible = true;
                labelGeneratorInfo.Visible = true;
                buttonGenerator.Visible = true;
                labelSoftCap.Visible = true;
                UpdateUI();
            }
        }
        private void UpdateGeneratorInfo()
        {
            if (generatorCount == 0)
            { return; }
            BigDouble pps = Math.Pow(10,generatorCount) * 0.01 * pointMultiplier;
            labelGeneratorInfo.Text = $"Generators: {generatorCount} | Cost: {FormatNumbers(generatorCost)} | Point/second: {FormatNumbers(pps)}";
        }

        private void buttonGenerator_Click(object sender, EventArgs e)
        {
            if (point >= (BigDouble)generatorCost)
            {
                point -= (BigDouble)generatorCost;
                generatorCount++;
                generatorCost = BigDouble.Pow(generatorCost, 2);
                labelPoint.Text = FormatNumbers(point);
                UpdateUI();
            }
        }
        private void GeneratorTimer_Tick(object sender, EventArgs e)
        {
            if (generatorCount > 0)
            {
                BigDouble divisor = GetSoftCapDivisor(point);

                BigDouble passiveGain = BigDouble.Pow(10, generatorCount) * 0.1 * pointMultiplier / divisor;

                point += passiveGain;

                labelPoint.Text = FormatNumbers(point);
                UpdateUI();
            }
        }
        private void buttonAscend_Click(object sender, EventArgs e)
        {
            if (point >= ascendCost)
            {
                point = 0;
                pointMultiplier = 1.0;
                upgradeCost = 10.0;
                prestigeBonus = 0.0;
                prestigeCost = 1000.0;
                ascendCost = BigDouble.Pow(ascendCost, 1.1); // Optional scaling
                ascensionPoints += 1;

                labelPoint.Text = point.ToString("F1");
                labelUpgradeCost.Text = $"Upgrade Cost: {FormatNumbers(upgradeCost)}";
                labelPrestigeCost.Text = $"Prestige Cost: {FormatNumbers(prestigeCost)}";
                labelAscendCost.Text = $"Ascend Cost: {FormatNumbers(ascendCost)}";
                labelPoint.Text = $"Points: {FormatNumbers(point)}";
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

            var settings = new JsonSerializerSettings();
            settings.Converters.Add(new BigDoubleConverter());

            Directory.CreateDirectory(Path.GetDirectoryName(savePath));
            string json = JsonConvert.SerializeObject(state, settings);
            File.WriteAllText(savePath, json);
        }
        private void LoadGame()
        {
            if (!File.Exists(savePath)) return;

            string json = File.ReadAllText(savePath);
            var settings = new JsonSerializerSettings();
            settings.Converters.Add(new BigDoubleConverter());

            GameState state = JsonConvert.DeserializeObject<GameState>(json, settings);

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

        //soft cap
        private BigDouble GetSoftCapDivisor(BigDouble currentPoints)
        {
            if (point>10000)
            {
                BigDouble baseSoftCap = 2; // starting soft cap value
                int oom = (int)Math.Floor(BigDouble.Log10(point))-4; // get order of magnitude
                BigDouble softCap = baseSoftCap * BigDouble.Pow(2, oom);
                return softCap;
            }
            return 1;
        }
        private void UpdateSoftCapLabel()
        {
            BigDouble divisor = GetSoftCapDivisor(point);
            labelSoftCap.Text = $"Soft Cap: {FormatNumbers(divisor)}";
        }
        private string FormatNumbers(BigDouble value)
        {
            // Use scientific notation only for extremely large values (>= 1e308)
            if (value >= BigDouble.Pow(10, 308))
                return value.ToString("E1");

            // Custom suffixes for readable formatting
            string[] suffixes = {
    "", "K", "M", "B", "T", "Qa", "Qi", "Sx", "Sp", "Oc", "No", // 10^3 to 10^33
    "Dc", "Ud", "Dd", "Td", "Qad", "Qid", "Sxd", "Spd", "Ocd", "Nod", "Vg", // 10^36 to 10^63
    "Uvg", "Dvg", "Tvg", "Qavg", "Qivg", "Sxvg", "Spvg", "Ocvg", "Novg", "Tg", // 10^66 to 10^93
    "Utg", "Dtg", "Ttg", "Qatg", "Qitg", "Sxtg", "Sptg", "Octg", "Notg", "Qag", // 10^96 to 10^123
    "Uqag", "Dqag", "Tqag", "Qaqag", "Qiqag", "Sxqag", "Spqag", "Ocqag", "Noqag", "Qig", // 10^126 to 10^153
    "Uqig", "Dqig", "Tqig", "Qaqig", "Qiqig", "Sxqig", "Spqig", "Ocqig", "Noqig", "Sxg", // 10^156 to 10^183
    "Usxg", "Dsxg", "Tsxg", "Qasxg", "Qisxg", "Sxsxg", "Spsxg", "Ocsxg", "Nosxg", "Spg", // 10^186 to 10^213
    "Uspg", "Dspg", "Tspg", "Qaspg", "Qispg", "Sxspg", "Spspg", "Ocspg", "Nospg", "Ocg", // 10^216 to 10^243
    "Uocg", "Docg", "Tocg", "Qaocg", "Qiocg", "Sxocg", "Spocg", "Ococg", "Noocg", "Nog", // 10^246 to 10^273
    "Unog", "Dnog", "Tnog", "Qanog", "Qinog", "Sxnog", "Spnog", "Ocnog", "Nonog", "C" // 10^276 to 10^303
};

            int suffixIndex = 0;

            while (value >= 1000 && suffixIndex < suffixes.Length - 1)
            {
                value /= 1000;
                suffixIndex++;
            }

            return $"{value:F1}{suffixes[suffixIndex]}";
        }


        private void ApplyOfflineProgress(DateTime lastSaved)
        {
            TimeSpan offlineTime = DateTime.Now - lastSaved;
            BigDouble seconds = offlineTime.TotalSeconds;

            BigDouble passiveGain = generatorCount * 0.1 * pointMultiplier * seconds;
            point += (BigDouble)passiveGain;

            MessageBox.Show($"Welcome back! You earned {(BigDouble)passiveGain} points while you were away.");
        }
        //saves on close
        protected override void OnFormClosing(FormClosingEventArgs e)
        {
            SaveGame();
            MessageBox.Show("Your progress has been saved!", "Game Saved", MessageBoxButtons.OK, MessageBoxIcon.Information);

            base.OnFormClosing(e);
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
            labelUpgradeInfo.Text = $"improve click gain by 1 + x*({FormatNumbers(multiplier)})";
        }

        private void labelUpgradeInfo_Click(object sender, EventArgs e)
        {

        }
    }
}
