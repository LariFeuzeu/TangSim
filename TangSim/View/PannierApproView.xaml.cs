using CommunityToolkit.Mvvm.Messaging;
using System.Text.Json;
using TangSim.Models;
using TangSim.ViewModels;
using static TangSim.ViewModels.ArticleVM;


namespace TangSim.View;

public partial class PannierApproView : ContentPage
{
    private readonly ApprovisionnementVM _approVM;
    // public string DateApprov => DateTime.Now.ToString("dd/MM/yyyy"); // Format de la date (par exemple : 25/01/2025)
    public Article SelectedArticle { get; set; }
    public PannierApproView(string selectedArticleJson)
    {
        InitializeComponent();
        string dbPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "MyBusiness.db");
        _approVM = new ApprovisionnementVM(new DBService(dbPath));
        BindingContext = _approVM;

        if (!string.IsNullOrEmpty(selectedArticleJson))
        {
            // Désérialiser l'article depuis la chaîne JSON
            SelectedArticle = JsonSerializer.Deserialize<Article>(selectedArticleJson);
            _approVM.SelectedArticle = SelectedArticle; // Mettre à jour le ViewModel
        }
        // S'abonner au message de mise à jour de l'article
        WeakReferenceMessenger.Default.Register<ArticleModifieMessage>(this, (r, m) =>
        {
            // Recharger la liste des articles à partir de la base de données
            _approVM.LoadArticlesAsync();
        });



    }

    protected override void OnAppearing()
    {
        base.OnAppearing();

        // Assigner l'article à SelectedArticle dans le ViewModel
        if (SelectedArticle != null)
        {
            _approVM.SelectedArticle = SelectedArticle;
        }
    }
}