using System;
using BreakInfinity;
using WinFormsApp1.Core.Game;
using WinFormsApp1.UI.Services;

namespace WinFormsApp1.UI.Controllers
{
    // Encapsulates game actions: click, upgrades, prestige, ascend, generator purchase.
    // No UI controls are held; UI updates via delegates.
    public sealed class GameActionsController
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

        private readonly GameController _controller;

        public GameActionsController(
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
            Action checkChallengeCompletion)
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
    }
}