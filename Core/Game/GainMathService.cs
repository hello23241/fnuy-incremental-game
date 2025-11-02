using BreakInfinity;

namespace WinFormsApp1.Core.Game;

public static class GainMathService
{
    public static BigDouble ComputePointGain(
        int effectiveUpgradeCount,
        double prestigeEffect,
        BigDouble prestigeIncrement,
        BigDouble softCapDivisor,
        bool challenge0Completed,
        bool challenge0Or3Active,
        BigDouble milkSpent0)
    {
        if (softCapDivisor <= BigDouble.Zero) return BigDouble.Zero;

        double challenge0Multi = challenge0Completed ? 3.0 : 1.0;
        double challengeDebuff = (challenge0Or3Active ? 1.0 / 5.0 : 1.0);

        BigDouble k = prestigeEffect * prestigeIncrement / 100 / softCapDivisor;
        BigDouble core = BigDouble.One + effectiveUpgradeCount * (1 + effectiveUpgradeCount * k);

        return core
             * challenge0Multi
             * challengeDebuff
             * (BigDouble.One + milkSpent0 * 0.0001);
    }

    public static BigDouble ComputeGainPerUpgradeForDisplay(
        int effectiveUpgradeCount,
        double prestigeEffect,
        BigDouble prestigeIncrement,
        BigDouble softCapDivisor,
        bool challenge0Completed,
        bool challenge0Or3Active,
        BigDouble milkSpent0,
        double ascensionMultiplier)
    {
        if (softCapDivisor <= BigDouble.Zero) return BigDouble.Zero;

        double challenge0Multi = challenge0Completed ? 3.0 : 1.0;
        double challengeDebuff = (challenge0Or3Active ? 1.0 / 10.0 : 1.0);

        BigDouble k = prestigeEffect * prestigeIncrement / 100 / softCapDivisor;

        BigDouble perUpgrade = (BigDouble.One + (2 * effectiveUpgradeCount + 1) * k)
                             * challenge0Multi
                             * challengeDebuff
                             * (BigDouble.One + milkSpent0 * 0.0001)
                             * ascensionMultiplier;

        return perUpgrade;
    }
}