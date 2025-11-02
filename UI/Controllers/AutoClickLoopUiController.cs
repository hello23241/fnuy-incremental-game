using System;
using System.Windows.Forms;
using WinFormsApp1.UI.Services;
using WinFormsApp1.Core.Game;

namespace WinFormsApp1.UI.Controllers
{
    // Owns auto-click progress bar visibility, timer tick, and triggering the click.
    public sealed class AutoClickLoopUiController : IDisposable
    {
        private readonly System.Windows.Forms.Timer _timer;
        private readonly Panel _barBg;
        private readonly Panel _barFill;
        private readonly Func<int> _getAscendCount;
        private readonly Func<int> _getEffectiveCooldownMs;
        private readonly Action<int> _updateBar;      // percent -> UI
        private readonly Action _performClick;        // executes a game click
        private readonly int _uiTickMs;
        private int _elapsedMs;

        public AutoClickLoopUiController(
            System.Windows.Forms.Timer timer,
            Panel barBg,
            Panel barFill,
            Func<int> getAscendCount,
            Func<int> getEffectiveCooldownMs,
            Action<int> updateBar,
            Action performClick,
            int uiTickIntervalMs = 50)
        {
            _timer = timer ?? throw new ArgumentNullException(nameof(timer));
            _barBg = barBg ?? throw new ArgumentNullException(nameof(barBg));
            _barFill = barFill ?? throw new ArgumentNullException(nameof(barFill));
            _getAscendCount = getAscendCount ?? throw new ArgumentNullException(nameof(getAscendCount));
            _getEffectiveCooldownMs = getEffectiveCooldownMs ?? throw new ArgumentNullException(nameof(getEffectiveCooldownMs));
            _updateBar = updateBar ?? throw new ArgumentNullException(nameof(updateBar));
            _performClick = performClick ?? throw new ArgumentNullException(nameof(performClick));

            _uiTickMs = uiTickIntervalMs;
            _timer.Interval = _uiTickMs;
            _timer.Tick += OnTick;
        }

        // Called from UpdateUI; sets visibility and timer enable based on ascension unlocks
        public void ApplyUi(int ascendCount)
        {
            bool show = ascendCount >= 2;
            if (_barBg.Visible != show) _barBg.Visible = show;

            if (!show)
            {
                if (_timer.Enabled) _timer.Enabled = false;
                Reset();
                return;
            }

            if (!_timer.Enabled) _timer.Enabled = true;
        }

        public void Reset()
        {
            _elapsedMs = 0;
            _updateBar(0);
        }

        private void OnTick(object? sender, EventArgs e)
        {
            if (!_barBg.Visible) return;

            int intervalMs = PassiveGainService.ComputeAutoClickIntervalMs(_getEffectiveCooldownMs());

            // Reuse existing tick helper from UI layer
            var (trigger, percent) = WinFormsApp1.UI.AutoClickTick.Process(
                ref _elapsedMs,
                uiTickMs: _timer.Interval,
                intervalMs: intervalMs);

            if (trigger)
            {
                _updateBar(100);
                _performClick();
            }
            else
            {
                _updateBar(percent);
            }
        }

        public void Dispose()
        {
            _timer.Tick -= OnTick;
            _timer.Dispose();
        }
    }
}