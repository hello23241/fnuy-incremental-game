using BreakInfinity;

namespace WinFormsApp1.Core.Game;

public static class SoftCapPresenter
{
    public static (bool Visible, string Text) Build(
        BigDouble point,
        BigDouble threshold,
        BigDouble divisor,
        System.Func<BigDouble, string> format)
    {
        var threshold1000 = threshold * 1000;
        if (point >= threshold1000)
            return (true, $"Current points exceed {format(threshold1000)} (1000× softcap). All gain is disabled.");

        if (point > threshold)
            return (true, $"Current points is over {format(threshold)}, gain is divided by {format(divisor)}");

        return (false, string.Empty);
    }
}