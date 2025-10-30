using BreakInfinity;
using Newtonsoft.Json;
using System.IO;
using System.Net.Http;
using System.Reflection;
using System.Threading.Tasks;
using System.Globalization;
using static System.Windows.Forms.VisualStyles.VisualStyleElement.TaskbarClock;
using System.ComponentModel;

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
        private BigDouble point = new BigDouble(10000);
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

        // New: store colors for the click button to restore after cooldown
        private Color defaultClickButtonColor;
        private Color defaultClickButtonForeColor;

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

            // When the WinForms designer instantiates this form at design-time, many runtime operations
            // (network calls, file IO, timers, DPAPI) can throw and prevent the designer from loading.
            // Detect design-time and skip runtime-only initialization.
            if (LicenseManager.UsageMode == LicenseUsageMode.Designtime)
            {
                return;
            }

            // Ensure button shows custom BackColor and bold text
            button1.UseVisualStyleBackColor = false;
            button1.Font = new Font(button1.Font, button1.Font.Style | FontStyle.Bold);
            defaultClickButtonColor = button1.BackColor;
            defaultClickButtonForeColor = button1.ForeColor;
            // If you expect white by default, enforce it here
            button1.BackColor = Color.White;

            // Remove debug button entirely in Release builds
#if DEBUG
            buttonDebug.Visible = true;
            try { buttonDebug.Click += buttonDebug_Click; } catch { }
#else
            try
            {
                if (buttonDebug != null)
                {
                    buttonDebug.Visible = false;
                    buttonDebug.Enabled = false;
                    Controls.Remove(buttonDebug);
                    buttonDebug.Dispose();
                    buttonDebug = null;
                }
            }
            catch { }
#endif

            // Register SFX based on actual resources found
            try
            {
                // Coin/gain (ensure coin is registered by key)
                AudioManagerNAudio.RegisterFromEmbeddedResource("gain", "Resources.coin.wav");
                // Upgrade (flexible)
                AudioManagerNAudio.RegisterFromEmbeddedResource("upgrade");
                // Prestige
                AudioManagerNAudio.RegisterFromEmbeddedResource("prestige", "Resources.prestige.aif");
                // Ascend
                AudioManagerNAudio.RegisterFromEmbeddedResource("ascend", "Resources.ascend.mp3");
                // Challenge lifecycle
                AudioManagerNAudio.RegisterFromEmbeddedResource("challengecomplete", "Resources.challengecomplete.mp3");
                AudioManagerNAudio.RegisterFromEmbeddedResource("challengecancelled", "Resources.challengecancelled.aif");
                AudioManagerNAudio.RegisterFromEmbeddedResource("challengestart", "Resources.challengestarted.wav");
                AudioManagerNAudio.RegisterFromEmbeddedResource("challengestart", "Resources.levelupTRANS.aif");
                AudioManagerNAudio.RegisterFromEmbeddedResource("challengestart", "Resources.completetask.mp3");
                // Milk earned
                AudioManagerNAudio.RegisterFromEmbeddedResource("milkearned", "Resources.milkearned.wav");
                // Milk spent
                AudioManagerNAudio.RegisterFromEmbeddedResource("milkspent", "Resources.milkspent.mp3");
                // Generator purchase
                AudioManagerNAudio.RegisterFromEmbeddedResource("genpurchase", "Resources.genpurchase.mp3");
                // Transcend
                AudioManagerNAudio.RegisterFromEmbeddedResource("transcend", "Resources.transcend.wav");
            }
            catch { }

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

            this.Text = "Myrtle incremental";
            this.Icon = Properties.Resources.NianBean;

            // Wire events that can cause designer issues only at runtime
            try { buttonUpgrade.MouseDown += buttonUpgrade_RightClick; } catch { }

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

            LoadGame();
            // removed any labelCooldown.Text usage
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
            var formattedGain = softCapDivisor == 0 ? "0" : FormatNumbers(pointGain / softCapDivisor);
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
                        challengeText += "\nReduced point gain,\nprestige effectiveness\nand increased cooldown";
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
            // Play gain sound (slightly reduced)
            AudioManagerNAudio.Play("gain", 0.75f);
            UpdateUI();
            isCooldown = true;
            cooldownElapsed = 0;
            cooldownTimer.Interval = 50; // Always 50ms for smooth updates
            cooldownTimer.Start();
            // Visual: set button red with white text while on cooldown
            button1.BackColor = Color.Red;
            button1.ForeColor = Color.White;
            CheckChallengeCompletion();
        }
        private void CooldownTimer_Tick(object sender, EventArgs e)
        {
            cooldownElapsed += cooldownTimer.Interval;
            int remaining = EffectiveCooldownDuration - cooldownElapsed;
            if (remaining > 0)
            {
                // keep red/white during cooldown
                if (button1.BackColor != Color.Red) button1.BackColor = Color.Red;
                if (button1.ForeColor != Color.White) button1.ForeColor = Color.White;
            }
            else
            {
                cooldownTimer.Stop();
                isCooldown = false;
                cooldownElapsed = 0;
                // restore button colors when cooldown ends
                button1.BackColor = Color.White;
                button1.ForeColor = defaultClickButtonForeColor;
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
                    // Show full bar before resetting
                    UpdateAutoclickBar(100);
                    BigDouble gain = pointGain * AscensionMultiplier;
                    gain = ApplySoftCap(point, gain);
                    point += gain;
                    // Play gain sound for autoclick (slightly reduced)
                    AudioManagerNAudio.Play("gain", 0.7f);
                    // Important: update UI before resetting elapsed so the bar reaches 100% visibly
                    UpdateUI();
                    autoClickElapsed = 0;
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
                // Clamp to hard cap: 1000x soft cap
                point = BigDouble.Min(point, GetSoftCapThreshold() * 1000);
                return;
            }
            double prestigeEffect = GetPrestigeEffect();
            double challenge0Multi = (challengesCompleted != null && challengesCompleted.Length > 0 && challengesCompleted[0]) ? 3 : 1.0;
            // Challenge 0: Point gain is divided by 5
            double challengeDebuff = 1.0;
            if (activeChallengeIndex == 0 || activeChallengeIndex == 3)
                challengeDebuff /= 5.0;

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
                // Apply softer deduction based on current softcap divisor
                BigDouble deduction = GetEffectiveUpgradeDeduction(cost);
                point -= deduction;

                upgradeCount++;
                if (cooldownDuration == 1000)
                {
                    UnlockPrestigeFeature();
                    cooldownDuration = 500;
                }
                RecalculatePointGain();
                // Update the upgrade note to reflect current upgrades
                labelUpgradeNote.Text = $"Upgrade count: {EffectiveUpgradeCount}";
                // Play upgrade sfx
                AudioManagerNAudio.Play("upgrade", 0.5f);
                UpdateUI();
            }
        }

        // Right-click handler: if player has >=3 ascensions, offer Buy Max menu
        private void buttonUpgrade_RightClick(object sender, MouseEventArgs e)
        {
            if (e.Button != MouseButtons.Right)
                return;

            if (ascendCount < 3)
                return; // feature unlocks at 3 ascensions

            var menu = new ContextMenuStrip();
            var buyMaxItem = new ToolStripMenuItem("Buy Max Upgrades");
            buyMaxItem.Click += (s, ev) =>
            {
                if (BuyMaxUpgrades())
                {
                    // Play upgrade sfx
                    AudioManagerNAudio.Play("upgrade", 0.9f);
                    UpdateUI();
                    SaveGame();
                }
            };
            menu.Items.Add(buyMaxItem);
            menu.Show(buttonUpgrade, e.Location);
        }

        // Compute and purchase the maximum number of upgrades affordable with current points
        private bool BuyMaxUpgrades()
        {
            // Current next-upgrade cost (A in geometric series)
            BigDouble A = GetUpgradeCost();
            double a = upgradeScale; // ratio > 1

            if (a <= 1.0)
            {
                // Fallback: buy in a loop if scaling is non-increasing (should not happen)
                bool purchasedAny = false;
                while (point >= GetUpgradeCost())
                {
                    BigDouble loopCost = GetUpgradeCost();
                    BigDouble loopDeduction = GetEffectiveUpgradeDeduction(loopCost);
                    if (loopDeduction > point)
                        break;
                    point -= loopDeduction;
                    upgradeCount++;
                    purchasedAny = true;
                }
                if (purchasedAny)
                {
                    if (cooldownDuration == 1000)
                    {
                        UnlockPrestigeFeature();
                        cooldownDuration = 500;
                    }
                    RecalculatePointGain();
                }
                return purchasedAny;
            }

            // Solve n from: A * (a^n - 1)/(a - 1) <= point
            BigDouble denom = new BigDouble(a - 1.0);
            if (A <= BigDouble.Zero || point <= BigDouble.Zero)
                return false;

            BigDouble t = BigDouble.One + (point / A) * denom; // 1 + ((a-1)*point/A)
            double nDouble = System.Math.Floor(BigDouble.Log(t, a));
            int n = (int)System.Math.Max(0, nDouble);
            if (n <= 0)
                return false;

            // Compute total base cost for n upgrades and adjust for rounding if needed
            BigDouble aPowN = BigDouble.Pow(a, n);
            BigDouble sum = A * (aPowN - BigDouble.One) / denom;
            while (n > 0 && sum > point)
            {
                n--;
                aPowN = BigDouble.Pow(a, n);
                sum = A * (aPowN - BigDouble.One) / denom;
            }
            if (n <= 0)
                return false;

            // Apply softcap discount to the total deduction
            BigDouble totalDeduction = GetEffectiveUpgradeDeduction(sum);
            if (totalDeduction > point)
            {
                // If discounted sum still exceeds points due to rounding, reduce n conservatively via loop
                bool purchasedAny = false;
                while (n > 0)
                {
                    // buy one by one with discounted cost
                    BigDouble oneCost = GetUpgradeCost();
                    BigDouble oneDeduction = GetEffectiveUpgradeDeduction(oneCost);
                    if (oneDeduction > point)
                        break;
                    point -= oneDeduction;
                    upgradeCount++;
                    n--;
                    purchasedAny = true;
                }
                if (purchasedAny)
                {
                    if (cooldownDuration == 1000)
                    {
                        UnlockPrestigeFeature();
                        cooldownDuration = 500;
                    }
                    RecalculatePointGain();
                    // Update the upgrade note to reflect current upgrades after bulk purchase
                    labelUpgradeNote.Text = $"Upgrade count: {EffectiveUpgradeCount}";
                }
                return purchasedAny;
            }

            // Deduct discounted total and apply count
            point -= totalDeduction;
            upgradeCount += n;

            if (cooldownDuration == 1000)
            {
                UnlockPrestigeFeature();
                cooldownDuration = 500;
            }
            RecalculatePointGain();
            // Update the upgrade note to reflect current upgrades after bulk purchase
            labelUpgradeNote.Text = $"Upgrade count: {EffectiveUpgradeCount}";
            return true;
        }

        //debug
        private void buttonDebug_Click(object sender, EventArgs e)
        {
            milk *= 10000000000000000000;
            SaveGame();
            UpdateUI();
        }
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
                    // Play cancel sfx
                    AudioManagerNAudio.Play("challengecancelled", 0.9f);
                    MessageBox.Show("Challenge cancelled.", "Challenge", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    return;
                }
                // Only start a challenge if a new one was started
                if (challengeWindow.ChallengeActive && challengeWindow.ActiveChallengeIndex != previousActiveChallengeIndex)
                {
                    int idx = challengeWindow.ActiveChallengeIndex;

                    // Confirm before starting a challenge (warning)
                    var confirm = MessageBox.Show(
                        $"Start challenge {idx + 1}?\n\nThis will perform an ascension reset while also resetting your generator count.\nYour generators will NOT be refunded!",
                        "Start Challenge",
                        MessageBoxButtons.YesNo,
                        MessageBoxIcon.Information,
                        MessageBoxDefaultButton.Button2);
                    if (confirm != DialogResult.Yes)
                    {
                        return;
                    }

                    StartChallenge(idx);
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
                // Persist challenge session state
                ActiveChallengeIndex = activeChallengeIndex,
                PrevCooldownDurationForChallenge = prevCooldownDurationForChallenge
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

                // Restore challenge session state
                activeChallengeIndex = state.ActiveChallengeIndex;
                prevCooldownDurationForChallenge = state.PrevCooldownDurationForChallenge;

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

            buttonGenerator.Visible = state.HasUnlockedGenerators;
            labelGeneratorInfo.Visible = state.HasUnlockedGenerators;
            labelSoftCap.Visible = state.HasUnlockedGenerators;
            labelPrestigeInfo.Visible = state.HasUnlockedGenerators;
            buttonPremiumShop.Visible = state.HasUnlockedGenerators;
            buttonInfoDailyGain.Visible = state.HasUnlockedGenerators;

            // Daily reset based on user's local timezone (midnight local)
            if (state.HasUnlockedGenerators)
            {
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
                    AudioManagerNAudio.Play("milkearned", 0.95f);
                    MessageBox.Show(
                        $"You earned {milkEarned} milk for logging in today!\nBase gain: {baseMilkUpgradeCount * 10 + 100}\nStreak: {milkStreak} day(s)",
                        "Daily Reward",
                        MessageBoxButtons.OK,
                        MessageBoxIcon.Information
                    );
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
            // Removed labelUpgradeNote.Visible = false; keep note text as-is; it will change on purchase
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
                MessageBox.Show("Here's 10000 milk so you can progress faster in place of the lost data.\n" +
                    "I'd recommend spending 950 in offline gain, 1000 in softcap reduction and the rest in points gain",
                    "Progress loss compensation :)", MessageBoxButtons.OK, MessageBoxIcon.Information);
                milk += 10000;
                // Play milk earned sfx for compensation
                AudioManagerNAudio.Play("milkearned", 0.95f);
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
                    // Play milk earned sfx for daily reward
                    AudioManagerNAudio.Play("milkearned", 0.95f);
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
                Environment.SpecialFolder.LocalApplicationData.ToString(),
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
                // Play transcend sfx
                AudioManagerNAudio.Play("transcend", 0.95f);
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
                    // Play milk earned sfx for transcend reward
                    AudioManagerNAudio.Play("milkearned", 0.95f);

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

        private bool dragging = false;
        private Point dragCursorPoint;
        private Point dragFormPoint;

        private void panelTitleBar_MouseDown(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Left)
            {
                dragging = true;
                dragCursorPoint = Cursor.Position;
                dragFormPoint = this.Location;
            }
        }

        private void panelTitleBar_MouseMove(object sender, MouseEventArgs e)
        {
            if (dragging)
            {
                Point diff = Point.Subtract(Cursor.Position, new Size(dragCursorPoint));
                this.Location = Point.Add(dragFormPoint, new Size(diff));
            }
        }

        private void panelTitleBar_MouseUp(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Left)
            {
                dragging = false;
            }
        }

        private double AscensionMultiplier => Math.Pow(2, ascendCount);

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

            // Challenge-specific: also reset generators
            generatorCount = 0;
            generatorCost = new BigDouble(100);

            // Reset cooldown state and timers so clicks work normally (do not change base cooldownDuration)
            isCooldown = false;
            cooldownTimer.Stop();
            cooldownElapsed = 0;
            // restore button colors when challenge starts
            button1.BackColor = Color.White;
            button1.ForeColor = defaultClickButtonForeColor;
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

            // Play challenge start sfx
            AudioManagerNAudio.Play("challengestart", 0.9f);

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
                    // Play milk spent sfx
                    AudioManagerNAudio.Play("milkspent", 0.95f);
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
                    // Play milk spent sfx
                    AudioManagerNAudio.Play("milkspent", 0.95f);
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
                    if (point >= 1_000_000)
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
                // restore button colors when challenge completes
                button1.BackColor = Color.White;
                button1.ForeColor = defaultClickButtonForeColor;
                autoClickElapsed = 0;

                UpdateUI();
                SaveGame();
                // Play completion sfx
                AudioManagerNAudio.Play("challengecomplete", 0.95f);
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

        // Compute the effective deduction for upgrade costs under softcap rules
        private BigDouble GetEffectiveUpgradeDeduction(BigDouble baseCost)
        {
            BigDouble divisor = GetSoftCapDivisor(point);
            // If hard cap (divisor == 0), upgrades cost 0 points
            if (divisor <= BigDouble.Zero)
                return BigDouble.Zero;

            BigDouble factor = divisor * 0.5; // divide by 0.5x the softcap divisor
            if (factor < BigDouble.One)
                factor = BigDouble.One; // clamp denominator to at least 1

            return baseCost / factor;
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

        // Generator timer tick handler
        private void GeneratorTimer_Tick(object sender, EventArgs e)
        {
            if (generatorCount > 0)
            {
                labelPointsPerSecond.Visible = true;
                BigDouble passiveGain = Math.Pow(10, generatorCount) * 0.01 * pointGain * AscensionMultiplier;
                passiveGain = ApplySoftCap(point, passiveGain);
                point += passiveGain;
                AudioManagerNAudio.Play("gain", 0.5f);
                labelPoint.Text = FormatNumbers(point);
                UpdateUI();
            }
            else
            {
                labelPointsPerSecond.Visible = false;
            }
        }

        // Cost calculators
        private BigDouble GetUpgradeCost() => baseUpgradeCost * BigDouble.Pow(upgradeScale, upgradeCount);
        private BigDouble GetPrestigeCost() => basePrestigeCost * BigDouble.Pow(prestigeScale, prestigeCount);
        private BigDouble GetAscendCost()
        {
            if (ascendCount <= 2)
                return baseAscendCost;
            return baseAscendCost * BigDouble.Pow(ascendScale, ascendCount - 2);
        }

        // Prestige effect with challenge modifiers
        private double GetPrestigeEffect()
        {
            double logBase = 2.0;
            if (challengesCompleted != null && challengesCompleted.Length > 2 && challengesCompleted[2])
                logBase = 1.9;
            double effect = BigDouble.Log(prestigeCount + 1, logBase);
            if (activeChallengeIndex == 1 || activeChallengeIndex == 3)
                effect /= 2.0;
            return effect;
        }

        // Soft cap helpers
        private BigDouble GetSoftCapDivisor(BigDouble currentPoint)
        {
            BigDouble divisor = BigDouble.One;
            if (milkSpent != null && milkSpent.Length > 1 && milkSpent[1] > BigDouble.Zero)
                divisor /= (BigDouble.Min(BigDouble.Log(milkSpent[1], 1.1), milkSpent[1]) * 0.01 + BigDouble.One);

            var threshold = GetSoftCapThreshold();
            var threshold1000 = threshold * 1000;

            if (currentPoint <= threshold)
            {
                return BigDouble.One;
            }
            else if (currentPoint <= threshold1000)
            {
                var linearDivisor = currentPoint / 2 / threshold;
                if (linearDivisor * divisor <= 1) return 1;
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
            var threshold1000 = threshold * 1000;
            BigDouble divisor = GetSoftCapDivisor(point);

            if (point >= threshold1000)
            {
                labelSoftCap.Visible = true;
                labelSoftCap.Text = $"Current points exceed {FormatNumbers(threshold1000)} (1000× softcap). All gain is disabled by the soft cap.";
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

        // UI info helpers
        private void UpdateUpgradeInfoLabel()
        {
            double prestigeEffect = GetPrestigeEffect();
            BigDouble divisor = GetSoftCapDivisor(point);

            // Each additional upgrade increases the inner multiplier by:
            // f(U) = 1 + U + U^2 * k, where k = prestigeEffect * GetPrestigeIncrement()/100/divisor
            // Delta per upgrade = f(U+1) - f(U) = 1 + (2U + 1) * k
            BigDouble k = prestigeEffect * GetPrestigeIncrement() / 100 / divisor;

            double challenge0Multi = (challengesCompleted != null && challengesCompleted.Length > 0 && challengesCompleted[0]) ? 3 : 1.0;
            double challengeDebuff = 1.0;
            if (activeChallengeIndex == 0 || activeChallengeIndex == 3)
                challengeDebuff /= 10;

            BigDouble gainPerUpgrade = (BigDouble.One + (2 * EffectiveUpgradeCount + 1) * k)
                * challenge0Multi * challengeDebuff
                * (BigDouble.One + milkSpent[0] * 0.0001)
                * AscensionMultiplier;

            labelUpgradeInfo.Text = $"each upgrade adds {FormatNumbers(gainPerUpgrade)} to your click multiplier";

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

            double prestigeEff = GetPrestigeEffect();
            labelPrestigeInfo.Text = $"Prestige effect: log{baseText}({prestigeCount + 1}) = {prestigeEff:F2}{extraText}";
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

        // Number formatting
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
                "Uqig", "Dqig", "Tqig", "Qaqig", "Qiqag", "Sxqig", "Spqig", "Ocqig", "Noqig", "Sxg",
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

        // Offline progress
        private void ApplyOfflineProgress(DateTime lastSaved, DateTime serverNow)
        {
            TimeSpan offlineTime = serverNow - lastSaved;
            int seconds = (int)offlineTime.TotalSeconds;
            if (seconds <= 0)
            {
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

            AudioManagerNAudio.Play("gain", 0.8f);
            MessageBox.Show(
                $"Welcome back! You earned {FormatNumbers(passiveGain)} points while you were away for {seconds}s.\n" +
                $"Effective time was {trueSeconds}s\n" +
                $"Current offline multi: x{offlineMultiplier}.",
                "Offline progress"
            );

            UpdateUI();
            SaveGame();
        }

        // Designer-wired handlers
        private void buttonPrestige_Click(object sender, EventArgs e)
        {
            var cost = GetPrestigeCost();
            if (point >= cost)
            {
                point = BigDouble.Zero;
                upgradeCount = 0;
                pointGain = BigDouble.One;
                prestigeCount++;
                AudioManagerNAudio.Play("prestige", 0.9f);
                UnlockGeneratorFeature();
                UpdateUI();
                CheckChallengeCompletion();
            }
        }
        private void buttonAscend_Click(object sender, EventArgs e)
        {
            // If ascending while in a challenge, confirm with warning and show objective
            if (activeChallengeIndex != -1)
            {
                int idx = activeChallengeIndex;
                string objective = GetChallengeObjectivePlainText(idx);
                var confirmAscendInChallenge = MessageBox.Show(
                    $"You're in challenge {idx + 1} right now, the objective is to {objective}. Are you sure you want to ascend while in a challenge?",
                    "Challenge in progress",
                    MessageBoxButtons.YesNo,
                    MessageBoxIcon.Warning,
                    MessageBoxDefaultButton.Button2);
                if (confirmAscendInChallenge != DialogResult.Yes)
                {
                    return;
                }
            }

            var cost = GetAscendCost();
            if (point >= cost)
            {
                point = BigDouble.Zero;
                pointGain = BigDouble.One;
                upgradeCount = 0;
                prestigeCount = 0;
                ascendCount++;
                ascensionPoints++;
                AudioManagerNAudio.Play("ascend", 0.9f);
                UnlockAscensionFeature();
                UpdateUI();
            }
        }

        private string GetChallengeObjectivePlainText(int idx)
        {
            switch (idx)
            {
                case 0: return "reach 1,000,000 points";
                case 1: return "prestige 8 times";
                case 2: return "buy 2 generators";
                case 3: return "reach 25,000,000 points";
                default: return "complete the challenge";
            }
        }
        private void buttonGenerator_Click(object sender, EventArgs e)
        {
            if (point >= generatorCost)
            {
                point -= generatorCost;
                generatorCount++;
                generatorCost = BigDouble.Pow(generatorCost, 2);
                // Play generator purchase sfx
                AudioManagerNAudio.Play("genpurchase", 0.9f);
                labelPoint.Text = FormatNumbers(point);
                UpdateUI();
                CheckChallengeCompletion();
            }
        }

        private async void buttonInfoDailyGain_Click(object sender, EventArgs e)
        {
            int tomorrowMilk = 100 + milkStreak * 10 + baseMilkUpgradeCount * 10;

            DateTime localNow = DateTime.Now;
            TimeZoneInfo localTz = TimeZoneInfo.Local;
            TimeSpan utcOffset = localTz.GetUtcOffset(localNow);
            string UtcOffsetText(TimeSpan offset)
            {
                string sign = offset >= TimeSpan.Zero ? "+" : "-";
                offset = offset.Duration();
                return offset.Minutes == 0
                    ? $"UTC{sign}{offset.Hours}"
                    : $"UTC{sign}{offset.Hours}:{offset.Minutes:00}";
            }
            string tzDisplay = UtcOffsetText(utcOffset);

            DateTime nextReset = localNow.Date.AddDays(1);

            using var infoForm = new Form
            {
                Text = "Daily Milk Gain Info",
                Size = new Size(520, 280),
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
                       $"Daily reset occurs at 00:00 in your timezone ({tzDisplay}).",
                Location = new Point(12, 12),
                Size = new Size(480, 120),
                Font = new Font("Segoe UI", 9F)
            };
            infoForm.Controls.Add(lblInfo);

            var lblTomorrow = new Label
            {
                Text = $"Your milk gain for tomorrow: {tomorrowMilk}",
                Location = new Point(12, 135),
                Size = new Size(480, 22),
                Font = new Font("Segoe UI", 10F, FontStyle.Bold)
            };
            infoForm.Controls.Add(lblTomorrow);

            var lblCountdown = new Label
            {
                Text = "",
                Location = new Point(12, 180),
                Size = new Size(480, 46),
                Font = new Font("Segoe UI", 11.5F, FontStyle.Bold),
                ForeColor = Color.DarkBlue
            };
            infoForm.Controls.Add(lblCountdown);

            var countdownTimer = new System.Windows.Forms.Timer { Interval = 1000 };
            countdownTimer.Tick += (s, ev) =>
            {
                DateTime nowLocal = DateTime.Now;
                TimeSpan remaining = nextReset - nowLocal;
                if (remaining <= TimeSpan.Zero)
                {
                    nextReset = nowLocal.Date.AddDays(1);
                    remaining = nextReset - nowLocal;
                }

                string remainingText = remaining.TotalDays >= 1
                    ? string.Format("{0}d {1:00}h {2:00}m {3:00}s", (int)remaining.TotalDays, remaining.Hours, remaining.Minutes, remaining.Seconds)
                    : string.Format("{0:00}h {1:00}m {2:00}s", remaining.Hours, remaining.Minutes, remaining.Seconds);

                lblCountdown.Text = $"Time until daily reset (your timezone {tzDisplay}): {remainingText}\n" +
                                    $"Local time: {nowLocal:yyyy-MM-dd HH:mm:ss} ({tzDisplay})";
            };

            infoForm.FormClosing += (s, ev) => countdownTimer.Stop();
            countdownTimer.Start();

            infoForm.ShowDialog(this);
            countdownTimer.Stop();
        }

        protected override void OnFormClosing(FormClosingEventArgs e)
        {
            try
            {
                SaveGame();
            }
            catch { }

            try
            {
                MessageBox.Show("Your progress has been saved!", "Game saved", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            catch { }

            base.OnFormClosing(e);
        }
    }
}