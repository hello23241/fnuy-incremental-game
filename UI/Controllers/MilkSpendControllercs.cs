using BreakInfinity;
using System;
using System.Windows.Forms;
using WinFormsApp1.Core.Game;
using WinFormsApp1.UI.Services;
using WinFormsApp1.UI.Views;

namespace WinFormsApp1.UI.Controllers
{
    // Orchestrates milk spending and Premium shop dialog.
    public sealed class MilkSpendController
    {
        private readonly Func<BigDouble> _getMilk;
        private readonly Action<BigDouble> _setMilk;

        private readonly Func<int> _getBaseMilkUpgradeCount;
        private readonly Action<int> _setBaseMilkUpgradeCount;

        private readonly Func<BigDouble[]> _getMilkSpent;
        private readonly Action<BigDouble[]> _setMilkSpent;

        private readonly Action _saveGame;
        private readonly Action _updateUI;

        public MilkSpendController(
            Func<BigDouble> getMilk,
            Action<BigDouble> setMilk,
            Func<int> getBaseMilkUpgradeCount,
            Action<int> setBaseMilkUpgradeCount,
            Func<BigDouble[]> getMilkSpent,
            Action<BigDouble[]> setMilkSpent,
            Action saveGame,
            Action updateUI)
        {
            _getMilk = getMilk ?? throw new ArgumentNullException(nameof(getMilk));
            _setMilk = setMilk ?? throw new ArgumentNullException(nameof(setMilk));
            _getBaseMilkUpgradeCount = getBaseMilkUpgradeCount ?? throw new ArgumentNullException(nameof(getBaseMilkUpgradeCount));
            _setBaseMilkUpgradeCount = setBaseMilkUpgradeCount ?? throw new ArgumentNullException(nameof(setBaseMilkUpgradeCount));
            _getMilkSpent = getMilkSpent ?? throw new ArgumentNullException(nameof(getMilkSpent));
            _setMilkSpent = setMilkSpent ?? throw new ArgumentNullException(nameof(setMilkSpent));
            _saveGame = saveGame ?? throw new ArgumentNullException(nameof(saveGame));
            _updateUI = updateUI ?? throw new ArgumentNullException(nameof(updateUI));
        }

        // Delegate that PremiumWindow will call for each purchase.
        public bool Spend(int upgradeIndex, BigDouble amount)
        {
            var milk = _getMilk();

            if (upgradeIndex == 3)
            {
                int cost = MilkShopService.GetBaseUpgradeCost(_getBaseMilkUpgradeCount());
                if (milk >= cost && amount == BigDouble.One)
                {
                    _setMilk(milk - cost);
                    _setBaseMilkUpgradeCount(_getBaseMilkUpgradeCount() + 1);
                    SoundEffects.MilkSpent(0.95f);
                    _saveGame();
                    _updateUI();
                    return true;
                }
                return false;
            }

            if (upgradeIndex >= 0 && upgradeIndex <= 2)
            {
                if (milk >= amount)
                {
                    _setMilk(milk - amount);
                    var spent = _getMilkSpent();
                    if (spent == null || spent.Length < 3)
                        spent = new BigDouble[3];

                    spent[upgradeIndex] += amount;
                    _setMilkSpent(spent);

                    SoundEffects.MilkSpent(0.95f);
                    _saveGame();
                    _updateUI();
                    return true;
                }
                return false;
            }

            return false;
        }

        public void OpenShop(Form owner)
        {
            using var shop = new PremiumWindow(_getMilk(), Spend, _getBaseMilkUpgradeCount(), _getMilkSpent());
            shop.ShowDialog(owner);
            _updateUI();
        }
    }
}