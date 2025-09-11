using BreakInfinity;

public partial class AscensionShop : Form
{
    private BigDouble ascensionPoints;

    public AscensionShop(BigDouble points)
    {
        ascensionPoints = points;

        this.Text = "Ascension Shop";
        this.Size = new Size(300, 200);

        Label labelPoints = new Label();
        labelPoints.Location = new Point(20, 20);
        labelPoints.Size = new Size(200, 30);
        labelPoints.Text = $"Ascension Points: {points.ToString()}";
        labelPoints.Name = "labelPoints";

        this.Controls.Add(labelPoints);
    }

    // Add logic to spend points here
}
