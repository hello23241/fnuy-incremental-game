using System;

namespace WinFormsApp1.Core.Game;

public static class SoundEffects
{
    private static bool _registered;

    private static void TryPlay(string key, float volume = 1.0f)
    {
        try { WinFormsApp1.AudioManagerNAudio.Play(key, volume); } catch { }
    }

    public static void Gain(float volume = 0.75f) => TryPlay("gain", volume);
    public static void Upgrade(float volume = 0.5f) => TryPlay("upgrade", volume);
    public static void Prestige(float volume = 0.9f) => TryPlay("prestige", volume);
    public static void Ascend(float volume = 0.9f) => TryPlay("ascend", volume);
    public static void ChallengeStart(float volume = 0.9f) => TryPlay("challengestart", volume);
    public static void ChallengeComplete(float volume = 0.95f) => TryPlay("challengecomplete", volume);
    public static void ChallengeCancelled(float volume = 0.9f) => TryPlay("challengecancelled", volume);
    public static void MilkEarned(float volume = 0.95f) => TryPlay("milkearned", volume);
    public static void MilkSpent(float volume = 0.95f) => TryPlay("milkspent", volume);
    public static void GeneratorPurchase(float volume = 0.9f) => TryPlay("genpurchase", volume);
    public static void Transcend(float volume = 0.95f) => TryPlay("transcend", volume);

    // Call once at startup
    public static void RegisterAll()
    {
        if (_registered) return;
        _registered = true;

        // Try to register specific resource names; fall back to key-only overload
        TryRegister("gain", "Resources.coin.wav");
        TryRegister("upgrade"); // auto-resolve embedded resource named "upgrade"
        TryRegister("prestige", "Resources.prestige.aif");
        TryRegister("ascend", "Resources.ascend.mp3");
        TryRegister("challengecomplete", "Resources.challengecomplete.mp3");
        TryRegister("challengecancelled", "Resources.challengecancelled.aif");
        TryRegister("challengestart", "Resources.challengestarted.wav", "Resources.levelupTRANS.aif", "Resources.completetask.mp3");
        TryRegister("milkearned", "Resources.milkearned.wav");
        TryRegister("milkspent", "Resources.milkspent.mp3");
        TryRegister("genpurchase", "Resources.genpurchase.mp3");
        TryRegister("transcend", "Resources.transcend.wav");
    }

    private static void TryRegister(string key, params string[] resourceNames)
    {
        // Try explicit names first
        foreach (var rn in resourceNames)
        {
            try
            {
                WinFormsApp1.AudioManagerNAudio.RegisterFromEmbeddedResource(key, rn);
                return;
            }
            catch { /* try next candidate */ }
        }
        // Fall back to key-only overload if available
        try { WinFormsApp1.AudioManagerNAudio.RegisterFromEmbeddedResource(key); } catch { }
    }
}