using WinFormsApp1.Core.Game;

namespace WinFormsApp1.UI.Presenters;

public static class AutoclickPresenter
{
    public static (bool Enabled, int IntervalMs, int Percent) Build(int ascendCount, int effectiveCooldownMs, int elapsedMs)
    {
        bool enabled = ascendCount >= 2;
        int interval = PassiveGainService.ComputeAutoClickIntervalMs(effectiveCooldownMs);

        int percent = 0;
        if (enabled && interval > 0)
        {
            long p = 100L * elapsedMs / interval;
            if (p < 0) p = 0;
            if (p > 100) p = 100;
            percent = (int)p;
        }
        return (enabled, interval, percent);
    }
}