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
        public bool HasUnlockedGenerators;
        public bool HasUnlockedAscension;
        public bool HasAscended;
    }
}
