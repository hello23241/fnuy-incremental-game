using System;

namespace WinFormsApp1.Core.Infrastructure;

public static class TimeText
{
    public static string UtcOffsetText(TimeSpan offset)
    {
        string sign = offset >= TimeSpan.Zero ? "+" : "-";
        offset = offset.Duration();
        return offset.Minutes == 0
            ? $"UTC{sign}{offset.Hours}"
            : $"UTC{sign}{offset.Hours}:{offset.Minutes:00}";
    }
}