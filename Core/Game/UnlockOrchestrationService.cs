using System;

namespace WinFormsApp1.Core.Game;

public readonly record struct UnlockEffects(
    bool FirstTimeUnlock,
    int CompensationMilk,
    bool AwardDaily,
    int DailyMilk,
    int NewMilkStreak,
    DateTime NewLastMilkClaimDate
);

public static class UnlockOrchestrationService
{
    public static UnlockEffects HandleGeneratorUnlock(
        bool hasUnlockedPremiumShop,
        DateTime lastMilkClaimDate,
        int currentMilkStreak,
        int baseMilkUpgradeCount,
        DateTime nowLocalDate)
    {
        bool firstTime = !hasUnlockedPremiumShop;
        int compensation = firstTime ? 10_000 : 0;

        var daily = DailyRewardService.Evaluate(
            hasUnlockedGenerators: true,
            lastMilkClaimDate: lastMilkClaimDate,
            currentStreak: currentMilkStreak,
            baseMilkUpgradeCount: baseMilkUpgradeCount,
            todayLocalDate: nowLocalDate.Date);

        return new UnlockEffects(
            FirstTimeUnlock: firstTime,
            CompensationMilk: compensation,
            AwardDaily: daily.ShouldAward,
            DailyMilk: daily.MilkEarned,
            NewMilkStreak: daily.ShouldAward ? daily.NewMilkStreak : currentMilkStreak,
            NewLastMilkClaimDate: daily.ShouldAward ? daily.NewLastMilkClaimDate : lastMilkClaimDate);
    }
}