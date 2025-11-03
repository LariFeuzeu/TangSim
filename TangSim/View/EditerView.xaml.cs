
using CommunityToolkit.Mvvm.Messaging;
using TangSim.Models;
using TangSim.ViewModels;

namespace TangSim.View;

public partial class EditerView : ContentPage
{
    private readonly ArticleVM _viewModel;

    public EditerView(Article article)
    {


        InitializeComponent();
        // Crée une nouvelle instance du ViewModel en passant le service de base de données
        _viewModel = new ArticleVM(new DBService(Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "MyBusiness.db")));

        // Associe l'article sélectionné au ViewModel
        _viewModel.SelectedArticle = article;

        // Initialise les propriétés du ViewModel avec les données de l'article à modifier
        _viewModel.Nom = article.Nom;
        _viewModel.PrixU = article.PrixU;
        _viewModel.QteStock = article.QteStock;
        _viewModel.ImagePath = article.ImagePath;

        // Associe le ViewModel à la page pour que les éléments de l'UI soient liés au ViewModel
        BindingContext = _viewModel;

        // S'abonner au message "ArticleModifieMessage" pour actualiser la vue
        WeakReferenceMessenger.Default.Register<ArticleVM.ArticleModifieMessage>(this, async (r, m) =>
        {
            // Vérifiez si l'article modifié est celui qui est actuellement affiché
            if (m.SelectedArticle.IdProd == article.IdProd)
            {
                // Recharger la liste des articles ou actualiser la vue après la mise à jour
                await _viewModel.LoadArticlesAsync();  // Recharger les articles dans le ViewModel
                OnPropertyChanged(nameof(_viewModel.Articles)); // Si nécessaire, forcez la mise à jour de la vue
                                                                // Facultatif : Recharger l'interface utilisateur si nécessaire
                                                                // OnPropertyChanged(nameof(_viewModel.Articles));
            }
        });
    }

    // Méthode appelée lorsqu'on clique sur le bouton de modification
    private async void ModifArt_Clicked(object sender, EventArgs e)
    {
        // Appel de la méthode UpdateArticleAsync du ViewModel
        var viewModel = (ArticleVM)BindingContext;
        await viewModel.UpdateArticleAsync();

        // Une fois l'article modifié, vous pouvez soit fermer la vue, soit recharger la liste d'articles

        await _viewModel.LoadArticlesAsync();
        await Navigation.PopAsync();  // Ou vous pouvez appeler `` si vous voulez mettre à jour immédiatement
    }

    // Désabonnement du message lorsque la page disparaît
    protected override void OnDisappearing()
    {
        // Se désabonner des messages lorsque la page disparaît
        WeakReferenceMessenger.Default.UnregisterAll(this);
        base.OnDisappearing();
    }
}

