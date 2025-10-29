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
        private PictureBox[] pictureBoxChallenges = new PictureBox[4]; // Array to hold challenge icon
        private Label labelChallengeRequirement;

        // Parameterless constructor for the WinForms designer and default runtime use
        public AscensionWindow() : this(ascensionCount: 0, challengesCompleted: new bool[4], activeChallengeIndex: -1)
        {
        }

        // Add this method to handle all icon clicks:
        private void PictureBoxChallengeIcon_Click(int challengeIndex)
        {
            // If a challenge is already active, only allow clicking the active one
            if (challengeActive && challengeIndex != ActiveChallengeIndex)
                return;

            currentChallengeIndex = challengeIndex;
            string challengeName = "";
            string challengeDesc = "";
            string challengeReward = "";
            string challengeRequirement = "";

            switch (challengeIndex)
            {
                case 0:
                    challengeName = "Simple nerf simple buff";
                    challengeDesc = "Point gain is divided by 5";
                    challengeReward = "Challenge Reward:\nPoint gain x3";
                    challengeRequirement = "Requirement: Reach 1,000,000 points.";
                    break;
                case 1:
                    challengeName = "Prestiging is for newbies";
                    challengeDesc = "Prestige effectiveness halved";
                    challengeReward = "Challenge Reward:\nIncreased prestige\neffectiveness";
                    challengeRequirement = "Requirement: Prestige 8 times.";
                    break;
                case 2:
                    challengeName = "Have some patience";
                    challengeDesc = "Click cooldown and generator tick speed is set to 10 seconds";
                    challengeReward = "Challenge Reward:\nClick cooldown and\ngenerator tick -0.1s";
                    challengeRequirement = "Requirement: Buy 2 generators.";
                    break;
                case 3:
                    challengeName = "Final challenge";
                    challengeDesc = "Challenges 1-3 all at once!";
                    challengeReward = "Challenge Reward:\nSoft cap threshold\nincreased to 100k";
                    challengeRequirement = "Requirement: Reach 25,000,000 points.";
                    break;
                default:
                    challengeName = "Unknown challenge";
                    challengeDesc = "";
                    challengeReward = "";
                    challengeRequirement = "";
                    break;
            }

            richTextBoxChallengeInfo.Visible = true;
            labelChallengeReward.Text = challengeReward;
            labelChallengeReward.Visible = !string.IsNullOrEmpty(challengeReward);

            // Button styling and text
            if (challengesCompleted[challengeIndex])
            {
                buttonChallengeAction.Text = "Active";
                buttonChallengeAction.BackColor = Color.LightGreen;
                buttonChallengeAction.ForeColor = Color.Black;
                buttonChallengeAction.Enabled = false;
            }
            else
            {
                buttonChallengeAction.Text = (challengeActive && challengeIndex == ActiveChallengeIndex) ? "Cancel" : "Start";
                buttonChallengeAction.BackColor = Color.White;
                buttonChallengeAction.ForeColor = Color.Black;
                buttonChallengeAction.Enabled = true;
            }
            buttonChallengeAction.Visible = true;

            richTextBoxChallengeInfo.Clear();
            richTextBoxChallengeInfo.SelectionFont = new Font(richTextBoxChallengeInfo.Font, FontStyle.Bold);
            richTextBoxChallengeInfo.AppendText(challengeName + "\n\n");
            richTextBoxChallengeInfo.SelectionFont = new Font(richTextBoxChallengeInfo.Font, FontStyle.Regular);
            richTextBoxChallengeInfo.AppendText(challengeDesc + "\n\n");
            // Add requirement label below description
            labelChallengeRequirement.Text = challengeRequirement;
            labelChallengeRequirement.Visible = !string.IsNullOrEmpty(challengeRequirement);
            // Ensure label is positioned below the challenge info and within window bounds
            labelChallengeRequirement.Width = richTextBoxChallengeInfo.Width;
            // Position just below the rich text box; previous +62 pushed it outside the tab.
            labelChallengeRequirement.Location = new Point(richTextBoxChallengeInfo.Left, richTextBoxChallengeInfo.Bottom + 4);
            labelChallengeRequirement.BringToFront();
            richTextBoxChallengeInfo.Visible = true;
            labelChallengeReward.Text = challengeReward;
            labelChallengeReward.Visible = !string.IsNullOrEmpty(challengeReward);
            buttonChallengeAction.Visible = true;
        }
        private void buttonChallengeAction_Click(object sender, EventArgs e)
        {
            // If challenge is active and the button says Cancel, cancel the challenge
            if (challengeActive && currentChallengeIndex == ActiveChallengeIndex)
            {
                ChallengeCancelled = true;
                this.DialogResult = DialogResult.OK;
                this.Close();
                return;
            }
            // Only allow starting a challenge if not already active
            if (!challengeActive)
            {
                challengeActive = true;
                ActiveChallengeIndex = currentChallengeIndex;
                buttonChallengeAction.Text = "Cancel";
                richTextBoxChallengeInfo.Text = "Challenge is now active! (placeholder)";
                this.DialogResult = DialogResult.OK;
                this.Close();
            }
            UpdateChallengeIcons();
        }
        private void UpdateChallengeIcons()
        {
            for (int i = 0; i < 4; i++)
            {
                if (challengesCompleted[i])
                {
                    pictureBoxChallenges[i].Enabled = true; // Keep clickable
                    pictureBoxChallenges[i].BackColor = Color.LightGreen;
                }
                else if (challengeActive)
                {
                    if (i == ActiveChallengeIndex)
                    {
                        pictureBoxChallenges[i].Enabled = true;
                        pictureBoxChallenges[i].BackColor = Color.White;
                    }
                    else
                    {
                        pictureBoxChallenges[i].Enabled = false;
                        pictureBoxChallenges[i].BackColor = Color.LightGray;
                    }
                }
                else
                {
                    pictureBoxChallenges[i].Enabled = true;
                    pictureBoxChallenges[i].BackColor = Color.White;
                }
            }
        }
        public bool ChallengeActive => challengeActive;
        public int ActiveChallengeIndex { get; private set; } = -1;
        private int currentChallengeIndex = -1;
        public bool ChallengeCancelled { get; private set; } = false;
        public AscensionWindow(int ascensionCount, bool[] challengesCompleted, int activeChallengeIndex = -1)
        {
            InitializeComponent();
            this.challengesCompleted = (bool[])challengesCompleted.Clone();
            pictureBoxChallenges[0] = pictureBoxChallengeIcon0;
            pictureBoxChallenges[1] = pictureBoxChallengeIcon1;
            pictureBoxChallenges[2] = pictureBoxChallengeIcon2;
            pictureBoxChallenges[3] = pictureBoxChallengeIcon3;
            // Set challengeActive and ActiveChallengeIndex based on passed value
            if (activeChallengeIndex != -1)
            {
                challengeActive = true;
                ActiveChallengeIndex = activeChallengeIndex;
            }
            // Create and position the requirement label under the challenge info
            labelChallengeRequirement = new Label();
            labelChallengeRequirement.AutoSize = false;
            labelChallengeRequirement.Font = new Font(richTextBoxChallengeInfo.Font, FontStyle.Bold);
            labelChallengeRequirement.ForeColor = Color.Red;
            labelChallengeRequirement.Width = richTextBoxChallengeInfo.Width;
            labelChallengeRequirement.Height = 24;
            labelChallengeRequirement.TextAlign = ContentAlignment.MiddleLeft;
            labelChallengeRequirement.Location = new Point(richTextBoxChallengeInfo.Left, richTextBoxChallengeInfo.Bottom + 4);
            labelChallengeRequirement.Visible = false;
            labelChallengeRequirement.BackColor = Color.Transparent;
            // Add the requirement label to the Challenges tab so it only appears when that tab is active
            tabChallenges.Controls.Add(labelChallengeRequirement);
            labelChallengeRequirement.BringToFront();
            InitializeWindow(ascensionCount);
            UpdateChallengeIcons();
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
                double multiplier = Math.Pow(1.5, ascensionCount);
                labelBoosts.Text += $"1. [Active] Point multiplier: x{multiplier:F2} (1.5x compounding per ascension)\n";
            }
            else
            {
                labelBoosts.Text += $"1. [Locked] Point multiplier: Unlocks at 1 ascension\n";
            }

            if (ascensionCount >= 2)
            {
                int reductions = ascensionCount / 2;
                double cooldownReduction = reductions * 5;
                labelBoosts.Text += $"2. [Active] Button cooldown reduced by {cooldownReduction}% ({reductions}×5%), starts autoclicking at half the speed\n";
            }
            else
            {
                labelBoosts.Text += $"2. [Locked] Button cooldown reduction and auto clicker: Unlocks at 2 ascensions\n";
            }

            // Ascension milestone at 3
            if (ascensionCount >= 3)
            {
                labelBoosts.Text += $"3. [Active] Right click upgrade to buy max\n";
            }
            else
            {
                labelBoosts.Text += $"3. [Locked] Unlocks buy max for upgrade\n";
            }

            // Ascension milestone at 5
            if (ascensionCount >= 5)
            {
                labelBoosts.Text += $"4. [Active] Placeholder for 5 ascensions effect\n";
            }
            else
            {
                labelBoosts.Text += $"4. [Locked] Placeholder: Unlocks at 5 ascensions\n";
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

        // Designer-wired event handlers
        private void AscensionWindow_Resize(object sender, System.EventArgs e)
        {
            panelTitleBar.Width = this.ClientSize.Width;
            buttonClose.Location = new Point(panelTitleBar.Width - buttonClose.Width, 0);
        }
        private void panelTitleBar_Resize(object sender, System.EventArgs e)
        {
            buttonClose.Location = new Point(panelTitleBar.Width - buttonClose.Width, 0);
        }
        private void pictureBoxChallengeIcon0_Click(object sender, System.EventArgs e) => PictureBoxChallengeIcon_Click(0);
        private void pictureBoxChallengeIcon1_Click(object sender, System.EventArgs e) => PictureBoxChallengeIcon_Click(1);
        private void pictureBoxChallengeIcon2_Click(object sender, System.EventArgs e) => PictureBoxChallengeIcon_Click(2);
        private void pictureBoxChallengeIcon3_Click(object sender, System.EventArgs e) => PictureBoxChallengeIcon_Click(3);
    }
}