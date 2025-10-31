using BreakInfinity;

namespace WinFormsApp1.Core.Game;

public static class MilkShopService
{
    public static (bool Success, BigDouble NewMilk, int NewBaseCount, int Cost) TryBuyBaseGain(BigDouble milk, int baseCount)
    {
        int cost = 10 + baseCount * 2;
        if (milk >= cost)
            return (true, milk - cost, baseCount + 1, cost);

        return (false, milk, baseCount, cost);
    }

    public static (bool Success, BigDouble NewMilk, BigDouble[] NewSpent) TrySpendStat(
        BigDouble milk, int index, BigDouble amount, BigDouble[] currentSpent)
    {
        if (index < 0 || index > 2) return (false, milk, currentSpent);
        if (milk < amount) return (false, milk, currentSpent);

        var nextSpent = (BigDouble[])currentSpent.Clone();
        nextSpent[index] += amount;
        return (true, milk - amount, nextSpent);
    }
}