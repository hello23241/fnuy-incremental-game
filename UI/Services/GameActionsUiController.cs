using System;
using BreakInfinity;
using WinFormsApp1.Core.Game;
using WinFormsApp1.UI.Services;

namespace WinFormsApp1.UI.Controllers
{
    // Encapsulates game actions: click, upgrades, prestige, ascend, generator, transcend.
    // UI updates are passed via delegates; no direct control references here.
    public sealed class GameActionsUiController
    {
        private readonly Func<double> _getAscensionMultiplier;
        private readonly Func<BigDouble, BigDouble, BigDouble> _applySoftCap;
        private readonly Action _ensureController;
        private readonly Action _applyControllerStateToFields;

        private readonly Func<BigDouble> _getUpgradeCost;
        private readonly Func<BigDouble, BigDouble> _getEffectiveUpgradeDeduction;
        private readonly double _upgradeScale;

        private readonly Func<BigDouble> _getPrestigeCost;
        private readonly Func<BigDouble> _getAscendCost;

        private readonly Func<int> _getCooldownDuration;
        private readonly Action<int> _setCooldownDuration;

        private readonly Action _unlockPrestigeFeature;
        private readonly Action _unlockGeneratorFeature;
        private readonly Action _unlockAscensionFeature;

        private readonly Action _recalcPointGain;
        private readonly Func<int> _getEffectiveUpgradeCount;
        private readonly Action<string> _setUpgradeNoteText;

        private readonly Action _updateUI;
        private readonly Action _saveGame;
        private readonly Action _checkChallengeCompletion;

        // Transcend dependencies
        private readonly Func<BigDouble> _getTranscendCost;
        private readonly Func<int> _getBaseMilkUpgradeCount;
        private readonly Func<int> _getMilkStreak;
        private readonly Action<int> _setMilkStreak;
        private readonly Action<BigDouble> _addMilk;
        private readonly Action _onTranscendUiPerformed;
        private readonly Action<BigDouble, int> _showTranscendRewardDialog;

        private readonly GameController _controller;

        public GameActionsUiController(
            GameController controller,
            Func<double> getAscensionMultiplier,
            Func<BigDouble, BigDouble, BigDouble> applySoftCap,
            Action ensureController,
            Action applyControllerStateToFields,
            Func<BigDouble> getUpgradeCost,
            Func<BigDouble, BigDouble> getEffectiveUpgradeDeduction,
            double upgradeScale,
            Func<BigDouble> getPrestigeCost,
            Func<BigDouble> getAscendCost,
            Func<int> getCooldownDuration,
            Action<int> setCooldownDuration,
            Action unlockPrestigeFeature,
            Action unlockGeneratorFeature,
            Action unlockAscensionFeature,
            Action recalcPointGain,
            Func<int> getEffectiveUpgradeCount,
            Action<string> setUpgradeNoteText,
            Action updateUI,
            Action saveGame,
            Action checkChallengeCompletion,
            // Transcend
            Func<BigDouble> getTranscendCost,
            Func<int> getBaseMilkUpgradeCount,
            Func<int> getMilkStreak,
            Action<int> setMilkStreak,
            Action<BigDouble> addMilk,
            Action onTranscendUiPerformed,
            Action<BigDouble, int> showTranscendRewardDialog)
        {
            _controller = controller ?? throw new ArgumentNullException(nameof(controller));
            _getAscensionMultiplier = getAscensionMultiplier ?? throw new ArgumentNullException(nameof(getAscensionMultiplier));
            _applySoftCap = applySoftCap ?? throw new ArgumentNullException(nameof(applySoftCap));
            _ensureController = ensureController ?? throw new ArgumentNullException(nameof(ensureController));
            _applyControllerStateToFields = applyControllerStateToFields ?? throw new ArgumentNullException(nameof(applyControllerStateToFields));
            _getUpgradeCost = getUpgradeCost ?? throw new ArgumentNullException(nameof(getUpgradeCost));
            _getEffectiveUpgradeDeduction = getEffectiveUpgradeDeduction ?? throw new ArgumentNullException(nameof(getEffectiveUpgradeDeduction));
            _upgradeScale = upgradeScale;
            _getPrestigeCost = getPrestigeCost ?? throw new ArgumentNullException(nameof(getPrestigeCost));
            _getAscendCost = getAscendCost ?? throw new ArgumentNullException(nameof(getAscendCost));
            _getCooldownDuration = getCooldownDuration ?? throw new ArgumentNullException(nameof(getCooldownDuration));
            _setCooldownDuration = setCooldownDuration ?? throw new ArgumentNullException(nameof(setCooldownDuration));
            _unlockPrestigeFeature = unlockPrestigeFeature ?? throw new ArgumentNullException(nameof(unlockPrestigeFeature));
            _unlockGeneratorFeature = unlockGeneratorFeature ?? throw new ArgumentNullException(nameof(unlockGeneratorFeature));
            _unlockAscensionFeature = unlockAscensionFeature ?? throw new ArgumentNullException(nameof(unlockAscensionFeature));
            _recalcPointGain = recalcPointGain ?? throw new ArgumentNullException(nameof(recalcPointGain));
            _getEffectiveUpgradeCount = getEffectiveUpgradeCount ?? throw new ArgumentNullException(nameof(getEffectiveUpgradeCount));
            _setUpgradeNoteText = setUpgradeNoteText ?? throw new ArgumentNullException(nameof(setUpgradeNoteText));
            _updateUI = updateUI ?? throw new ArgumentNullException(nameof(updateUI));
            _saveGame = saveGame ?? throw new ArgumentNullException(nameof(saveGame));
            _checkChallengeCompletion = checkChallengeCompletion ?? throw new ArgumentNullException(nameof(checkChallengeCompletion));

            _getTranscendCost = getTranscendCost ?? throw new ArgumentNullException(nameof(getTranscendCost));
            _getBaseMilkUpgradeCount = getBaseMilkUpgradeCount ?? throw new ArgumentNullException(nameof(getBaseMilkUpgradeCount));
            _getMilkStreak = getMilkStreak ?? throw new ArgumentNullException(nameof(getMilkStreak));
            _setMilkStreak = setMilkStreak ?? throw new ArgumentNullException(nameof(setMilkStreak));
            _addMilk = addMilk ?? throw new ArgumentNullException(nameof(addMilk));
            _onTranscendUiPerformed = onTranscendUiPerformed ?? throw new ArgumentNullException(nameof(onTranscendUiPerformed));
            _showTranscendRewardDialog = showTranscendRewardDialog ?? throw new ArgumentNullException(nameof(showTranscendRewardDialog));
        }

        public void ClickOnce()
        {
            _ensureController();
            _controller.Click(_getAscensionMultiplier(), _applySoftCap);
            _applyControllerStateToFields();
            SoundEffects.Gain(0.75f);
            _updateUI();
        }

        public bool TryBuyUpgrade()
        {
            _ensureController();
            if (!_controller.TryBuyUpgrade(_getUpgradeCost, _getEffectiveUpgradeDeduction))
                return false;

            _applyControllerStateToFields();

            if (_getCooldownDuration() == 1000)
            {
                _unlockPrestigeFeature();
                _setCooldownDuration(500);
            }

            _recalcPointGain();
            _setUpgradeNoteText($"Upgrade count: {_getEffectiveUpgradeCount()}");
            SoundEffects.Upgrade(0.5f);
            _updateUI();
            return true;
        }

        public bool BuyMaxUpgrades()
        {
            _ensureController();
            var (purchased, _) = _controller.BuyMaxUpgrades(_getUpgradeCost, _upgradeScale, _getEffectiveUpgradeDeduction);
            if (purchased <= 0) return false;

            _applyControllerStateToFields();

            if (_getCooldownDuration() == 1000)
            {
                _unlockPrestigeFeature();
                _setCooldownDuration(500);
            }

            _recalcPointGain();
            _setUpgradeNoteText($"Upgrade count: {_getEffectiveUpgradeCount()}");
            SoundEffects.Upgrade(0.9f);
            _updateUI();
            _saveGame();
            return true;
        }

        public bool TryPrestige()
        {
            _ensureController();
            var cost = _getPrestigeCost();
            if (!_controller.TryPrestige(cost)) return false;

            _applyControllerStateToFields();

            SoundEffects.Prestige(0.9f);
            _unlockGeneratorFeature();
            _updateUI();
            _checkChallengeCompletion();
            return true;
        }

        public bool TryAscend()
        {
            _ensureController();
            var cost = _getAscendCost();
            if (!_controller.TryAscend(cost)) return false;

            _applyControllerStateToFields();

            SoundEffects.Ascend(0.9f);
            _unlockAscensionFeature();
            _updateUI();
            return true;
        }

        public bool TryBuyGenerator()
        {
            _ensureController();
            if (!_controller.TryBuyGenerator()) return false;

            _applyControllerStateToFields();

            SoundEffects.GeneratorPurchase(0.8f);
            _updateUI();
            _checkChallengeCompletion();
            return true;
        }

        public bool TryTranscend()
        {
            _ensureController();
            var cost = _getTranscendCost();
            int baseMilkUp = _getBaseMilkUpgradeCount();
            int currentStreak = _getMilkStreak();

            if (!_controller.TryTranscend(cost, baseMilkUp, currentStreak, out var milkEarned, out var newStreak))
                return false;

            SoundEffects.Transcend(0.95f);
            _onTranscendUiPerformed();

            _applyControllerStateToFields();

            _setMilkStreak(newStreak);
            _addMilk(milkEarned);

            _saveGame();
            _updateUI();

            try
            {
                SoundEffects.MilkEarned(0.95f);
                _showTranscendRewardDialog(milkEarned, newStreak);
            }
            catch { }

            return true;
        }
    }
}