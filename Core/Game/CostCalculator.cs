using BreakInfinity;

namespace WinFormsApp1.Core.Game;

public static class CostCalculator
{
    public static BigDouble UpgradeCost(BigDouble baseCost, double scale, int count)
        => baseCost * BigDouble.Pow(scale, count);

    public static BigDouble PrestigeCost(BigDouble baseCost, double scale, int count)
        => baseCost * BigDouble.Pow(scale, count);

    public static BigDouble AscendCost(BigDouble baseCost, double scale, int ascendCount)
        => ascendCount <= 2 ? baseCost : baseCost * BigDouble.Pow(scale, ascendCount - 2);
}