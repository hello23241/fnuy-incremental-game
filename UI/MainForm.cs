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
using WinFormsApp1.Core.Infrastructure;

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
            defaultClickButtonForeColor = button1.ForeColor;
            // If you expect white by default, enforce it here
            button1.BackColor = Color.White;

            try { SoundEffects.RegisterAll(); } catch { }
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
            // Smooth animation ~30 FPS
            transcendFlashTimer.Interval = 33;
            transcendFlashTimer.Tick += TranscendFlashTimer_Tick;

            // initialize animator (tweak hueStepDeg/saturation/value to taste)
            transcendAnimator = new TranscendFlashAnimator(
                initialHueDeg: 0,
                hueStepDeg: 10.0,     // lower = slower color change, higher = faster
                saturation: 0.95,
                value: 1.0);

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
        private void RevealPrestigeControls()
        {
            buttonPrestige.Visible = true;
            labelPrestigeCost.Visible = true;
        }

        private void RevealGeneratorAndAscensionPrereqs()
        {
            buttonGenerator.Visible = true;
            labelGeneratorInfo.Visible = true;
            labelSoftCap.Visible = true;
            labelPrestigeInfo.Visible = true;
            buttonPremiumShop.Visible = true;
            buttonInfoDailyGain.Visible = true;
        }

        private void RevealAscensionControls()
        {
            buttonAscend.Visible = true;
            labelAscendCost.Visible = true;
            buttonOpenAscensionShop.Visible = true;
            buttonTranscend.Visible = true;
            labelTranscendCost.Visible = true;
        }

        // Derive feature visibility from progression counts; do not grant rewards.
        // Call this after loading state when flags might be wrong in the save.
        private void RecoverFeatureVisibilityFromProgress()
        {
            // Prestige UI: show if player has prestiged or if reduced cooldown was already set
            if (!buttonPrestige.Visible && (prestigeCount > 0 || cooldownDuration <= 500))
                RevealPrestigeControls();

            // Generators and related HUD: show if player has prestiged at least once
            if (!buttonGenerator.Visible && prestigeCount > 0)
                RevealGeneratorAndAscensionPrereqs();

            // Ascension shop/transcend: show if player has at least one ascension
            if (!buttonOpenAscensionShop.Visible && ascendCount > 0)
                RevealAscensionControls();
        }
        private void ResetClickUiState()
        {
            isCooldown = false;
            cooldownTimer.Stop();
            cooldownElapsed = 0;
            button1.BackColor = Color.White;
            button1.ForeColor = defaultClickButtonForeColor;
            autoClickElapsed = 0;
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
        private TranscendFlashAnimator transcendAnimator;
        private async Task<DateTime?> GetServerUtcDateAsync()
        {
            return await TimeService.TryGetServerUtcAsync();
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
            this.SuspendLayout();

            RecalculatePointGain();

            var softCapDivisor = GetSoftCapDivisor(point);
            var upgradeCost = GetUpgradeCost();
            var prestigeCost = GetPrestigeCost();
            var ascendCost = GetAscendCost();

            var hud = HudPresenter.Build(
                point,
                pointGain,
                softCapDivisor,
                upgradeCost,
                prestigeCost,
                ascendCost,
                transcendCost,
                milk,
                FormatNumbers);

            UpdateButtonStates();
            UpdateUpgradeInfoLabel();
            UpdateGeneratorInfo();
            UpdateSoftCapLabel();
            UpdateGeneratorTimerInterval();

            // Hard cap = 1000x softcap threshold
            var hardCapPoint = GetSoftCapThreshold() * 1000;

            var decorated = HardCapPresenter.Decorate(hud, point, hardCapPoint);

            if (labelPoint.Text != hud.PointsText) labelPoint.Text = hud.PointsText;
            if (button1.Text != decorated.ClickButtonText) button1.Text = decorated.ClickButtonText;
            if (labelUpgradeCost.Text != hud.UpgradeCostText) labelUpgradeCost.Text = hud.UpgradeCostText;
            if (labelPrestigeCost.Text != hud.PrestigeCostText) labelPrestigeCost.Text = hud.PrestigeCostText;
            if (labelAscendCost.Text != hud.AscendCostText) labelAscendCost.Text = hud.AscendCostText;
            if (labelPointGain.Text != decorated.PointGainText) labelPointGain.Text = decorated.PointGainText;
            if (labelTranscendCost.Text != hud.TranscendCostText) labelTranscendCost.Text = hud.TranscendCostText;
            if (buttonPremiumShop.Text != hud.MilkButtonText) buttonPremiumShop.Text = hud.MilkButtonText;

            if (buttonTranscend.Enabled != hud.CanTranscend) buttonTranscend.Enabled = hud.CanTranscend;
            if (hud.CanTranscend)
            {
                if (!transcendFlashTimer.Enabled) transcendFlashTimer.Start();
            }
            else
            {
                if (transcendFlashTimer.Enabled) transcendFlashTimer.Stop();
                if (buttonTranscend.BackColor != Color.Gray) buttonTranscend.BackColor = Color.Gray;
            }

            // Auto-click UI centralized
            UI.AutoClickUiApplier.Apply(
                ascendCount,
                EffectiveCooldownDuration,
                autoClickElapsed,
                autoClickTimer,
                autoclickBarBg,
                autoclickBarFill,
                UpdateAutoclickBar,
                uiTickIntervalMs: 50
            );

            var ch = ChallengeTextPresenter.Build(activeChallengeIndex);
            labelChallengeState.Visible = ch.Visible;
            if (ch.Visible)
            {
                if (labelChallengeState.Text != ch.Text) labelChallengeState.Text = ch.Text;
                if (labelChallengeState.Font.Style != FontStyle.Bold)
                    labelChallengeState.Font = new Font(labelChallengeState.Font, FontStyle.Bold);
            }

            this.ResumeLayout();
        }
        private void UpdateButtonStates()
        {
            var states = ButtonStateService.Compute(
                point,
                GetUpgradeCost(),
                GetPrestigeCost(),
                GetAscendCost(),
                generatorCost);

            buttonUpgrade.Enabled = states.UpgradeEnabled;
            buttonPrestige.Enabled = states.PrestigeEnabled;
            buttonAscend.Enabled = states.AscendEnabled;
            buttonGenerator.Enabled = states.GeneratorEnabled;

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

            SoundEffects.Gain(0.75f);

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

                    EnsureController();
                    controller.Click(AscensionMultiplier, ApplySoftCap);
                    ApplyControllerStateToFields();

                    SoundEffects.Gain(0.7f);

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
            double prestigeEffect = GetPrestigeEffect();
            bool challenge0Completed = challengesCompleted != null && challengesCompleted.Length > 0 && challengesCompleted[0];
            bool challenge0Or3Active = activeChallengeIndex == 0 || activeChallengeIndex == 3;
            BigDouble milk0 = (milkSpent != null && milkSpent.Length > 0) ? milkSpent[0] : BigDouble.Zero;

            // Hard cap = 1000x softcap threshold
            var hardCap = GetSoftCapThreshold() * 1000;

            EnsureController();
            controller.RecalculatePointMultiplier(
                EffectiveUpgradeCount,
                prestigeEffect,
                GetPrestigeIncrement(),
                divisor,
                challenge0Completed,
                challenge0Or3Active,
                milk0,
                hardCap);

            ApplyControllerStateToFields();
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
            SoundEffects.Upgrade(0.5f);
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
                    SoundEffects.Upgrade(0.9f);
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

                // In buttonOpenAscensionShop_Click, use DialogService.ConfirmStartChallenge and InfoChallengeCancelled
                if (challengeWindow.ChallengeCancelled)
                {
                    activeChallengeIndex = -1;

                    EnsureController();
                    controller.State = controller.State with { ActiveChallengeIndex = -1 };

                    if (prevCooldownDurationForChallenge.HasValue)
                    {
                        cooldownDuration = prevCooldownDurationForChallenge.Value;
                        prevCooldownDurationForChallenge = null;
                    }

                    RecalculatePointGain();
                    UpdateUI();
                    SaveGame();
                    SoundEffects.ChallengeCancelled(0.9f);
                    DialogService.InfoChallengeCancelled();
                    return;
                }

                // Only start a challenge if a new one was started
                if (challengeWindow.ChallengeActive && challengeWindow.ActiveChallengeIndex != previousActiveChallengeIndex)
                {
                    int idx = challengeWindow.ActiveChallengeIndex;

                    if (!DialogService.ConfirmStartChallenge(idx))
                        return;

                    StartChallenge(idx);
                }
            }
        } 
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
                    RevealPrestigeControls(); // was UnlockPrestigeFeature(); no side effects
                }

                DateTime? serverTime = await GetServerUtcDateAsync();
                if (serverTime != null)
                    ApplyOfflineProgress(state.LastSavedTime, serverTime.Value);

                // Apply flags from visibility service as usual
                ApplyUnlocks(state, serverTime);

                // Final safety: derive visibility from counts and persist
                RecoverFeatureVisibilityFromProgress();
                SaveGame();
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
            UI.FeatureVisibilityApplier.Apply(
                state,
                buttonPrestige,
                labelPrestigeCost,
                labelPrestigeInfo,
                buttonGenerator,
                labelGeneratorInfo,
                labelSoftCap,
                buttonPremiumShop,
                buttonInfoDailyGain,
                buttonAscend,
                labelAscendCost,
                buttonOpenAscensionShop,
                buttonTranscend,
                labelTranscendCost);

            // Safety: if DTO flags were missing/wrong, fix them based on counts.
            RecoverFeatureVisibilityFromProgress();

            // Daily reward (pure calculation)
            var daily = DailyRewardService.Evaluate(
                hasUnlockedGenerators: state.HasUnlockedGenerators, // if absent in DTO, the safety above ensures the UI is correct anyway
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
                    SoundEffects.MilkEarned(0.95f);
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
                    SoundEffects.MilkEarned(0.95f);
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
                    SoundEffects.MilkEarned(0.95f);
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
            buttonTranscend.BackColor = transcendAnimator.Next();
        }
        // In buttonTranscend_Click, use DialogService for the reward dialog
        private void buttonTranscend_Click(object sender, EventArgs e)
        {
            EnsureController();
            if (!controller.TryTranscend(transcendCost, baseMilkUpgradeCount, milkStreak, out var milkEarned, out var newStreak))
                return;

            SoundEffects.Transcend(0.95f);
            transcendFlashTimer.Stop();
            buttonTranscend.BackColor = Color.DarkViolet;

            ApplyControllerStateToFields();

            milkStreak = newStreak;
            milk += milkEarned;

            SaveGame();
            UpdateUI();

            try
            {
                SoundEffects.MilkEarned(0.95f);
                DialogService.InfoTranscendReward(milkEarned, milkStreak, FormatNumbers);
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
            ResetClickUiState();

            UnlockAscensionFeature();
            UpdateUI();
            SaveGame();

            SoundEffects.ChallengeStart(0.9f);
            DialogService.InfoChallengeStarted(challengeIndex, canAscend);
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
                int cost = MilkShopService.GetBaseUpgradeCost(baseMilkUpgradeCount);
                if (milk >= cost && amount == BigDouble.One)
                {
                    milk -= cost;
                    baseMilkUpgradeCount++;
                    SoundEffects.MilkSpent(0.95f);
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
                    if (milkSpent == null || milkSpent.Length < 3)
                        milkSpent = new BigDouble[3];

                    milkSpent[upgradeIndex] += amount;

                    SoundEffects.MilkSpent(0.95f);
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

            bool completed = ChallengeService.IsCompleted(activeChallengeIndex, point, prestigeCount, generatorCount);
            if (!completed) return;

            int completedIndex = activeChallengeIndex;
            challengesCompleted[activeChallengeIndex] = true;
            activeChallengeIndex = -1;

            if (prevCooldownDurationForChallenge.HasValue)
            {
                cooldownDuration = prevCooldownDurationForChallenge.Value;
                prevCooldownDurationForChallenge = null;
            }

            EnsureController();
            controller.State = controller.State with
            {
                ActiveChallengeIndex = -1,
                AscChallenges = challengesCompleted
            };

            RecalculatePointGain();

            if (point == BigDouble.Zero)
                point = BigDouble.One;

            ResetClickUiState();

            UpdateUI();
            SaveGame();
            SoundEffects.ChallengeComplete(0.95f);
            DialogService.InfoChallengeComplete(completedIndex);
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
        // Replace the GeneratorTimer_Tick handler body
        private void GeneratorTimer_Tick(object sender, EventArgs e)
        {
            if (generatorCount > 0)
            {
                labelPointsPerSecond.Visible = true;

                EnsureController();
                var passiveGain = controller.ApplyGeneratorTick(AscensionMultiplier, ApplySoftCap);
                ApplyControllerStateToFields();

                if (passiveGain > BigDouble.Zero)
                    SoundEffects.Gain(0.5f);

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
        private void UpdateUpgradeInfoLabel()
        {
            var vm = UpgradeInfoPresenter.Build(
                effectiveUpgradeCount: EffectiveUpgradeCount,
                prestigeEffect: GetPrestigeEffect(),
                prestigeIncrement: GetPrestigeIncrement(),
                softCapDivisor: GetSoftCapDivisor(point),
                challengesCompleted: challengesCompleted,
                activeChallengeIndex: activeChallengeIndex,
                milkSpent: milkSpent,
                ascensionMultiplier: AscensionMultiplier,
                prestigeCount: prestigeCount,
                format: FormatNumbers);

            if (labelUpgradeInfo.Text != vm.UpgradeInfoText)
                labelUpgradeInfo.Text = vm.UpgradeInfoText;

            if (labelPrestigeInfo.Text != vm.PrestigeInfoText)
                labelPrestigeInfo.Text = vm.PrestigeInfoText;
        }
        private void UpdateGeneratorInfo()
        {
            if (generatorCount == 0)
            {
                labelGeneratorInfo.Text = "Generators: 0 | Cost: 100";
                labelPointsPerSecond.Visible = false;
                return;
            }

            // When at hard cap, always show PPS as 0
            var hardCapPoint = GetSoftCapThreshold() * 1000;
            bool atHardCap = point >= hardCapPoint;

            BigDouble divisor = GetSoftCapDivisor(point);
            BigDouble pps = atHardCap
                ? BigDouble.Zero
                : BigDouble.Pow(10, generatorCount) * 0.01 * pointGain / (divisor <= BigDouble.Zero ? BigDouble.One : divisor);

            labelGeneratorInfo.Text = $"Generators: {generatorCount} | Cost: {FormatNumbers(generatorCost)} | Every generators 10x your current passive gain after the first";
            labelPointsPerSecond.Text = $"Points/second: {FormatNumbers(pps)}";
            labelPointsPerSecond.Visible = true;
        }
        // Number formatting now delegates to NumberFormatter (no behavior change)
        private string FormatNumbers(BigDouble value) => NumberFormatter.Format(value);
        // Replace ApplyOfflineProgress body
        private void ApplyOfflineProgress(DateTime lastSaved, DateTime serverNow)
        {
            TimeSpan offlineTime = serverNow - lastSaved;
            int seconds = (int)offlineTime.TotalSeconds;
            if (seconds <= 0) return;

            if (generatorCount <= 0)
            {
                bool hasPrestiged = prestigeCount > 0;
                MessageBox.Show(OfflineProgressPresenter.BuildNoGeneratorMessage(hasPrestiged));
                return;
            }

            EnsureController();
            var result = controller.ApplyOfflineProgress(
                seconds: seconds,
                milkSpent2: milkSpent[2],
                applySoftCap: (cur, gain) => ApplySoftCap(cur, gain));

            ApplyControllerStateToFields();

            if (result.PassiveGain > BigDouble.Zero)
            {
                SoundEffects.Gain(0.8f);
                MessageBox.Show(
                    OfflineProgressPresenter.BuildGainMessage(
                        result.PassiveGain,
                        result.Seconds,
                        result.EffectiveSeconds,
                        result.OfflineMultiplier,
                        FormatNumbers),
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

            SoundEffects.Prestige(0.9f);
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
                if (!DialogService.ConfirmAscendInChallenge(idx, objective))
                    return;
            }

            EnsureController();
            var cost = GetAscendCost();
            if (!controller.TryAscend(cost)) return;

            ApplyControllerStateToFields();

            SoundEffects.Ascend(0.9f);
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

            SoundEffects.GeneratorPurchase(0.8f);
            labelPoint.Text = FormatNumbers(point);
            UpdateUI();
            CheckChallengeCompletion();
        }
        private void buttonInfoDailyGain_Click(object sender, EventArgs e)
        {
            using var info = new DailyInfoWindow(baseMilkUpgradeCount, milkStreak);
            info.ShowDialog(this);
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