using BreakInfinity;

namespace WinFormsApp1.Core.Game;

public readonly record struct OfflineProgressResult(BigDouble PassiveGain, int Seconds, int EffectiveSeconds, double OfflineMultiplier);

public static class OfflineProgressService
{
    public static int ComputeEffectiveSeconds(int seconds)
    {
        return Math.Min((int)BigDouble.Log(seconds, 1.01), seconds);
    }

    public static double ComputeOfflineMultiplier(BigDouble milkSpent2)
    {
        return 0.05 + milkSpent2.ToDouble() * 0.001;
    }

    // applySoftCap: delegate that applies the current softcap rules given (current, gain)
    public static OfflineProgressResult Compute(
        BigDouble currentPoints,
        BigDouble pointGain,
        int generatorCount,
        BigDouble milkSpent2,
        int seconds,
        Func<BigDouble, BigDouble, BigDouble> applySoftCap)
    {
        if (seconds <= 0 || generatorCount <= 0)
            return new OfflineProgressResult(BigDouble.Zero, seconds, 0, ComputeOfflineMultiplier(milkSpent2));

        int effectiveSeconds = ComputeEffectiveSeconds(seconds);
        double offlineMultiplier = ComputeOfflineMultiplier(milkSpent2);

        BigDouble ratePerSecond = BigDouble.Pow(10, generatorCount) * 0.01 * pointGain * offlineMultiplier;
        BigDouble passiveGain = ratePerSecond * effectiveSeconds;
        passiveGain = applySoftCap(currentPoints, passiveGain);

        return new OfflineProgressResult(passiveGain, seconds, effectiveSeconds, offlineMultiplier);
    }
}