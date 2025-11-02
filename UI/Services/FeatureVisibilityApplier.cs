using System.Windows.Forms;
using WinFormsApp1.Core.Game;
using WinFormsApp1.SaveSystem;
using GameStateDto = WinFormsApp1.SaveSystem.GameState;

namespace WinFormsApp1.UI.Services
{
    public static class FeatureVisibilityApplier
    {
        public static void Apply(
            GameStateDto state,
            Button buttonPrestige,
            Label labelPrestigeCost,
            Label labelPrestigeInfo,
            Button buttonGenerator,
            Label labelGeneratorInfo,
            Label labelSoftCap,
            Button buttonPremiumShop,
            Button buttonInfoDailyGain,
            Button buttonAscend,
            Label labelAscendCost,
            Button buttonOpenAscensionShop,
            Button buttonTranscend,
            Label labelTranscendCost)
        {
            var flags = FeatureUnlockService.ComputeVisibility(state);

            buttonPrestige.Visible = flags.ShowPrestigeButton;
            labelPrestigeCost.Visible = flags.ShowPrestigeCost;
            labelPrestigeInfo.Visible = flags.ShowPrestigeInfo;

            buttonGenerator.Visible = flags.ShowGeneratorButton;
            labelGeneratorInfo.Visible = flags.ShowGeneratorInfo;
            labelSoftCap.Visible = flags.ShowSoftCap;

            buttonPremiumShop.Visible = flags.ShowPremiumShop;
            buttonInfoDailyGain.Visible = flags.ShowInfoDailyGain;

            buttonAscend.Visible = flags.ShowAscendButton;
            labelAscendCost.Visible = flags.ShowAscendCost;

            buttonOpenAscensionShop.Visible = flags.ShowAscensionShop;
            buttonTranscend.Visible = flags.ShowTranscendButton;
            labelTranscendCost.Visible = flags.ShowTranscendCost;
        }
    }
}