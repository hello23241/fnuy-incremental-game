using BreakInfinity;
using System;
using System.Windows.Forms;
using WinFormsApp1.Core.Game;
using WinFormsApp1.Core.Infrastructure;
using WinFormsApp1.UI.Dialogs;
using WinFormsApp1.UI.Services;

namespace WinFormsApp1.UI.Controllers
{
    // Orchestrates starting/cancelling/completing challenges.
    // Holds no game state; relies on delegates to read/update state and call into services.
    public sealed class ChallengeFlowController
    {
        private readonly Func<int> _getActiveChallengeIndex;
        private readonly Action<int> _setActiveChallengeIndex;
        private readonly Func<bool[]> _getChallengesCompleted;
        private readonly Action<bool[]> _setChallengesCompleted;

        private readonly Func<int> _getCooldownMs;
        private readonly Action<int> _setCooldownMs;
        private readonly Func<int?> _getPrevCooldownMs;
        private readonly Action<int?> _setPrevCooldownMs;

        private readonly Func<BigDouble> _getPoint;
        private readonly Action<BigDouble> _setPoint;
        private readonly Func<int> _getPrestigeCount;
        private readonly Func<int> _getGeneratorCount;

        private readonly Func<BigDouble> _getAscendCost;

        private readonly Action _ensureController;
        private readonly Action _applyControllerStateToFields;
        private readonly Action<int, bool[]> _updateControllerChallengeState;

        private readonly Action _unlockAscensionFeature;
        private readonly Action _resetClickUiState;
        private readonly Action _recalcPointGain;
        private readonly Action _updateUI;
        private readonly Action _saveGame;

        public ChallengeFlowController(
            Func<int> getActiveChallengeIndex,
            Action<int> setActiveChallengeIndex,
            Func<bool[]> getChallengesCompleted,
            Action<bool[]> setChallengesCompleted,
            Func<int> getCooldownMs,
            Action<int> setCooldownMs,
            Func<int?> getPrevCooldownMs,
            Action<int?> setPrevCooldownMs,
            Func<BigDouble> getPoint,
            Action<BigDouble> setPoint,
            Func<int> getPrestigeCount,
            Func<int> getGeneratorCount,
            Func<BigDouble> getAscendCost,
            Action ensureController,
            Action applyControllerStateToFields,
            Action<int, bool[]> updateControllerChallengeState,
            Action unlockAscensionFeature,
            Action resetClickUiState,
            Action recalcPointGain,
            Action updateUI,
            Action saveGame)
        {
            _getActiveChallengeIndex = getActiveChallengeIndex;
            _setActiveChallengeIndex = setActiveChallengeIndex;
            _getChallengesCompleted = getChallengesCompleted;
            _setChallengesCompleted = setChallengesCompleted;

            _getCooldownMs = getCooldownMs;
            _setCooldownMs = setCooldownMs;
            _getPrevCooldownMs = getPrevCooldownMs;
            _setPrevCooldownMs = setPrevCooldownMs;

            _getPoint = getPoint;
            _setPoint = setPoint;
            _getPrestigeCount = getPrestigeCount;
            _getGeneratorCount = getGeneratorCount;

            _getAscendCost = getAscendCost;

            _ensureController = ensureController;
            _applyControllerStateToFields = applyControllerStateToFields;
            _updateControllerChallengeState = updateControllerChallengeState;

            _unlockAscensionFeature = unlockAscensionFeature;
            _resetClickUiState = resetClickUiState;
            _recalcPointGain = recalcPointGain;
            _updateUI = updateUI;
            _saveGame = saveGame;
        }

        public void OpenAscensionShop(Form owner, int ascendCount)
        {
            var outcome = AscensionShopFlow.Run(owner, ascendCount, _getChallengesCompleted(), _getActiveChallengeIndex());

            // persist updated completion checkboxes from dialog
            _setChallengesCompleted(outcome.ChallengesCompleted);
            _saveGame();
            _updateUI();

            switch (outcome.Kind)
            {
                case AscensionDialogOutcomeKind.Cancelled:
                    _setActiveChallengeIndex(-1);

                    _ensureController();
                    _updateControllerChallengeState(-1, _getChallengesCompleted());

                    if (_getPrevCooldownMs().HasValue)
                    {
                        _setCooldownMs(_getPrevCooldownMs()!.Value);
                        _setPrevCooldownMs(null);
                    }

                    _recalcPointGain();
                    _updateUI();
                    _saveGame();
                    SoundEffects.ChallengeCancelled(0.9f);
                    DialogService.InfoChallengeCancelled();
                    break;

                case AscensionDialogOutcomeKind.StartChallenge:
                    {
                        int idx = outcome.NewActiveChallengeIndex!.Value;

                        if (!DialogService.ConfirmStartChallenge(idx))
                            return;

                        StartChallenge(idx);
                        break;
                    }

                case AscensionDialogOutcomeKind.None:
                default:
                    break;
            }
        }

        public void StartChallenge(int challengeIndex)
        {
            var cost = _getAscendCost();
            bool canAscend = _getPoint() >= cost;

            // 1) Record previous cooldown
            _setPrevCooldownMs(_getCooldownMs());

            // 2) Set the active challenge index BEFORE delegating into the game controller
            _setActiveChallengeIndex(challengeIndex);

            // 3) Let the host call into GameController.StartChallenge(...) and sync fields back
            _ensureController();
            _applyControllerStateToFields();

            // 4) Reset local UI state and unlock features as before
            _resetClickUiState();
            _unlockAscensionFeature();
            _updateUI();
            _saveGame();

            SoundEffects.ChallengeStart(0.9f);
            DialogService.InfoChallengeStarted(challengeIndex, canAscend);
        }

        public void CheckCompletion()
        {
            int activeIdx = _getActiveChallengeIndex();
            if (activeIdx == -1)
                return;

            bool completed = ChallengeService.IsCompleted(
                activeIdx,
                _getPoint(),
                _getPrestigeCount(),
                _getGeneratorCount());

            if (!completed) return;

            int completedIndex = activeIdx;
            var comp = _getChallengesCompleted();
            comp[activeIdx] = true;
            _setChallengesCompleted(comp);
            _setActiveChallengeIndex(-1);

            if (_getPrevCooldownMs().HasValue)
            {
                _setCooldownMs(_getPrevCooldownMs()!.Value);
                _setPrevCooldownMs(null);
            }

            _ensureController();
            _updateControllerChallengeState(-1, _getChallengesCompleted());

            _recalcPointGain();

            if (_getPoint() == BigDouble.Zero)
                _setPoint(BigDouble.One);

            _resetClickUiState();

            _updateUI();
            _saveGame();
            SoundEffects.ChallengeComplete(0.95f);
            DialogService.InfoChallengeComplete(completedIndex);
        }
    }
}