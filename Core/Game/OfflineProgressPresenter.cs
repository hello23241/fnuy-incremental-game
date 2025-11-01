using BreakInfinity;

namespace WinFormsApp1.Core.Game;

public static class OfflineProgressPresenter
{
    public static string BuildGainMessage(
        BigDouble passiveGain,
        int seconds,
        int effectiveSeconds,
        double offlineMultiplier,
        System.Func<BigDouble, string> format)
    {
        return
            $"Welcome back! You earned {format(passiveGain)} points while you were away for {seconds}s.\n" +
            $"Effective time was {effectiveSeconds}s\n" +
            $"Current offline multi: x{offlineMultiplier}.";
    }

    public static string BuildNoGeneratorMessage(bool hasPrestiged)
    {
        return hasPrestiged
            ? "Welcome back! You currently don't own any generator for offline progress."
            : "Welcome back! You currently don't own any generator for offline progress. Unlock it after your first prestige!";
    }
}