using TangSim.Models;
using TangSim.ViewModels;

namespace TangSim.View;

public partial class HistAppView : ContentPage
{
    private HistAppVM vm;
    public HistAppView()
    {
        InitializeComponent();
        // Initialisation différée du DBService
        string dbPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "MyBusiness.db");
        var dbService = new DBService(dbPath);
        vm = new HistAppVM();
        vm.Initialize(dbService);
        BindingContext = vm;
    }
}