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
        private Func<int, int, bool> spendMilkCallback; // (upgradeIndex, amount) => success
        private int baseMilkUpgradeCount;
        private int[] milkSpent;

        // Describe what each upgrade does here
        private readonly string[] upgradeNames = { "Boost 1", "Boost 2", "Offline gain boost", "Base milk gain" };
        private readonly string[] upgradeDescriptions =
        {
            "Slightly increase point multiplier",
            "Very slightly reduce soft cap (diminishing returns)",
            "Improve offline gain",
            "Increase daily milk reward"
        };

        public PremiumWindow(BigDouble milk, Func<int, int, bool> spendMilkCallback, int baseMilkUpgradeCount, int[] milkSpent)
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

            int upgradeCount = 3;
            labelUpgrades = new Label[upgradeCount];
            labelDescriptions = new Label[upgradeCount];
            customAmountBoxes = new TextBox[upgradeCount];
            customSpendButtons = new Button[upgradeCount];

            // Add array for spent labels
            Label[] labelSpent = new Label[upgradeCount];

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
                    Text = $"Milk spent: {(milkSpent != null && milkSpent.Length > i ? milkSpent[i] : 0)}",
                    Location = new Point(150, y + 35),
                    Size = new Size(200, 22),
                    ForeColor = Color.DarkSlateGray
                };
                this.Controls.Add(labelSpent[i]);

                // Custom amount textbox below the upgrade name/description
                customAmountBoxes[i] = new TextBox
                {
                    Location = new Point(20, y + 35),
                    Size = new Size(120, 28),
                    PlaceholderText = "Enter amount"
                };
                // Only allow digits
                customAmountBoxes[i].KeyPress += CustomAmountBox_KeyPress;
                this.Controls.Add(customAmountBoxes[i]);

                // Custom spend button next to the input
                customSpendButtons[i] = new Button
                {
                    Text = "Spend",
                    Location = new Point(20 + 120 + 10, y + 35),
                    Size = new Size(80, 28),
                    Tag = i
                };
                customSpendButtons[i].Click += CustomSpend_Click;
                this.Controls.Add(customSpendButtons[i]);
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
                Text = $"{upgradeDescriptions[3]} (Cost: {10 + baseMilkUpgradeCount * 2} milk, Owned: {baseMilkUpgradeCount})",
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
        private void CustomAmountBox_KeyPress(object sender, KeyPressEventArgs e)
        {
            // Allow control keys (e.g., backspace), and digits only
            if (!char.IsControl(e.KeyChar) && !char.IsDigit(e.KeyChar))
            {
                e.Handled = true;
            }
        }

        private void CustomSpend_Click(object sender, EventArgs e)
        {
            if (sender is Button btn && btn.Tag is int upgradeIndex)
            {
                if (int.TryParse(customAmountBoxes[upgradeIndex].Text, out int amount) && amount > 0)
                {
                    if (spendMilkCallback?.Invoke(upgradeIndex, amount) == true)
                    {
                        this.Close();
                    }
                    else
                    {
                        MessageBox.Show("Not enough milk.", "Info");
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
            int cost = 10 + baseMilkUpgradeCount * 2;
            if (milk >= cost)
            {
                bool success = spendMilkCallback?.Invoke(3, 1) ?? false;
                if (success)
                {
                    MessageBox.Show($"Bought 1 base milk upgrade for {cost} milk!\nYour base daily milk is now {9 + baseMilkUpgradeCount + 1}.", "Upgrade Purchased");
                    this.Close();
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