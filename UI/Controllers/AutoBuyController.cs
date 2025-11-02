using System;
using System.Windows.Forms;

namespace WinFormsApp1.UI.Controllers
{
    // Owns Auto Buy state, UI toggling, and timer tick. No game math here.
    public sealed class AutoBuyController : IDisposable
    {
        private readonly System.Windows.Forms.Timer _timer;
        private readonly Button _button;
        private readonly Func<int> _getAscendCount;
        private readonly Func<bool> _tryBuyUpgrade;
        private readonly Action _afterPurchase;

        private bool _enabled;

        public AutoBuyController(
            Button button,
            int autoBuyTickIntervalMs,
            Func<int> getAscendCount,
            Func<bool> tryBuyUpgrade,
            Action afterPurchase)
        {
            _button = button ?? throw new ArgumentNullException(nameof(button));
            _getAscendCount = getAscendCount ?? throw new ArgumentNullException(nameof(getAscendCount));
            _tryBuyUpgrade = tryBuyUpgrade ?? throw new ArgumentNullException(nameof(tryBuyUpgrade));
            _afterPurchase = afterPurchase ?? throw new ArgumentNullException(nameof(afterPurchase));

            _timer = new System.Windows.Forms.Timer { Interval = autoBuyTickIntervalMs, Enabled = false };
            _timer.Tick += OnTick;

            // The designer wires button.Click to MainWindow.buttonAutoBuy_Click, which calls Toggle().
            // We only own text/visibility and timer here.
            UpdateButtonText();
        }

        public void Toggle()
        {
            _enabled = !_enabled;
            UpdateButtonText();
            UpdateTimerEnabled();
        }

        public void ApplyUi(int ascendCount)
        {
            bool canShowAutoBuy = ascendCount >= 5;
            if (_button.Visible != canShowAutoBuy) _button.Visible = canShowAutoBuy;

            UpdateButtonText();
            UpdateTimerEnabled();
        }

        private void UpdateTimerEnabled()
        {
            bool shouldRun = _enabled && _getAscendCount() >= 5;
            if (_timer.Enabled != shouldRun) _timer.Enabled = shouldRun;
        }

        private void UpdateButtonText()
        {
            _button.Text = _enabled ? "Auto Buy: On" : "Auto Buy: Off";
        }

        private void OnTick(object? sender, EventArgs e)
        {
            if (!_enabled) return;
            if (_getAscendCount() < 5) return;

            // Attempt purchase; if success, allow caller to persist and refresh UI
            if (_tryBuyUpgrade())
            {
                _afterPurchase();
            }
        }

        public void Dispose()
        {
            _timer.Tick -= OnTick;
            _timer.Dispose();
        }
    }
}