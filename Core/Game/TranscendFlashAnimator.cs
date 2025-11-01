using System.Drawing;

namespace WinFormsApp1.Core.Game;

// Smoothly cycles a color using HSV hue rotation.
// Configure hue step per tick and HSV saturation/value for appearance.
public sealed class TranscendFlashAnimator
{
    private double _hue;                  // 0..360
    private readonly double _hueStepDeg;  // degrees per tick
    private readonly double _s;           // 0..1
    private readonly double _v;           // 0..1

    public TranscendFlashAnimator(double initialHueDeg = 0, double hueStepDeg = 2.0, double saturation = 0.95, double value = 1.0)
    {
        _hue = NormalizeHue(initialHueDeg);
        _hueStepDeg = hueStepDeg <= 0 ? 1.0 : hueStepDeg;
        _s = Clamp01(saturation);
        _v = Clamp01(value);
    }

    public Color Next()
    {
        _hue = NormalizeHue(_hue + _hueStepDeg);
        return FromHsv(_hue, _s, _v);
    }

    private static double NormalizeHue(double h)
    {
        h %= 360.0;
        if (h < 0) h += 360.0;
        return h;
    }

    private static double Clamp01(double x) => x < 0 ? 0 : (x > 1 ? 1 : x);

    // Standard HSV -> RGB conversion
    private static Color FromHsv(double hueDeg, double s, double v)
    {
        if (s <= 0.0001) // gray
        {
            int gray = (int)(v * 255.0);
            return Color.FromArgb(gray, gray, gray);
        }

        double h = hueDeg / 60.0; // sector 0..6
        int i = (int)System.Math.Floor(h);
        double f = h - i;
        double p = v * (1.0 - s);
        double q = v * (1.0 - s * f);
        double t = v * (1.0 - s * (1.0 - f));

        (double r, double g, double b) = i switch
        {
            0 => (v, t, p),
            1 => (q, v, p),
            2 => (p, v, t),
            3 => (p, q, v),
            4 => (t, p, v),
            _ => (v, p, q) // 5
        };

        return Color.FromArgb(
            (int)System.Math.Round(r * 255.0),
            (int)System.Math.Round(g * 255.0),
            (int)System.Math.Round(b * 255.0));
    }
}