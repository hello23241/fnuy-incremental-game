using System.Windows.Forms;
using System.Drawing;
using WinFormsApp1.UI.Presenters;

namespace WinFormsApp1.UI.Appliers
{
    public static class HudApplier
    {
        public static void Apply(
            HudTexts hud,
            string clickButtonText,
            string pointGainText,
            Label labelPoint,
            Button buttonClick,
            Label labelUpgradeCost,
            Label labelPrestigeCost,
            Label labelAscendCost,
            Label labelPointGain,
            Label labelTranscendCost,
            Button buttonPremiumShop,
            Button buttonTranscend,
            System.Windows.Forms.Timer transcendFlashTimer,
            bool canTranscend)
        {
            if (labelPoint.Text != hud.PointsText) labelPoint.Text = hud.PointsText;
            if (buttonClick.Text != clickButtonText) buttonClick.Text = clickButtonText;
            if (labelUpgradeCost.Text != hud.UpgradeCostText) labelUpgradeCost.Text = hud.UpgradeCostText;
            if (labelPrestigeCost.Text != hud.PrestigeCostText) labelPrestigeCost.Text = hud.PrestigeCostText;
            if (labelAscendCost.Text != hud.AscendCostText) labelAscendCost.Text = hud.AscendCostText;
            if (labelPointGain.Text != pointGainText) labelPointGain.Text = pointGainText;
            if (labelTranscendCost.Text != hud.TranscendCostText) labelTranscendCost.Text = hud.TranscendCostText;
            if (buttonPremiumShop.Text != hud.MilkButtonText) buttonPremiumShop.Text = hud.MilkButtonText;

            if (canTranscend)
            {
                if (!transcendFlashTimer.Enabled) transcendFlashTimer.Start();
            }
            else
            {
                if (transcendFlashTimer.Enabled) transcendFlashTimer.Stop();
                if (buttonTranscend.BackColor != Color.Gray) buttonTranscend.BackColor = Color.Gray;
            }
        }
    }
}