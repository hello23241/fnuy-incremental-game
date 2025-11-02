using System.Drawing;
using System.Windows.Forms;
using BreakInfinity;
using WinFormsApp1.Core.Game;

namespace WinFormsApp1.UI.Appliers
{
    public static class ButtonStateApplier
    {
        public static void Apply(
            BigDouble point,
            BigDouble upgradeCost,
            BigDouble prestigeCost,
            BigDouble ascendCost,
            BigDouble generatorCost,
            Button buttonUpgrade,
            Button buttonPrestige,
            Button buttonAscend,
            Button buttonGenerator)
        {
            var states = ButtonStateService.Compute(
                point,
                upgradeCost,
                prestigeCost,
                ascendCost,
                generatorCost);

            buttonUpgrade.Enabled = states.UpgradeEnabled;
            buttonPrestige.Enabled = states.PrestigeEnabled;
            buttonAscend.Enabled = states.AscendEnabled;
            buttonGenerator.Enabled = states.GeneratorEnabled;

            buttonUpgrade.BackColor = buttonUpgrade.Enabled ? Color.LightGreen : Color.Gray;
            buttonPrestige.BackColor = buttonPrestige.Enabled ? Color.LightBlue : Color.Gray;
            buttonAscend.BackColor = buttonAscend.Enabled ? Color.MediumPurple : Color.Gray;
            buttonGenerator.BackColor = buttonGenerator.Enabled ? Color.LightGreen : Color.Gray;
        }
    }
}