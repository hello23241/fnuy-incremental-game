using System;
using System.Drawing;
using System.Windows.Forms;
using WinFormsApp1.UI.Animations;

namespace WinFormsApp1.UI.Controllers
{
    // Encapsulates Transcend button interactivity + visual/animation state.
    public sealed class TranscendUiController : IDisposable
    {
        private readonly Button _button;
        private readonly System.Windows.Forms.Timer _timer;
        private readonly TranscendFlashAnimator _animator;

        public TranscendUiController(Button button, System.Windows.Forms.Timer timer, TranscendFlashAnimator animator)
        {
            _button = button ?? throw new ArgumentNullException(nameof(button));
            _timer = timer ?? throw new ArgumentNullException(nameof(timer));
            _animator = animator ?? throw new ArgumentNullException(nameof(animator));

            _timer.Tick += OnTick;
        }

        // Call each UI refresh with the latest capability state.
        public void Apply(bool canTranscend)
        {
            if (_button.Enabled != canTranscend)
                _button.Enabled = canTranscend;

            if (canTranscend)
            {
                _button.UseVisualStyleBackColor = false;

                if (!_timer.Enabled) _timer.Start();

                if (_button.BackColor == Color.Gray || _button.BackColor == SystemColors.Control)
                    _button.BackColor = Color.DarkViolet;
            }
            else
            {
                if (_timer.Enabled) _timer.Stop();

                _button.UseVisualStyleBackColor = false;
                _button.BackColor = Color.Gray; // match other disabled buttons
            }
        }

        // Call when a transcend is performed successfully to set base color and stop glow.
        public void OnTranscendPerformed()
        {
            if (_timer.Enabled) _timer.Stop();
            _button.BackColor = Color.DarkViolet;
        }

        private void OnTick(object? sender, EventArgs e)
        {
            // Animate only when interactable and using custom background
            if (!_button.Enabled || _button.UseVisualStyleBackColor)
                return;

            _button.BackColor = _animator.Next();
        }

        public void Dispose()
        {
            _timer.Tick -= OnTick;
        }
    }
}