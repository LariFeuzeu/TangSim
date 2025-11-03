using TangSim.Models;
using TangSim.ViewModels;

namespace TangSim.View;

public partial class PanierView : ContentPage
{
    public PanierView()
    {

        InitializeComponent();
        // Chemin de la base de données

        string dbPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "Mybusiness.db");

        // Créer une instance de DBService avec le chemin de la base de données

        BindingContext = new panierVM(new DBService(dbPath), Navigation); // On  utilisez une instance existante de panierVM


    }
    public DateTime DateAujourdhui => DateTime.Now;


    protected override void OnDisappearing()
    {
        if (BindingContext is IDisposable disposable)
            disposable.Dispose();

        base.OnDisappearing();
    }
}

