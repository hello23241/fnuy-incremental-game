using System;

namespace WinFormsApp1.Core.Game;

public readonly record struct DailyRewardResult(
    bool ShouldAward,
    int NewMilkStreak,
    int MilkEarned,
    DateTime NewLastMilkClaimDate
);

public static class DailyRewardService
{
    // Mirrors current game rules:
    // - Only after generators are unlocked
    // - Award once per local day
    // - Streak increments if last claim was exactly yesterday, else reset to 1
    // - Milk earned = 90 + streak*10 + baseMilkUpgradeCount*10
    public static DailyRewardResult Evaluate(
        bool hasUnlockedGenerators,
        DateTime lastMilkClaimDate,
        int currentStreak,
        int baseMilkUpgradeCount,
        DateTime todayLocalDate
    )
    {
        if (!hasUnlockedGenerators)
            return new DailyRewardResult(false, currentStreak, 0, lastMilkClaimDate);

        var lastClaim = lastMilkClaimDate.Date;
        var today = todayLocalDate.Date;

        if (lastClaim >= today)
            return new DailyRewardResult(false, currentStreak, 0, lastMilkClaimDate);

        int newStreak = (lastClaim == today.AddDays(-1)) ? currentStreak + 1 : 1;
        int milkEarned = 90 + newStreak * 10 + baseMilkUpgradeCount * 10;

        return new DailyRewardResult(true, newStreak, milkEarned, today);
    }
}