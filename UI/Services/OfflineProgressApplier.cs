using BreakInfinity;
using System;
using System.Windows.Forms;
using WinFormsApp1.Core.Game;
using WinFormsApp1.UI.Presenters;

namespace WinFormsApp1.UI.Services
{
    public static class OfflineProgressApplier
    {
        // Returns true if progress was applied and a message was shown.
        public static bool TryApply(
            IWin32Window owner,
            int generatorCount,
            int prestigeCount,
            DateTime lastSaved,
            DateTime serverNow,
            Func<int, (BigDouble PassiveGain, int Seconds, int EffectiveSeconds, double OfflineMultiplier)> apply,
            Func<BigDouble, string> format)
        {
            var seconds = (int)(serverNow - lastSaved).TotalSeconds;
            if (seconds <= 0) return false;

            if (generatorCount <= 0)
            {
                bool hasPrestiged = prestigeCount > 0;
                MessageBox.Show(owner, OfflineProgressPresenter.BuildNoGeneratorMessage(hasPrestiged));
                return false;
            }

            var result = apply(seconds);

            if (result.PassiveGain > BigDouble.Zero)
            {
                try { SoundEffects.Gain(0.8f); } catch { }
                MessageBox.Show(
                    owner,
                    OfflineProgressPresenter.BuildGainMessage(
                        result.PassiveGain,
                        result.Seconds,
                        result.EffectiveSeconds,
                        result.OfflineMultiplier,
                        format),
                    "Offline progress");
                return true;
            }

            return false;
        }
    }
}