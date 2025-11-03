namespace TangSim.View;

public partial class HistoriqueView : ContentPage
{
    public HistoriqueView()
    {
        InitializeComponent();

    }
    // Lorsque le bouton "Ventes" est cliqué
    private void OnVentesButtonClicked(object sender, EventArgs e)
    {
        // Afficher le contenu des ventes et masquer celui d'approvisionnement
        VentesContent.IsVisible = true;
        ApprovisionnementContent.IsVisible = false;

        // Mettre en surbrillance le bouton Ventes
        VentesButton.BackgroundColor = Colors.LightBlue; // Utilisez Colors.LightBlue ici
        ApprovisionnementButton.BackgroundColor = Colors.LightGray; // Utilisez Colors.LightGray ici
    }

    // Lorsque le bouton "Approvisionnement" est cliqué
    private void OnApprovisionnementButtonClicked(object sender, EventArgs e)
    {
        // Afficher le contenu d'approvisionnement et masquer celui des ventes
        ApprovisionnementContent.IsVisible = true;
        VentesContent.IsVisible = false;

        // Mettre en surbrillance le bouton Approvisionnement
        ApprovisionnementButton.BackgroundColor = Colors.LightBlue; // Utilisez Colors.LightBlue ici
        VentesButton.BackgroundColor = Colors.LightGray; // Utilisez Colors.LightGray ici
    }
}
