using System;

namespace WinFormsApp1.Core.Game;

public static class ChallengeTextPresenter
{
    public static (bool Visible, string Text) Build(int activeChallengeIndex)
    {
        if (activeChallengeIndex == -1)
            return (false, string.Empty);

        string text = $"Challenge {activeChallengeIndex + 1} active";
        switch (activeChallengeIndex)
        {
            case 0:
                text += "\nPoint gain /10";
                break;
            case 1:
                text += "\nPrestige effectiveness /2";
                break;
            case 2:
                text += "\nPoint gain interval 10s";
                break;
            case 3:
                text += "\nReduced point gain,\nprestige effectiveness\nand increased cooldown";
                break;
        }

        return (true, text);
    }
}