using System;
using System.Windows.Forms;
using WinFormsApp1.Core.Game;
using WinFormsApp1.UI.Presenters;

namespace WinFormsApp1.UI
{
    public static class AutoClickUiApplier
    {
        public static void Apply(
            int ascendCount,
            int effectiveCooldownMs,
            int autoClickElapsedMs,
            System.Windows.Forms.Timer autoClickTimer,
            Panel progressBarBg,
            Panel progressBarFill,
            Action<int> updateBar,
            int uiTickIntervalMs = 50)
        {
            var auto = AutoclickPresenter.Build(ascendCount, effectiveCooldownMs, autoClickElapsedMs);

            if (autoClickTimer.Enabled != auto.Enabled)
                autoClickTimer.Enabled = auto.Enabled;

            if (autoClickTimer.Interval != uiTickIntervalMs)
                autoClickTimer.Interval = uiTickIntervalMs;

            progressBarBg.Visible = auto.Enabled;
            if (auto.Enabled)
            {
                progressBarFill.Height = progressBarBg.Height;
                updateBar(auto.Percent);
            }
        }
    }
}