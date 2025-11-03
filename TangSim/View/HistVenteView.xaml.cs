using TangSim.Models;
using TangSim.ViewModels;
namespace TangSim.View;


public partial class HistVenteView : ContentPage
{
    private HistVenteVM viewModel;
    public HistVenteView()
    {
        InitializeComponent();

        string dbPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "MyBusiness.db");
        var dbService = new DBService(dbPath);
        viewModel = new HistVenteVM();
        viewModel.Initialize(dbService);
        //string dbPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "MyBusiness.db");
        BindingContext = viewModel;
    }
}