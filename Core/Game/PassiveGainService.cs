using BreakInfinity;

namespace WinFormsApp1.Core.Game;

public static class PassiveGainService
{
    public static BigDouble ComputeClickGain(
        BigDouble pointGain,
        double ascensionMultiplier,
        BigDouble currentPoints,
        Func<BigDouble, BigDouble, BigDouble> applySoftCap)
    {
        BigDouble gain = pointGain * ascensionMultiplier;
        return applySoftCap(currentPoints, gain);
    }

    public static BigDouble ComputeGeneratorPassiveGain(
        int generatorCount,
        BigDouble pointGain,
        double ascensionMultiplier,
        BigDouble currentPoints,
        Func<BigDouble, BigDouble, BigDouble> applySoftCap)
    {
        if (generatorCount <= 0) return BigDouble.Zero;
        BigDouble passiveGain = Math.Pow(10, generatorCount) * 0.01 * pointGain * ascensionMultiplier;
        return applySoftCap(currentPoints, passiveGain);
    }

    public static int ComputeAutoClickIntervalMs(int effectiveCooldownDurationMs)
    {
        return effectiveCooldownDurationMs * 2;
    }
}