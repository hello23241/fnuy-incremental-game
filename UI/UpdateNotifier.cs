using System;
using System.Diagnostics;
using System.Reflection;
using System.Threading.Tasks;
using System.Windows.Forms;
using WinFormsApp1.Core.Updates;

namespace WinFormsApp1.UI
{
    public static class UpdateNotifier
    {
        private const string ManifestUrl = "https://hello23241.github.io/fnuy-incremental-manifest/manifest.json";

        public static async Task CheckAsync(IWin32Window owner)
        {
            try
            {
                var currentVersion = new Version(Assembly.GetExecutingAssembly().GetName().Version?.ToString() ?? "1.0.0.0");
                var info = await UpdateService.CheckAsync(ManifestUrl, currentVersion);

                string changelogMessage = $"{info.LastUpdateText}\n\nChangelog:\n{info.Changelog}";

                if (info.IsUpdateAvailable)
                {
                    var result = MessageBox.Show(
                        owner,
                        $"A new version ({info.LatestVersion}) is available!\n\n{changelogMessage}\n\nDo you want to download it now?",
                        "Update Available",
                        MessageBoxButtons.YesNo,
                        MessageBoxIcon.Information);

                    if (result == DialogResult.Yes)
                    {
                        Process.Start(new ProcessStartInfo
                        {
                            FileName = info.DownloadUrl,
                            UseShellExecute = true
                        });
                    }
                }
                else
                {
                    MessageBox.Show(
                        owner,
                        $"You are running the latest version ({info.CurrentVersion}).\n\n{changelogMessage}",
                        "No Update",
                        MessageBoxButtons.OK,
                        MessageBoxIcon.Information);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(owner, $"Failed to check for updates: {ex.Message}", "Update Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
    }
}