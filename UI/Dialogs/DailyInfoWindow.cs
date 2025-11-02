using System;
using System.Drawing;
using System.Windows.Forms;

namespace WinFormsApp1.UI.Dialogs;

public sealed class DailyInfoWindow : Form
{
    private readonly int _tomorrowMilk;
    private readonly string _tzDisplay;
    private DateTime _nextReset;
    private readonly Label _lblCountdown;

    public DailyInfoWindow(int baseMilkUpgradeCount, int milkStreak)
    {
        _tomorrowMilk = 100 + milkStreak * 10 + baseMilkUpgradeCount * 10;

        DateTime localNow = DateTime.Now;
        TimeZoneInfo localTz = TimeZoneInfo.Local;
        _tzDisplay = Core.Infrastructure.TimeText.UtcOffsetText(localTz.GetUtcOffset(localNow));
        _nextReset = localNow.Date.AddDays(1);

        Text = "Daily Milk Gain Info";
        Size = new Size(520, 280);
        FormBorderStyle = FormBorderStyle.FixedDialog;
        StartPosition = FormStartPosition.CenterParent;
        MaximizeBox = false;
        MinimizeBox = false;

        var lblInfo = new Label
        {
            Text = "Your daily milk gain is a base amount plus your current login streak.\n\n" +
                   "Base gain: 10 x number of milk gain upgrades purchased + 100\n" +
                   "Streak bonus: +10 milk per consecutive day logged in\n\n" +
                   $"Daily reset occurs at 00:00 in your timezone ({_tzDisplay}).",
            Location = new Point(12, 12),
            Size = new Size(480, 120),
            Font = new Font("Segoe UI", 9F)
        };
        Controls.Add(lblInfo);

        var lblTomorrow = new Label
        {
            Text = $"Your milk gain for tomorrow: {_tomorrowMilk}",
            Location = new Point(12, 135),
            Size = new Size(480, 22),
            Font = new Font("Segoe UI", 10F, FontStyle.Bold)
        };
        Controls.Add(lblTomorrow);

        _lblCountdown = new Label
        {
            Text = "",
            Location = new Point(12, 180),
            Size = new Size(480, 46),
            Font = new Font("Segoe UI", 11.5F, FontStyle.Bold),
            ForeColor = Color.DarkBlue
        };
        Controls.Add(_lblCountdown);

        var timer = new System.Windows.Forms.Timer { Interval = 1000 };
        timer.Tick += (s, ev) =>
        {
            DateTime nowLocal = DateTime.Now;
            TimeSpan remaining = _nextReset - nowLocal;
            if (remaining <= TimeSpan.Zero)
            {
                _nextReset = nowLocal.Date.AddDays(1);
                remaining = _nextReset - nowLocal;
            }

            string remainingText = remaining.TotalDays >= 1
                ? string.Format("{0}d {1:00}h {2:00}m {3:00}s", (int)remaining.TotalDays, remaining.Hours, remaining.Minutes, remaining.Seconds)
                : string.Format("{0:00}h {1:00}m {2:00}s", remaining.Hours, remaining.Minutes, remaining.Seconds);

            _lblCountdown.Text = $"Time until daily reset (your timezone {_tzDisplay}): {remainingText}\n" +
                                 $"Local time: {nowLocal:yyyy-MM-dd HH:mm:ss} ({_tzDisplay})";
        };

        FormClosing += (s, ev) => timer.Stop();
        Shown += (s, ev) => timer.Start();
    }
}