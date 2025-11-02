using BreakInfinity;
using System;
using System.Windows.Forms;
using WinFormsApp1.Core.Game;
using WinFormsApp1.UI.Services;

namespace WinFormsApp1.UI.Controllers
{
    // Centralizes feature unlocks + associated compensation/daily dialogs.
    public sealed class FeatureUnlockController
    {
        // Controls toggled by unlocks
        private readonly Button _buttonPrestige;
        private readonly Label _labelPrestigeCost;

        private readonly Button _buttonGenerator;
        private readonly Label _labelGeneratorInfo;
        private readonly Label _labelSoftCap;
        private readonly Button _buttonAscend;
        private readonly Label _labelAscendCost;
        private readonly Label _labelPrestigeInfo;
        private readonly Button _buttonPremiumShop;
        private readonly Button _buttonInfoDailyGain;

        private readonly Button _buttonOpenAscensionShop;
        private readonly Button _buttonTranscend;
        private readonly Label _labelTranscendCost;

        // State accessors
        private readonly Func<bool> _getHasUnlockedPremiumShop;
        private readonly Action<bool> _setHasUnlockedPremiumShop;
        private readonly Func<BigDouble> _getMilk;
        private readonly Action<BigDouble> _setMilk;
        private readonly Func<DateTime> _getLastMilkClaimDate;
        private readonly Action<DateTime> _setLastMilkClaimDate;
        private readonly Func<int> _getMilkStreak;
        private readonly Action<int> _setMilkStreak;
        private readonly Func<int> _getBaseMilkUpgradeCount;

        private readonly Action _saveGame;
        private readonly Action _updateUI;

        public FeatureUnlockController(
            // Prestige
            Button buttonPrestige,
            Label labelPrestigeCost,
            // Generator group
            Button buttonGenerator,
            Label labelGeneratorInfo,
            Label labelSoftCap,
            Button buttonAscend,
            Label labelAscendCost,
            Label labelPrestigeInfo,
            Button buttonPremiumShop,
            Button buttonInfoDailyGain,
            // Ascension group
            Button buttonOpenAscensionShop,
            Button buttonTranscend,
            Label labelTranscendCost,
            // State accessors
            Func<bool> getHasUnlockedPremiumShop,
            Action<bool> setHasUnlockedPremiumShop,
            Func<BigDouble> getMilk,
            Action<BigDouble> setMilk,
            Func<DateTime> getLastMilkClaimDate,
            Action<DateTime> setLastMilkClaimDate,
            Func<int> getMilkStreak,
            Action<int> setMilkStreak,
            Func<int> getBaseMilkUpgradeCount,
            // Persistence/UI
            Action saveGame,
            Action updateUI)
        {
            _buttonPrestige = buttonPrestige ?? throw new ArgumentNullException(nameof(buttonPrestige));
            _labelPrestigeCost = labelPrestigeCost ?? throw new ArgumentNullException(nameof(labelPrestigeCost));

            _buttonGenerator = buttonGenerator ?? throw new ArgumentNullException(nameof(buttonGenerator));
            _labelGeneratorInfo = labelGeneratorInfo ?? throw new ArgumentNullException(nameof(labelGeneratorInfo));
            _labelSoftCap = labelSoftCap ?? throw new ArgumentNullException(nameof(labelSoftCap));
            _buttonAscend = buttonAscend ?? throw new ArgumentNullException(nameof(buttonAscend));
            _labelAscendCost = labelAscendCost ?? throw new ArgumentNullException(nameof(labelAscendCost));
            _labelPrestigeInfo = labelPrestigeInfo ?? throw new ArgumentNullException(nameof(labelPrestigeInfo));
            _buttonPremiumShop = buttonPremiumShop ?? throw new ArgumentNullException(nameof(buttonPremiumShop));
            _buttonInfoDailyGain = buttonInfoDailyGain ?? throw new ArgumentNullException(nameof(buttonInfoDailyGain));

            _buttonOpenAscensionShop = buttonOpenAscensionShop ?? throw new ArgumentNullException(nameof(buttonOpenAscensionShop));
            _buttonTranscend = buttonTranscend ?? throw new ArgumentNullException(nameof(buttonTranscend));
            _labelTranscendCost = labelTranscendCost ?? throw new ArgumentNullException(nameof(labelTranscendCost));

            _getHasUnlockedPremiumShop = getHasUnlockedPremiumShop ?? throw new ArgumentNullException(nameof(getHasUnlockedPremiumShop));
            _setHasUnlockedPremiumShop = setHasUnlockedPremiumShop ?? throw new ArgumentNullException(nameof(setHasUnlockedPremiumShop));
            _getMilk = getMilk ?? throw new ArgumentNullException(nameof(getMilk));
            _setMilk = setMilk ?? throw new ArgumentNullException(nameof(setMilk));
            _getLastMilkClaimDate = getLastMilkClaimDate ?? throw new ArgumentNullException(nameof(getLastMilkClaimDate));
            _setLastMilkClaimDate = setLastMilkClaimDate ?? throw new ArgumentNullException(nameof(setLastMilkClaimDate));
            _getMilkStreak = getMilkStreak ?? throw new ArgumentNullException(nameof(getMilkStreak));
            _setMilkStreak = setMilkStreak ?? throw new ArgumentNullException(nameof(setMilkStreak));
            _getBaseMilkUpgradeCount = getBaseMilkUpgradeCount ?? throw new ArgumentNullException(nameof(getBaseMilkUpgradeCount));

            _saveGame = saveGame ?? throw new ArgumentNullException(nameof(saveGame));
            _updateUI = updateUI ?? throw new ArgumentNullException(nameof(updateUI));
        }

        public void UnlockPrestige()
        {
            _buttonPrestige.Visible = true;
            _labelPrestigeCost.Visible = true;
            _saveGame();
        }

        public void UnlockGenerator()
        {
            _buttonGenerator.Visible = true;
            _labelGeneratorInfo.Visible = true;
            _labelSoftCap.Visible = true;
            _buttonAscend.Visible = true;
            _labelAscendCost.Visible = true;
            _labelPrestigeInfo.Visible = true;
            _buttonPremiumShop.Visible = true;
            _buttonInfoDailyGain.Visible = true;

            // Handle compensation/daily award flow on first unlock or daily case
            var effects = UnlockOrchestrationService.HandleGeneratorUnlock(
                hasUnlockedPremiumShop: _getHasUnlockedPremiumShop(),
                lastMilkClaimDate: _getLastMilkClaimDate(),
                currentMilkStreak: _getMilkStreak(),
                baseMilkUpgradeCount: _getBaseMilkUpgradeCount(),
                nowLocalDate: DateTime.Now);

            if (effects.FirstTimeUnlock)
            {
                _setHasUnlockedPremiumShop(true);
                _setMilk(_getMilk() + effects.CompensationMilk);
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
                _setMilk(_getMilk() + effects.DailyMilk);
                _setMilkStreak(effects.NewMilkStreak);
                _setLastMilkClaimDate(effects.NewLastMilkClaimDate);
                try
                {
                    SoundEffects.MilkEarned(0.95f);
                    MessageBox.Show(
                        $"You earned {effects.DailyMilk} milk for logging in today!\n" +
                        $"Base gain: {_getBaseMilkUpgradeCount() * 10 + 100}\nStreak: {_getMilkStreak()} day(s)",
                        "Daily Reward", MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
                catch { }
            }

            _saveGame();
            _updateUI();
        }

        public void UnlockAscension()
        {
            _buttonOpenAscensionShop.Visible = true;
            _buttonTranscend.Visible = true;
            _labelTranscendCost.Visible = true;
            _saveGame();
        }
    }
}