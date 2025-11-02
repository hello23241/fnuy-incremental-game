using BreakInfinity;

namespace WinFormsApp1.UI.Presenters;

public static class HardCapPresenter
{
    // Returns the text to display for the click button and the "Point Gain" label.
    public static (string ClickButtonText, string PointGainText) Decorate(HudTexts hud, BigDouble currentPoints, BigDouble hardCapPoint)
    {
        bool atHardCap = currentPoints >= hardCapPoint;
        if (atHardCap)
            return ("+0 points", "Point Gain: 0");

        return (hud.ClickButtonText, hud.PointGainText);
    }
}