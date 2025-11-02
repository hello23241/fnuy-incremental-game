using BreakInfinity;
using System.Reflection;
using System.ComponentModel;
using WinFormsApp1.Core.Formatting;
using WinFormsApp1.Core.Game;
using WinFormsApp1.SaveSystem;
using WinFormsApp1.Core.Updates;
using WinFormsApp1.Core.Persistence;
using WinFormsApp1.Core.Diagnostics;
using WinFormsApp1.Core.Infrastructure;
using WinFormsApp1.UI.Animations;
using WinFormsApp1.UI.Dialogs;
using WinFormsApp1.UI.Controllers;
using WinFormsApp1.UI.Presenters;
using WinFormsApp1.UI.Services;
using WinFormsApp1.UI.Appliers;
using GameStateDto = WinFormsApp1.SaveSystem.GameState;

namespace WinFormsApp1.UI.Views
{
    public partial class MainWindow : Form
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
        private int cooldownDuration = 1000; // ms
        private int? prevCooldownDurationForChallenge; // snapshot to restore after challenge
        private bool isCooldown = false;

        private Color defaultClickButtonForeColor;

        // Save path
        private readonly string savePath = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
            "FnuyIncrementalGame",
            "savegame.json");
        private readonly IGameSaveService saveService;
        private readonly GamePersistence persistence;
        private int activeChallengeIndex = -1;
        private CooldownUiController cooldownUi;
        //debug
        public MainWindow()
        {
            InitializeComponent();
            dragController = new FormDragController(this);
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

            this.Text = "Myrtle incremental";
            this.Icon = Properties.Resources.NianBean;

            // Wire events that can cause designer issues only at runtime
            try { buttonUpgrade.MouseDown += buttonUpgrade_RightClick; } catch { }

            //Transcend flash timer
            transcendFlashTimer = new System.Windows.Forms.Timer();
            // Smooth animation ~30 FPS
            transcendFlashTimer.Interval = 33;

            // initialize animator (tweak hueStepDeg/saturation/value to taste)
            transcendAnimator = new TranscendFlashAnimator(
                initialHueDeg: 0,
                hueStepDeg: 10.0,
                saturation: 0.95,
                value: 1.0);

            // SRP: controller owns the glow tick hookup/logic
            transcendUi = new TranscendUiController(buttonTranscend, transcendFlashTimer, transcendAnimator);
            this.Disposed += (_, __) => transcendUi.Dispose();

            // Cooldown for click
            cooldownTimer = new System.Windows.Forms.Timer { Interval = 50 };
            cooldownTimer.Tick += CooldownTimer_Tick;
            cooldownUi = new CooldownUiController(
                cooldownTimer,
                () => EffectiveCooldownDuration,
                button1,
                defaultClickButtonForeColor);
            // Timer for point generator
            generatorTimer = new System.Windows.Forms.Timer { Interval = 1000 };
            generatorTimer.Start();
            generatorLoop = new GeneratorLoopController(
                    generatorTimer,
                    hasGenerators: () => generatorCount > 0,
                    applyTick: () =>
                    {
                        EnsureController();
                        var passiveGain = controller.ApplyGeneratorTick(AscensionMultiplier, ApplySoftCap);
                        ApplyControllerStateToFields();
                        return passiveGain;
                    },
                    afterTick: passiveGain =>
                    {
                        if (passiveGain > BigDouble.Zero)
                            SoundEffects.Gain(0.5f);

                        labelPoint.Text = FormatNumbers(point);
                        UpdateUI();
                    }
                );
            autoClickLoop = new AutoClickLoopUiController(
                    timer: new System.Windows.Forms.Timer(),
                    barBg: autoclickBarBg,
                    barFill: autoclickBarFill,
                    getAscendCount: () => ascendCount,
                    getEffectiveCooldownMs: () => EffectiveCooldownDuration,
                    updateBar: UpdateAutoclickBar,
                    performClick: () =>
                    {
                        EnsureController();
                        controller.Click(AscensionMultiplier, ApplySoftCap);
                        ApplyControllerStateToFields();
                        SoundEffects.Gain(0.7f);
                        UpdateUI();
                    },
                    uiTickIntervalMs: 50
                );
            this.Disposed += (_, __) => autoClickLoop.Dispose();
            // Ensure controller detaches its event
            this.Disposed += (_, __) => generatorLoop.Dispose();
            autoBuy = new AutoBuyController(
                buttonAutoBuy,
                autoBuyTickIntervalMs: 1000,
                getAscendCount: () => ascendCount,
                tryBuyUpgrade: () =>
                {
                    EnsureController();
                    if (!controller.TryBuyUpgrade(GetUpgradeCost, GetEffectiveUpgradeDeduction))
                        return false;

                    ApplyControllerStateToFields();

                    if (cooldownDuration == 1000)
                    {
                        featureUnlock.UnlockPrestige();
                        cooldownDuration = 500;
                    }

                    RecalculatePointGain();
                    labelUpgradeNote.Text = $"Upgrade count: {EffectiveUpgradeCount}";
                    return true;
                },
                afterPurchase: () =>
                {
                    UpdateUI();
                    SaveGame();
                }
            );
            // Ensure internal timer is disposed with the form
            this.Disposed += (_, __) => autoBuy.Dispose();

            // Hide locked features by default
            buttonPrestige.Visible = labelPrestigeCost.Visible = labelPrestigeInfo.Visible = false;
            buttonGenerator.Visible = labelGeneratorInfo.Visible = labelSoftCap.Visible = false;
            buttonAscend.Visible = labelAscendCost.Visible = buttonOpenAscensionShop.Visible = false;
            buttonTranscend.Visible = labelTranscendCost.Visible = buttonPremiumShop.Visible = false;
            labelChallengeState.Visible = buttonInfoDailyGain.Visible = false;

            saveService = new GameSaveService(savePath);
            persistence = new GamePersistence(saveService);
            lifecycle = new GameLifecycleCoordinator(saveService, savePath);
            controller = new GameController(CaptureRuntimeState());
            challengeFlow = new ChallengeFlowController(
                    getActiveChallengeIndex: () => activeChallengeIndex,
                    setActiveChallengeIndex: v => activeChallengeIndex = v,
                    getChallengesCompleted: () => challengesCompleted,
                    setChallengesCompleted: v => challengesCompleted = v,
                    getCooldownMs: () => cooldownDuration,
                    setCooldownMs: v => cooldownDuration = v,
                    getPrevCooldownMs: () => prevCooldownDurationForChallenge,
                    setPrevCooldownMs: v => prevCooldownDurationForChallenge = v,
                    getPoint: () => point,
                    setPoint: v => point = v,
                    getPrestigeCount: () => prestigeCount,
                    getGeneratorCount: () => generatorCount,
                    getAscendCost: () => GetAscendCost(),
                    ensureController: () => EnsureController(),
                    applyControllerStateToFields: () =>
                    {
                        // delegate challenge start into controller, then sync fields
                        var cost = GetAscendCost();
                        bool canAscend = point >= cost;
                        controller.StartChallenge(canAscend, activeChallengeIndex);
                        ApplyControllerStateToFields();
                    },
                    updateControllerChallengeState: (idx, completed) =>
                    {
                        controller.State = controller.State with
                        {
                            ActiveChallengeIndex = idx,
                            AscChallenges = completed
                        };
                    },
                    unlockAscensionFeature: () => featureUnlock.UnlockAscension(),
                    resetClickUiState: () => ResetClickUiState(),
                    recalcPointGain: () => RecalculatePointGain(),
                    updateUI: () => UpdateUI(),
                    saveGame: () => SaveGame()
                );
            milkController = new MilkSpendController(
                    getMilk: () => milk,
                    setMilk: v => milk = v,
                    getBaseMilkUpgradeCount: () => baseMilkUpgradeCount,
                    setBaseMilkUpgradeCount: v => baseMilkUpgradeCount = v,
                    getMilkSpent: () => milkSpent,
                    setMilkSpent: v => milkSpent = v,
                    saveGame: () => SaveGame(),
                    updateUI: () => UpdateUI()
                );
            featureUnlock = new FeatureUnlockController(
                    // Prestige
                    buttonPrestige, labelPrestigeCost,
                    // Generator group
                    buttonGenerator, labelGeneratorInfo, labelSoftCap,
                    buttonAscend, labelAscendCost, labelPrestigeInfo,
                    buttonPremiumShop, buttonInfoDailyGain,
                    // Ascension group
                    buttonOpenAscensionShop, buttonTranscend, labelTranscendCost,
                    // State accessors
                    getHasUnlockedPremiumShop: () => hasUnlockedPremiumShop,
                    setHasUnlockedPremiumShop: v => hasUnlockedPremiumShop = v,
                    getMilk: () => milk,
                    setMilk: v => milk = v,
                    getLastMilkClaimDate: () => lastMilkClaimDate,
                    setLastMilkClaimDate: v => lastMilkClaimDate = v,
                    getMilkStreak: () => milkStreak,
                    setMilkStreak: v => milkStreak = v,
                    getBaseMilkUpgradeCount: () => baseMilkUpgradeCount,
                    // Persistence/UI
                    saveGame: () => SaveGame(),
                    updateUI: () => UpdateUI()
                );
            actions = BuildActionsController();
            LoadGame();
            CheckForUpdates();
            UpdateUI();
        }
        private GeneratorLoopController generatorLoop;
        private AutoBuyController autoBuy;
        private FormDragController dragController;
        private ChallengeFlowController challengeFlow;
        private TranscendUiController transcendUi;
        private AutoClickLoopUiController autoClickLoop;
        private GameLifecycleCoordinator lifecycle;
        private GameActionsUiController actions;
        private MilkSpendController milkController;
        private FeatureUnlockController featureUnlock;
        private void buttonAutoBuy_Click(object sender, EventArgs e)
        {
            autoBuy.Toggle();
            UpdateUI();
        }
        private void ResetClickUiState()
        {
            isCooldown = false;
            cooldownUi?.ResetVisuals();
            autoClickLoop?.Reset();
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
            await UpdateNotifier.CheckAsync(this);
        }
        private GameController controller;
        private void EnsureController()
        {
            if (controller == null)
                controller = new GameController(CaptureRuntimeState());
        }
        private GameActionsUiController BuildActionsController()
        {
            return new GameActionsUiController(
                controller: controller,
                getAscensionMultiplier: () => AscensionMultiplier,
                applySoftCap: (cur, gain) => ApplySoftCap(cur, gain),
                ensureController: () => EnsureController(),
                applyControllerStateToFields: () => ApplyControllerStateToFields(),
                getUpgradeCost: () => GetUpgradeCost(),
                getEffectiveUpgradeDeduction: baseCost => GetEffectiveUpgradeDeduction(baseCost),
                upgradeScale: upgradeScale,
                getPrestigeCost: () => GetPrestigeCost(),
                getAscendCost: () => GetAscendCost(),
                getCooldownDuration: () => cooldownDuration,
                setCooldownDuration: v => cooldownDuration = v,
                unlockPrestigeFeature: () => featureUnlock.UnlockPrestige(),
                unlockGeneratorFeature: () => featureUnlock.UnlockGenerator(),
                unlockAscensionFeature: () => featureUnlock.UnlockAscension(),
                recalcPointGain: () => RecalculatePointGain(),
                getEffectiveUpgradeCount: () => EffectiveUpgradeCount,
                setUpgradeNoteText: text => labelUpgradeNote.Text = text,
                updateUI: () => UpdateUI(),
                saveGame: () => SaveGame(),
                checkChallengeCompletion: () => challengeFlow.CheckCompletion(),
                // Transcend params
                getTranscendCost: () => transcendCost,
                getBaseMilkUpgradeCount: () => baseMilkUpgradeCount,
                getMilkStreak: () => milkStreak,
                setMilkStreak: v => milkStreak = v,
                addMilk: v => milk += v,
                onTranscendUiPerformed: () => transcendUi.OnTranscendPerformed(),
                showTranscendRewardDialog: (milkEarned, newStreak) =>
                    DialogService.InfoTranscendReward(milkEarned, newStreak, FormatNumbers)
            );
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

            // Apply HUD texts
            HudApplier.Apply(
                hud,
                decorated.ClickButtonText,
                decorated.PointGainText,
                labelPoint,
                button1,
                labelUpgradeCost,
                labelPrestigeCost,
                labelAscendCost,
                labelPointGain,
                labelTranscendCost,
                buttonPremiumShop,
                buttonTranscend,
                transcendFlashTimer,
                hud.CanTranscend);

            // Transcend visuals/interactivity (SRP)
            transcendUi.Apply(CanTranscendNow());

            autoClickLoop.ApplyUi(ascendCount);

            var ch = ChallengeTextPresenter.Build(activeChallengeIndex);
            labelChallengeState.Visible = ch.Visible;
            if (ch.Visible)
            {
                if (labelChallengeState.Text != ch.Text) labelChallengeState.Text = ch.Text;
                if (labelChallengeState.Font.Style != FontStyle.Bold)
                    labelChallengeState.Font = new Font(labelChallengeState.Font, FontStyle.Bold);
            }

            autoBuy.ApplyUi(ascendCount);
            this.ResumeLayout();
        }
        private void UpdateButtonStates()
        {
            ButtonStateApplier.Apply(
                point,
                GetUpgradeCost(),
                GetPrestigeCost(),
                GetAscendCost(),
                generatorCost,
                buttonUpgrade,
                buttonPrestige,
                buttonAscend,
                buttonGenerator);
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

            actions.ClickOnce();

            isCooldown = true;
            cooldownUi.Start();
            challengeFlow.CheckCompletion();
        }
        private void CooldownTimer_Tick(object sender, EventArgs e)
        {
            if (cooldownUi.OnTick())
                isCooldown = false;
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
            actions.TryBuyUpgrade();
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
                actions.BuyMaxUpgrades();
            };
            menu.Items.Add(buyMaxItem);
            menu.Show(buttonUpgrade, e.Location);
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
            challengeFlow.OpenAscensionShop(this, ascendCount);
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
            await lifecycle.LoadAsync(
                owner: this,
                applyRuntime: ApplyRuntimeStateFromLoaded,
                createOrResetController: () =>
                {
                    controller = new GameController(CaptureRuntimeState());
                    actions = BuildActionsController();
                },
                getServerUtcDateAsync: GetServerUtcDateAsync,
                applyOfflineProgress: ApplyOfflineProgress,
                applyUnlocks: ApplyUnlocks,
                recoverVisibility: () => UI.FeatureVisibilityRecovery.Recover(
                    prestigeCount, ascendCount, cooldownDuration,
                    buttonPrestige, labelPrestigeCost,
                    buttonGenerator, labelGeneratorInfo, labelSoftCap, labelPrestigeInfo,
                    buttonPremiumShop, buttonInfoDailyGain,
                    buttonAscend, labelAscendCost,
                    buttonOpenAscensionShop, buttonTranscend, labelTranscendCost),
                saveGame: SaveGame,
                updateUi: UpdateUI,
                logCrash: LogCrash,
                resetToDefaults: () =>
                {
                    point = BigDouble.Zero;
                    pointGain = BigDouble.One;
                    generatorCost = new BigDouble(100);
                    generatorCount = 0;
                    ascensionPoints = BigDouble.Zero;
                    cooldownDuration = 1000;
                }
            );
        }
        private void ApplyRuntimeStateFromLoaded(GameRuntimeState rs)
        {
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
        }
        private void ApplyUnlocks(GameStateDto state, DateTime? serverTime)
        {
            FeatureVisibilityApplier.Apply(
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

            // Safety: ensure visibility from progression counts in case of missing/older flags.
            UI.FeatureVisibilityRecovery.Recover(
                prestigeCount, ascendCount, cooldownDuration,
                buttonPrestige, labelPrestigeCost,
                buttonGenerator, labelGeneratorInfo, labelSoftCap, labelPrestigeInfo,
                buttonPremiumShop, buttonInfoDailyGain,
                buttonAscend, labelAscendCost,
                buttonOpenAscensionShop, buttonTranscend, labelTranscendCost);

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
        private void LogCrash(Exception ex)
        {
            CrashLogger.Log(ex);
        }
        // In buttonTranscend_Click, use DialogService for the reward dialog
        private void buttonTranscend_Click(object sender, EventArgs e)
        {
            actions.TryTranscend();
        }
        private void panelTitleBar_MouseDown(object sender, MouseEventArgs e)
        {
            dragController.OnMouseDown(e);
        }

        private void panelTitleBar_MouseMove(object sender, MouseEventArgs e)
        {
            dragController.OnMouseMove(e);
        }

        private void panelTitleBar_MouseUp(object sender, MouseEventArgs e)
        {
            dragController.OnMouseUp(e);
        }
        private double AscensionMultiplier => Math.Pow(2, ascendCount);
        private int EffectiveCooldownDuration
        {
            get
            {
                bool challenge2Completed = challengesCompleted != null && challengesCompleted.Length > 2 && challengesCompleted[2];
                int computed = CooldownService.ComputeEffectiveCooldownMs(
                    baseCooldownMs: cooldownDuration,
                    ascendCount: ascendCount,
                    activeChallengeIndex: activeChallengeIndex,
                    challenge2Completed: challenge2Completed);

                // Ascend milestone >= 7: fixes click cooldown to 2s in challenges 3 and 4 (0-based indices 2 and 3)
                if (ascendCount >= 7 && (activeChallengeIndex == 2 || activeChallengeIndex == 3))
                    return 2000;

                return computed;
            }
        }
        private void buttonPremiumShop_Click(object sender, EventArgs e)
        {
            milkController.OpenShop(this);
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
        private bool CanTranscendNow()
        {
            // Feature must be visible/unlocked and points must meet cost
            return buttonTranscend.Visible && point >= transcendCost;
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

            var hardCapPoint = GetSoftCapThreshold() * 1000;
            bool atHardCap = point >= hardCapPoint;

            BigDouble divisor = GetSoftCapDivisor(point);
            var vm = GeneratorInfoPresenter.Build(
                generatorCount,
                pointGain,
                divisor,
                generatorCost,
                atHardCap,
                FormatNumbers);

            labelGeneratorInfo.Text = vm.InfoText;
            labelPointsPerSecond.Text = vm.PpsText;
            labelPointsPerSecond.Visible = true;
        }
        // Number formatting now delegates to NumberFormatter (no behavior change)
        private string FormatNumbers(BigDouble value) => NumberFormatter.Format(value);
        private void ApplyOfflineProgress(DateTime lastSaved, DateTime serverNow)
        {
            bool applied = OfflineProgressApplier.TryApply(
                this,
                generatorCount,
                prestigeCount,
                lastSaved,
                serverNow,
                apply: secs =>
                {
                    EnsureController();
                    var res = controller.ApplyOfflineProgress(
                        seconds: secs,
                        milkSpent2: milkSpent[2],
                        applySoftCap: (cur, gain) => ApplySoftCap(cur, gain));

                    ApplyControllerStateToFields();
                    return res;
                },
                format: FormatNumbers);

            if (applied)
            {
                UpdateUI();
                SaveGame();
            }
        }
        private void buttonPrestige_Click(object sender, EventArgs e)
        {
            actions.TryPrestige();
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

            actions.TryAscend();
        }
        private string GetChallengeObjectivePlainText(int idx)
        {
            return ChallengeService.GetObjectivePlainText(idx);
        }
        private void buttonGenerator_Click(object sender, EventArgs e)
        {
            actions.TryBuyGenerator();
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