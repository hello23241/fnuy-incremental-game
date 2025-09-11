using BreakInfinity;
using System;

namespace WinFormsApp1
{
    [Serializable]
    public class GameState
    {
        public BigDouble Point;
        public BigDouble PointMultiplier;
        public BigDouble UpgradeCost;
        public BigDouble PrestigeBonus;
        public BigDouble PrestigeCost;
        public BigDouble GeneratorCost;
        public int GeneratorCount;
        public BigDouble AscendCost;
        public BigDouble AscensionPoints;
        public DateTime LastSavedTime;
    }
}
