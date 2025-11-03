using BreakInfinity;

namespace WinFormsApp1.Core.Game;

public static class ChallengeService
{
    public static string GetObjectivePlainText(int idx)
    {
        return idx switch
        {
            0 => "reach 1,000,000 points",
            1 => "prestige 8 times",
            2 => "buy 2 generators",
            3 => "reach 10,000,000 points",
            _ => "complete the challenge"
        };
    }

    public static bool IsCompleted(int idx, BigDouble point, int prestigeCount, int generatorCount)
    {
        return idx switch
        {
            0 => point >= 1_000_000,
            1 => prestigeCount >= 8,
            2 => generatorCount >= 2,
            3 => point >= 10_000_000,
            _ => false
        };
    }
}