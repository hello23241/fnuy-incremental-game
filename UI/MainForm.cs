using BreakInfinity;
using System.Reflection;
using System.ComponentModel;
using WinFormsApp1.Core.Formatting;
using WinFormsApp1.Core.Game;
using WinFormsApp1.SaveSystem;
using GameStateDto = WinFormsApp1.SaveSystem.GameState;
using WinFormsApp1.Core.Updates;
using WinFormsApp1.Core.Persistence;
using WinFormsApp1.Core.Diagnostics;

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
        private readonly IGameSaveService saveService;
        private readonly GamePersistence persistence;
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

            saveService = new GameSaveService(savePath);
            persistence = new GamePersistence(saveService);
            controller = new GameController(CaptureRuntimeState());
            LoadGame();
            CheckForUpdates();
            UpdateUI();
        }
        private GameRuntimeState CaptureRuntimeState()
        {
            // Persist UI-driven unlock flags as booleans (UI orchestrates visibility)
            return new GameRuntimeState(
                Point: point,
                PointMultiplier: pointGain,
                UpgradeCount: upgradeCount,
                PrestigeCount: prestigeCount,
                GeneratorCost: generatorCost,
                GeneratorCount: generatorCount,
                AscensionPoints: ascensionPoints,
                AscensionCount: ascendCount,
                CooldownDuration: cooldownDuration,
                LastSavedTime: DateTime.UtcNow,
                HasUnlockedPrestige: buttonPrestige.Visible,
                HasUnlockedGenerators: buttonGenerator.Visible,
                HasUnlockedAscension: buttonAscend.Visible,
                HasAscended: buttonOpenAscensionShop.Visible,
                AscChallenges: challengesCompleted,
                HasUnlockedPremiumShop: hasUnlockedPremiumShop,
                Milk: milk,
                LastMilkClaimDate: lastMilkClaimDate,
                MilkStreak: milkStreak,
                BaseMilkUpgradeCount: baseMilkUpgradeCount,
                MilkSpent: milkSpent,
                ActiveChallengeIndex: activeChallengeIndex,
                PrevCooldownDurationForChallenge: prevCooldownDurationForChallenge
            );
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
            const string manifestUrl = "https://hello23241.github.io/fnuy-incremental-manifest/manifest.json";
            try
            {
                var currentVersion = new Version(Assembly.GetExecutingAssembly().GetName().Version?.ToString() ?? "1.0.0.0");
                var info = await UpdateService.CheckAsync(manifestUrl, currentVersion);

                string changelogMessage = $"{info.LastUpdateText}\n\nChangelog:\n{info.Changelog}";

                if (info.IsUpdateAvailable)
                {
                    var result = MessageBox.Show(
                        $"A new version ({info.LatestVersion}) is available!\n\n{changelogMessage}\n\nDo you want to download it now?",
                        "Update Available",
                        MessageBoxButtons.YesNo,
                        MessageBoxIcon.Information);

                    if (result == DialogResult.Yes)
                    {
                        System.Diagnostics.Process.Start(new System.Diagnostics.ProcessStartInfo
                        {
                            FileName = info.DownloadUrl,
                            UseShellExecute = true
                        });
                    }
                }
                else
                {
                    MessageBox.Show(
                        $"You are running the latest version ({info.CurrentVersion}).\n\n{changelogMessage}",
                        "No Update",
                        MessageBoxButtons.OK,
                        MessageBoxIcon.Information);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Failed to check for updates: {ex.Message}", "Update Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
        private GameController controller;
        private void EnsureController()
        {
            if (controller == null)
                controller = new GameController(CaptureRuntimeState());
        }

        private void ApplyControllerStateToFields()
        {
            var s = controller.State;
            point = s.Point;
            pointGain = s.PointMultiplier;
            upgradeCount = s.UpgradeCount;
            prestigeCount = s.PrestigeCount;
            generatorCost = s.GeneratorCost;
            generatorCount = s.GeneratorCount;
            ascendCount = s.AscensionCount;
            ascensionPoints = s.AscensionPoints;
            cooldownDuration = s.CooldownDuration;
            challengesCompleted = s.AscChallenges ?? challengesCompleted;
            activeChallengeIndex = s.ActiveChallengeIndex;
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
            bool challenge2Completed = challengesCompleted != null && challengesCompleted.Length > 2 && challengesCompleted[2];
            generatorTimer.Interval = CooldownService.ComputeGeneratorIntervalMs(
                baseIntervalMs: 1000,
                activeChallengeIndex: activeChallengeIndex,
                challenge2Completed: challenge2Completed);
        }
        private void Button1_Click(object sender, EventArgs e)
        {
            if (isCooldown) return;

            EnsureController();
            var gain = controller.Click(AscensionMultiplier, ApplySoftCap);
            ApplyControllerStateToFields();

            AudioManagerNAudio.Play("gain", 0.75f);

            UpdateUI();
            isCooldown = true;
            cooldownElapsed = 0;
            cooldownTimer.Interval = 50;
            cooldownTimer.Start();
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
            int autoClickInterval = PassiveGainService.ComputeAutoClickIntervalMs(EffectiveCooldownDuration);
            autoClickElapsed += autoClickTimer.Interval;

            if (autoclickBarBg.Visible)
            {
                int percent = (int)(100L * autoClickElapsed / autoClickInterval);
                if (autoClickElapsed >= autoClickInterval)
                {
                    UpdateAutoclickBar(100);

                    BigDouble gain = PassiveGainService.ComputeClickGain(
                        pointGain,
                        AscensionMultiplier,
                        point,
                        ApplySoftCap);

                    point += gain;
                    AudioManagerNAudio.Play("gain", 0.7f);

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
                point = BigDouble.Min(point, GetSoftCapThreshold() * 1000);
                return;
            }

            double prestigeEffect = GetPrestigeEffect();
            bool challenge0Completed = challengesCompleted != null && challengesCompleted.Length > 0 && challengesCompleted[0];
            bool challenge0Or3Active = activeChallengeIndex == 0 || activeChallengeIndex == 3;
            BigDouble milk0 = (milkSpent != null && milkSpent.Length > 0) ? milkSpent[0] : BigDouble.Zero;

            pointGain = GainMathService.ComputePointGain(
                EffectiveUpgradeCount,
                prestigeEffect,
                GetPrestigeIncrement(),
                divisor,
                challenge0Completed,
                challenge0Or3Active,
                milk0);
        }
        private void buttonUpgrade_Click(object sender, EventArgs e)
        {
            EnsureController();
            if (!controller.TryBuyUpgrade(GetUpgradeCost, GetEffectiveUpgradeDeduction))
                return;

            ApplyControllerStateToFields();

            if (cooldownDuration == 1000)
            {
                UnlockPrestigeFeature();
                cooldownDuration = 500;
            }

            RecalculatePointGain();
            labelUpgradeNote.Text = $"Upgrade count: {EffectiveUpgradeCount}";
            AudioManagerNAudio.Play("upgrade", 0.5f);
            UpdateUI();
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
            EnsureController();
            var (purchased, _) = controller.BuyMaxUpgrades(GetUpgradeCost, upgradeScale, GetEffectiveUpgradeDeduction);
            if (purchased <= 0) return false;

            ApplyControllerStateToFields();

            if (cooldownDuration == 1000)
            {
                UnlockPrestigeFeature();
                cooldownDuration = 500;
            }

            RecalculatePointGain();
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
        // replace SaveGame body with a delegation to persistence
        private void SaveGame()
        {
#if DEBUG
    // Do not save the game in debug build
    return;
#endif
            persistence.Save(CaptureRuntimeState());
        }
        // LoadGame — delegate to GameSaveService (with old plaintext fallback/backup)
        private async void LoadGame()
        {
            try
            {
                if (!File.Exists(savePath))
                {
                    SaveGame();
                    return;
                }

                var (ok, loaded) = await saveService.TryLoadAsync();
                if (!ok || loaded == null)
                {
                    var backup = SaveFileCoordinator.TryBackup(savePath);

                    if (backup.ok)
                    {
                        try
                        {
                            MessageBox.Show(
                                "Your old data will NOT CARRY OVER in this version.\nThere's a special bonus waiting after prestiging\n\n" +
                                $"A backup of your old save has been created at:\n{backup.backupPath}\n\n" +
                                "The game will start a fresh save now.",
                                "Haha get reset son",
                                MessageBoxButtons.OK,
                                MessageBoxIcon.Warning
                            );
                        }
                        catch { }

                        SaveGame();
                        return;
                    }
                    else
                    {
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

                var state = loaded;

                var rs = GamePersistence.ToRuntimeState(state);

                // Restore all game state variables first
                point = rs.Point;
                pointGain = rs.PointMultiplier;
                upgradeCount = rs.UpgradeCount;
                prestigeCount = rs.PrestigeCount;
                generatorCost = rs.GeneratorCost;
                generatorCount = rs.GeneratorCount;
                ascendCount = rs.AscensionCount;
                ascensionPoints = rs.AscensionPoints;
                cooldownDuration = rs.CooldownDuration;
                hasUnlockedPremiumShop = rs.HasUnlockedPremiumShop;
                milk = rs.Milk;
                lastMilkClaimDate = rs.LastMilkClaimDate;
                milkStreak = rs.MilkStreak;
                baseMilkUpgradeCount = rs.BaseMilkUpgradeCount;
                milkSpent = rs.MilkSpent;
                challengesCompleted = rs.AscChallenges;
                activeChallengeIndex = rs.ActiveChallengeIndex;
                prevCooldownDurationForChallenge = rs.PrevCooldownDurationForChallenge;

                controller = new GameController(CaptureRuntimeState());

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
        private void ApplyUnlocks(GameStateDto state, DateTime? serverTime)
        {
            // Compute visibility flags
            var flags = FeatureUnlockService.ComputeVisibility(state);

            buttonPrestige.Visible = flags.ShowPrestigeButton;
            labelPrestigeCost.Visible = flags.ShowPrestigeCost;

            buttonGenerator.Visible = flags.ShowGeneratorButton;
            labelGeneratorInfo.Visible = flags.ShowGeneratorInfo;
            labelSoftCap.Visible = flags.ShowSoftCap;
            labelPrestigeInfo.Visible = flags.ShowPrestigeInfo;
            buttonPremiumShop.Visible = flags.ShowPremiumShop;
            buttonInfoDailyGain.Visible = flags.ShowInfoDailyGain;

            buttonAscend.Visible = flags.ShowAscendButton;
            labelAscendCost.Visible = flags.ShowAscendCost;

            buttonOpenAscensionShop.Visible = flags.ShowAscensionShop;
            buttonTranscend.Visible = flags.ShowTranscendButton;
            labelTranscendCost.Visible = flags.ShowTranscendCost;

            // Daily reward (pure calculation)
            var daily = DailyRewardService.Evaluate(
                hasUnlockedGenerators: state.HasUnlockedGenerators,
                lastMilkClaimDate: lastMilkClaimDate,
                currentStreak: milkStreak,
                baseMilkUpgradeCount: baseMilkUpgradeCount,
                todayLocalDate: DateTime.Now.Date
            );

            if (daily.ShouldAward)
            {
                milkStreak = daily.NewMilkStreak;
                milk += daily.MilkEarned;
                lastMilkClaimDate = daily.NewLastMilkClaimDate;

                try
                {
                    AudioManagerNAudio.Play("milkearned", 0.95f);
                    MessageBox.Show(
                        $"You earned {daily.MilkEarned} milk for logging in today!\nBase gain: {baseMilkUpgradeCount * 10 + 100}\nStreak: {milkStreak} day(s)",
                        "Daily Reward",
                        MessageBoxButtons.OK,
                        MessageBoxIcon.Information
                    );
                }
                catch { }

                SaveGame();
            }
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

            var effects = UnlockOrchestrationService.HandleGeneratorUnlock(
                hasUnlockedPremiumShop: hasUnlockedPremiumShop,
                lastMilkClaimDate: lastMilkClaimDate,
                currentMilkStreak: milkStreak,
                baseMilkUpgradeCount: baseMilkUpgradeCount,
                nowLocalDate: DateTime.Now);

            if (effects.FirstTimeUnlock)
            {
                hasUnlockedPremiumShop = true;
                milk += effects.CompensationMilk;
                try
                {
                    AudioManagerNAudio.Play("milkearned", 0.95f);
                    MessageBox.Show(
                        "Here's 10000 milk so you can progress faster in place of the lost data.\n" +
                        "I'd recommend spending 950 in offline gain, 1000 in softcap reduction and the rest in points gain",
                        "Progress loss compensation :)", MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
                catch { }
            }

            if (effects.AwardDaily)
            {
                milk += effects.DailyMilk;
                milkStreak = effects.NewMilkStreak;
                lastMilkClaimDate = effects.NewLastMilkClaimDate;
                try
                {
                    AudioManagerNAudio.Play("milkearned", 0.95f);
                    MessageBox.Show(
                        $"You earned {effects.DailyMilk} milk for logging in today!\n" +
                        $"Base gain: {baseMilkUpgradeCount * 10 + 100}\nStreak: {milkStreak} day(s)",
                        "Daily Reward", MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
                catch { }
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
            CrashLogger.Log(ex);
        }
        private void TranscendFlashTimer_Tick(object sender, EventArgs e)
        {
            Color[] colors = { Color.Lime, Color.Cyan, Color.Yellow, Color.Magenta, Color.Orange, Color.Red };
            buttonTranscend.BackColor = colors[transcendFlashStep % colors.Length];
            transcendFlashStep++;
        }
        private void buttonTranscend_Click(object sender, EventArgs e)
        {
            if (point < transcendCost) return;

            EnsureController();

            point -= transcendCost; // keep existing UX flow
            AudioManagerNAudio.Play("transcend", 0.95f);
            transcendFlashTimer.Stop();
            buttonTranscend.BackColor = Color.DarkViolet;

            // push the cost deduction into controller state before reset
            controller = new GameController(CaptureRuntimeState());
            var (milkEarned, newStreak) = controller.DoTranscend(baseMilkUpgradeCount, milkStreak);
            ApplyControllerStateToFields();

            milkStreak = newStreak;
            milk += milkEarned;

            SaveGame();
            UpdateUI();

            try
            {
                AudioManagerNAudio.Play("milkearned", 0.95f);
                MessageBox.Show(
                    $"Transcend complete! You received {FormatNumbers(milkEarned)} milk as a bonus reward.\nCurrent streak: {milkStreak}",
                    "Transcend Reward",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Information);
            }
            catch { }

            var window = new TranscendWindow();
            window.ShowDialog();
            UpdateUI();
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
                bool challenge2Completed = challengesCompleted != null && challengesCompleted.Length > 2 && challengesCompleted[2];
                return CooldownService.ComputeEffectiveCooldownMs(
                    baseCooldownMs: cooldownDuration,
                    ascendCount: ascendCount,
                    activeChallengeIndex: activeChallengeIndex,
                    challenge2Completed: challenge2Completed);
            }
        }
        private void StartChallenge(int challengeIndex)
        {
            EnsureController();
            var cost = GetAscendCost();
            bool canAscend = point >= cost;

            prevCooldownDurationForChallenge = cooldownDuration;

            controller.StartChallenge(canAscend, challengeIndex);
            ApplyControllerStateToFields();

            // Reset cooldown UI state
            isCooldown = false;
            cooldownTimer.Stop();
            cooldownElapsed = 0;
            button1.BackColor = Color.White;
            button1.ForeColor = defaultClickButtonForeColor;
            autoClickElapsed = 0;

            UnlockAscensionFeature();
            UpdateUI();
            SaveGame();

            AudioManagerNAudio.Play("challengestart", 0.9f);
            MessageBox.Show(
                canAscend
                    ? $"Challenge {challengeIndex + 1} started! (Ascension performed)"
                    : $"Challenge {challengeIndex + 1} started! (No ascension performed)",
                "Challenge Active",
                MessageBoxButtons.OK,
                MessageBoxIcon.Information);
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
            if (upgradeIndex == 3)
            {
                var (ok, newMilk, newBase, _) = MilkShopService.TryBuyBaseGain(milk, baseMilkUpgradeCount);
                if (!ok) return false;

                milk = newMilk;
                baseMilkUpgradeCount = newBase;
                AudioManagerNAudio.Play("milkspent", 0.95f);
                SaveGame();
                UpdateUI();
                return true;
            }

            var (success, nm, ns) = MilkShopService.TrySpendStat(milk, upgradeIndex, amount, milkSpent);
            if (!success) return false;

            milk = nm;
            milkSpent = ns;
            AudioManagerNAudio.Play("milkspent", 0.95f);
            SaveGame();
            UpdateUI();
            return true;
        }
        private void CheckChallengeCompletion()
        {
            if (activeChallengeIndex == -1)
                return;

            bool completed = ChallengeService.IsCompleted(activeChallengeIndex, point, prestigeCount, generatorCount);
            if (!completed) return;

            challengesCompleted[activeChallengeIndex] = true;
            int completedIndex = activeChallengeIndex;
            activeChallengeIndex = -1;

            if (prevCooldownDurationForChallenge.HasValue)
            {
                cooldownDuration = prevCooldownDurationForChallenge.Value;
                prevCooldownDurationForChallenge = null;
            }

            RecalculatePointGain();

            if (point == BigDouble.Zero)
                point = BigDouble.One;

            isCooldown = false;
            cooldownTimer.Stop();
            cooldownElapsed = 0;
            button1.BackColor = Color.White;
            button1.ForeColor = defaultClickButtonForeColor;
            autoClickElapsed = 0;

            UpdateUI();
            SaveGame();
            AudioManagerNAudio.Play("challengecomplete", 0.95f);
            MessageBox.Show($"Challenge {completedIndex + 1} completed!", "Challenge Complete", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }
        private BigDouble GetSoftCapThreshold()
        {
            bool challenge3Completed = challengesCompleted != null && challengesCompleted.Length > 3 && challengesCompleted[3];
            return SoftCapCalculator.GetSoftCapThreshold(challenge3Completed, softcapThreshold);
        }

        private BigDouble GetSoftCapDivisor(BigDouble currentPoint)
        {
            var threshold = GetSoftCapThreshold();
            BigDouble milkSoftcap = (milkSpent != null && milkSpent.Length > 1) ? milkSpent[1] : BigDouble.Zero;
            return SoftCapCalculator.GetSoftCapDivisor(currentPoint, threshold, milkSoftcap);
        }

        private BigDouble ApplySoftCap(BigDouble current, BigDouble gain)
        {
            var threshold = GetSoftCapThreshold();
            var divisorAtNewTotal = GetSoftCapDivisor(current + gain);
            return SoftCapCalculator.ApplySoftCap(current, gain, threshold, divisorAtNewTotal);
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

                BigDouble passiveGain = PassiveGainService.ComputeGeneratorPassiveGain(
                    generatorCount,
                    pointGain,
                    AscensionMultiplier,
                    point,
                    ApplySoftCap);

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
        // Cost calculators delegate to CostCalculator
        private BigDouble GetUpgradeCost() => CostCalculator.UpgradeCost(baseUpgradeCost, upgradeScale, upgradeCount);
        private BigDouble GetPrestigeCost() => CostCalculator.PrestigeCost(basePrestigeCost, prestigeScale, prestigeCount);
        private BigDouble GetAscendCost() => CostCalculator.AscendCost(baseAscendCost, ascendScale, ascendCount);
        // Prestige effect with challenge modifiers
        private double GetPrestigeEffect()
        {
            bool challenge2Completed = challengesCompleted != null && challengesCompleted.Length > 2 && challengesCompleted[2];
            bool challenge1Or3Active = activeChallengeIndex == 1 || activeChallengeIndex == 3;
            return ProgressionMath.PrestigeEffect(prestigeCount, challenge2Completed, challenge1Or3Active);
        }

        private BigDouble GetPrestigeIncrement()
        {
            bool challenge1Completed = challengesCompleted != null && challengesCompleted.Length > 1 && challengesCompleted[1];
            return ProgressionMath.PrestigeIncrement(PrestigeIncrement, challenge1Completed);
        }
        private void UpdateSoftCapLabel()
        {
            var threshold = GetSoftCapThreshold();
            BigDouble divisor = GetSoftCapDivisor(point);

            var vm = SoftCapPresenter.Build(point, threshold, divisor, FormatNumbers);
            labelSoftCap.Visible = vm.Visible;
            if (vm.Visible && labelSoftCap.Text != vm.Text)
                labelSoftCap.Text = vm.Text;
        }
        // UI info helpers
        private void UpdateUpgradeInfoLabel()
        {
            double prestigeEffect = GetPrestigeEffect();
            BigDouble divisor = GetSoftCapDivisor(point);
            bool challenge0Completed = challengesCompleted != null && challengesCompleted.Length > 0 && challengesCompleted[0];
            bool challenge0Or3Active = activeChallengeIndex == 0 || activeChallengeIndex == 3;
            BigDouble milk0 = (milkSpent != null && milkSpent.Length > 0) ? milkSpent[0] : BigDouble.Zero;

            BigDouble perUpgrade = GainMathService.ComputeGainPerUpgradeForDisplay(
                EffectiveUpgradeCount,
                prestigeEffect,
                GetPrestigeIncrement(),
                divisor,
                challenge0Completed,
                challenge0Or3Active,
                milk0,
                AscensionMultiplier);

            labelUpgradeInfo.Text = $"each upgrade adds {FormatNumbers(perUpgrade)} to your click multiplier";

            string baseText = "₂";
            string extraText = "";
            if (challengesCompleted != null && challengesCompleted.Length > 1 && challengesCompleted[1])
                extraText = " (multiplied by 1.1)";
            if (challengesCompleted != null && challengesCompleted.Length > 2 && challengesCompleted[2])
            {
                baseText = "_{1.9}";
                extraText = "";
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
        // Number formatting now delegates to NumberFormatter (no behavior change)
        private string FormatNumbers(BigDouble value) => NumberFormatter.Format(value);

        // Offline progress
        private void ApplyOfflineProgress(DateTime lastSaved, DateTime serverNow)
        {
            TimeSpan offlineTime = serverNow - lastSaved;
            int seconds = (int)offlineTime.TotalSeconds;
            if (seconds <= 0)
                return;

            if (generatorCount <= 0)
            {
                if (prestigeCount == 0)
                    MessageBox.Show("Welcome back! You currently don't own any generator for offline progress. Unlock it after your first prestige!");
                else
                    MessageBox.Show("Welcome back! You currently don't own any generator for offline progress.");
                return;
            }

            var result = OfflineProgressService.Compute(
                currentPoints: point,
                pointGain: pointGain,
                generatorCount: generatorCount,
                milkSpent2: milkSpent[2],
                seconds: seconds,
                applySoftCap: (cur, gain) => ApplySoftCap(cur, gain));

            if (result.PassiveGain > BigDouble.Zero)
            {
                point += result.PassiveGain;
                AudioManagerNAudio.Play("gain", 0.8f);
                MessageBox.Show(
                    $"Welcome back! You earned {FormatNumbers(result.PassiveGain)} points while you were away for {result.Seconds}s.\n" +
                    $"Effective time was {result.EffectiveSeconds}s\n" +
                    $"Current offline multi: x{result.OfflineMultiplier}.",
                    "Offline progress"
                );
                UpdateUI();
                SaveGame();
            }
        }
        private void buttonPrestige_Click(object sender, EventArgs e)
        {
            EnsureController();
            var cost = GetPrestigeCost();
            if (!controller.TryPrestige(cost)) return;

            ApplyControllerStateToFields();

            AudioManagerNAudio.Play("prestige", 0.9f);
            UnlockGeneratorFeature();
            UpdateUI();
            CheckChallengeCompletion();
        }
        private void buttonAscend_Click(object sender, EventArgs e)
        {
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
                    return;
            }

            EnsureController();
            var cost = GetAscendCost();
            if (!controller.TryAscend(cost)) return;

            ApplyControllerStateToFields();

            AudioManagerNAudio.Play("ascend", 0.9f);
            UnlockAscensionFeature();
            UpdateUI();
        }
        private string GetChallengeObjectivePlainText(int idx)
        {
            return ChallengeService.GetObjectivePlainText(idx);
        }
        private void buttonGenerator_Click(object sender, EventArgs e)
        {
            EnsureController();
            if (!controller.TryBuyGenerator()) return;

            ApplyControllerStateToFields();

            AudioManagerNAudio.Play("genpurchase", 0.9f);
            labelPoint.Text = FormatNumbers(point);
            UpdateUI();
            CheckChallengeCompletion();
        }
        private void buttonInfoDailyGain_Click(object sender, EventArgs e)
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