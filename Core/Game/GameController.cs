using System;
using BreakInfinity;
using WinFormsApp1.Core.Persistence;

namespace WinFormsApp1.Core.Game;

public sealed class GameController
{
    public GameRuntimeState State { get; set; }

    public GameController(GameRuntimeState initialState)
    {
        State = initialState;
    }

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

    public BigDouble ApplyGeneratorTick(double ascensionMultiplier, Func<BigDouble, BigDouble, BigDouble> applySoftCap)
    {
        if (State.GeneratorCount <= 0) return BigDouble.Zero;

        var passiveGain = PassiveGainService.ComputeGeneratorPassiveGain(
            State.GeneratorCount,
            State.PointMultiplier,
            ascensionMultiplier,
            State.Point,
            applySoftCap);

        if (passiveGain > BigDouble.Zero)
            State = State with { Point = State.Point + passiveGain };

        return passiveGain;
    }

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

    public bool TryTranscend(BigDouble transcendCost, int baseMilkUpgradeCount, int currentMilkStreak, out int milkEarned, out int newMilkStreak)
    {
        milkEarned = 0;
        newMilkStreak = currentMilkStreak;

        if (State.Point < transcendCost)
            return false;

        // Deduct visually (ResetService resets anyway)
        var _afterCost = State.Point - transcendCost;

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

        milkEarned = res.MilkEarned;
        newMilkStreak = res.NewMilkStreak;
        return true;
    }

    public void RecalculatePointMultiplier(
        int effectiveUpgradeCount,
        double prestigeEffect,
        BigDouble prestigeIncrement,
        BigDouble softCapDivisor,
        bool challenge0Completed,
        bool challenge0Or3Active,
        BigDouble milkSpent0,
        BigDouble hardCapPoint)
    {
        if (softCapDivisor == BigDouble.Zero)
        {
            State = State with
            {
                PointMultiplier = BigDouble.Zero,
                Point = BigDouble.Min(State.Point, hardCapPoint)
            };
            return;
        }

        var newGain = GainMathService.ComputePointGain(
            effectiveUpgradeCount,
            prestigeEffect,
            prestigeIncrement,
            softCapDivisor,
            challenge0Completed,
            challenge0Or3Active,
            milkSpent0);

        State = State with { PointMultiplier = newGain };
    }

    public (BigDouble PassiveGain, int Seconds, int EffectiveSeconds, double OfflineMultiplier) ApplyOfflineProgress(
        int seconds,
        BigDouble milkSpent2,
        Func<BigDouble, BigDouble, BigDouble> applySoftCap)
    {
        var result = OfflineProgressService.Compute(
            currentPoints: State.Point,
            pointGain: State.PointMultiplier,
            generatorCount: State.GeneratorCount,
            milkSpent2: milkSpent2,
            seconds: seconds,
            applySoftCap: applySoftCap);

        if (result.PassiveGain > BigDouble.Zero)
            State = State with { Point = State.Point + result.PassiveGain };

        return (result.PassiveGain, result.Seconds, result.EffectiveSeconds, result.OfflineMultiplier);
    }
}