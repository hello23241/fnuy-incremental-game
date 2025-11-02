using System;
using BreakInfinity;
using WinFormsApp1.SaveSystem;

namespace WinFormsApp1.Core.Persistence;

public sealed class GamePersistence
{
    private readonly IGameSaveService saveService;

    public GamePersistence(IGameSaveService saveService)
    {
        this.saveService = saveService ?? throw new ArgumentNullException(nameof(saveService));
    }

    public void Save(GameRuntimeState runtime)
    {
        var dto = ToDto(runtime);
        saveService.Save(dto);
    }

    public static GameRuntimeState ToRuntimeState(GameState dto)
    {
        if (dto == null) throw new ArgumentNullException(nameof(dto));

        // Normalize arrays to stable sizes
        var milkSpent = (dto.MilkSpent != null && dto.MilkSpent.Length == 3)
            ? dto.MilkSpent
            : new BigDouble[3];

        var ascChallenges = (dto.AscChallenges != null && dto.AscChallenges.Length == 4)
            ? dto.AscChallenges
            : new bool[4];

        return new GameRuntimeState(
            Point: dto.Point,
            PointMultiplier: dto.PointMultiplier,
            UpgradeCount: dto.UpgradeCount,
            PrestigeCount: dto.PrestigeCount,
            GeneratorCost: dto.GeneratorCost,
            GeneratorCount: dto.GeneratorCount,
            AscensionPoints: dto.AscensionPoints,
            AscensionCount: dto.AscensionCount,
            CooldownDuration: dto.CooldownDuration,
            LastSavedTime: dto.LastSavedTime,
            HasUnlockedPrestige: dto.HasUnlockedPrestige,
            HasUnlockedGenerators: dto.HasUnlockedGenerators,
            HasUnlockedAscension: dto.HasUnlockedAscension,
            HasAscended: dto.HasAscended,
            AscChallenges: ascChallenges,
            HasUnlockedPremiumShop: dto.HasUnlockedPremiumShop,
            Milk: dto.Milk,
            LastMilkClaimDate: dto.LastMilkClaimDate,
            MilkStreak: dto.MilkStreak,
            BaseMilkUpgradeCount: dto.BaseMilkUpgradeCount,
            MilkSpent: milkSpent,
            ActiveChallengeIndex: dto.ActiveChallengeIndex,
            PrevCooldownDurationForChallenge: dto.PrevCooldownDurationForChallenge
        );
    }

    public static GameState ToDto(GameRuntimeState runtime)
    {
        return new GameState
        {
            Point = runtime.Point,
            PointMultiplier = runtime.PointMultiplier,
            UpgradeCount = runtime.UpgradeCount,
            PrestigeCount = runtime.PrestigeCount,
            GeneratorCost = runtime.GeneratorCost,
            GeneratorCount = runtime.GeneratorCount,
            AscensionPoints = runtime.AscensionPoints,
            AscensionCount = runtime.AscensionCount,
            CooldownDuration = runtime.CooldownDuration,
            LastSavedTime = runtime.LastSavedTime,
            HasUnlockedPrestige = runtime.HasUnlockedPrestige,
            HasUnlockedGenerators = runtime.HasUnlockedGenerators,
            HasUnlockedAscension = runtime.HasUnlockedAscension,
            HasAscended = runtime.HasAscended,
            AscChallenges = runtime.AscChallenges,
            HasUnlockedPremiumShop = runtime.HasUnlockedPremiumShop,
            Milk = runtime.Milk,
            LastMilkClaimDate = runtime.LastMilkClaimDate,
            MilkStreak = runtime.MilkStreak,
            BaseMilkUpgradeCount = runtime.BaseMilkUpgradeCount,
            MilkSpent = runtime.MilkSpent,
            ActiveChallengeIndex = runtime.ActiveChallengeIndex,
            PrevCooldownDurationForChallenge = runtime.PrevCooldownDurationForChallenge
        };
    }
}