using System.Windows.Forms;
using WinFormsApp1.UI.Views;

namespace WinFormsApp1.UI.Services
{
    public enum AscensionDialogOutcomeKind
    {
        None,
        Cancelled,
        StartChallenge
    }

    public sealed class AscensionDialogOutcome
    {
        public AscensionDialogOutcome(AscensionDialogOutcomeKind kind, int? newActiveChallengeIndex, bool[] challengesCompleted)
        {
            Kind = kind;
            NewActiveChallengeIndex = newActiveChallengeIndex;
            ChallengesCompleted = challengesCompleted;
        }
        public AscensionDialogOutcomeKind Kind { get; }
        public int? NewActiveChallengeIndex { get; }
        public bool[] ChallengesCompleted { get; }
    }

    public static class AscensionShopFlow
    {
        // Shows the dialog and returns what action the caller should perform.
        public static AscensionDialogOutcome Run(IWin32Window owner, int ascendCount, bool[] challengesCompleted, int activeChallengeIndex)
        {
            var window = new AscensionWindow(ascendCount, challengesCompleted, activeChallengeIndex);
            var result = window.ShowDialog(owner);

            if (result != DialogResult.OK)
                return new AscensionDialogOutcome(AscensionDialogOutcomeKind.None, null, challengesCompleted);

            var updated = window.GetChallengesCompleted();

            if (window.ChallengeCancelled)
            {
                return new AscensionDialogOutcome(AscensionDialogOutcomeKind.Cancelled, -1, updated);
            }

            if (window.ChallengeActive && window.ActiveChallengeIndex != activeChallengeIndex)
            {
                return new AscensionDialogOutcome(AscensionDialogOutcomeKind.StartChallenge, window.ActiveChallengeIndex, updated);
            }

            return new AscensionDialogOutcome(AscensionDialogOutcomeKind.None, null, updated);
        }
    }
}