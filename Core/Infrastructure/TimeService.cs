using System;
using System.Net.Http;
using System.Threading.Tasks;

namespace WinFormsApp1.Core.Infrastructure;

public static class TimeService
{
    private static readonly HttpClient Http = new HttpClient();

    public static async Task<DateTime?> TryGetServerUtcAsync()
    {
        try
        {
            using var req = new HttpRequestMessage(HttpMethod.Head, "https://www.google.com");
            using var resp = await Http.SendAsync(req).ConfigureAwait(false);
            if (resp.Headers.Date.HasValue)
                return resp.Headers.Date.Value.UtcDateTime;
        }
        catch
        {
            // ignore
        }
        return null;
    }
}