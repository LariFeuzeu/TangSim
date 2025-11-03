using CommunityToolkit.Mvvm.Messaging;
using System.Text.Json;
using TangSim.Models;
using TangSim.ViewModels;
using static TangSim.ViewModels.ApprovisionnementVM;
using static TangSim.ViewModels.ArticleVM;
using static TangSim.ViewModels.panierVM;


namespace TangSim.View;

// Dans ApproView.xaml.cs

// Dans ApproView.xaml.cs

public partial class ApproView : ContentPage
{
    private readonly ApprovisionnementVM _approVM;

    // Constructeur pour recevoir un article sérialisé en JSON
    public ApproView(string selectedArticleJson = null)
    {
        InitializeComponent();

        // Initialisation de la vue et du ViewModel
        string dbPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "MyBusiness.db");
        _approVM = new ApprovisionnementVM(new DBService(dbPath));

        BindingContext = _approVM;

        // Si un article est passé, le désérialiser et l'afficher
        if (!string.IsNullOrEmpty(selectedArticleJson))
        {
            var selectedArticle = JsonSerializer.Deserialize<Article>(selectedArticleJson);
            _approVM.SelectedArticle = selectedArticle;
        }
        //// S'abonner au message d'ajout ou de modification d'article
        WeakReferenceMessenger.Default.Register<ArticleVM.ArticleModifieMessage>(this, async (r, m) =>
        {
            // Recharger les articles lorsque la quantité est mise à jour (via approvisionnement)
            await _approVM.LoadArticlesAsync();
        });


        //// S'abonner au message indiquant que les articles avec stock faible doivent être actualisés
        //WeakReferenceMessenger.Default.Register<ArticlesAvecStockFaibleMessage>(this, async (r, m) =>
        //{
        //    // Charger à nouveau la liste des articles dont le stock est faible
        //    await _approVM.LoadArticlesAsync();
        //});

        // S'abonner au message de déplacement vers la liste d'approvisionnement
        WeakReferenceMessenger.Default.Register<ArticleDeplaceVersApprovisionnementMessage>(this, async (r, m) =>
        {
            // Recharger les articles lorsque l'article est déplacé vers la liste d'approvisionnement
            await _approVM.LoadArticlesAsync();
        });

        WeakReferenceMessenger.Default.Register<ArticleAjouteMessage>(this, async (r, m) =>
        {
            if (BindingContext is ApprovisionnementVM viewModel)
            {
                await viewModel.LoadArticlesAsync(); // Recharger les articles d'approvisionnement
            }
        });
    }

    // Chargement des articles à l'apparition de la page

    // Dans ApproView.xaml.cs
    private bool _isSubscribed = false;

    protected override void OnAppearing()
    {
        base.OnAppearing();

        if (!_isSubscribed)
        {
            WeakReferenceMessenger.Default.Register<ArticleDeplaceVersApprovisionnementMessag>(this, async (r, m) =>
            {
                if (BindingContext is ApprovisionnementVM vm)
                {
                    await MainThread.InvokeOnMainThreadAsync(async () =>
                    {
                        await vm.LoadArticlesAsync();
                    });
                }
            });
            _isSubscribed = true;
        }

        if (BindingContext is ApprovisionnementVM vm)
        {
            vm.LoadArticlesAsync();
        }
    }


    protected override void OnDisappearing()
    {
        // Se désabonner des messages
        WeakReferenceMessenger.Default.Unregister<ArticleVM.ArticleModifieMessage>(this);
        WeakReferenceMessenger.Default.Unregister<ArticleDeplaceVersApprovisionnementMessage>(this);
        // Nettoyage de l'abonnement
        WeakReferenceMessenger.Default.Unregister<ArticleDeplaceVersApprovisionnementMessag>(this);
        _isSubscribed = false;
        base.OnDisappearing();
    }
    // Gérer la sélection d'un article
    private async void OnArticleSelected(object sender, SelectionChangedEventArgs e)
    {
        if (e.CurrentSelection != null && e.CurrentSelection.Count > 0)
        {
            var article = e.CurrentSelection[0] as Article;

            if (article != null)
            {
                // Convertir l'article en chaîne JSON
                var selectedArticleJson = JsonSerializer.Serialize(article);

                // Naviguer vers la page PanierApproView et passer l'article sélectionné
                await Navigation.PushAsync(new PannierApproView(selectedArticleJson));
            }
        }
         ((CollectionView)sender).SelectedItem = null;
    }

    // Méthode pour valider l'approvisionnement
    private async void OnValiderApprovClicked(object sender, EventArgs e)
    {
        // Lancer la validation de l'approvisionnement depuis le ViewModel
        await _approVM.ValiderApprovAsync(); // Cette méthode mettra à jour la quantité et enverra le message pour actualiser la liste

        await Navigation.PopAsync(); // revenir a la page precedente
    }
}




