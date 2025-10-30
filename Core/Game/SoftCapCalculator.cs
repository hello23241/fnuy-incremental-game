using BreakInfinity;

namespace WinFormsApp1.Core.Game;

public static class SoftCapCalculator
{
    public static BigDouble GetSoftCapThreshold(bool challenge3Completed, BigDouble baseThreshold)
        => challenge3Completed ? new BigDouble(1_000_000) : baseThreshold;

    public static BigDouble GetSoftCapDivisor(BigDouble currentPoint, BigDouble threshold, BigDouble milkSpentSoftcap)
    {
        BigDouble divisor = BigDouble.One;
        if (milkSpentSoftcap > BigDouble.Zero)
            divisor /= (BigDouble.Min(BigDouble.Log(milkSpentSoftcap, 1.1), milkSpentSoftcap) * 0.01 + BigDouble.One);

        var threshold1000 = threshold * 1000;

        if (currentPoint <= threshold) return BigDouble.One;
        if (currentPoint <= threshold1000)
        {
            var linearDivisor = currentPoint / 2 / threshold;
            if (linearDivisor * divisor <= 1) return BigDouble.One;
            return linearDivisor * divisor;
        }
        return BigDouble.Zero; // hard cap
    }

    public static BigDouble ApplySoftCap(BigDouble current, BigDouble gain, BigDouble threshold, BigDouble divisorAtNewTotal)
    {
        var newTotal = current + gain;
        if (newTotal <= threshold) return gain;
        if (divisorAtNewTotal <= BigDouble.Zero) return gain;
        return gain / divisorAtNewTotal;
    }
}