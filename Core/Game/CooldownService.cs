using System;

namespace WinFormsApp1.Core.Game;

public static class CooldownService
{
    // Effective click cooldown with challenge/ascension modifiers
    public static int ComputeEffectiveCooldownMs(
        int baseCooldownMs,
        int ascendCount,
        int activeChallengeIndex,
        bool challenge2Completed)
    {
        // Challenge 2 or 3 active: fixed 10 seconds
        if (activeChallengeIndex == 2 || activeChallengeIndex == 3)
            return 10_000;

        int reductions = ascendCount / 2;
        double reductionPercent = reductions * 0.05;
        double effective = baseCooldownMs * (1.0 - reductionPercent);

        // Challenge 2 completed: decrease by 0.1s (100ms)
        if (challenge2Completed)
            effective -= 100;

        return Math.Max((int)effective, 50);
    }

    // Generator tick interval with challenge modifiers
    public static int ComputeGeneratorIntervalMs(
        int baseIntervalMs,
        int activeChallengeIndex,
        bool challenge2Completed)
    {
        int interval = baseIntervalMs;

        // Challenge 2 completed: 0.1s faster
        if (challenge2Completed)
            interval -= 100;

        // Challenge 2 or 3 active: increase to 10 seconds
        if (activeChallengeIndex == 2 || activeChallengeIndex == 3)
            interval = 10_000;

        return Math.Max(interval, 100);
    }
}