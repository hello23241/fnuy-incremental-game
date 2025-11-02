using BreakInfinity;

namespace WinFormsApp1.Core.Game;

public static class ButtonStateService
{
    public static (bool UpgradeEnabled, bool PrestigeEnabled, bool AscendEnabled, bool GeneratorEnabled) Compute(
        BigDouble point,
        BigDouble upgradeCost,
        BigDouble prestigeCost,
        BigDouble ascendCost,
        BigDouble generatorCost)
    {
        return (
            UpgradeEnabled: point >= upgradeCost,
            PrestigeEnabled: point >= prestigeCost,
            AscendEnabled: point >= ascendCost,
            GeneratorEnabled: point >= generatorCost
        );
    }
}