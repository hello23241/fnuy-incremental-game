using BreakInfinity;
using System.IO;
using Newtonsoft.Json;
using System.Security.Policy;
using System.Net.Http;
using System.Reflection;

namespace WinFormsApp1
{
    public partial class MainForm : Form
    {
        private BigDouble point = new BigDouble(0);
        private BigDouble pointGain = new BigDouble(1);
        private BigDouble PrestigeIncrement = new BigDouble(3);
        private BigDouble generatorCost = new BigDouble(100);
        private BigDouble ascensionPoints = new BigDouble(0);

        // Base costs and scaling factors
        private readonly BigDouble baseUpgradeCost = new BigDouble(10);
        private readonly double upgradeScale = 1.05;

        private readonly BigDouble basePrestigeCost = new BigDouble(1000);
        private readonly double prestigeScale = 3;

        private readonly BigDouble baseAscendCost = new BigDouble(1_000_000);
        private readonly double ascendScale = 2.5;

        // Purchase counts (persist these, not the cost)
        private int upgradeCount = 0;
        private int prestigeCount = 0;
        private int ascendCount = 0;
        private int generatorCount = 0;
        private System.Windows.Forms.Timer generatorTimer;
        private System.Windows.Forms.Timer cooldownTimer;
        private int cooldownDuration = 1000; // milliseconds
        private int cooldownElapsed = 0;
        private double offlineMultiplier = 0.05;
        private bool isCooldown = false;
        //constructor
        public MainForm()
        {
            InitializeComponent();
            this.Text = "Myrtle incremental";
            this.Icon = Properties.Resources.NianBean;
            //cooldown on "click me"
            cooldownTimer = new System.Windows.Forms.Timer();
            cooldownTimer.Interval = 50;
            cooldownTimer.Tick += CooldownTimer_Tick;
            //timer for the point generator
            generatorTimer = new System.Windows.Forms.Timer();
            generatorTimer.Interval = 1000; // 1 second
            generatorTimer.Tick += GeneratorTimer_Tick;
            generatorTimer.Start();
            // hide locked features by default
            buttonPrestige.Visible = labelPrestigeCost.Visible = labelPrestigeInfo.Visible = false;
            buttonGenerator.Visible = labelGeneratorInfo.Visible = labelSoftCap.Visible = false;
            buttonAscend.Visible = labelAscendCost.Visible = buttonOpenAscensionShop.Visible = false;
            // This is necessary
            LoadGame();
            CheckForUpdates();
            UpdateUI();
        }
        private readonly string savePath = Path.Combine(
        Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
        "FnuyIncrementalGame",
        "savegame.json"
);
        private async void CheckForUpdates()
        {
            string manifestUrl = "https://hello23241.github.io/fnuy-incremental-manifest/manifest.json";
            using var client = new HttpClient();
            try
            {
                string json = await client.GetStringAsync(manifestUrl);

                if (string.IsNullOrWhiteSpace(json))
                    throw new Exception("Manifest response was empty.");

                dynamic manifest = Newtonsoft.Json.JsonConvert.DeserializeObject(json);
                if (manifest == null)
                    throw new Exception("Manifest deserialization failed.");

                string latestVersion = manifest.latestVersion;
                string downloadUrl = manifest.downloadUrl;
                string changelog = manifest.changelog;
                string releaseDateStr = manifest.releaseDate;

                // Parse releaseDate in YYYY-MM-DD or YYYYMMDD format
                DateTime releaseDate;
                if (!DateTime.TryParseExact(releaseDateStr, new[] { "yyyyMMdd", "yyyy-MM-dd" }, null, System.Globalization.DateTimeStyles.None, out releaseDate))
                {
                    releaseDate = DateTime.MinValue; // fallback if parsing fails
                }
                int daysAgo = releaseDate == DateTime.MinValue ? 0 : (DateTime.Now.Date - releaseDate.Date).Days;
                string lastUpdateText = releaseDate == DateTime.MinValue
                    ? "Last update: unknown"
                    : $"Last update: {daysAgo} day{(daysAgo == 1 ? "" : "s")} ago";

                string currentVersion = Assembly.GetExecutingAssembly().GetName().Version.ToString();

                string changelogMessage = $"{lastUpdateText}\n\nChangelog:\n{changelog}";

                if (new Version(latestVersion) > new Version(currentVersion))
                {
                    var result = MessageBox.Show(
                        $"A new version ({latestVersion}) is available!\n\n{changelogMessage}\n\nDo you want to download it now?",
                        "Update Available",
                        MessageBoxButtons.YesNo,
                        MessageBoxIcon.Information);

                    if (result == DialogResult.Yes)
                    {
                        System.Diagnostics.Process.Start(new System.Diagnostics.ProcessStartInfo
                        {
                            FileName = downloadUrl,
                            UseShellExecute = true
                        });
                    }
                }
                else
                {
                    MessageBox.Show($"You are running the latest version.\n\n{changelogMessage}", "No Update", MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Failed to check for updates: {ex.Message}", "Update Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
        private void UpdateUI()
        {
            UpdateButtonStates();
            UpdateUpgradeInfoLabel();
            UpdateGeneratorInfo();
            UpdateSoftCapLabel();
            BigDouble gain = pointGain / GetSoftCapDivisor(point);
            labelPoint.Text = $"Points: {FormatNumbers(point)}";
            button1.Text = $"+{FormatNumbers(gain)} points";
            labelUpgradeCost.Text = $"Upgrade Cost: {FormatNumbers(GetUpgradeCost())}";
            labelPrestigeCost.Text = $"Prestige Cost: {FormatNumbers(GetPrestigeCost())}";
            labelAscendCost.Text = $"Ascend Cost: {FormatNumbers(GetAscendCost())}";
            buttonUpgrade.Enabled = point >= GetUpgradeCost();
            buttonPrestige.Enabled = point >= GetPrestigeCost();
            buttonAscend.Enabled = point >= GetAscendCost();
        }
        private void UpdateButtonStates()
        {
            // Check if player has enough points for each action
            buttonUpgrade.Enabled = point >= GetUpgradeCost();
            buttonPrestige.Enabled = point >= GetPrestigeCost();
            buttonAscend.Enabled = point >= GetAscendCost();
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

        private void Button1_Click(object sender, EventArgs e)
        {
            if (isCooldown) return;

            point += pointGain;

            UpdateUI();

            isCooldown = true;
            button1.Enabled = false;
            cooldownElapsed = 0;
            cooldownTimer.Start();
        }



        private void buttonUpgrade_Click(object sender, EventArgs e)
        {
            var cost = GetUpgradeCost();
            BigDouble divisor = GetSoftCapDivisor(point);
            if (point >= cost)
            {
                point -= cost;
                upgradeCount++;
                // Use log2 for prestigeCount to apply diminishing returns
                double prestigeEffect = Math.Max(1, BigDouble.Log2(prestigeCount + 1));
                pointGain += 1 + pointGain * prestigeEffect * PrestigeIncrement / 100 / divisor;
                if (cooldownDuration == 1000)
                {
                    UnlockPrestigeFeature();
                    cooldownDuration = 500;
                }
                UpdateUI();
            }
        }
        private void UpdateUpgradeInfoLabel()
        {
            BigDouble divisor = GetSoftCapDivisor(point);
            double prestigeEffect = Math.Max(1, BigDouble.Log2(prestigeCount + 1));
            BigDouble gain = 1 + pointGain * prestigeEffect * PrestigeIncrement / 100 / divisor;
            BigDouble prestigeGain = PrestigeIncrement / 100;
            labelUpgradeInfo.Text = $"each upgrade adds {FormatNumbers(gain)} to your click multiplier";
            labelPrestigeInfo.Text = $"Prestige effect: log₂({prestigeCount + 1}) = {prestigeEffect:F2} (diminishing returns)";
        }
        private void buttonPrestige_Click(object sender, EventArgs e)
        {
            var cost = GetPrestigeCost();
            if (point >= cost)
            {
                point = 0;
                upgradeCount = 0;
                pointGain = 1.0;
                prestigeCount++;
                UnlockGeneratorFeature();
                UpdateUI();
            }
        }
        private void UpdateGeneratorInfo()
        {
            if (generatorCount == 0)
            { return; }
            BigDouble pps = Math.Pow(10, generatorCount) * 0.01 * pointGain;
            labelGeneratorInfo.Text = $"Generators: {generatorCount} | Cost: {FormatNumbers(generatorCost)} | Points/second: {FormatNumbers(pps)}";
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
                BigDouble passiveGain = Math.Pow(10, generatorCount) * 0.01 * pointGain;

                point += passiveGain;

                labelPoint.Text = FormatNumbers(point);
                UpdateUI();
            }
        }
        private void buttonAscend_Click(object sender, EventArgs e)
        {
            var cost = GetAscendCost();
            if (point >= cost)
            {
                point = 0;
                pointGain = 1.0;
                upgradeCount = 0;
                prestigeCount = 0;
                ascendCount++;
                ascensionPoints++;
                UnlockAscensionFeature();
                UpdateUI();
            }
        }
        private BigDouble GetUpgradeCost()
        {
            return baseUpgradeCost * BigDouble.Pow(upgradeScale, upgradeCount);
        }

        private BigDouble GetPrestigeCost()
        {
            return basePrestigeCost * BigDouble.Pow(prestigeScale, prestigeCount);
        }

        private BigDouble GetAscendCost()
        {
            return baseAscendCost * BigDouble.Pow(ascendScale, ascendCount);
        }

        //offline progress
        private void SaveGame()
        {
            var state = new GameState
            {
                Point = point,
                UpgradeCount = upgradeCount,
                PointMultiplier = pointGain,
                PrestigeCount = prestigeCount,
                GeneratorCost = generatorCost,
                GeneratorCount = generatorCount,
                AscensionPoints = ascensionPoints,
                AscensionCount = ascendCount,
                CooldownDuration = cooldownDuration,
                LastSavedTime = DateTime.Now,

                HasUnlockedPrestige = buttonPrestige.Visible,
                HasUnlockedGenerators = buttonGenerator.Visible,
                HasUnlockedAscension = buttonAscend.Visible,
                HasAscended = buttonOpenAscensionShop.Visible,
            };

            var settings = new JsonSerializerSettings();
            settings.Converters.Add(new BigDoubleConverter());

            Directory.CreateDirectory(Path.GetDirectoryName(savePath));
            string json = JsonConvert.SerializeObject(state, settings);
            File.WriteAllText(savePath, json);
        }
        private void ApplyUnlocks(GameState state)
        {
            buttonPrestige.Visible = state.HasUnlockedPrestige;
            labelPrestigeCost.Visible = state.HasUnlockedPrestige;
            labelUpgradeNote.Visible = !state.HasUnlockedPrestige;

            buttonGenerator.Visible = state.HasUnlockedGenerators;
            labelGeneratorInfo.Visible = state.HasUnlockedGenerators;
            labelSoftCap.Visible = state.HasUnlockedGenerators;
            labelPrestigeInfo.Visible = state.HasUnlockedGenerators;

            buttonAscend.Visible = state.HasUnlockedAscension;
            labelAscendCost.Visible = state.HasUnlockedAscension;

            buttonOpenAscensionShop.Visible = state.HasAscended;
        }
        private void LoadGame()
        {
            try
            {
                if (!File.Exists(savePath))
                {
                    SaveGame(); // Create a fresh save file
                    return;
                }

                string json = File.ReadAllText(savePath);
                if (string.IsNullOrWhiteSpace(json))
                {
                    SaveGame(); // Handle empty file
                    return;
                }

                var settings = new JsonSerializerSettings();
                settings.Converters.Add(new BigDoubleConverter());

                GameState state = JsonConvert.DeserializeObject<GameState>(json, settings);
                if (state == null)
                    throw new Exception("Deserialized GameState is null");

                point = state.Point;
                pointGain = state.PointMultiplier;
                upgradeCount = state.UpgradeCount;
                prestigeCount = state.PrestigeCount;
                generatorCost = state.GeneratorCost;
                generatorCount = state.GeneratorCount;
                ascendCount = state.AscensionCount;
                ascensionPoints = state.AscensionPoints;
                cooldownDuration = state.CooldownDuration;

                if (cooldownDuration == 500)
                {
                    UnlockPrestigeFeature();
                }

                ApplyOfflineProgress(state.LastSavedTime);
                ApplyUnlocks(state);
                UpdateUI();
            }
            catch (Exception ex)
            {
                LogCrash(ex);
                MessageBox.Show(
                    "Your save file is missing or corrupted. A new save has been created.",
                    "Load Error",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Warning
                );

                // Reset to defaults and create a fresh save
                point = BigDouble.Zero;
                pointGain = new BigDouble(1);
                generatorCost = new BigDouble(100);
                generatorCount = 0;
                ascensionPoints = BigDouble.Zero;
                cooldownDuration = 1000;
                SaveGame();
                UpdateUI();
            }
        }

        private void UnlockPrestigeFeature()
        {
            buttonPrestige.Visible = true;
            labelPrestigeCost.Visible = true;
            labelUpgradeNote.Visible = false;

            // Immediately persist
            SaveGame();
        }
        private void UnlockGeneratorFeature()
        {
            buttonGenerator.Visible = true;
            labelGeneratorInfo.Visible = true;
            labelSoftCap.Visible = true;
            buttonAscend.Visible = true;
            labelAscendCost.Visible = true;
            labelPrestigeInfo.Visible = true;
            SaveGame();
        }

        private void UnlockAscensionFeature()
        {
            buttonOpenAscensionShop.Visible = true;
            SaveGame();
        }
        //soft cap
        private BigDouble GetSoftCapDivisor(BigDouble point)
        {
            // No cap below 1 000
            if (point <= 1_000)
                return BigDouble.One;

            // “Decades” above 10^3: e.g. point=2e6 → logDec≈3.3010; 
            // at 1e4→1.0, 1e5→2.0, etc.
            double logDec = (double)(BigDouble.Log10(point) - 3.0);

            // Compute Gamma(logDec + 1) for a smooth factorial
            double gammaValue = LanczosGamma(logDec + 1.0);

            // Convert back to BigDouble
            return new BigDouble(gammaValue + 1.0);
        }

        // Lanczos approximation for Gamma(z)
        private static double LanczosGamma(double z)
        {
            // Coefficients for g=7, n=9 (Robust for z > 0)
            double[] p = {
         0.99999999999980993,
        676.5203681218851,
       -1259.1392167224028,
        771.32342877765313,
       -176.61502916214059,
         12.507343278686905,
         -0.13857109526572012,
          9.9843695780195716e-6,
          1.5056327351493116e-7
    };

            if (z < 0.5)
            {
                // Reflection for z < 0.5
                return Math.PI / (Math.Sin(Math.PI * z) * LanczosGamma(1.0 - z));
            }

            z -= 1.0;
            double x = p[0];
            for (int i = 1; i < p.Length; i++)
                x += p[i] / (z + i);

            double t = z + p.Length - 0.5;
            return Math.Sqrt(2 * Math.PI) * Math.Pow(t, z + 0.5) * Math.Exp(-t) * x;
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
            // Calculate how long the player was away
            TimeSpan offlineTime = DateTime.Now - lastSaved;
            BigDouble seconds = offlineTime.TotalSeconds;
            if (generatorCount <= 0 || seconds <= 0)
            {
                if (prestigeCount==0)
                MessageBox.Show(
              $"Welcome back! You currently don't own any generator for offline progress. Unlock it after your first prestige!");
                else
                    MessageBox.Show(
                  $"Welcome back! You currently don't own any generator for offline progress.");
                return;
            };

            // Apply the soft cap divisor as of the starting point
            BigDouble divisor = GetSoftCapDivisor(point);
            BigDouble ratePerSecond = BigDouble.Pow(10, generatorCount)
                                      * 0.01
                                      * pointGain
                                      * offlineMultiplier
                                      / divisor;

            BigDouble passiveGain = ratePerSecond * seconds;
            point += passiveGain;

            MessageBox.Show(
              $"Welcome back! You earned {FormatNumbers(passiveGain)} points while you were away.\n" +
              $"Current offline multi: x{offlineMultiplier}."
            );

            UpdateUI();    // Refresh labels/buttons to reflect the new point total
            SaveGame();    // Persist the updated state immediately
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
            pointGain *= 10;
            button1.Text = $"+{((BigDouble)pointGain).ToString("F1")} points";
            UpdateUI();
        }
#endif

        private void buttonOpenAscensionShop_Click(object sender, EventArgs e)
        {
            // Pass current ascension points to the shop
            AscensionShop shop = new AscensionShop(ascensionPoints);
            shop.ShowDialog(); // Opens the shop as a modal window
        }
        private void LogCrash(Exception ex)
        {
            string crashLogPath = Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
                "FnuyIncrementalGame",
                "crashlog.txt"
            );

            Directory.CreateDirectory(Path.GetDirectoryName(crashLogPath));

            string log = $"[{DateTime.Now}] {ex.GetType().Name}: {ex.Message}\n{ex.StackTrace}\n\n";
            File.AppendAllText(crashLogPath, log);
        }
    }
}