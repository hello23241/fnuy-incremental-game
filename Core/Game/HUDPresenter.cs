using BreakInfinity;

namespace WinFormsApp1.Core.Game;

public readonly record struct HudTexts(
    string PointsText,
    string ClickButtonText,
    string UpgradeCostText,
    string PrestigeCostText,
    string AscendCostText,
    string PointGainText,
    string TranscendCostText,
    string MilkButtonText,
    bool CanTranscend
);

public static class HudPresenter
{
    public static HudTexts Build(
        BigDouble point,
        BigDouble pointGain,
        BigDouble softCapDivisor,
        BigDouble upgradeCost,
        BigDouble prestigeCost,
        BigDouble ascendCost,
        BigDouble transcendCost,
        BigDouble milk,
        System.Func<BigDouble, string> format)
    {
        string formattedPoint = format(point);
        string formattedGain = softCapDivisor == 0 ? "0" : format(pointGain / softCapDivisor);
        string formattedUpgradeCost = format(upgradeCost);
        string formattedPrestigeCost = format(prestigeCost);
        string formattedAscendCost = format(ascendCost);
        string formattedPointGain = format(pointGain);
        string formattedTranscendCost = format(transcendCost);
        string formattedMilk = format(milk);

        return new HudTexts(
            PointsText: $"Points: {formattedPoint}",
            ClickButtonText: $"+{formattedGain} points",
            UpgradeCostText: $"Upgrade Cost: {formattedUpgradeCost}",
            PrestigeCostText: $"Prestige Cost: {formattedPrestigeCost}",
            AscendCostText: $"Ascend Cost: {formattedAscendCost}",
            PointGainText: $"Point Gain: {formattedPointGain}",
            TranscendCostText: $"Transcend Cost: {formattedTranscendCost}",
            MilkButtonText: $"🥛 {formattedMilk}",
            CanTranscend: point >= transcendCost
        );
    }
}