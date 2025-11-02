using WinFormsApp1.SaveSystem;

namespace WinFormsApp1.Core.Game;

public readonly record struct FeatureVisibility(
    bool ShowPrestigeButton,
    bool ShowPrestigeCost,
    bool ShowPrestigeInfo,
    bool ShowGeneratorButton,
    bool ShowGeneratorInfo,
    bool ShowSoftCap,
    bool ShowPremiumShop,
    bool ShowInfoDailyGain,
    bool ShowAscendButton,
    bool ShowAscendCost,
    bool ShowAscensionShop,
    bool ShowTranscendButton,
    bool ShowTranscendCost
);

public static class FeatureUnlockService
{
    public static FeatureVisibility ComputeVisibility(GameState state)
    {
        // Base on persisted flags to keep logic centralized and testable
        bool prestige = state.HasUnlockedPrestige;
        bool generators = state.HasUnlockedGenerators;
        bool ascend = state.HasUnlockedAscension;
        bool ascended = state.HasAscended;

        return new FeatureVisibility(
            ShowPrestigeButton: prestige,
            ShowPrestigeCost: prestige,
            ShowPrestigeInfo: generators, // kept consistent with existing behavior
            ShowGeneratorButton: generators,
            ShowGeneratorInfo: generators,
            ShowSoftCap: generators,
            ShowPremiumShop: generators,
            ShowInfoDailyGain: generators,
            ShowAscendButton: ascend,
            ShowAscendCost: ascend,
            ShowAscensionShop: ascended,
            ShowTranscendButton: ascended,
            ShowTranscendCost: ascended
        );
    }
}