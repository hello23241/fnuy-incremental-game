using System;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace WinFormsApp1.Core.Updates;

public sealed class UpdateService
{
    private static readonly HttpClient SharedClient = new HttpClient();

    private sealed class Manifest
    {
        [JsonProperty("latestVersion")] public string LatestVersion { get; set; } = "0.0.0.0";
        [JsonProperty("downloadUrl")] public string DownloadUrl { get; set; } = "";
        [JsonProperty("changelog")] public string Changelog { get; set; } = "";
        [JsonProperty("releaseDate")] public string ReleaseDate { get; set; } = "";
    }

    public sealed record UpdateInfo(
        bool IsUpdateAvailable,
        Version CurrentVersion,
        Version LatestVersion,
        string DownloadUrl,
        string Changelog,
        DateTime? ReleaseDateUtc,
        string LastUpdateText
    );

    public static async Task<UpdateInfo> CheckAsync(
        string manifestUrl,
        Version currentVersion,
        CancellationToken cancellationToken = default)
    {
        string json = await SharedClient.GetStringAsync(manifestUrl, cancellationToken).ConfigureAwait(false);
        if (string.IsNullOrWhiteSpace(json))
            throw new InvalidOperationException("Manifest response was empty.");

        var manifest = JsonConvert.DeserializeObject<Manifest>(json)
            ?? throw new InvalidOperationException("Manifest deserialization failed.");

        var latest = new Version(manifest.LatestVersion);
        var (releaseDate, lastUpdateText) = ParseReleaseDateToText(manifest.ReleaseDate);

        bool isUpdate = latest > currentVersion;
        return new UpdateInfo(
            IsUpdateAvailable: isUpdate,
            CurrentVersion: currentVersion,
            LatestVersion: latest,
            DownloadUrl: manifest.DownloadUrl,
            Changelog: manifest.Changelog ?? "",
            ReleaseDateUtc: releaseDate,
            LastUpdateText: lastUpdateText
        );
    }

    private static (DateTime? releaseDateUtc, string lastUpdateText) ParseReleaseDateToText(string? releaseDateRaw)
    {
        if (string.IsNullOrWhiteSpace(releaseDateRaw))
            return (null, "Last update: unknown");

        if (DateTime.TryParseExact(
                releaseDateRaw,
                new[] { "yyyyMMdd", "yyyy-MM-dd" },
                System.Globalization.CultureInfo.InvariantCulture,
                System.Globalization.DateTimeStyles.AssumeUniversal | System.Globalization.DateTimeStyles.AdjustToUniversal,
                out var releaseUtc))
        {
            int daysAgo = (int)Math.Floor((DateTime.UtcNow.Date - releaseUtc.Date).TotalDays);
            string text = daysAgo < 0
                ? "Last update: in the future"
                : $"Last update: {daysAgo} day{(daysAgo == 1 ? "" : "s")} ago";
            return (releaseUtc, text);
        }

        return (null, "Last update: unknown");
    }
}