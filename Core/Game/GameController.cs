using System;
using BreakInfinity;
using WinFormsApp1.Core.Persistence;

namespace WinFormsApp1.Core.Game;

public sealed class GameController
{
    public GameRuntimeState State { get; private set; }

    public GameController(GameRuntimeState initialState)
    {
        State = initialState;
    }

    // Click: returns gain applied
    public BigDouble Click(double ascensionMultiplier, Func<BigDouble, BigDouble, BigDouble> applySoftCap)
    {
        var gain = PassiveGainService.ComputeClickGain(
            State.PointMultiplier,
            ascensionMultiplier,
            State.Point,
            applySoftCap);

        State = State with { Point = State.Point + gain };
        return gain;
    }

    // Try to buy one upgrade using caller's cost/deduction rules
    public bool TryBuyUpgrade(Func<BigDouble> getNextUpgradeCost, Func<BigDouble, BigDouble> getEffectiveUpgradeDeduction)
    {
        var cost = getNextUpgradeCost();
        if (State.Point < cost) return false;

        var deduction = getEffectiveUpgradeDeduction(cost);
        State = State with
        {
            Point = State.Point - deduction,
            UpgradeCount = State.UpgradeCount + 1
        };
        return true;
    }

    // Buy-max via existing purchase service
    public (int purchased, BigDouble remaining) BuyMaxUpgrades(
        Func<BigDouble> getNextUpgradeCost,
        double upgradeScale,
        Func<BigDouble, BigDouble> getEffectiveUpgradeDeduction)
    {
        var (purchased, remaining) = PurchaseService.BuyMaxUpgrades(
            State.Point,
            getNextUpgradeCost,
            upgradeScale,
            getEffectiveUpgradeDeduction);

        if (purchased > 0)
        {
            State = State with
            {
                Point = remaining,
                UpgradeCount = State.UpgradeCount + purchased
            };
        }
        return (purchased, remaining);
    }

    // Try buy 1 generator (cost growth is squared)
    public bool TryBuyGenerator()
    {
        if (State.Point < State.GeneratorCost) return false;

        var newPoint = State.Point - State.GeneratorCost;
        var newCount = State.GeneratorCount + 1;
        var newCost = BigDouble.Pow(State.GeneratorCost, 2);

        State = State with
        {
            Point = newPoint,
            GeneratorCount = newCount,
            GeneratorCost = newCost
        };
        return true;
    }

    // Prestige (caller decides cost)
    public bool TryPrestige(BigDouble prestigeCost)
    {
        if (State.Point < prestigeCost) return false;

        var res = ResetService.DoPrestige(State.Point, State.PointMultiplier, State.UpgradeCount, State.PrestigeCount);
        State = State with
        {
            Point = res.Point,
            PointMultiplier = res.PointGain,
            UpgradeCount = res.UpgradeCount,
            PrestigeCount = res.PrestigeCount
        };
        return true;
    }

    // Ascend (caller decides cost and any UI confirmations)
    public bool TryAscend(BigDouble ascendCost)
    {
        if (State.Point < ascendCost) return false;

        var res = ResetService.DoAscend(
            State.Point,
            State.PointMultiplier,
            State.UpgradeCount,
            State.PrestigeCount,
            State.AscensionCount,
            State.AscensionPoints);

        State = State with
        {
            Point = res.Point,
            PointMultiplier = res.PointGain,
            UpgradeCount = res.UpgradeCount,
            PrestigeCount = res.PrestigeCount,
            AscensionCount = res.AscendCount,
            AscensionPoints = res.AscensionPoints
        };
        return true;
    }

    // Start challenge (caller decides if ascend can occur)
    public void StartChallenge(bool canAscend, int challengeIndex)
    {
        var res = ChallengeSessionService.Start(
            canAscend,
            State.AscensionCount,
            State.AscensionPoints,
            challengeIndex);

        State = State with
        {
            Point = res.Point,
            PointMultiplier = res.PointGain,
            UpgradeCount = res.UpgradeCount,
            PrestigeCount = res.PrestigeCount,
            GeneratorCount = res.GeneratorCount,
            GeneratorCost = res.GeneratorCost,
            AscensionCount = res.AscendCount,
            AscensionPoints = res.AscensionPoints,
            ActiveChallengeIndex = res.ActiveChallengeIndex
        };
    }

    // Transcend: caller should subtract the cost before calling this (to keep UI flow unchanged)
    public (int milkEarned, int newMilkStreak) DoTranscend(int baseMilkUpgradeCount, int currentMilkStreak)
    {
        var res = ResetService.DoTranscend(baseMilkUpgradeCount, currentMilkStreak);

        State = State with
        {
            Point = res.Point,
            PointMultiplier = res.PointGain,
            UpgradeCount = res.UpgradeCount,
            PrestigeCount = res.PrestigeCount,
            GeneratorCount = res.GeneratorCount,
            GeneratorCost = res.GeneratorCost,
            CooldownDuration = res.CooldownDuration,
            AscensionCount = res.AscendCount,
            AscensionPoints = res.AscensionPoints,
            ActiveChallengeIndex = res.ActiveChallengeIndex,
            AscChallenges = res.ChallengesCompleted
        };

        return (res.MilkEarned, res.NewMilkStreak);
    }
}