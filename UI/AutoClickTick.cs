using System;

namespace WinFormsApp1.UI
{
    // Pure tick math for auto-click loop; returns whether a click should trigger and the current percent.
    public static class AutoClickTick
    {
        // Returns (triggered, percent). Mutates elapsedMs.
        public static (bool triggered, int percent) Process(ref int elapsedMs, int uiTickMs, int intervalMs)
        {
            elapsedMs += uiTickMs;

            if (intervalMs <= 0)
                return (false, 0);

            long p = 100L * elapsedMs / intervalMs;
            if (p < 0) p = 0;
            if (p > 100) p = 100;
            int percent = (int)p;

            if (elapsedMs >= intervalMs)
            {
                elapsedMs = 0;
                return (true, 100);
            }
            return (false, percent);
        }
    }
}