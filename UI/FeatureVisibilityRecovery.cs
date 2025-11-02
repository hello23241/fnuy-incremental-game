using System.Windows.Forms;

namespace WinFormsApp1.UI
{
    // Ensures features are visible based on progression counts, regardless of persisted flags.
    public static class FeatureVisibilityRecovery
    {
        public static void Recover(
            int prestigeCount,
            int ascendCount,
            int cooldownDuration,
            Button buttonPrestige,
            Label labelPrestigeCost,
            Button buttonGenerator,
            Label labelGeneratorInfo,
            Label labelSoftCap,
            Label labelPrestigeInfo,
            Button buttonPremiumShop,
            Button buttonInfoDailyGain,
            Button buttonAscend,
            Label labelAscendCost,
            Button buttonOpenAscensionShop,
            Button buttonTranscend,
            Label labelTranscendCost)
        {
            // Prestige UI: has prestiged or cooldown already reduced
            if (!buttonPrestige.Visible && (prestigeCount > 0 || cooldownDuration <= 500))
            {
                buttonPrestige.Visible = true;
                labelPrestigeCost.Visible = true;
            }

            // Generators/prereqs: after first prestige
            if (!buttonGenerator.Visible && prestigeCount > 0)
            {
                buttonGenerator.Visible = true;
                labelGeneratorInfo.Visible = true;
                labelSoftCap.Visible = true;
                labelPrestigeInfo.Visible = true;
                buttonPremiumShop.Visible = true;
                buttonInfoDailyGain.Visible = true;
            }

            // Ascension controls: after first ascension
            if (!buttonOpenAscensionShop.Visible && ascendCount > 0)
            {
                buttonAscend.Visible = true;
                labelAscendCost.Visible = true;
                buttonOpenAscensionShop.Visible = true;
                buttonTranscend.Visible = true;
                labelTranscendCost.Visible = true;
            }
        }
    }
}