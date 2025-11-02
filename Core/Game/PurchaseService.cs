using BreakInfinity;

namespace WinFormsApp1.Core.Game;

public static class PurchaseService
{
    // Returns (purchasedCount, remainingPoints)
    public static (int purchased, BigDouble remainingPoints) BuyMaxUpgrades(
        BigDouble points,
        Func<BigDouble> getNextUpgradeCost,
        double upgradeScale,
        Func<BigDouble, BigDouble> getEffectiveUpgradeDeduction)
    {
        BigDouble A = getNextUpgradeCost();
        double a = upgradeScale;

        if (A <= BigDouble.Zero || points <= BigDouble.Zero)
            return (0, points);

        // Non-increasing scaling fallback: buy in a loop
        if (a <= 1.0)
        {
            int purchased = 0;
            while (true)
            {
                BigDouble cost = getNextUpgradeCost();
                BigDouble deduction = getEffectiveUpgradeDeduction(cost);
                if (deduction > points) break;
                points -= deduction;
                purchased++;
            }
            return (purchased, points);
        }

        // Geometric sum approach, with discount then safety loop if needed
        BigDouble denom = new BigDouble(a - 1.0);
        BigDouble t = BigDouble.One + (points / A) * denom; // 1 + ((a-1)*points/A)
        int n = (int)System.Math.Floor(BigDouble.Log(t, a));
        if (n <= 0) return (0, points);

        BigDouble aPowN = BigDouble.Pow(a, n);
        BigDouble sum = A * (aPowN - BigDouble.One) / denom;

        // If discounted total still too high due to rounding, fall back to per-item purchases
        BigDouble discountedTotal = getEffectiveUpgradeDeduction(sum);
        if (discountedTotal > points)
        {
            int purchased = 0;
            while (n > 0)
            {
                BigDouble oneCost = getNextUpgradeCost();
                BigDouble oneDeduction = getEffectiveUpgradeDeduction(oneCost);
                if (oneDeduction > points) break;
                points -= oneDeduction;
                purchased++;
                n--;
            }
            return (purchased, points);
        }

        // Deduct discounted total at once
        points -= discountedTotal;
        return (n, points);
    }
}