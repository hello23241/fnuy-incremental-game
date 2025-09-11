public partial class AscensionShop : Form
{
    private int ascensionPoints;

    public AscensionShop(int points)
    {
        ascensionPoints = points;

        // Set up the form
        this.Text = "Ascension Shop";
        this.Size = new Size(300, 200);

        // Create labelPoints
        Label labelPoints = new Label();
        labelPoints.Location = new Point(20, 20);
        labelPoints.Size = new Size(200, 30);
        labelPoints.Text = $"Ascension Points: {ascensionPoints}";
        labelPoints.Name = "labelPoints";

        // Add to form
        this.Controls.Add(labelPoints);
    }

    // Add logic to spend points here
}
