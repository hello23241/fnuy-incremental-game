using BreakInfinity;
using System.IO;
using Newtonsoft.Json;
using System.Net.Http;
using System.Reflection;

namespace WinFormsApp1
{
    public partial class MainForm : Form
    {
        // Core game state
        private BigDouble point = BigDouble.Zero;
        private BigDouble pointGain = BigDouble.One;
        private BigDouble PrestigeIncrement = new BigDouble(4);
        private BigDouble generatorCost = new BigDouble(100);
        private BigDouble ascensionPoints = BigDouble.Zero;
        private readonly BigDouble softcapThreshold = new BigDouble(1000);
        private System.Windows.Forms.Timer transcendFlashTimer;
        private int transcendFlashStep = 0;
        private readonly BigDouble transcendCost = new BigDouble(1_000_000_000);

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
        private bool[] upgradesBought = new bool[4];
        private int PermanentUpgradeBonus => upgradesBought != null && upgradesBought.Length > 1 && upgradesBought[0] ? 5 : 0;
        private int EffectiveUpgradeCount => upgradeCount + PermanentUpgradeBonus;

        // Timers and cooldowns
        private System.Windows.Forms.Timer generatorTimer;
        private System.Windows.Forms.Timer cooldownTimer;
        private System.Windows.Forms.Timer holdTimer;
        private int cooldownDuration = 1000; // ms
        private int cooldownElapsed = 0;
        private double offlineMultiplier = 0.05;
        private bool isCooldown = false;

        // Save path
        private readonly string savePath = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
            "FnuyIncrementalGame",
            "savegame.json");
        private Panel panelTitleBar;
        private Button buttonMinimize;
        private Button buttonClose;
        private Label labelTitle;

        public MainForm()
        {
            InitializeComponent();
            this.Text = "Myrtle incremental";
            this.Icon = Properties.Resources.NianBean;

            // --- Hold Timer for button1 ---
            holdTimer = new System.Windows.Forms.Timer();
            holdTimer.Tick += HoldTimer_Tick;
            button1.MouseDown += Button1_MouseDown;
            button1.MouseUp += Button1_MouseUp;
            button1.MouseLeave += Button1_MouseUp;

            // --- Custom Title Bar Panel ---
            panelTitleBar = new Panel();
            panelTitleBar.Size = new Size(this.ClientSize.Width, 28);
            panelTitleBar.Location = new Point(0, 0);
            panelTitleBar.BackColor = Color.FromArgb(24, 24, 24);
            panelTitleBar.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
            this.Controls.Add(panelTitleBar);
            panelTitleBar.BringToFront();

            // --- Title label ---
            labelTitle = new Label();
            labelTitle.Text = "Myrtle incremental";
            labelTitle.ForeColor = Color.Gainsboro;
            labelTitle.Font = new Font("Segoe UI", 12, FontStyle.Bold);
            labelTitle.Location = new Point(10, 4);
            labelTitle.AutoSize = true;
            panelTitleBar.Controls.Add(labelTitle);

            // --- Hold Timer for button1 ---
            button1.MouseDown += Button1_MouseDown;
            button1.MouseUp += Button1_MouseUp;
            button1.MouseLeave += Button1_MouseUp;

            //Transcend flash timer
            transcendFlashTimer = new System.Windows.Forms.Timer();
            transcendFlashTimer.Interval = 200;
            transcendFlashTimer.Tick += TranscendFlashTimer_Tick;

            // --- Custom Minimize Button ---
            buttonMinimize = new Button();
            buttonMinimize.Size = new Size(40, 28);
            buttonMinimize.Location = new Point(panelTitleBar.Width - 80, 0);
            buttonMinimize.Text = "—";
            buttonMinimize.FlatStyle = FlatStyle.Flat;
            buttonMinimize.BackColor = Color.FromArgb(32, 32, 32);
            buttonMinimize.ForeColor = Color.Gainsboro;
            buttonMinimize.FlatAppearance.BorderSize = 0;
            buttonMinimize.MouseEnter += (s, e) => buttonMinimize.BackColor = Color.FromArgb(64, 64, 64);
            buttonMinimize.MouseLeave += (s, e) => buttonMinimize.BackColor = Color.FromArgb(32, 32, 32);
            buttonMinimize.Click += (s, e) => this.WindowState = FormWindowState.Minimized;
            panelTitleBar.Controls.Add(buttonMinimize);

            // --- Custom Close Button ---
            buttonClose = new Button();
            buttonClose.Size = new Size(40, 28);
            buttonClose.Location = new Point(panelTitleBar.Width - 40, 0);
            buttonClose.Text = "X";
            buttonClose.FlatStyle = FlatStyle.Flat;
            buttonClose.BackColor = Color.FromArgb(32, 32, 32);
            buttonClose.ForeColor = Color.Gainsboro;
            buttonClose.FlatAppearance.BorderSize = 0;
            buttonClose.MouseEnter += (s, e) => buttonClose.BackColor = Color.FromArgb(64, 64, 64);
            buttonClose.MouseLeave += (s, e) => buttonClose.BackColor = Color.FromArgb(32, 32, 32);
            buttonClose.Click += (s, e) => this.Close();
            panelTitleBar.Controls.Add(buttonClose);

            // --- Adjust button positions on resize ---
            panelTitleBar.Resize += (s, e) =>
            {
                buttonMinimize.Location = new Point(panelTitleBar.Width - 80, 0);
                buttonClose.Location = new Point(panelTitleBar.Width - 40, 0);
            };

            // --- Dragging support on the panel ---
            panelTitleBar.MouseDown += panelTitleBar_MouseDown;
            panelTitleBar.MouseMove += panelTitleBar_MouseMove;
            panelTitleBar.MouseUp += panelTitleBar_MouseUp;

            // Remove system title bar
            this.FormBorderStyle = FormBorderStyle.None;

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
            buttonTranscend.Visible = labelTranscendCost.Visible = false;

#if DEBUG
            buttonDebug.Visible = true;
            buttonDebug.Click += buttonDebug_Click;
#endif
            LoadGame();
            labelCooldown.Text = "";
            CheckForUpdates();
            UpdateUI();
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
            labelPointGain.Text = $"Point Gain: {FormatNumbers(pointGain)}";
            labelTranscendCost.Text = $"Transcend Cost: {FormatNumbers(transcendCost)}";

            if (upgradeCount != 0)
            {
                if (PermanentUpgradeBonus > 0)
                    labelUpgradeNote.Text = $"Upgrade count: {EffectiveUpgradeCount} (+{PermanentUpgradeBonus} permanent)";
                else
                    labelUpgradeNote.Text = $"Upgrade count: {EffectiveUpgradeCount}";
                labelUpgradeNote.Visible = true;
            }
            else
            {
                labelUpgradeNote.Visible = false;
            }

            // Transcend button logic
            buttonTranscend.Enabled = point >= transcendCost;
            if (point >= transcendCost)
            {
                if (!transcendFlashTimer.Enabled)
                    transcendFlashTimer.Start();
            }
            else
            {
                transcendFlashTimer.Stop();
                buttonTranscend.BackColor = Color.Gray; // Match other disabled buttons
            }

            buttonUpgrade.Enabled = point >= GetUpgradeCost();
            buttonPrestige.Enabled = point >= GetPrestigeCost();
            buttonAscend.Enabled = point >= GetAscendCost();
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
        private void Button1_Click(object sender, EventArgs e)
        {
            if (isCooldown) return;
            BigDouble divisor = GetSoftCapDivisor(point);
            BigDouble gain = pointGain * AscensionMultiplier / divisor;
            point += gain;
            UpdateUI();
            isCooldown = true;
            cooldownElapsed = 0;
            cooldownTimer.Interval = 50; // Always 50ms for smooth updates
            cooldownTimer.Start();
            labelCooldown.Text = $"Cooldown: {EffectiveCooldownDuration / 1000.0:F2}s";
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
        private void Button1_MouseDown(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Left)
            {
                if (!isCooldown)
                    Button1_Click(sender, e); // Immediate click

                // Only allow hold if ascension count is 2 or more
                if (ascendCount >= 2)
                {
                    holdTimer.Interval = EffectiveCooldownDuration;
                    holdTimer.Start();
                }
            }
        }
        private void Button1_MouseUp(object sender, EventArgs e)
        {
            holdTimer.Stop();
        }

        private void HoldTimer_Tick(object sender, EventArgs e)
        {
            if (!isCooldown)
                Button1_Click(button1, EventArgs.Empty);
            holdTimer.Interval = EffectiveCooldownDuration; // Update in case ascendCount changed
        }

        private void buttonUpgrade_Click(object sender, EventArgs e)
        {
            var cost = GetUpgradeCost();
            BigDouble divisor = GetSoftCapDivisor(point);
            if (point >= cost)
            {
                point -= cost;
                upgradeCount++;
                double prestigeEffect = GetPrestigeEffect();
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
            double prestigeEffect = GetPrestigeEffect();
            BigDouble gain = 1 + pointGain * prestigeEffect * PrestigeIncrement / 100 / divisor;
            labelUpgradeInfo.Text = $"each upgrade adds {FormatNumbers(gain)} to your click multiplier";
            if (upgradesBought != null && upgradesBought.Length > 2 && upgradesBought[2])
                labelPrestigeInfo.Text = $"Prestige effect: log₂({prestigeCount + 1}) = {prestigeEffect:F2} ×1.25 (diminishing returns)";
            else
                labelPrestigeInfo.Text = $"Prestige effect: log₂({prestigeCount + 1}) = {prestigeEffect:F2} (diminishing returns)";
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
            }
        }
        private double GetPrestigeEffect()
        {
            double effect = BigDouble.Log2(prestigeCount + 1);
            if (upgradesBought != null && upgradesBought.Length > 2 && upgradesBought[2])
                effect *= 1.25; // Stronger prestiges
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
            }
        }

        private void GeneratorTimer_Tick(object sender, EventArgs e)
        {
            if (generatorCount > 0)
            {
                labelPointsPerSecond.Visible = true;
                BigDouble divisor = GetSoftCapDivisor(point);
                BigDouble passiveGain = Math.Pow(10, generatorCount) * 0.01 * pointGain * AscensionMultiplier / divisor;
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
        private BigDouble GetAscendCost() => baseAscendCost * BigDouble.Pow(ascendScale, ascendCount);

        // Soft cap
        private BigDouble GetSoftCapDivisor(BigDouble point)
        {
            double divisor = 1.0;
            if (upgradesBought != null && upgradesBought.Length > 1 && upgradesBought[1])
                divisor /= 3.0; // Less punishing upgrade

            if (point <= GetSoftCapThreshold())
                return BigDouble.One * divisor;

            double scale = BigDouble.Log10(point / GetSoftCapThreshold()) * 4;
            return (BigDouble.One + scale) * divisor;
        }
        private BigDouble GetSoftCapThreshold()
        {
            if (upgradesBought != null && upgradesBought.Length > 3 && upgradesBought[3])
                return new BigDouble(10000); // Lifted cap
            return softcapThreshold;
        }
        private void UpdateSoftCapLabel()
        {
            BigDouble divisor = GetSoftCapDivisor(point);
            if (point > GetSoftCapThreshold())
            {
                labelSoftCap.Visible = true;
                labelSoftCap.Text = $"Current points is over {FormatNumbers(GetSoftCapThreshold())}, gain is divided by {FormatNumbers(divisor)}";
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

        private void ApplyOfflineProgress(DateTime lastSaved)
        {
            TimeSpan offlineTime = DateTime.Now - lastSaved;
            BigDouble seconds = offlineTime.TotalSeconds;
            if (generatorCount <= 0 || seconds <= 0)
            {
                if (prestigeCount == 0)
                    MessageBox.Show("Welcome back! You currently don't own any generator for offline progress. Unlock it after your first prestige!");
                else
                    MessageBox.Show("Welcome back! You currently don't own any generator for offline progress.");
                return;
            }

            BigDouble divisor = GetSoftCapDivisor(point);
            BigDouble ratePerSecond = BigDouble.Pow(10, generatorCount) * 0.01 * pointGain * offlineMultiplier / divisor;
            BigDouble passiveGain = ratePerSecond * seconds;
            point += passiveGain;

            MessageBox.Show(
                $"Welcome back! You earned {FormatNumbers(passiveGain)} points while you were away.\n" +
                $"Current offline multi: x{offlineMultiplier}."
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
            pointGain *= 10;
            button1.Text = $"+{pointGain:F1} points";
            UpdateUI();
        }
#endif
        private void buttonOpenAscensionShop_Click(object sender, EventArgs e)
        {
            var shop = new AscensionShop(ascensionPoints, ascendCount, upgradesBought);
            var result = shop.ShowDialog();
            if (result == DialogResult.OK)
            {
                upgradesBought = shop.GetUpgradesBought();
                ascensionPoints = shop.GetRemainingAscensionPoints();
                SaveGame();
                UpdateUI();
            }
        }
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
                PurchasedAscensionUpgrades = upgradesBought,
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

            buttonOpenAscensionShop.Visible = buttonTranscend.Visible = labelTranscendCost.Visible = state.HasAscended;
        }

        private void LoadGame()
        {
            try
            {
                if (!File.Exists(savePath))
                {
                    SaveGame();
                    return;
                }

                string json = File.ReadAllText(savePath);
                if (string.IsNullOrWhiteSpace(json))
                {
                    SaveGame();
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
                if (state.PurchasedAscensionUpgrades != null && state.PurchasedAscensionUpgrades.Length == 4)
                    upgradesBought = state.PurchasedAscensionUpgrades;
                else
                    upgradesBought = new bool[4];

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
                // Show the transcend window
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
                int reductions = ascendCount / 2;
                double reductionPercent = reductions * 0.05;
                double effective = cooldownDuration * (1.0 - reductionPercent);
                // Minimum cooldown (optional, to prevent zero/negative)
                return Math.Max((int)effective, 50);
            }
        }
    }
}