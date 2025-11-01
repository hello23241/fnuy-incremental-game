using System.Windows.Forms;
using BreakInfinity;

namespace WinFormsApp1.Core.Infrastructure;

public static class DialogService
{
    public static bool ConfirmStartChallenge(int index)
    {
        var res = MessageBox.Show(
            $"Start challenge {index + 1}?\n\nThis will perform an ascension reset while also resetting your generator count.\nYour generators will NOT be refunded!",
            "Start Challenge",
            MessageBoxButtons.YesNo,
            MessageBoxIcon.Information,
            MessageBoxDefaultButton.Button2);
        return res == DialogResult.Yes;
    }

    public static bool ConfirmAscendInChallenge(int index, string objective)
    {
        var res = MessageBox.Show(
            $"You're in challenge {index + 1} right now, the objective is to {objective}. Are you sure you want to ascend while in a challenge?",
            "Challenge in progress",
            MessageBoxButtons.YesNo,
            MessageBoxIcon.Warning,
            MessageBoxDefaultButton.Button2);
        return res == DialogResult.Yes;
    }

    public static void InfoChallengeCancelled()
    {
        MessageBox.Show("Challenge cancelled.", "Challenge", MessageBoxButtons.OK, MessageBoxIcon.Information);
    }

    public static void InfoChallengeComplete(int index)
    {
        MessageBox.Show($"Challenge {index + 1} completed!", "Challenge Complete", MessageBoxButtons.OK, MessageBoxIcon.Information);
    }

    public static void InfoTranscendReward(BigDouble milkEarned, int streak, System.Func<BigDouble, string> fmt)
    {
        MessageBox.Show(
            $"Transcend complete! You received {fmt(milkEarned)} milk as a bonus reward.\nCurrent streak: {streak}",
            "Transcend Reward",
            MessageBoxButtons.OK,
            MessageBoxIcon.Information);
    }

    public static void InfoChallengeStarted(int index, bool ascensionPerformed)
    {
        MessageBox.Show(
            ascensionPerformed
                ? $"Challenge {index + 1} started! (Ascension performed)"
                : $"Challenge {index + 1} started! (No ascension performed)",
            "Challenge Active",
            MessageBoxButtons.OK,
            MessageBoxIcon.Information);
    }
}