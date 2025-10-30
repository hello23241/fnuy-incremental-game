using BreakInfinity;

namespace WinFormsApp1.Core.Game;

public readonly record struct ChallengeStartResult(
    BigDouble Point,
    BigDouble PointGain,
    int UpgradeCount,
    int PrestigeCount,
    int GeneratorCount,
    BigDouble GeneratorCost,
    int AscendCount,
    BigDouble AscensionPoints,
    int ActiveChallengeIndex,
    bool AscendPerformed
);

public static class ChallengeSessionService
{
    // Pure reset math for starting a challenge. UI concerns stay in the form.
    public static ChallengeStartResult Start(
        bool canAscend,
        int currentAscendCount,
        BigDouble currentAscensionPoints,
        int challengeIndex
    )
    {
        int newAscendCount = currentAscendCount + (canAscend ? 1 : 0);
        BigDouble newAscensionPoints = currentAscensionPoints + (canAscend ? 1 : 0);

        return new ChallengeStartResult(
            Point: BigDouble.Zero,
            PointGain: BigDouble.One,
            UpgradeCount: 0,
            PrestigeCount: 0,
            GeneratorCount: 0,
            GeneratorCost: new BigDouble(100),
            AscendCount: newAscendCount,
            AscensionPoints: newAscensionPoints,
            ActiveChallengeIndex: challengeIndex,
            AscendPerformed: canAscend
        );
    }
}