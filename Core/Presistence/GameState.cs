using System;
using BreakInfinity;

namespace WinFormsApp1.SaveSystem;

[Serializable]
public class GameState
{
    public BigDouble Point { get; set; }
    public int UpgradeCount { get; set; }
    public BigDouble PointMultiplier { get; set; }
    public int PrestigeCount { get; set; }
    public BigDouble GeneratorCost { get; set; }
    public int GeneratorCount { get; set; }
    public BigDouble AscensionPoints { get; set; }
    public int AscensionCount { get; set; }
    public int CooldownDuration { get; set; }
    public DateTime LastSavedTime { get; set; }
    public bool HasUnlockedPrestige { get; set; }
    public bool HasUnlockedGenerators { get; set; }
    public bool HasUnlockedAscension { get; set; }
    public bool HasAscended { get; set; }
    public bool[] AscChallenges { get; set; } = new bool[4];
    public bool HasUnlockedPremiumShop { get; set; }
    public BigDouble Milk { get; set; }
    public DateTime LastMilkClaimDate { get; set; }
    public int MilkStreak { get; set; }
    public int BaseMilkUpgradeCount { get; set; }
    public BigDouble[] MilkSpent { get; set; } = new BigDouble[3];
    public int ActiveChallengeIndex { get; set; } = -1;
    public int? PrevCooldownDurationForChallenge { get; set; }
}