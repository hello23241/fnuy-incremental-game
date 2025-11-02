using System;
using BreakInfinity;

namespace WinFormsApp1.Core.Persistence;

public sealed record GameRuntimeState(
    BigDouble Point,
    BigDouble PointMultiplier,
    int UpgradeCount,
    int PrestigeCount,
    BigDouble GeneratorCost,
    int GeneratorCount,
    BigDouble AscensionPoints,
    int AscensionCount,
    int CooldownDuration,
    DateTime LastSavedTime,
    bool HasUnlockedPrestige,
    bool HasUnlockedGenerators,
    bool HasUnlockedAscension,
    bool HasAscended,
    bool[] AscChallenges,
    bool HasUnlockedPremiumShop,
    BigDouble Milk,
    DateTime LastMilkClaimDate,
    int MilkStreak,
    int BaseMilkUpgradeCount,
    BigDouble[] MilkSpent,
    int ActiveChallengeIndex,
    int? PrevCooldownDurationForChallenge
);