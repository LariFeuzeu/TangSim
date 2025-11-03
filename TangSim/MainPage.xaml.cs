using TangSim.Models;
using TangSim.View;
namespace TangSim
{
    public partial class MainPage : ContentPage
    {

        public MainPage()
        {
            InitializeComponent();

        }
        private async void OnRegisterArticleClicked(object sender, EventArgs e)
        {

            string dbPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "MyBusiness.db");
            DBService dbService = new DBService(dbPath);
            var ajoutPage = new AjoutView(dbService);
            await Navigation.PushAsync(ajoutPage);
        }

        private async void OnVentesButtonClicked(object sender, EventArgs e)
        {
            // Masquer le menu déroulant
            MenuDropdown.IsVisible = false;
            var Hs = new HistVenteView();
            await Navigation.PushAsync(Hs);
        }

        private async void OnApprovisionnementButtonClicked(object sender, EventArgs e)
        {
            // Masquer le menu déroulant
            MenuDropdown.IsVisible = false;

            // Naviguer vers la page des approvisionnements
            var Hv = new HistAppView();
            await Navigation.PushAsync(Hv);
        }

        private void OnMenuButtonClicked(object sender, EventArgs e)
        {
            // Afficher ou masquer le menu déroulant
            MenuDropdown.IsVisible = !MenuDropdown.IsVisible;
        }

        private void OnPageTapped(object sender, TappedEventArgs e)
        {
            // Masquer le menu déroulant
            MenuDropdown.IsVisible = false;
        }
    }


    // Extension pour l'animation de pulsation
    // Extension pour l'animation de pulsation
    public static class ViewExtensions
    {
        public static async Task Pulse(this Microsoft.Maui.Controls.View view)
        {
            await view.ScaleTo(1.2, 500, Easing.SinOut);
            await view.ScaleTo(1, 500, Easing.SinOut);
        }
    }


}
