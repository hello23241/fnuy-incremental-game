using BreakInfinity;

namespace WinFormsApp1.UI.Presenters;

public readonly record struct GeneratorInfoVm(string InfoText, string PpsText);

public static class GeneratorInfoPresenter
{
    public static GeneratorInfoVm Build(
        int generatorCount,
        BigDouble pointGain,
        BigDouble softCapDivisor,
        BigDouble generatorCost,
        bool atHardCap,
        System.Func<BigDouble, string> format)
    {
        if (generatorCount == 0)
        {
            return new GeneratorInfoVm(
                InfoText: "Generators: 0 | Cost: 100",
                PpsText: ""
            );
        }

        BigDouble pps;
        if (atHardCap)
        {
            pps = BigDouble.Zero;
        }
        else
        {
            var divisor = softCapDivisor <= BigDouble.Zero ? BigDouble.One : softCapDivisor;
            pps = BigDouble.Pow(10, generatorCount) * 0.01 * pointGain / divisor;
        }

        string info = $"Generators: {generatorCount} | Cost: {format(generatorCost)} | Every generators 10x your current passive gain after the first";
        string ppsText = $"Points/second: {format(pps)}";
        return new GeneratorInfoVm(info, ppsText);
    }
}