using BreakInfinity;

namespace WinFormsApp1.Core.Game;

public static class ProgressionMath
{
    public static double PrestigeEffect(int prestigeCount, bool challenge2Completed, bool challenge1Or3Active)
    {
        double logBase = challenge2Completed ? 1.9 : 2.0;
        double effect = BigDouble.Log(prestigeCount + 1, logBase);
        if (challenge1Or3Active) effect /= 2.0;
        return effect;
    }

    public static BigDouble PrestigeIncrement(BigDouble baseIncrement, bool challenge1Completed)
        => baseIncrement * (challenge1Completed ? 1.1 : 1.0);
}