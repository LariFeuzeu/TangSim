using CommunityToolkit.Mvvm.Messaging;
using System.Diagnostics;
using TangSim.Models;
using TangSim.ViewModels;
using static TangSim.ViewModels.ArticleVM;

namespace TangSim.View;

public partial class AchatView : ContentPage
{
    private readonly panierVM _viewModel;
    private bool _isDisposed;
    public AchatView()
    {
        InitializeComponent();
        _viewModel = new panierVM();
        string dbPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "MyBusiness.db");
        _viewModel = new panierVM(new DBService(dbPath), Navigation);
        BindingContext = _viewModel;


        // Charger les articles dès l'initialisation
        _viewModel.LoadArticlesCommand.ExecuteAsync(null);

        WeakReferenceMessenger.Default.Register<ArticleModifieMessage>(this, async (r, m) =>
        {
            await _viewModel.LoadArticlesAsync();
        });
    }

    protected override void OnAppearing()
    {
        base.OnAppearing();
        _isDisposed = false;

        WeakReferenceMessenger.Default.Register<ArticleVM.ArticleModifiedMessage>(this, (r, m) =>
        {
            MainThread.BeginInvokeOnMainThread(async () =>
            {
                // Solution 1 (simple) - Rechargement complet
                await _viewModel.LoadArticlesAsync();

   
            });
        });
    }



    protected override void OnDisappearing()
    {
        WeakReferenceMessenger.Default.Unregister<ArticleModifiedMessage>(this);
        base.OnDisappearing();
        _isDisposed = true;
    }


    private void OnArticleTapped(object sender, TappedEventArgs e)
    {
        if (e is not TappedEventArgs tappedEventArgs) return;
        if (tappedEventArgs.Parameter is not Article article) return;

        Debug.WriteLine($"Article cliqué : {article.Nom}");

        if (BindingContext is panierVM viewModel)
        {
            viewModel.SelectedArticles = article;
            if (viewModel.AjouterAuPanierCommand.CanExecute(article))
            {
                viewModel.AjouterAuPanierCommand.Execute(article);
            }
            Debug.WriteLine($"Article ajouté au panier: {article.Nom}");
        }
    }
}
