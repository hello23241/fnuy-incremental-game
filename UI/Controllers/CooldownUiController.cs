using System.Drawing;
using System.Windows.Forms;

namespace WinFormsApp1.UI.Controllers
{
    // Owns the click-button cooldown visuals and timing. No game logic here.
    public sealed class CooldownUiController
    {
        private readonly System.Windows.Forms.Timer _timer;
        private readonly System.Func<int> _getEffectiveCooldownMs;
        private readonly Button _clickButton;
        private readonly Color _defaultForeColor;
        private int _elapsed;

        public CooldownUiController(
            System.Windows.Forms.Timer timer,
            System.Func<int> getEffectiveCooldownMs,
            Button clickButton,
            Color defaultForeColor)
        {
            _timer = timer;
            _getEffectiveCooldownMs = getEffectiveCooldownMs;
            _clickButton = clickButton;
            _defaultForeColor = defaultForeColor;
        }

        public void Start()
        {
            _elapsed = 0;
            _timer.Interval = 50;
            _timer.Start();
            // Red/white while cooling down
            _clickButton.BackColor = Color.Red;
            _clickButton.ForeColor = Color.White;
        }

        // Returns true when cooldown finishes (so caller can clear its isCooldown flag)
        public bool OnTick()
        {
            _elapsed += _timer.Interval;
            int remaining = _getEffectiveCooldownMs() - _elapsed;

            if (remaining > 0)
            {
                if (_clickButton.BackColor != Color.Red) _clickButton.BackColor = Color.Red;
                if (_clickButton.ForeColor != Color.White) _clickButton.ForeColor = Color.White;
                return false;
            }

            // Finish cooldown
            _timer.Stop();
            _elapsed = 0;
            _clickButton.BackColor = Color.White;
            _clickButton.ForeColor = _defaultForeColor;
            return true;
        }

        // Stop and restore visuals (used when resetting UI externally)
        public void ResetVisuals()
        {
            _timer.Stop();
            _elapsed = 0;
            _clickButton.BackColor = Color.White;
            _clickButton.ForeColor = _defaultForeColor;
        }
    }
}