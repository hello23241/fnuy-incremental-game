using BreakInfinity;

namespace WinFormsApp1.Core.Game;

public readonly record struct PrestigeResetResult(
    BigDouble Point,
    BigDouble PointGain,
    int UpgradeCount,
    int PrestigeCount
);

public readonly record struct AscendResetResult(
    BigDouble Point,
    BigDouble PointGain,
    int UpgradeCount,
    int PrestigeCount,
    int AscendCount,
    BigDouble AscensionPoints
);

public readonly record struct TranscendResetResult(
    BigDouble Point,
    BigDouble PointGain,
    int UpgradeCount,
    int PrestigeCount,
    int GeneratorCount,
    BigDouble GeneratorCost,
    int CooldownDuration,
    int AscendCount,
    BigDouble AscensionPoints,
    int ActiveChallengeIndex,
    bool[] ChallengesCompleted,
    int NewMilkStreak,
    int MilkEarned
);

public static class ResetService
{
    public static PrestigeResetResult DoPrestige(
        BigDouble point,
        BigDouble pointGain,
        int upgradeCount,
        int prestigeCount)
    {
        return new PrestigeResetResult(
            Point: BigDouble.Zero,
            PointGain: BigDouble.One,
            UpgradeCount: 0,
            PrestigeCount: prestigeCount + 1
        );
    }

    public static AscendResetResult DoAscend(
        BigDouble point,
        BigDouble pointGain,
        int upgradeCount,
        int prestigeCount,
        int ascendCount,
        BigDouble ascensionPoints)
    {
        return new AscendResetResult(
            Point: BigDouble.Zero,
            PointGain: BigDouble.One,
            UpgradeCount: 0,
            PrestigeCount: 0,
            AscendCount: ascendCount + 1,
            AscensionPoints: ascensionPoints + 1
        );
    }

    public static TranscendResetResult DoTranscend(
        int baseMilkUpgradeCount,
        int currentMilkStreak)
    {
        int newStreak = currentMilkStreak + 1;
        int milkEarned = 90 + newStreak * 10 + baseMilkUpgradeCount * 10;

        return new TranscendResetResult(
            Point: BigDouble.Zero,
            PointGain: BigDouble.One,
            UpgradeCount: 0,
            PrestigeCount: 0,
            GeneratorCount: 0,
            GeneratorCost: new BigDouble(100),
            CooldownDuration: 1000,
            AscendCount: 0,
            AscensionPoints: BigDouble.Zero,
            ActiveChallengeIndex: -1,
            ChallengesCompleted: new bool[4],
            NewMilkStreak: newStreak,
            MilkEarned: milkEarned
        );
    }
}