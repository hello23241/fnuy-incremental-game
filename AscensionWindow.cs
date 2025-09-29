using BreakInfinity;
using System.Drawing;
using System.Windows.Forms;

namespace WinFormsApp1
{
    public partial class AscensionWindow : Form
    {
        private bool[] challengesCompleted;

        // Main constructor: always called
        private bool challengeActive = false;
        // Add this method to handle all icon clicks:
        private void PictureBoxChallengeIcon_Click(int challengeIndex)
        {
            currentChallengeIndex = challengeIndex;
            string challengeName = "";
            string challengeDesc = "";
            string challengeReward = "";

            switch (challengeIndex)
            {
                case 0:
                    challengeName = "Simple nerf simple buff";
                    challengeDesc = "Point gain is divided by 10";
                    challengeReward = "Challenge Reward:\nPoint gain x1.5";
                    break;
                case 1:
                    challengeName = "Prestiging is for newbies";
                    challengeDesc = "Prestige effectiveness halved";
                    challengeReward = "Challenge Reward:\nPrestige multi x1.1";
                    break;
                case 2:
                    challengeName = "Have some patience";
                    challengeDesc = "Click cooldown and generator tick speed increased to 10 seconds";
                    challengeReward = "Challenge Reward:\nClick cooldown and\ngenerator tick -0.1s";
                    break;
                case 3:
                    challengeName = "Final challenge";
                    challengeDesc = "Challenges 1-3 all at once!";
                    challengeReward = "Challenge Reward:\nSoft cap threshold 10k";
                    break;
                default:
                    challengeName = "Unknown challenge";
                    challengeDesc = "";
                    challengeReward = "";
                    break;
            }

            richTextBoxChallengeInfo.Clear();
            richTextBoxChallengeInfo.SelectionFont = new Font(richTextBoxChallengeInfo.Font, FontStyle.Bold);
            richTextBoxChallengeInfo.AppendText(challengeName + "\n\n");
            richTextBoxChallengeInfo.SelectionFont = new Font(richTextBoxChallengeInfo.Font, FontStyle.Regular);
            richTextBoxChallengeInfo.AppendText(challengeDesc);

            richTextBoxChallengeInfo.Visible = true;
            labelChallengeReward.Text = challengeReward;
            labelChallengeReward.Visible = !string.IsNullOrEmpty(challengeReward);
            buttonChallengeAction.Text = challengeActive ? "Cancel" : "Start";
            buttonChallengeAction.Visible = true;
        }
        private void buttonChallengeAction_Click(object sender, EventArgs e)
        {
            challengeActive = !challengeActive;
            buttonChallengeAction.Text = challengeActive ? "Cancel" : "Start";
            richTextBoxChallengeInfo.Text = challengeActive
                ? "Challenge is now active! (placeholder)"
                : "Choose a challenge...";

            if (challengeActive)
            {
                ActiveChallengeIndex = currentChallengeIndex;
                this.DialogResult = DialogResult.OK;
                this.Close();
            }
        }
        public bool ChallengeActive => challengeActive;
        public int ActiveChallengeIndex { get; private set; } = -1;
        private int currentChallengeIndex = -1;
        public AscensionWindow(int ascensionCount, bool[] challengesCompleted)
        {
            InitializeComponent();
            this.challengesCompleted = (bool[])challengesCompleted.Clone();
            InitializeWindow(ascensionCount);

            // Ensure close button is placed correctly after layout
            this.Shown += (s, e) =>
            {
                buttonClose.Location = new Point(panelTitleBar.Width - buttonClose.Width, 0);
            };
        }
        private void InitializeWindow(int ascensionCount)
        {
            // Set dynamic text for boosts
            labelBoosts.Text = $"Ascension Count Boosts:\n" +
                $"You have {ascensionCount} ascensions.\n\n";

            if (ascensionCount >= 1)
            {
                double multiplier = Math.Pow(1.1, ascensionCount);
                labelBoosts.Text += $"1. [Active] Point multiplier: x{multiplier:F2} (1.1x per ascension)\n";
            }
            else
            {
                labelBoosts.Text += $"1. [Locked] Point multiplier: Unlocks at 1 ascension\n";
            }

            if (ascensionCount >= 2)
            {
                int reductions = ascensionCount / 2;
                double cooldownReduction = reductions * 5;
                labelBoosts.Text += $"2. [Active] Button cooldown reduced by {cooldownReduction}% ({reductions}×5%), can hold for points\n";
            }
            else
            {
                labelBoosts.Text += $"2. [Locked] Button cooldown reduction & hold: Unlocks at 2 ascensions\n";
            }

            // Event handlers for window controls

            buttonClose.MouseEnter += (s, e) => buttonClose.BackColor = Color.FromArgb(96, 32, 32);
            buttonClose.MouseLeave += (s, e) => buttonClose.BackColor = Color.FromArgb(32, 64, 128);
            buttonClose.Click += (s, e) => { this.DialogResult = DialogResult.OK; this.Close(); };

            panelTitleBar.MouseDown += panelTitleBar_MouseDown;
            panelTitleBar.MouseMove += panelTitleBar_MouseMove;
            panelTitleBar.MouseUp += panelTitleBar_MouseUp;

            panelTitleBar.Resize += (s, e) =>
            {
                buttonClose.Location = new Point(panelTitleBar.Width - 40, 0);
            };
        }

        public bool[] GetChallengesCompleted()
        {
            return (bool[])challengesCompleted.Clone();
        }

        // --- Dragging logic (same as MainForm) ---
        private bool dragging = false;
        private Point dragCursorPoint;
        private Point dragFormPoint;

        private void panelTitleBar_MouseDown(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Left)
            {
                dragging = true;
                dragCursorPoint = Cursor.Position;
                dragFormPoint = this.Location;
            }
        }

        private void panelTitleBar_MouseMove(object sender, MouseEventArgs e)
        {
            if (dragging)
            {
                Point diff = Point.Subtract(Cursor.Position, new Size(dragCursorPoint));
                this.Location = Point.Add(dragFormPoint, new Size(diff));
            }
        }

        private void panelTitleBar_MouseUp(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Left)
            {
                dragging = false;
            }
        }
    }
}