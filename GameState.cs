using BreakInfinity;
using System;

namespace WinFormsApp1
{
    [Serializable]
    public class GameState
    {
        public BigDouble Point;
        public BigDouble PointMultiplier;
        public int UpgradeCount;
        public int PrestigeCount;
        public BigDouble GeneratorCost;
        public int GeneratorCount;
        public BigDouble AscensionPoints;
        public int AscensionCount;
        public DateTime LastSavedTime;
        public int CooldownDuration;
        public double OfflineMultiplier;
        //hidden variables
        public bool HasUnlockedPrestige;
        public bool HasUnlockedPremiumShop { get; set; }
        public bool HasUnlockedGenerators;
        public bool HasUnlockedAscension;
        public bool HasAscended;
        public bool[] AscChallenges = new bool[4];
        public BigDouble Milk { get; set; }
        public DateTime LastMilkClaimDate { get; set; }
        public int MilkStreak { get; set; }
        public BigDouble[] MilkSpent { get; set; } = new BigDouble[3];
        public int BaseMilkUpgradeCount { get; set; }
        // Challenge state persistence
        public int ActiveChallengeIndex { get; set; } = -1;
        public int? PrevCooldownDurationForChallenge { get; set; }
    }
}
