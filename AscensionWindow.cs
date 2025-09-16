using BreakInfinity;
using System.Drawing;
using System.Windows.Forms;
using WinFormsApp1.Properties;
namespace WinFormsApp1
{
    public partial class AscensionShop : Form
    {
        private BigDouble ascensionPoints;
        private bool[] upgradesBought = new bool[4];
        private int selectedUpgrade = -1;
        private BigDouble[] upgradeCosts = { 1, 1, 2, 3, }; // Testing costs
        public BigDouble GetRemainingAscensionPoints() => ascensionPoints;
        public bool[] GetUpgradesBought() => (bool[])upgradesBought.Clone();

        // Main constructor: all initialization logic goes here
        public AscensionShop(BigDouble points, int ascensionCount = 0)
        {
            ascensionPoints = points;
            InitializeComponent();
            // Load resources
            icon1.Image = Properties.Resources.icon1;
            icon2.Image = Properties.Resources.icon2;
            icon3.Image = Properties.Resources.icon3;
            icon4.Image = Properties.Resources.icon4;
            upgradePanel1.Controls.Add(icon1);
            upgradePanel2.Controls.Add(icon2);
            upgradePanel3.Controls.Add(icon3);
            upgradePanel4.Controls.Add(icon4);
            labelEffect.Text = GetUpgradeEffectText(-1);

            // Set dynamic text for boosts
            labelBoosts.Text = $"Ascension Count Boosts:\n" +
                $"You have {ascensionCount} ascensions.\n\n";

            if (ascensionCount >= 1)
            {
                double multiplier = Math.Pow(1.1, ascensionCount);
                labelBoosts.Text += $"1. [Active] Point multiplier: x{multiplier:F2} (1.1x per ascension)\n";
            }
            else
            {
                labelBoosts.Text += $"1. [Locked] Point multiplier: Unlocks at 1 ascension\n";
            }

            if (ascensionCount >= 2)
            {
                int reductions = ascensionCount / 2;
                double cooldownReduction = reductions * 5;
                labelBoosts.Text += $"2. [Active] Button cooldown reduced by {cooldownReduction}% ({reductions}×5%), can hold for points\n";
            }
            else
            {
                labelBoosts.Text += $"2. [Locked] Button cooldown reduction & hold: Unlocks at 2 ascensions\n";
            }

            labelPoints.Text = $"Ascension Points: {ascensionPoints}";

            // Event handlers
            buttonMinimize.MouseEnter += (s, e) => buttonMinimize.BackColor = Color.FromArgb(64, 96, 160);
            buttonMinimize.MouseLeave += (s, e) => buttonMinimize.BackColor = Color.FromArgb(32, 64, 128);
            buttonMinimize.Click += (s, e) => this.WindowState = FormWindowState.Minimized;

            buttonClose.MouseEnter += (s, e) => buttonClose.BackColor = Color.FromArgb(96, 32, 32);
            buttonClose.MouseLeave += (s, e) => buttonClose.BackColor = Color.FromArgb(32, 64, 128);
            buttonClose.Click += (s, e) => { this.DialogResult = DialogResult.OK; this.Close(); };

            panelTitleBar.MouseDown += panelTitleBar_MouseDown;
            panelTitleBar.MouseMove += panelTitleBar_MouseMove;
            panelTitleBar.MouseUp += panelTitleBar_MouseUp;

            panelTitleBar.Resize += (s, e) =>
            {
                buttonMinimize.Location = new Point(panelTitleBar.Width - 80, 0);
                buttonClose.Location = new Point(panelTitleBar.Width - 40, 0);
            };

            // Wire up click events
            upgradePanel1.Click += (s, e) => ShowUpgradeDetails(0);
            upgradePanel2.Click += (s, e) => ShowUpgradeDetails(1);
            upgradePanel3.Click += (s, e) => ShowUpgradeDetails(2);
            upgradePanel4.Click += (s, e) => ShowUpgradeDetails(3);
            buttonBuy.Click += (s, e) => BuySelectedUpgrade();
            icon1.Click += (s, e) => ShowUpgradeDetails(0);
            icon2.Click += (s, e) => ShowUpgradeDetails(1);
            icon3.Click += (s, e) => ShowUpgradeDetails(2);
            icon4.Click += (s, e) => ShowUpgradeDetails(3);
            UpdateUpgradePanels();
        }

        // Overload for loading upgrades
        public AscensionShop(BigDouble points, int ascensionCount, bool[] loadedUpgrades)
            : this(points, ascensionCount)
        {
            if (loadedUpgrades != null && loadedUpgrades.Length == 4)
                upgradesBought = (bool[])loadedUpgrades.Clone();
            UpdateUpgradePanels();
        }
        // --- Dragging logic (same as MainForm) ---
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

        private void ShowUpgradeDetails(int index)
        {
            selectedUpgrade = index;
            detailsPanel.Visible = true;
            labelEffect.Text = GetUpgradeEffectText(index);
            labelCost.Text = $"Cost: {upgradeCosts[index]} Ascension Points";
            buttonBuy.Enabled = !upgradesBought[index] && ascensionPoints >= upgradeCosts[index];
            buttonBuy.BackColor = upgradesBought[index] ? Color.LightGreen : SystemColors.Control;
            buttonBuy.Text = upgradesBought[index] ? "Bought" : "Buy";

            // Set all panels to default, then set bought ones to green
            upgradePanel1.BackColor = upgradesBought[0] ? Color.LightGreen : Color.Gray;
            upgradePanel2.BackColor = upgradesBought[1] ? Color.LightGreen : Color.Gray;
            upgradePanel3.BackColor = upgradesBought[2] ? Color.LightGreen : Color.Gray;
            upgradePanel4.BackColor = upgradesBought[3] ? Color.LightGreen : Color.Gray;
        }
        private void BuySelectedUpgrade()
        {
            if (selectedUpgrade < 0 || upgradesBought[selectedUpgrade]) return;
            if (ascensionPoints < upgradeCosts[selectedUpgrade]) return;

            ascensionPoints -= upgradeCosts[selectedUpgrade];
            upgradesBought[selectedUpgrade] = true;
            labelPoints.Text = $"Ascension Points: {ascensionPoints}";
            buttonBuy.BackColor = Color.LightGreen;
            buttonBuy.Text = "Bought";
            buttonBuy.Enabled = false;

            // Set the icon's panel to green
            switch (selectedUpgrade)
            {
                case 0: upgradePanel1.BackColor = Color.LightGreen; break;
                case 1: upgradePanel2.BackColor = Color.LightGreen; break;
                case 2: upgradePanel3.BackColor = Color.LightGreen; break;
                case 3: upgradePanel4.BackColor = Color.LightGreen; break;
            }
            // Optionally: apply the upgrade effect here

            // Optionally set DialogResult if you want to close after buying
            // this.DialogResult = DialogResult.OK;
        }
        private string GetUpgradeEffectText(int index)
        {
            switch (index)
            {
                case 0: return "Ascended upgrades\nGain 5 free upgrades on ascension.";
                case 1: return "Less punishing\nSoft cap is divided by 3.";
                case 2: return "Stronger prestiges\nPrestige effect is increased by 25%.";
                case 3: return "Lifted cap\nSoft cap threshold is increased to 10k points.";
                default: return "Choose an upgrade...";
            }
        }
        private void UpdateUpgradePanels()
        {
            upgradePanel1.BackColor = upgradesBought[0] ? Color.LightGreen : Color.Gray;
            upgradePanel2.BackColor = upgradesBought[1] ? Color.LightGreen : Color.Gray;
            upgradePanel3.BackColor = upgradesBought[2] ? Color.LightGreen : Color.Gray;
            upgradePanel4.BackColor = upgradesBought[3] ? Color.LightGreen : Color.Gray;
        }
    }
}