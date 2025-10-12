using System;
using System.Windows.Forms;
using System.Drawing;
using BreakInfinity;

namespace WinFormsApp1
{
    public class PremiumWindow : Form
    {
        private Label labelCurrency;
        private Label[] labelUpgrades;
        private Label[] labelDescriptions;
        private TextBox[] customAmountBoxes;
        private Button[] customSpendButtons;
        private Button buttonBaseMilkUpgrade;
        private Label labelBaseMilkDesc;
        private BigDouble milk;
        private Func<int, BigDouble, bool> spendMilkCallback; // (upgradeIndex, amount) => success
        private int baseMilkUpgradeCount;
        private BigDouble[] milkSpent;
        private Label[] labelSpent;

        // Describe what each upgrade does here
        private readonly string[] upgradeNames = { "Boost 1", "Boost 2", "Offline gain boost", "Base milk gain" };
        private readonly string[] upgradeDescriptions =
        {
            "Slightly increase point multiplier",
            "Slightly reduce soft cap (diminishing returns)",
            "Improve offline gain",
            "Increase daily milk reward"
        };

        public PremiumWindow(BigDouble milk, Func<int, BigDouble, bool> spendMilkCallback, int baseMilkUpgradeCount, BigDouble[] milkSpent)
        {
            this.milk = milk;
            this.spendMilkCallback = spendMilkCallback;
            this.baseMilkUpgradeCount = baseMilkUpgradeCount;
            this.milkSpent = milkSpent;

            this.Text = "Premium Shop";
            this.Size = new Size(500, 420); // Height for 4 upgrades
            this.FormBorderStyle = FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.StartPosition = FormStartPosition.CenterParent;

            labelCurrency = new Label
            {
                Text = $"Milk: {milk}",
                Location = new Point(20, 20),
                Size = new Size(360, 30),
                Font = new Font("Segoe UI", 11, FontStyle.Bold)
            };
            this.Controls.Add(labelCurrency);

            // --- Info icon button in top right ---
            var infoButton = new Button
            {
                Size = new Size(28, 28),
                Location = new Point(this.ClientSize.Width - 36, 8), // 8px from top, 8px from right
                FlatStyle = FlatStyle.Flat,
                Text = "ⓘ",
                Font = new Font("Segoe UI", 14, FontStyle.Bold),
                BackColor = Color.Transparent,
                TabStop = false
            };
            infoButton.FlatAppearance.BorderSize = 0;
            infoButton.Click += (s, e) =>
            {
                BigDouble boost1 = milkSpent != null && milkSpent.Length > 0 ? milkSpent[0] : BigDouble.Zero;
                BigDouble boost2 = milkSpent != null && milkSpent.Length > 1 ? milkSpent[1] : BigDouble.Zero;
                BigDouble boost3 = milkSpent != null && milkSpent.Length > 2 ? milkSpent[2] : BigDouble.Zero;

                // Boost 1: Increases point gain by milkSpent[0] * 1%
                double boost1Percent = boost1.ToDouble() * 0.01;
                // Boost 2: Divides soft cap by log1.1(milkSpent[1]) * 1%
                double boost2Divisor = boost2 > BigDouble.Zero ? Math.Min(Math.Log(boost2.ToDouble(), 1.1), boost2.ToDouble()) * 0.01 + 1 : 1;
                // Boost 3: Increases offline gain by +(0.001 * milkSpent[2]), capped at 1
                double boost3Multi = Math.Min(1, boost3.ToDouble() * 0.001);

                MessageBox.Show(
                    $"Current boosts:\n" +
                    $"Boost 1: +{(int)boost1Percent}% point gain ({boost1} milk spent)\n" +
                    $"Boost 2: Soft cap is divided by {boost2Divisor:F4} ({boost2} milk spent)\n" +
                    $"Boost 3: Offline gain multi: +{boost3Multi:F3} ({boost3} milk spent, max 950)\n" +
                    $"Base offline gain: log₁.₀₁(x) × 0.05, x = seconds since last login",
                    "Boost details",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Information
                );
            };
            this.Controls.Add(infoButton);
            infoButton.BringToFront();

            int upgradeCount = 3;
            labelUpgrades = new Label[upgradeCount];
            labelDescriptions = new Label[upgradeCount];
            customAmountBoxes = new TextBox[upgradeCount];
            customSpendButtons = new Button[upgradeCount];

            // Add array for spent labels
            labelSpent = new Label[upgradeCount];

            for (int i = 0; i < upgradeCount; i++)
            {
                int y = 60 + i * 80;

                // Upgrade name
                labelUpgrades[i] = new Label
                {
                    Text = $"{upgradeNames[i]}",
                    Location = new Point(20, y),
                    Size = new Size(120, 30)
                };
                this.Controls.Add(labelUpgrades[i]);

                // Description to the right of the upgrade name
                labelDescriptions[i] = new Label
                {
                    Text = upgradeDescriptions[i],
                    Location = new Point(150, y),
                    Size = new Size(320, 30),
                    ForeColor = Color.DimGray
                };
                this.Controls.Add(labelDescriptions[i]);

                // Spent label below the description
                labelSpent[i] = new Label
                {
                    Text = $"Milk spent: {(milkSpent != null && milkSpent.Length > i ? milkSpent[i] : BigDouble.Zero)}",
                    Location = new Point(150, y + 35),
                    Size = new Size(200, 22),
                    ForeColor = Color.DarkSlateGray
                };
                this.Controls.Add(labelSpent[i]);

                if (i == 2 && milkSpent != null && milkSpent.Length > 2 && milkSpent[2] >= new BigDouble(950))
                {
                    // Boost 3 is maxed: remove textbox, grey out button, set text to "Maxed"
                    customSpendButtons[i] = new Button
                    {
                        Text = "Maxed",
                        Location = new Point(140 + 200 + 10, y + 30),
                        Size = new Size(80, 28),
                        Tag = i,
                        Enabled = false,
                        BackColor = Color.LightGray,
                        ForeColor = Color.DimGray
                    };
                    this.Controls.Add(customSpendButtons[i]);
                    // Do not add a textbox for boost 3
                    customAmountBoxes[i] = null;
                }
                else
                {
                    // Custom amount textbox below the upgrade name/description
                    customAmountBoxes[i] = new TextBox
                    {
                        Location = new Point(20, y + 35),
                        Size = new Size(120, 28),
                        PlaceholderText = "Enter amount"
                    };
                    // Allow digits, decimal, E/e, and control keys
                    customAmountBoxes[i].KeyPress += CustomAmountBox_KeyPress_BigDouble;
                    this.Controls.Add(customAmountBoxes[i]);

                    // Custom spend button next to the input
                    customSpendButtons[i] = new Button
                    {
                        Text = "Spend",
                        Location = new Point(140 + 200 + 10, y + 30),
                        Size = new Size(80, 28),
                        Tag = i
                    };
                    customSpendButtons[i].Click += CustomSpend_Click_BigDouble;
                    this.Controls.Add(customSpendButtons[i]);
                }
            }

            // --- Base milk gain upgrade (single purchase) ---
            int yBase = 60 + upgradeCount * 80;

            var labelBaseMilk = new Label
            {
                Text = upgradeNames[3],
                Location = new Point(20, yBase),
                Size = new Size(120, 30)
            };
            this.Controls.Add(labelBaseMilk);

            labelBaseMilkDesc = new Label
            {
                Text = $"{upgradeDescriptions[3]} (Cost: {100 + baseMilkUpgradeCount * 20} milk, Owned: {baseMilkUpgradeCount})",
                Location = new Point(150, yBase),
                Size = new Size(320, 30),
                ForeColor = Color.DimGray
            };
            this.Controls.Add(labelBaseMilkDesc);

            buttonBaseMilkUpgrade = new Button
            {
                Text = "Purchase",
                Location = new Point(20, yBase + 35),
                Size = new Size(120, 28)
            };
            buttonBaseMilkUpgrade.Click += ButtonBaseMilkUpgrade_Click;
            this.Controls.Add(buttonBaseMilkUpgrade);
        }
        
        // Helper to refresh displayed labels after a successful purchase
        private void RefreshUIAfterPurchase(int upgradeIndex, BigDouble amount)
        {
            // Update local milk estimate
            try
            {
                milk -= amount;
            }
            catch { }

            labelCurrency.Text = $"Milk: {milk}";

            // Update spent labels (milkSpent is a shared array reference)
            if (labelSpent != null && milkSpent != null)
            {
                for (int i = 0; i < labelSpent.Length; i++)
                {
                    labelSpent[i].Text = $"Milk spent: {(milkSpent.Length > i ? milkSpent[i] : BigDouble.Zero)}";
                }
            }

            // If boost 3 reached cap, adjust controls
            if (milkSpent != null && milkSpent.Length > 2 && milkSpent[2] >= new BigDouble(950))
            {
                if (customAmountBoxes[2] != null)
                    customAmountBoxes[2].Visible = false;
                if (customSpendButtons[2] != null)
                {
                    customSpendButtons[2].Text = "Maxed";
                    customSpendButtons[2].Enabled = false;
                    customSpendButtons[2].BackColor = Color.LightGray;
                    customSpendButtons[2].ForeColor = Color.DimGray;
                }
            }

            // If base milk upgraded, increment local count and update description
            if (upgradeIndex == 3)
            {
                baseMilkUpgradeCount++;
                labelBaseMilkDesc.Text = $"{upgradeDescriptions[3]} (Cost: {100 + baseMilkUpgradeCount * 20} milk, Owned: {baseMilkUpgradeCount})";
            }
        }

         // Allow digits, decimal, E/e, and control keys for BigDouble
         private void CustomAmountBox_KeyPress_BigDouble(object sender, KeyPressEventArgs e)
         {
             if (!char.IsControl(e.KeyChar) &&
                 !char.IsDigit(e.KeyChar) &&
                 e.KeyChar != '.' &&
                 e.KeyChar != 'E' &&
                 e.KeyChar != 'e' &&
                 e.KeyChar != '+')
             {
                 e.Handled = true;
             }
         }

         private void CustomSpend_Click_BigDouble(object sender, EventArgs e)
         {
             if (sender is Button btn && btn.Tag is int upgradeIndex)
             {
                 // Prevent spending on boost 3 if already maxed
                 if (upgradeIndex == 2 && milkSpent != null && milkSpent.Length > 2 && milkSpent[2] >= new BigDouble(950))
                 {
                     MessageBox.Show("Boost 3 is maxed out.", "Info");
                     return;
                 }

                 var text = customAmountBoxes[upgradeIndex]?.Text;
                 BigDouble amount;
                 try
                 {
                     amount = BigDouble.Parse(text);
                 }
                 catch
                 {
                     MessageBox.Show("Enter a number.", "Invalid character");
                     return;
                 }
                 if (amount > BigDouble.Zero)
                 {
                     // For boost 3, cap spending so total does not exceed 950
                     if (upgradeIndex == 2)
                     {
                         BigDouble currentSpent = milkSpent != null && milkSpent.Length > 2 ? milkSpent[2] : BigDouble.Zero;
                         if (currentSpent + amount > new BigDouble(950))
                         {
                             MessageBox.Show("You can only spend up to 950 milk on Boost 3.", "Limit Reached");
                             return;
                         }
                     }

                     if (spendMilkCallback?.Invoke(upgradeIndex, amount) == true)
                     {
                        // Do not close the window; refresh local UI to reflect purchase
                        RefreshUIAfterPurchase(upgradeIndex, amount);
                        MessageBox.Show("Purchase successful.", "Success", MessageBoxButtons.OK, MessageBoxIcon.Information);
                     }
                     else
                     {
                         MessageBox.Show("Not enough milk for this purchase.", "Insufficient milk");
                     }
                 }
                 else
                 {
                     MessageBox.Show("Enter a valid amount.", "Input Error");
                 }
             }
         }

         private void ButtonBaseMilkUpgrade_Click(object sender, EventArgs e)
         {
             int cost = 100 + baseMilkUpgradeCount * 20;
             if (milk >= cost)
             {
                 bool success = spendMilkCallback?.Invoke(3, BigDouble.One) ?? false;
                 if (success)
                 {
                    // Reflect the purchase locally and update UI, do not close
                    RefreshUIAfterPurchase(3, BigDouble.One);
                    MessageBox.Show($"Bought 1 base milk upgrade for {cost} milk!\nYour base daily milk is now {100 + baseMilkUpgradeCount * 10}.", "Upgrade Purchased");
                 }
                 else
                 {
                     MessageBox.Show("Not enough milk for this upgrade.", "Info");
                 }
             }
             else
             {
                 MessageBox.Show("Not enough milk for this upgrade.", "Info");
             }
         }
     }
 }