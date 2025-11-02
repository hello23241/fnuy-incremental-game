using System;
using System.Windows.Forms;
using BreakInfinity;

namespace WinFormsApp1.UI.Controllers
{
    // Owns the generator tick loop. No game math inside; delegates to callbacks.
    public sealed class GeneratorLoopController : IDisposable
    {
        private readonly System.Windows.Forms.Timer _timer;
        private readonly Func<bool> _hasGenerators;
        private readonly Func<BigDouble> _applyTick;
        private readonly Action<BigDouble> _afterTick;

        public GeneratorLoopController(
            System.Windows.Forms.Timer timer,
            Func<bool> hasGenerators,
            Func<BigDouble> applyTick,
            Action<BigDouble> afterTick)
        {
            _timer = timer ?? throw new ArgumentNullException(nameof(timer));
            _hasGenerators = hasGenerators ?? throw new ArgumentNullException(nameof(hasGenerators));
            _applyTick = applyTick ?? throw new ArgumentNullException(nameof(applyTick));
            _afterTick = afterTick ?? throw new ArgumentNullException(nameof(afterTick));

            _timer.Tick += OnTick;
        }

        private void OnTick(object? sender, EventArgs e)
        {
            if (!_hasGenerators())
                return;

            var passiveGain = _applyTick();
            _afterTick(passiveGain);
        }

        public void Dispose()
        {
            _timer.Tick -= OnTick;
        }
    }
}