using BreakInfinity;

namespace WinFormsApp1.Core.Game;

public readonly record struct UpgradeInfoVm(string UpgradeInfoText, string PrestigeInfoText);

public static class UpgradeInfoPresenter
{
    public static UpgradeInfoVm Build(
        int effectiveUpgradeCount,
        double prestigeEffect,
        BigDouble prestigeIncrement,
        BigDouble softCapDivisor,
        bool[] challengesCompleted,
        int activeChallengeIndex,
        BigDouble[] milkSpent,
        double ascensionMultiplier,
        int prestigeCount,
        System.Func<BigDouble, string> format)
    {
        bool challenge0Completed = challengesCompleted != null && challengesCompleted.Length > 0 && challengesCompleted[0];
        bool challenge0Or3Active = activeChallengeIndex == 0 || activeChallengeIndex == 3;
        BigDouble milk0 = (milkSpent != null && milkSpent.Length > 0) ? milkSpent[0] : BigDouble.Zero;

        BigDouble perUpgrade = GainMathService.ComputeGainPerUpgradeForDisplay(
            effectiveUpgradeCount,
            prestigeEffect,
            prestigeIncrement,
            softCapDivisor,
            challenge0Completed,
            challenge0Or3Active,
            milk0,
            ascensionMultiplier);

        string upgradeInfo = $"each upgrade adds {format(perUpgrade)} to your click multiplier";

        // Prestige info text
        string baseText = "₂";
        string extraText = "";
        if (challengesCompleted != null && challengesCompleted.Length > 1 && challengesCompleted[1])
            extraText = " (multiplied by 1.1)";
        if (challengesCompleted != null && challengesCompleted.Length > 2 && challengesCompleted[2])
        {
            baseText = "_{1.9}";
            extraText = "";
        }

        string prestigeInfo = $"Prestige effect: log{baseText}({prestigeCount + 1}) = {prestigeEffect:F2}{extraText}";

        return new UpgradeInfoVm(upgradeInfo, prestigeInfo);
    }
}