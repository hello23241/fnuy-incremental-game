using System.Windows.Forms;
using System.Drawing;
using System.Net.Http;
using System.Threading.Tasks;
using System.Text.Json;

namespace WinFormsApp1
{
    public partial class TranscendWindow : Form
    {
        private PictureBox pictureBox;
        private Label label;
        private ProgressBar progressBar;
        private System.Windows.Forms.Timer progressTimer;
        private int progressElapsed;

        public TranscendWindow()
        {
            InitializeComponent();
            // Start fetching the image after the form is shown
            this.Shown += async (s, e) =>
            {
                StartProgressBar();
                await LoadRandomImageAsync();
            };
        }

        private void InitializeComponent()
        {
            this.Text = "Transcend";
            this.BackColor = Color.FromArgb(40, 0, 80);

            // Double the window size: 400x320 -> 800x640
            this.ClientSize = new Size(800, 640);

            // Make window size unchangeable and disable maximize button
            this.FormBorderStyle = FormBorderStyle.FixedSingle;
            this.MaximizeBox = false;

            // Double the image display size: 256x192 -> 512x384
            pictureBox = new PictureBox
            {
                SizeMode = PictureBoxSizeMode.Zoom,
                Size = new Size(512, 384),
                Location = new Point((this.ClientSize.Width - 512) / 2, 40),
                BackColor = Color.Black
            };

            // ProgressBar above the label
            progressBar = new ProgressBar
            {
                Size = new Size(600, 32),
                Location = new Point((this.ClientSize.Width - 600) / 2, this.ClientSize.Height - 180),
                Minimum = 0,
                Maximum = 100,
                Value = 0,
                Style = ProgressBarStyle.Continuous
            };

            label = new Label
            {
                Text = "You have transcended!",
                ForeColor = Color.White,
                Font = new Font("Segoe UI", 18, FontStyle.Bold),
                AutoSize = false,
                TextAlign = ContentAlignment.MiddleCenter,
                Dock = DockStyle.Bottom,
                Height = 120 // doubled for larger window
            };

            this.Controls.Add(pictureBox);
            this.Controls.Add(progressBar);
            this.Controls.Add(label);
        }
        private void StartProgressBar()
        {
            progressElapsed = 0;
            progressBar.Value = 0;
            progressTimer = new System.Windows.Forms.Timer();
            progressTimer.Interval = 10; // 50ms for smoothness
            progressTimer.Tick += (s, e) =>
            {
                progressElapsed += progressTimer.Interval;
                int percent = Math.Min(100, (int)(progressElapsed / 3000.0 * 100));
                progressBar.Value = percent;
                if (progressElapsed >= 3000)
                {
                    progressBar.Value = 100;
                    progressTimer.Stop();
                }
            };
            progressTimer.Start();
        }
        private async Task LoadRandomImageAsync()
        {
            try
            {
                using var http = new HttpClient();
                http.DefaultRequestHeaders.UserAgent.ParseAdd("Mozilla/5.0 (Windows NT 10.0; Win64; x64)");

                string apiUrl = "https://safebooru.org/index.php?page=dapi&s=post&q=index&json=1&tags=myrtle_(arknights)+1girl";
                var json = await http.GetStringAsync(apiUrl);

                var doc = JsonDocument.Parse(json);
                int count = doc.RootElement.GetArrayLength();
                if (count > 0)
                {
                    // Pick a random post from the results
                    var rand = new Random();
                    var post = doc.RootElement[rand.Next(count)];

                    if (post.TryGetProperty("directory", out var dirProp) &&
                        post.TryGetProperty("image", out var imgProp))
                    {
                        // directory may be a number, so use GetRawText() and trim quotes if present
                        string directory = dirProp.ValueKind == JsonValueKind.String
                            ? dirProp.GetString()
                            : dirProp.GetRawText().Trim('"');

                        string image = imgProp.GetString();
                        string imageUrl = $"https://safebooru.org/images/{directory}/{image}";

                        var imageBytes = await http.GetByteArrayAsync(imageUrl);
                        using var ms = new System.IO.MemoryStream(imageBytes);
                        pictureBox.Image = Image.FromStream(ms);
                        label.Text = "You have transcended!";
                        return;
                    }
                }
                label.Text = "No image found.";
            }
            catch (Exception ex)
            {
                label.Text = "Failed to fetch image: " + ex.Message;
            }
        }
    }
}