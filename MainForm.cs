using BreakInfinity;
using Newtonsoft.Json;
using System.IO;
using System.Net.Http;
using System.Reflection;
using System.Threading.Tasks;
using System.Globalization;
using static System.Windows.Forms.VisualStyles.VisualStyleElement.TaskbarClock;

namespace WinFormsApp1
{
    public partial class MainForm : Form
    {
        // Autoclick progressbar
        private Panel autoclickBarBg;
        private Panel autoclickBarFill;
        private readonly Point autoclickBarLocation = new Point(188, 118);
        private readonly Size autoclickBarSize = new Size(100, 23);
        // Core game state
        private BigDouble point = new BigDouble(0);
        private BigDouble pointGain = BigDouble.One;
        private BigDouble PrestigeIncrement = new BigDouble(4);
        private BigDouble generatorCost = new BigDouble(100);
        private BigDouble ascensionPoints = BigDouble.Zero;
        private readonly BigDouble softcapThreshold = new BigDouble(100_000);
        private System.Windows.Forms.Timer transcendFlashTimer;
        private int transcendFlashStep = 0;
        private readonly BigDouble transcendCost = new BigDouble(1_000_000_000);
        // Premium currency
        private bool hasUnlockedPremiumShop = false;
        private BigDouble milk = BigDouble.Zero;
        private DateTime lastMilkClaimDate = DateTime.MinValue;
        private int baseMilkUpgradeCount = 0;
        private int milkStreak = 0;
        private BigDouble[] milkSpent = new BigDouble[3];
        // Base costs and scaling factors
        private readonly BigDouble baseUpgradeCost = new BigDouble(10);
        private readonly double upgradeScale = 1.05;
        private readonly BigDouble basePrestigeCost = new BigDouble(1000);
        private readonly double prestigeScale = 3;
        private readonly BigDouble baseAscendCost = new BigDouble(1_000_000);
        private readonly double ascendScale = 2;
        // Purchase counts (persist these, not the cost)
        private int upgradeCount = 0;
        private int prestigeCount = 0;
        private int ascendCount = 0;
        private int generatorCount = 0;
        private bool[] challengesCompleted = new bool[4];
        private int PermanentUpgradeBonus => challengesCompleted != null && challengesCompleted.Length > 0 && challengesCompleted[0] ? 5 : 0;
        private int EffectiveUpgradeCount => upgradeCount + PermanentUpgradeBonus;

        // Timers and cooldowns
        private System.Windows.Forms.Timer generatorTimer;
        private System.Windows.Forms.Timer cooldownTimer;
        private System.Windows.Forms.Timer autoClickTimer;
        private int cooldownDuration = 1000; // ms
        private int? prevCooldownDurationForChallenge; // snapshot to restore after challenge
        private int cooldownElapsed = 0;
        private bool isCooldown = false;
        private int autoClickElapsed = 0; // ms

        // Save path
        private readonly string savePath = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
            "FnuyIncrementalGame",
            "savegame.json");

        private int activeChallengeIndex = -1;
        //debug
        public MainForm()
        {
            InitializeComponent();
            autoclickBarBg = new Panel
            {
                BackColor = Color.FromArgb(48, 48, 48),
                Location = autoclickBarLocation,
                Size = autoclickBarSize,
                Visible = false
            };
            autoclickBarFill = new Panel
            {
                BackColor = Color.LimeGreen,
                Location = new Point(0, 0),
                Size = new Size(0, autoclickBarBg.Height),
                Visible = true
            };
            autoclickBarBg.Controls.Add(autoclickBarFill);
            Controls.Add(autoclickBarBg);
            autoclickBarBg.BringToFront();

            autoClickTimer = new System.Windows.Forms.Timer();
            autoClickTimer.Tick += AutoClickTimer_Tick;
            autoClickTimer.Enabled = false;
#if DEBUG
            buttonDebug.Visible = true;
#else
        buttonDebug.Visible = false;
#endif
            this.Text = "Myrtle incremental";
            this.Icon = Properties.Resources.NianBean;

            //Transcend flash timer
            transcendFlashTimer = new System.Windows.Forms.Timer();
            transcendFlashTimer.Interval = 200;
            transcendFlashTimer.Tick += TranscendFlashTimer_Tick;

            // Cooldown for click
            cooldownTimer = new System.Windows.Forms.Timer { Interval = 50 };
            cooldownTimer.Tick += CooldownTimer_Tick;

            // Timer for point generator
            generatorTimer = new System.Windows.Forms.Timer { Interval = 1000 };
            generatorTimer.Tick += GeneratorTimer_Tick;
            generatorTimer.Start();

            // Hide locked features by default
            buttonPrestige.Visible = labelPrestigeCost.Visible = labelPrestigeInfo.Visible = false;
            buttonGenerator.Visible = labelGeneratorInfo.Visible = labelSoftCap.Visible = false;
            buttonAscend.Visible = labelAscendCost.Visible = buttonOpenAscensionShop.Visible = false;
            buttonTranscend.Visible = labelTranscendCost.Visible = buttonPremiumShop.Visible = false;
            labelChallengeState.Visible = buttonInfoDailyGain.Visible = false;
#if DEBUG
            buttonDebug.Visible = true;
            buttonDebug.Click += buttonDebug_Click;
#endif

            LoadGame();
            labelCooldown.Text = "";
            CheckForUpdates();
            UpdateUI();
        }

        private async Task<DateTime?> GetServerUtcDateAsync()
        {
            try
            {
                using var client = new HttpClient();
                // Use a reliable time API with good connectivity in Vietnam (Google's time API via HTTP HEAD)
                var response = await client.SendAsync(new HttpRequestMessage(HttpMethod.Head, "https://www.google.com"));
                if (response.Headers.Date.HasValue)
                    return response.Headers.Date.Value.UtcDateTime;
            }
            catch
            {
                // Network or API error
            }
            return null;
        }
        private async void CheckForUpdates()
        {
            string manifestUrl = "https://hello23241.github.io/fnuy-incremental-manifest/manifest.json";
            using var client = new HttpClient();
            try
            {
                string json = await client.GetStringAsync(manifestUrl);
                if (string.IsNullOrWhiteSpace(json))
                    throw new Exception("Manifest response was empty.");

                dynamic manifest = JsonConvert.DeserializeObject(json);
                if (manifest == null)
                    throw new Exception("Manifest deserialization failed.");

                string latestVersion = manifest.latestVersion;
                string downloadUrl = manifest.downloadUrl;
                string changelog = manifest.changelog;
                string releaseDateStr = manifest.releaseDate;

                DateTime releaseDate;
                if (!DateTime.TryParseExact(releaseDateStr, new[] { "yyyyMMdd", "yyyy-MM-dd" }, null, System.Globalization.DateTimeStyles.None, out releaseDate))
                    releaseDate = DateTime.MinValue;

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
            // Batch UI layout updates for performance
            this.SuspendLayout();

            // Recalculate point gain first so all formatted values are up-to-date
            RecalculatePointGain();

            // Cache expensive calculations and costs
            var softCapDivisor = GetSoftCapDivisor(point);
            var upgradeCost = GetUpgradeCost();
            var prestigeCost = GetPrestigeCost();
            var ascendCost = GetAscendCost();

            var formattedPoint = FormatNumbers(point);
            var formattedGain = FormatNumbers(pointGain / softCapDivisor);
            var formattedUpgradeCost = FormatNumbers(upgradeCost);
            var formattedPrestigeCost = FormatNumbers(prestigeCost);
            var formattedAscendCost = FormatNumbers(ascendCost);
            var formattedPointGain = FormatNumbers(pointGain);
            var formattedTranscendCost = FormatNumbers(transcendCost);
            var formattedMilk = FormatNumbers(milk);

            UpdateButtonStates();
            UpdateUpgradeInfoLabel();
            UpdateGeneratorInfo();
            UpdateSoftCapLabel();
            UpdateGeneratorTimerInterval();

            // Only update if changed
            if (labelPoint.Text != $"Points: {formattedPoint}")
                labelPoint.Text = $"Points: {formattedPoint}";
            if (button1.Text != $"+{formattedGain} points")
                button1.Text = $"+{formattedGain} points";
            if (labelUpgradeCost.Text != $"Upgrade Cost: {formattedUpgradeCost}")
                labelUpgradeCost.Text = $"Upgrade Cost: {formattedUpgradeCost}";
            if (labelPrestigeCost.Text != $"Prestige Cost: {formattedPrestigeCost}")
                labelPrestigeCost.Text = $"Prestige Cost: {formattedPrestigeCost}";
            if (labelAscendCost.Text != $"Ascend Cost: {formattedAscendCost}")
                labelAscendCost.Text = $"Ascend Cost: {formattedAscendCost}";
            if (labelPointGain.Text != $"Point Gain: {formattedPointGain}")
                labelPointGain.Text = $"Point Gain: {formattedPointGain}";
            if (labelTranscendCost.Text != $"Transcend Cost: {formattedTranscendCost}")
                labelTranscendCost.Text = $"Transcend Cost: {formattedTranscendCost}";
            if (buttonPremiumShop.Text != $"🥛 {formattedMilk}")
                buttonPremiumShop.Text = $"🥛 {formattedMilk}";

            if (upgradeCount != 0)
            {
                string upgradeNoteText = $"Upgrade count: {EffectiveUpgradeCount}";
                if (!labelUpgradeNote.Visible)
                    labelUpgradeNote.Visible = true;
            }
            else
            {
                if (labelUpgradeNote.Visible)
                    labelUpgradeNote.Visible = false;
            }

            // Transcend button logic
            bool canTranscend = point >= transcendCost;
            if (buttonTranscend.Enabled != canTranscend)
                buttonTranscend.Enabled = canTranscend;
            if (canTranscend)
            {
                if (!transcendFlashTimer.Enabled)
                    transcendFlashTimer.Start();
            }
            else
            {
                if (transcendFlashTimer.Enabled)
                    transcendFlashTimer.Stop();
                if (buttonTranscend.BackColor != Color.Gray)
                    buttonTranscend.BackColor = Color.Gray; // Match other disabled buttons
            }

            // Enable auto-click if ascendCount >= 2
            bool autoClickEnabled = ascendCount >= 2;
            int autoClickInterval = EffectiveCooldownDuration * 2;
            if (autoClickTimer.Enabled != autoClickEnabled)
                autoClickTimer.Enabled = autoClickEnabled;
            if (autoClickTimer.Interval != 50)
                autoClickTimer.Interval = 50;

            // Autoclick progress bar logic (custom bar replaces native ProgressBar)
            autoclickBarBg.Visible = autoClickEnabled;
            if (autoClickEnabled)
            {
                autoclickBarFill.Height = autoclickBarBg.Height;

                int percent = (int)(100L * autoClickElapsed / autoClickInterval);
                if (percent < 0) percent = 0;
                if (percent > 100) percent = 100;
                UpdateAutoclickBar(percent);
            }
            // Challenge state
            if (activeChallengeIndex == -1)
            {
                if (labelChallengeState.Visible)
                    labelChallengeState.Visible = false;
            }
            else
            {
                string challengeText = $"Challenge {activeChallengeIndex + 1} active";
                switch (activeChallengeIndex)
                {
                    case 0:
                        challengeText += "\nPoint gain /10";
                        break;
                    case 1:
                        challengeText += "\nPrestige effectiveness /2";
                        break;
                    case 2:
                        challengeText += "\nPoint gain interval 10s";
                        break;
                    case 3:
                        challengeText += "\nReduced point gain, prestige effectiveness and increased cooldown";
                        break;
                }
                if (!labelChallengeState.Visible)
                    labelChallengeState.Visible = true;
                if (labelChallengeState.Text != challengeText)
                    labelChallengeState.Text = challengeText;
                // Only set font if not already bold
                if (labelChallengeState.Font.Style != FontStyle.Bold)
                    labelChallengeState.Font = new Font(labelChallengeState.Font, FontStyle.Bold);
            }

            this.ResumeLayout();
        }
        private void UpdateButtonStates()
        {
            buttonUpgrade.Enabled = point >= GetUpgradeCost();
            buttonPrestige.Enabled = point >= GetPrestigeCost();
            buttonAscend.Enabled = point >= GetAscendCost();
            buttonGenerator.Enabled = point >= generatorCost;

            buttonUpgrade.BackColor = buttonUpgrade.Enabled ? Color.LightGreen : Color.Gray;
            buttonPrestige.BackColor = buttonPrestige.Enabled ? Color.LightBlue : Color.Gray;
            buttonAscend.BackColor = buttonAscend.Enabled ? Color.MediumPurple : Color.Gray;
            buttonGenerator.BackColor = buttonGenerator.Enabled ? Color.LightGreen : Color.Gray;
        }
        private void UpdateGeneratorTimerInterval()
        {
            int interval = 1000;
            if (challengesCompleted != null && challengesCompleted.Length > 2 && challengesCompleted[2])
                interval -= 100; // 0.1s faster

            // Challenge 2: increase to 10 seconds
            if (activeChallengeIndex == 2 || activeChallengeIndex == 3)
                interval = 10000;

            generatorTimer.Interval = Math.Max(interval, 100);
        }
        private void Button1_Click(object sender, EventArgs e)
        {
            if (isCooldown) return;
            BigDouble gain = pointGain * AscensionMultiplier;
            gain = ApplySoftCap(point, gain);
            point += gain;
            UpdateUI();
            isCooldown = true;
            cooldownElapsed = 0;
            cooldownTimer.Interval = 50; // Always 50ms for smooth updates
            cooldownTimer.Start();
            labelCooldown.Text = $"Cooldown: {EffectiveCooldownDuration / 1000.0:F2}s";
            CheckChallengeCompletion();
        }
        private void CooldownTimer_Tick(object sender, EventArgs e)
        {
            cooldownElapsed += cooldownTimer.Interval;
            int remaining = EffectiveCooldownDuration - cooldownElapsed;
            if (remaining > 0)
            {
                labelCooldown.Text = $"Cooldown: {remaining / 1000.0:F2}s";
            }
            else
            {
                cooldownTimer.Stop();
                isCooldown = false;
                cooldownElapsed = 0;
                labelCooldown.Text = ""; // Clear when ready
            }
        }
        private void AutoClickTimer_Tick(object sender, EventArgs e)
        {
            int autoClickInterval = EffectiveCooldownDuration * 2;
            autoClickElapsed += autoClickTimer.Interval;
            // Update custom progress bar
            if (autoclickBarBg.Visible)
            {
                int percent = (int)(100L * autoClickElapsed / autoClickInterval);
                if (autoClickElapsed >= autoClickInterval)
                {
                    UpdateAutoclickBar(100); // show full before reset
                    BigDouble gain = pointGain * AscensionMultiplier;
                    gain = ApplySoftCap(point, gain);
                    point += gain;
                    autoClickElapsed = 0;
                    UpdateUI();
                }
                else
                {
                    if (percent < 0) percent = 0;
                    if (percent > 100) percent = 100;
                    UpdateAutoclickBar(percent);
                }
            }
        }
        private void RecalculatePointGain()
        {
            BigDouble divisor = GetSoftCapDivisor(point);
            if (divisor == 0)
            {
                pointGain = BigDouble.Zero;
                point = BigDouble.Min(point, GetSoftCapThreshold() * 10_000);
                return;
            }
            double prestigeEffect = GetPrestigeEffect();
            double challenge0Multi = (challengesCompleted != null && challengesCompleted.Length > 0 && challengesCompleted[0]) ? 3 : 1.0;
            // Challenge 0: Point gain is divided by 10
            double challengeDebuff = 1.0;
            if (activeChallengeIndex == 0 || activeChallengeIndex == 3)
                challengeDebuff /= 10.0;

            pointGain = (BigDouble.One + EffectiveUpgradeCount * (1 + EffectiveUpgradeCount * prestigeEffect * GetPrestigeIncrement() / 100 / divisor))
                * challenge0Multi * challengeDebuff *
                (BigDouble.One + milkSpent[0] * 0.0001);
        }
        private BigDouble GetPrestigeIncrement()
        {
            double multiplier = (challengesCompleted != null && challengesCompleted.Length > 1 && challengesCompleted[1]) ? 1.1 : 1.0;
            return PrestigeIncrement * multiplier;
        }
        private void buttonUpgrade_Click(object sender, EventArgs e)
        {
            var cost = GetUpgradeCost();
            if (point >= cost)
            {
                point -= cost;
                upgradeCount++;
                if (cooldownDuration == 1000)
                {
                    UnlockPrestigeFeature();
                    cooldownDuration = 500;
                }
                RecalculatePointGain();
                UpdateUI();
            }
        }

        // Right-click handler: if player has >=5 ascensions, buy max upgrades on right-click
        private void buttonUpgrade_MouseDown(object sender, MouseEventArgs e)
        {
            if (e.Button != MouseButtons.Right)
                return;

            if (ascendCount < 5)
                return; // feature unlocks at 5 ascensions

            // Buy as many upgrades as possible
            bool purchasedAny = false;
            while (true)
            {
                var cost = GetUpgradeCost();
                if (point < cost)
                    break;
                point -= cost;
                upgradeCount++;
                purchasedAny = true;
                // Prevent infinite loop in case GetUpgradeCost misbehaves
                // (cost should increase each iteration)
            }

            if (purchasedAny)
            {
                if (cooldownDuration == 1000)
                {
                    UnlockPrestigeFeature();
                    cooldownDuration = 500;
                }
                RecalculatePointGain();
                UpdateUI();
                SaveGame();
            }
        }

        private void UpdateUpgradeInfoLabel()
        {
            double prestigeEffect = GetPrestigeEffect();
            BigDouble divisor = GetSoftCapDivisor(point);
            BigDouble gainPerUpgrade = (1 + EffectiveUpgradeCount * prestigeEffect * GetPrestigeIncrement() / 100 / divisor)
                * (1 + milkSpent[0] * 0.0001);

            labelUpgradeInfo.Text = $"each upgrade adds {FormatNumbers(gainPerUpgrade)} to your click multiplier";

            // Show correct log base in prestige info
            double logBase = 2.0;
            string baseText = "₂";
            string extraText = "";
            if (challengesCompleted != null && challengesCompleted.Length > 1 && challengesCompleted[1])
                extraText = " (multiplied by 1.1)";
            if (challengesCompleted != null && challengesCompleted.Length > 2 && challengesCompleted[2])
            {
                logBase = 1.9;
                baseText = "_{1.9}";
                extraText = ""; // No multiplier for challenge 2
            }

            labelPrestigeInfo.Text = $"Prestige effect: log{baseText}({prestigeCount + 1}) = {prestigeEffect:F2}{extraText} (diminishing returns)";
        }

        private void buttonPrestige_Click(object sender, EventArgs e)
        {
            var cost = GetPrestigeCost();
            if (point >= cost)
            {
                point = BigDouble.Zero;
                upgradeCount = 0;
                pointGain = BigDouble.One;
                prestigeCount++;
                UnlockGeneratorFeature();
                UpdateUI();
                CheckChallengeCompletion();
            }
        }
        private double GetPrestigeEffect()
        {
            double logBase = 2.0;
            // Challenge 2 completed: use log base 1.9 instead of 2
            if (challengesCompleted != null && challengesCompleted.Length > 2 && challengesCompleted[2])
                logBase = 1.9;

            double effect = BigDouble.Log(prestigeCount + 1, logBase);

            // Challenge 1: Prestige effectiveness halved
            if (activeChallengeIndex == 1 || activeChallengeIndex == 3)
                effect /= 2.0;

            return effect;
        }
        private void UpdateGeneratorInfo()
        {
            if (generatorCount == 0)
                labelGeneratorInfo.Text = $"Generators: 0 | Cost: 100";
            else
            {
                BigDouble divisor = GetSoftCapDivisor(point);
                BigDouble pps = Math.Pow(10, generatorCount) * 0.01 * pointGain / divisor;
                labelGeneratorInfo.Text = $"Generators: {generatorCount} | Cost: {FormatNumbers(generatorCost)} | Every generators 10x your current passive gain after the first";
                labelPointsPerSecond.Text = $"Points/second: {FormatNumbers(pps)}";
            }
        }

        private void buttonGenerator_Click(object sender, EventArgs e)
        {
            if (point >= generatorCost)
            {
                point -= generatorCost;
                generatorCount++;
                generatorCost = BigDouble.Pow(generatorCost, 2);
                labelPoint.Text = FormatNumbers(point);
                UpdateUI();
                CheckChallengeCompletion();
            }
        }

        private void GeneratorTimer_Tick(object sender, EventArgs e)
        {
            if (generatorCount > 0)
            {
                labelPointsPerSecond.Visible = true;
                BigDouble passiveGain = Math.Pow(10, generatorCount) * 0.01 * pointGain * AscensionMultiplier;
                passiveGain = ApplySoftCap(point, passiveGain);
                point += passiveGain;
                labelPoint.Text = FormatNumbers(point);
                UpdateUI();
            }
            else
                labelPointsPerSecond.Visible = false;
        }

        private void buttonAscend_Click(object sender, EventArgs e)
        {
            var cost = GetAscendCost();
            if (point >= cost)
            {
                point = BigDouble.Zero;
                pointGain = BigDouble.One;
                upgradeCount = 0;
                prestigeCount = 0;
                ascendCount++;
                ascensionPoints++;
                UnlockAscensionFeature();
                UpdateUI();
            }
        }

        private BigDouble GetUpgradeCost() => baseUpgradeCost * BigDouble.Pow(upgradeScale, upgradeCount);
        private BigDouble GetPrestigeCost() => basePrestigeCost * BigDouble.Pow(prestigeScale, prestigeCount);
        private BigDouble GetAscendCost()
        {
            if (ascendCount <= 2)
                return baseAscendCost;
            return baseAscendCost * BigDouble.Pow(ascendScale, ascendCount - 2);
        }

        // Soft cap
        private BigDouble GetSoftCapDivisor(BigDouble point)
        {
            BigDouble divisor = BigDouble.One;

            if (milkSpent != null && milkSpent.Length > 1 && milkSpent[1] > BigDouble.Zero)
                divisor /= (BigDouble.Min(BigDouble.Log(milkSpent[1], 1.1), milkSpent[1]) * 0.01 + BigDouble.One);

            var threshold = GetSoftCapThreshold();
            var threshold10000 = threshold * 10_000;

            if (point <= threshold)
            {
                return BigDouble.One * divisor;
            }
            else if (point <= threshold10000)
            {
                // Linear scaling: divisor = point / threshold
                var linearDivisor = point / threshold;
                return linearDivisor * divisor;
            }
            else
            {
                return 0; // Hard cap: gains are disabled
            }
        }
        private BigDouble GetSoftCapThreshold()
        {
            if (challengesCompleted != null && challengesCompleted.Length > 3 && challengesCompleted[3])
                return new BigDouble(1_000_000); // Challenge 3: lifted cap
            return softcapThreshold;
        }
        private void UpdateSoftCapLabel()
        {
            var threshold = GetSoftCapThreshold();
            var threshold10000 = threshold * 10_000;
            BigDouble divisor = GetSoftCapDivisor(point);

            if (point >= threshold10000)
            {
                // Hard cap: gains are disabled
                labelSoftCap.Visible = true;
                labelSoftCap.Text = $"Current points exceed {FormatNumbers(threshold10000)} (10k× softcap). All gain is disabled by the soft cap.";
            }
            else if (point > threshold)
            {
                labelSoftCap.Visible = true;
                labelSoftCap.Text = $"Current points is over {FormatNumbers(threshold)}, gain is divided by {FormatNumbers(divisor)}";
            }
            else
            {
                labelSoftCap.Visible = false;
            }
        }
        private string FormatNumbers(BigDouble value)
        {
            if (value >= BigDouble.Pow(10, 308))
                return value.ToString("E1");

            string[] suffixes = {
                "", "K", "M", "B", "T", "Qa", "Qi", "Sx", "Sp", "Oc", "No",
                "Dc", "Ud", "Dd", "Td", "Qad", "Qid", "Sxd", "Spd", "Ocd", "Nod", "Vg",
                "Uvg", "Dvg", "Tvg", "Qavg", "Qivg", "Sxvg", "Spvg", "Ocvg", "Novg", "Tg",
                "Utg", "Dtg", "Ttg", "Qatg", "Qitg", "Sxtg", "Sptg", "Octg", "Notg", "Qag",
                "Uqag", "Dqag", "Tqag", "Qaqag", "Qiqag", "Sxqag", "Spqag", "Ocqag", "Noqag", "Qig",
                "Uqig", "Dqig", "Tqig", "Qaqig", "Qiqig", "Sxqig", "Spqig", "Ocqig", "Noqig", "Sxg",
                "Usxg", "Dsxg", "Tsxg", "Qasxg", "Qisxg", "Sxsxg", "Spsxg", "Ocsxg", "Nosxg", "Spg",
                "Uspg", "Dspg", "Tspg", "Qaspg", "Qispg", "Sxspg", "Spspg", "Ocspg", "Nospg", "Ocg",
                "Uocg", "Docg", "Tocg", "Qaocg", "Qiocg", "Sxocg", "Spocg", "Ococg", "Noocg", "Nog",
                "Unog", "Dnog", "Tnog", "Qanog", "Qinog", "Sxnog", "Spnog", "Ocnog", "Nonog", "C"
            };

            int suffixIndex = 0;
            while (value >= 1000 && suffixIndex < suffixes.Length - 1)
            {
                value /= 1000;
                suffixIndex++;
            }
            return $"{value:F1}{suffixes[suffixIndex]}";
        }

        private void ApplyOfflineProgress(DateTime lastSaved, DateTime serverNow)
        {
            TimeSpan offlineTime = serverNow - lastSaved;
            int seconds = (int)offlineTime.TotalSeconds;
            if (seconds <= 0)
            {
                // If time difference is negative or zero, skip offline progress
                return;
            }
            int trueSeconds = Math.Min((int)BigDouble.Log(seconds, 1.01), seconds);
            if (generatorCount <= 0)
            {
                if (prestigeCount == 0)
                    MessageBox.Show("Welcome back! You currently don't own any generator for offline progress. Unlock it after your first prestige!");
                else
                    MessageBox.Show("Welcome back! You currently don't own any generator for offline progress.");
                return;
            }
            double offlineMultiplier = 0.05 + milkSpent[2].ToDouble() * 0.001;
            BigDouble ratePerSecond = BigDouble.Pow(10, generatorCount) * 0.01 * pointGain * offlineMultiplier;
            BigDouble passiveGain = ratePerSecond * trueSeconds;
            passiveGain = ApplySoftCap(point, passiveGain);
            point += passiveGain;

            MessageBox.Show(
                $"Welcome back! You earned {FormatNumbers(passiveGain)} points while you were away for {seconds}s.\n" +
                $"Effective time was {trueSeconds}s\n" +
                $"Current offline multi: x{offlineMultiplier}.",
                "Offline progress"
            );

            UpdateUI();
            SaveGame();
        }

        // Save on close
        protected override void OnFormClosing(FormClosingEventArgs e)
        {
            SaveGame();
            MessageBox.Show("Your progress has been saved!", "Game Saved", MessageBoxButtons.OK, MessageBoxIcon.Information);
            base.OnFormClosing(e);
        }

#if DEBUG
        private void buttonDebug_Click(object sender, EventArgs e)
        {
            milk *= 10000000000000000000;
            SaveGame();
            UpdateUI();
        }
#endif
        private void buttonOpenAscensionShop_Click(object sender, EventArgs e)
        {
            int previousActiveChallengeIndex = activeChallengeIndex;
            var challengeWindow = new AscensionWindow(ascendCount, challengesCompleted, activeChallengeIndex);
            var result = challengeWindow.ShowDialog();
            if (result == DialogResult.OK)
            {
                challengesCompleted = challengeWindow.GetChallengesCompleted();
                SaveGame();
                UpdateUI();

                // Cancel challenge if requested
                if (challengeWindow.ChallengeCancelled)
                {
                    activeChallengeIndex = -1;
                    UpdateUI();
                    SaveGame();
                    MessageBox.Show("Challenge cancelled.", "Challenge", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    return;
                }
                // Only start a challenge if a new one was started
                if (challengeWindow.ChallengeActive && challengeWindow.ActiveChallengeIndex != previousActiveChallengeIndex)
                {
                    StartChallenge(challengeWindow.ActiveChallengeIndex);
                }
            }
        }
        // Protect/unprotect helpers (DPAPI)
        private byte[] ProtectBytes(byte[] data)
        {
            try
            {
                var entropy = System.Text.Encoding.UTF8.GetBytes("FnuyIncrementalGame_v1"); // versioned entropy
                return System.Security.Cryptography.ProtectedData.Protect(data, entropy, System.Security.Cryptography.DataProtectionScope.CurrentUser);
            }
            catch
            {
                return null;
            }
        }

        private byte[] UnprotectBytes(byte[] data)
        {
            try
            {
                var entropy = System.Text.Encoding.UTF8.GetBytes("FnuyIncrementalGame_v1");
                return System.Security.Cryptography.ProtectedData.Unprotect(data, entropy, System.Security.Cryptography.DataProtectionScope.CurrentUser);
            }
            catch
            {
                return null;
            }
        }

        // SaveGame — serialize then protect before writing
        private void SaveGame()
        {
#if DEBUG
    // Do not save the game in debug build
    return;
#endif
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
                LastSavedTime = DateTime.UtcNow,
                HasUnlockedPrestige = buttonPrestige.Visible,
                HasUnlockedGenerators = buttonGenerator.Visible,
                HasUnlockedAscension = buttonAscend.Visible,
                HasAscended = buttonOpenAscensionShop.Visible,
                AscChallenges = challengesCompleted,
                HasUnlockedPremiumShop = hasUnlockedPremiumShop,
                Milk = milk,
                LastMilkClaimDate = lastMilkClaimDate,
                MilkStreak = milkStreak,
                BaseMilkUpgradeCount = baseMilkUpgradeCount,
                MilkSpent = milkSpent,
            };

            var settings = new JsonSerializerSettings();
            settings.Converters.Add(new BigDoubleConverter());

            string json = JsonConvert.SerializeObject(state, settings);
            byte[] plain = System.Text.Encoding.UTF8.GetBytes(json);
            byte[] cipher = ProtectBytes(plain);

            // If protection failed, fall back to writing plaintext (safer to preserve user data than to drop it)
            Directory.CreateDirectory(Path.GetDirectoryName(savePath));
            if (cipher != null)
                File.WriteAllBytes(savePath, cipher);
            else
                File.WriteAllText(savePath, json);
        }

        // LoadGame — try to unprotect, fallback to plaintext, then deserialize
        private async void LoadGame()
        {
            try
            {
                if (!File.Exists(savePath))
                {
                    SaveGame();
                    return;
                }

                byte[] fileBytes = File.ReadAllBytes(savePath);
                var settings = new JsonSerializerSettings();
                settings.Converters.Add(new BigDoubleConverter());

                string json = null;
                byte[] plain = UnprotectBytes(fileBytes);

                if (plain != null)
                {
                    // Successfully decrypted (new protected format)
                    json = System.Text.Encoding.UTF8.GetString(plain);
                }
                else
                {
                    // File is not a protected save (older plaintext format or corrupted).
                    // Per new policy: plaintext saves from earlier versions are now invalid.
                    // Back up the old file for manual recovery, inform the player, then create a fresh save.
                    try
                    {
                        // Make a deterministic backup name, avoid overwriting existing backups
                        string backupPath = savePath + ".plaintext-bak";
                        if (File.Exists(backupPath))
                            backupPath = savePath + $".plaintext-bak.{DateTime.UtcNow:yyyyMMddHHmmss}";

                        File.Move(savePath, backupPath);

                        MessageBox.Show(
                            "Your old data will NOT CARRY OVER in this version.\nThere's a special bonus waiting after prestiging\n\n" +
                            $"A backup of your old save has been created at:\n{backupPath}\n\n" +
                            "The game will start a fresh save now.",
                            "Haha get reset son",
                            MessageBoxButtons.OK,
                            MessageBoxIcon.Warning
                        );

                        // Create new protected save with default values
                        SaveGame();
                        return;
                    }
                    catch
                    {
                        // If backup fails for any reason, fall back to creating a fresh save to avoid leaving the app in an invalid state.
                        try
                        {
                            MessageBox.Show(
                                "An incompatible save was found and could not be backed up. The save will be reset to defaults.",
                                "Incompatible Save",
                                MessageBoxButtons.OK,
                                MessageBoxIcon.Warning
                            );
                        }
                        catch { }

                        SaveGame();
                        return;
                    }
                }

                GameState state = JsonConvert.DeserializeObject<GameState>(json, settings);
                if (state == null)
                    throw new Exception("Deserialized GameState is null");

                // Restore all game state variables first
                point = state.Point;
                pointGain = state.PointMultiplier;
                upgradeCount = state.UpgradeCount;
                prestigeCount = state.PrestigeCount;
                generatorCost = state.GeneratorCost;
                generatorCount = state.GeneratorCount;
                ascendCount = state.AscensionCount;
                ascensionPoints = state.AscensionPoints;
                cooldownDuration = state.CooldownDuration;
                hasUnlockedPremiumShop = state.HasUnlockedPremiumShop;
                milk = state.Milk;
                lastMilkClaimDate = state.LastMilkClaimDate;
                milkStreak = state.MilkStreak;
                baseMilkUpgradeCount = state.BaseMilkUpgradeCount;
                if (state.MilkSpent != null && state.MilkSpent.Length == 3)
                    milkSpent = state.MilkSpent;
                else
                    milkSpent = new BigDouble[3];
                if (state.AscChallenges != null && state.AscChallenges.Length == 4)
                    challengesCompleted = state.AscChallenges;
                else
                    challengesCompleted = new bool[4];

                if (cooldownDuration == 500)
                {
                    UnlockPrestigeFeature();
                }

                DateTime? serverTime = await GetServerUtcDateAsync();
                if (serverTime != null)
                    ApplyOfflineProgress(state.LastSavedTime, serverTime.Value);

                ApplyUnlocks(state, serverTime);
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

                point = BigDouble.Zero;
                pointGain = BigDouble.One;
                generatorCost = new BigDouble(100);
                generatorCount = 0;
                ascensionPoints = BigDouble.Zero;
                cooldownDuration = 1000;
                SaveGame();
                UpdateUI();
            }
        }
        private void ApplyUnlocks(GameState state, DateTime? serverTime)
        {
            buttonPrestige.Visible = state.HasUnlockedPrestige;
            labelPrestigeCost.Visible = state.HasUnlockedPrestige;
            labelUpgradeNote.Visible = !state.HasUnlockedPrestige;

            buttonGenerator.Visible = state.HasUnlockedGenerators;
            labelGeneratorInfo.Visible = state.HasUnlockedGenerators;
            labelSoftCap.Visible = state.HasUnlockedGenerators;
            labelPrestigeInfo.Visible = state.HasUnlockedGenerators;
            buttonPremiumShop.Visible = state.HasUnlockedGenerators;
            buttonInfoDailyGain.Visible = state.HasUnlockedGenerators;
            // After loading state.HasUnlockedGenerators
            if (state.HasUnlockedGenerators && serverTime != null)
            {
                var today = serverTime.Value.Date;
                var lastClaim = lastMilkClaimDate.Date;
                if (lastClaim < today)
                {
                    if (lastClaim == today.AddDays(-1))
                        milkStreak++;
                    else
                        milkStreak = 1;
                    int milkEarned = 90 + milkStreak * 10 + baseMilkUpgradeCount * 10;
                    milk += milkEarned;
                    lastMilkClaimDate = today;
                    MessageBox.Show($"You earned {milkEarned} milk for logging in today!\nBase gain: {baseMilkUpgradeCount * 10 + 100}\nStreak: {milkStreak} day(s)", "Daily Reward", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    SaveGame();
                }
            }

            buttonAscend.Visible = state.HasUnlockedAscension;
            labelAscendCost.Visible = state.HasUnlockedAscension;

            buttonOpenAscensionShop.Visible = buttonTranscend.Visible = labelTranscendCost.Visible = state.HasAscended;
        }
        private void UnlockPrestigeFeature()
        {
            buttonPrestige.Visible = true;
            labelPrestigeCost.Visible = true;
            labelUpgradeNote.Visible = false;
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
            buttonPremiumShop.Visible = true;
            buttonInfoDailyGain.Visible = true;
            // One-time premium shop unlock logic
            if (!hasUnlockedPremiumShop)
            {
                hasUnlockedPremiumShop = true;
                MessageBox.Show("Here's 10000 milk so you can progress faster in place of the lost data.\n"+
                    "I'd recommend spending 950 in offline gain, 1000 in softcap reduction and the rest in points gain",
                    "Progress loss compensation :)", MessageBoxButtons.OK, MessageBoxIcon.Information);
                milk += 10000;
                SaveGame();

                // Give today's milk daily reward
                var today = DateTime.Now.Date;
                var lastClaim = lastMilkClaimDate.Date;
                if (lastClaim < today)
                {
                    if (lastClaim == today.AddDays(-1))
                        milkStreak++;
                    else
                        milkStreak = 1;

                    int milkEarned = 90 + milkStreak * 10 + baseMilkUpgradeCount * 10;
                    milk += milkEarned;
                    lastMilkClaimDate = today;
                    MessageBox.Show($"You earned {milkEarned} milk for logging in today!\nBase gain: {baseMilkUpgradeCount * 10 + 100}\nStreak: {milkStreak} day(s)", "Daily Reward", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    SaveGame();
                }
            }

            SaveGame();
        }

        private void UnlockAscensionFeature()
        {
            buttonOpenAscensionShop.Visible = buttonTranscend.Visible = labelTranscendCost.Visible = true;
            SaveGame();
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
        private void TranscendFlashTimer_Tick(object sender, EventArgs e)
        {
            Color[] colors = { Color.Lime, Color.Cyan, Color.Yellow, Color.Magenta, Color.Orange, Color.Red };
            buttonTranscend.BackColor = colors[transcendFlashStep % colors.Length];
            transcendFlashStep++;
        }
        private void buttonTranscend_Click(object sender, EventArgs e)
        {
            if (point >= transcendCost)
            {
                point -= transcendCost;
                transcendFlashTimer.Stop();
                buttonTranscend.BackColor = Color.DarkViolet;

                // Reset everything like an ascension but then set ascend count back to 0
                point = BigDouble.Zero;
                pointGain = BigDouble.One;
                upgradeCount = 0;
                prestigeCount = 0;
                generatorCount = 0;
                generatorCost = new BigDouble(100);
                cooldownDuration = 1000;

                // Reset ascension-specific state
                ascendCount = 0;
                ascensionPoints = BigDouble.Zero;

                // Clear active challenge and completed challenges
                activeChallengeIndex = -1;
                challengesCompleted = new bool[4];

                // Milk-related data is NOT affected; instead increment daily streak and give today's milk
                try
                {
                    var today = DateTime.Now.Date;
                    // Increment streak regardless of last claim (as requested)
                    milkStreak++;
                    int milkEarned = 90 + milkStreak * 10 + baseMilkUpgradeCount * 10;
                    milk += milkEarned;
                    lastMilkClaimDate = today;

                    MessageBox.Show($"Transcend complete! You received {FormatNumbers(milkEarned)} milk as a bonus reward.\nCurrent streak: {milkStreak}", "Transcend Reward", MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
                catch
                {
                    // Swallow any unexpected errors related to dates/formatting to avoid crashing on transcend
                }

                SaveGame();
                UpdateUI();

                // Show the transcend window (optional UI)
                var window = new TranscendWindow();
                window.ShowDialog();
                UpdateUI();
            }
        }
        // Empty event handlers (if not used, consider removing from designer)
        private void labelGeneratorInfo_Click(object sender, EventArgs e) { }
        private void labelPrestigeCost_Click(object sender, EventArgs e) { }

        private void labelPointsPerSecond_Click(object sender, EventArgs e)
        {

        }

        private void labelSoftCap_Click(object sender, EventArgs e)
        {

        }

        private bool dragging = false;
        private Point dragCursorPoint;
        private Point dragFormPoint;

        private void panelTitleBar_MouseDown(object sender, MouseEventArgs e)
        {
            if ( e.Button == MouseButtons.Left )
            {
                dragging = true;
                dragCursorPoint = Cursor.Position;
                dragFormPoint = this.Location;
            }
        }

        private void panelTitleBar_MouseMove(object sender, MouseEventArgs e)
        {
            if ( dragging )
            {
                Point diff = Point.Subtract(Cursor.Position, new Size(dragCursorPoint));
                this.Location = Point.Add(dragFormPoint, new Size(diff));
            }
        }

        private void panelTitleBar_MouseUp(object sender, MouseEventArgs e)
        {
            if ( e.Button == MouseButtons.Left )
            {
                dragging = false;
            }
        }

        private void labelPoint_Click(object sender, EventArgs e)
        {

        }

        private void labelPointGain_Click(object sender, EventArgs e)
        {

        }

        private void labelUpgradeNote_Click(object sender, EventArgs e)
        {

        }

        private void labelPrestigeInfo_Click(object sender, EventArgs e)
        {

        }

        private double AscensionMultiplier => Math.Pow(1.1, ascendCount);

        private int EffectiveCooldownDuration
        {
            get
            {
                // Challenge 2 or 3 active: fixed 10 seconds
                if (activeChallengeIndex == 2 || activeChallengeIndex == 3)
                    return 10000;
                int reductions = ascendCount / 2;
                double reductionPercent = reductions * 0.05;
                double effective = cooldownDuration * (1.0 - reductionPercent);

                // Challenge 2 completed: decrease by 0.1s (100ms)
                if (challengesCompleted != null && challengesCompleted.Length > 2 && challengesCompleted[2])
                    effective -= 100;

                return Math.Max((int)effective, 50);
            }
        }
        private void StartChallenge(int challengeIndex)
        {
            var cost = GetAscendCost();
            bool canAscend = point >= cost;

            // Snapshot current cooldown to restore after challenge is completed
            prevCooldownDurationForChallenge = cooldownDuration;

            // Reset everything as in ascension
            point = BigDouble.Zero;
            pointGain = BigDouble.One;
            upgradeCount = 0;
            prestigeCount = 0;
            generatorCount = 0;
            generatorCost = new BigDouble(100);
            cooldownDuration = 1000;

            // Reset cooldown state and timers so clicks work normally
            isCooldown = false;
            cooldownTimer.Stop();
            cooldownElapsed = 0;
            labelCooldown.Text = "";
            autoClickElapsed = 0;

            if (canAscend)
            {
                ascendCount++;
                ascensionPoints++;
            }

            activeChallengeIndex = challengeIndex;
            UnlockAscensionFeature();
            UpdateUI();
            SaveGame();

            if (canAscend)
            {
                MessageBox.Show($"Challenge {challengeIndex + 1} started! (Ascension performed)", "Challenge Active", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            else
            {
                MessageBox.Show($"Challenge {challengeIndex + 1} started! (No ascension performed)", "Challenge Active", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
        }
        private void buttonPremiumShop_Click(object sender, EventArgs e)
        {
            using var shop = new PremiumWindow(milk, SpendMilkOnUpgrade, baseMilkUpgradeCount, milkSpent);
            shop.ShowDialog(this);
            UpdateUI();
        }
        // upgradeIndex: 0, 1, 2; amount: how much milk to spend
        private bool SpendMilkOnUpgrade(int upgradeIndex, BigDouble amount)
        {
            if (upgradeIndex == 3) // Base milk gain upgrade
            {
                int cost = 10 + baseMilkUpgradeCount * 2;
                if (milk >= cost && amount == BigDouble.One)
                {
                    milk -= cost;
                    baseMilkUpgradeCount++;
                    SaveGame();
                    UpdateUI();
                    return true;
                }
                return false;
            }

            if (upgradeIndex >= 0 && upgradeIndex <= 2)
            {
                if (milk >= amount)
                {
                    milk -= amount;
                    milkSpent[upgradeIndex] += amount;
                    SaveGame();
                    UpdateUI();
                    return true;
                }
                return false;
            }
            return false;
        }
        private void CheckChallengeCompletion()
        {
            if (activeChallengeIndex == -1)
                return;

            bool completed = false;
            switch (activeChallengeIndex)
            {
                case 0:
                    if (point >= 2_500_000)
                        completed = true;
                    break;
                case 1:
                    if (prestigeCount >= 8)
                        completed = true;
                    break;
                case 2:
                    if (generatorCount >= 2)
                        completed = true;
                    break;
                case 3:
                    if (point >= 25_000_000)
                        completed = true;
                    break;
            }
            if (completed)
            {
                challengesCompleted[activeChallengeIndex] = true;
                int completedIndex = activeChallengeIndex;
                activeChallengeIndex = -1;

                // Restore cooldownDuration to pre-challenge snapshot if available
                if (prevCooldownDurationForChallenge.HasValue)
                {
                    cooldownDuration = prevCooldownDurationForChallenge.Value;
                    prevCooldownDurationForChallenge = null;
                }

                // Ensure pointGain is recalculated now that challenge modifiers changed
                RecalculatePointGain();

                // If point is exactly zero, kick-start with one so gains show up immediately
                if (point == BigDouble.Zero)
                    point = BigDouble.One;

                // Reset cooldown UI state to reflect restored cooldown duration
                isCooldown = false;
                cooldownTimer.Stop();
                cooldownElapsed = 0;
                labelCooldown.Text = "";
                autoClickElapsed = 0;

                UpdateUI();
                SaveGame();
                MessageBox.Show($"Challenge {completedIndex + 1} completed!", "Challenge Complete", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
        }

        private BigDouble ApplySoftCap(BigDouble current, BigDouble gain)
        {
            var newTotal = current + gain;
            var threshold = GetSoftCapThreshold();
            if (newTotal <= threshold)
                return gain;

            // Calculate a non-zero divisor for the new total
            var divisor = GetSoftCapDivisor(newTotal);
            if (divisor <= BigDouble.Zero)
                return gain; // safety: don't kill gain

            return gain / divisor;
        }


        // Add event handler stubs for designer compatibility
        private void buttonMinimize_MouseEnter(object sender, EventArgs e) => buttonMinimize.BackColor = Color.FromArgb(64, 64, 64);
        private void buttonMinimize_MouseLeave(object sender, EventArgs e) => buttonMinimize.BackColor = Color.FromArgb(32, 32, 32);
        private void buttonMinimize_Click(object sender, EventArgs e) => this.WindowState = FormWindowState.Minimized;
        private void buttonClose_MouseEnter(object sender, EventArgs e) => buttonClose.BackColor = Color.FromArgb(64, 64, 64);
        private void buttonClose_MouseLeave(object sender, EventArgs e) => buttonClose.BackColor = Color.FromArgb(32, 32, 32);
        private void buttonClose_Click(object sender, EventArgs e) => this.Close();
        private void panelTitleBar_Resize(object sender, EventArgs e)
        {
            buttonMinimize.Location = new Point(panelTitleBar.Width - 80, 0);
            buttonClose.Location = new Point(panelTitleBar.Width - 40, 0);
        }

        private void UpdateAutoclickBar(int percent)
        {
            // Clamp 0..100
            if (percent < 0) percent = 0;
            if (percent > 100) percent = 100;
            int newWidth = (int)Math.Round(autoclickBarBg.Width * (percent / 100.0));
            autoclickBarFill.Width = Math.Max(0, Math.Min(newWidth, autoclickBarBg.Width));
        }
        private async void buttonInfoDailyGain_Click(object sender, EventArgs e)
        {
            // Compute tomorrow's milk using same formula used elsewhere
            int tomorrowMilk = 100 + milkStreak * 10 + baseMilkUpgradeCount * 10;

            // Get server time (fallback to UTC)
            DateTime serverNow = DateTime.UtcNow;
            try
            {
                var serverTime = await GetServerUtcDateAsync();
                if (serverTime.HasValue)
                    serverNow = serverTime.Value;
            }
            catch
            {
                serverNow = DateTime.UtcNow;
            }

            // Calculate server offset and next reset time (server midnight)
            TimeSpan serverOffset = serverNow - DateTime.UtcNow;
            DateTime nextReset = serverNow.Date.AddDays(1);

            using var infoForm = new Form
            {
                Text = "Daily Milk Gain Info",
                Size = new Size(480, 260),
                FormBorderStyle = FormBorderStyle.FixedDialog,
                StartPosition = FormStartPosition.CenterParent,
                MaximizeBox = false,
                MinimizeBox = false
            };

            var lblInfo = new Label
            {
                Text = "Your daily milk gain is a base amount plus your current login streak.\n\n" +
                       "Base gain: 10 x number of milk gain upgrades purchased + 100\n" +
                       "Streak bonus: +10 milk per consecutive day logged in\n\n" +
                       "Your streak resets if you miss a day.",
                Location = new Point(12, 12),
                Size = new Size(440, 120),
                Font = new Font("Segoe UI", 9F)
            };
            infoForm.Controls.Add(lblInfo);

            var lblTomorrow = new Label
            {
                Text = $"Your milk gain for tomorrow: {tomorrowMilk}",
                Location = new Point(12, 135),
                Size = new Size(440, 22),
                Font = new Font("Segoe UI", 10F, FontStyle.Bold)
            };
            infoForm.Controls.Add(lblTomorrow);

            var lblCountdown = new Label
            {
                Text = "",
                Location = new Point(12, 160),
                Size = new Size(440, 40),
                Font = new Font("Segoe UI", 12F, FontStyle.Bold),
                ForeColor = Color.DarkBlue
            };
            infoForm.Controls.Add(lblCountdown);

            var btnClose = new Button
            {
                Text = "Close",
                DialogResult = DialogResult.OK,
                Size = new Size(90, 30),
                Location = new Point(infoForm.ClientSize.Width - 110, infoForm.ClientSize.Height - 50),
                Anchor = AnchorStyles.Bottom | AnchorStyles.Right
            };
            infoForm.Controls.Add(btnClose);

            // Timer for updating countdown (declare after nextReset so lambda can capture it)
            var countdownTimer = new System.Windows.Forms.Timer { Interval = 1000 };
            countdownTimer.Tick += (s, ev) =>
            {
                DateTime currentServer = DateTime.UtcNow + serverOffset;
                TimeSpan remaining = nextReset - currentServer;
                if (remaining <= TimeSpan.Zero)
                {
                    // advance to next day
                    nextReset = nextReset.AddDays(1);
                    remaining = nextReset - currentServer;
                }

                string remainingText;
                if (remaining.TotalDays >= 1)
                    remainingText = string.Format("{0}d {1:00}h {2:00}m {3:00}s", (int)remaining.TotalDays, remaining.Hours, remaining.Minutes, remaining.Seconds);
                else
                    remainingText = string.Format("{0:00}h {1:00}m {2:00}s", remaining.Hours, remaining.Minutes, remaining.Seconds);

                lblCountdown.Text = $"Time until daily reset (server): {remainingText}\nServer time: {currentServer:yyyy-MM-dd HH:mm:ss} UTC";
            };

            infoForm.FormClosing += (s, ev) => countdownTimer.Stop();
            countdownTimer.Start();

            infoForm.ShowDialog(this);

            countdownTimer.Stop();
        }
    }
}