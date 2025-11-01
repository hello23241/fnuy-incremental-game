using BreakInfinity;

namespace WinFormsApp1.Core.Game;

public static class GeneratorInfoPresenter
{
    public static (string InfoText, string PpsText) Build(
        int generatorCount,
        BigDouble pointGain,
        BigDouble softCapDivisor,
        BigDouble generatorCost,
        System.Func<BigDouble, string> format)
    {
        if (generatorCount <= 0)
            return ("Generators: 0 | Cost: 100", string.Empty);

        BigDouble divisor = softCapDivisor <= BigDouble.Zero ? BigDouble.One : softCapDivisor;
        BigDouble pps = BigDouble.Pow(10, generatorCount) * 0.01 * pointGain / divisor;

        string info = $"Generators: {generatorCount} | Cost: {format(generatorCost)} | Every generators 10x your current passive gain after the first";
        string ppsText = $"Points/second: {format(pps)}";
        return (info, ppsText);
    }
}