using CommunityToolkit.Mvvm.Messaging;
using System.Collections.ObjectModel;
using System.Diagnostics;
using TangSim.Models;
using TangSim.ViewModels;
using static TangSim.ViewModels.ArticleVM;

namespace TangSim.View;

public partial class ArticleView : ContentPage
{
    private readonly ArticleVM _viewModel;

    public ArticleView()
    {
        InitializeComponent();
        string dbPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "MyBusiness.db");
        _viewModel = new ArticleVM(new DBService(dbPath));
        BindingContext = _viewModel;
        // Charger les articles dès l'initialisation
        Task.Run(async () => await _viewModel.LoadArticlesAsync());

        WeakReferenceMessenger.Default.Register<ArticleModifieMessage>(this, (r, m) =>
        {
            Task.Run(async () => await _viewModel.LoadArticlesAsync());
        });
    }

    protected override void OnAppearing()
    {
        base.OnAppearing();

        WeakReferenceMessenger.Default.Register<ArticleModifiedMessage>(this, async (r, m) =>
        {
            await MainThread.InvokeOnMainThreadAsync(() =>
            {
                switch (m.Action)
                {
                    case "ADD":
                        if (m.Article.QteStock > 0)
                            _viewModel.Articles.Add(m.Article);
                        break;
                    case "UPDATE":
                        var existing = _viewModel.Articles.FirstOrDefault(a => a.IdProd == m.Article.IdProd);
                        if (existing != null)
                        {
                            existing.Nom = m.Article.Nom;
                            existing.PrixU = m.Article.PrixU;
                            existing.QteStock = m.Article.QteStock;
                            existing.ImagePath = m.Article.ImagePath;
                        }
                        break;
                    case "DELETE":
                        var toRemove = _viewModel.Articles.FirstOrDefault(a => a.IdProd == m.Article.IdProd);
                        if (toRemove != null)
                            _viewModel.Articles.Remove(toRemove);
                        break;
                }

                // Tri si nécessaire
                _viewModel.Articles = new ObservableCollection<Article>(
                    _viewModel.Articles.OrderBy(a => a.Nom));
            });
        });
    }


    protected override void OnDisappearing()
    {
        WeakReferenceMessenger.Default.Unregister<ArticleModifiedMessage>(this);
        base.OnDisappearing();
    }

    private async void OnArticleTapped(object sender, SelectionChangedEventArgs e)
    {
        // Vérifiez si un élément a été sélectionné
        if (e.CurrentSelection != null && e.CurrentSelection.Count > 0)
        {
            var article = (Article)e.CurrentSelection[0]; // Récupérer l'article sélectionné

            // Mettre l'article sélectionné dans le ViewModel
            _viewModel.SelectedArticle = article;

            // Afficher un ActionSheet pour donner des options à l'utilisateur
            var action = await DisplayActionSheet("Action", "Cancel", null, "Edit", "Delete");

            switch (action)
            {
                case "Edit":
                    // Si l'option "Editer" est choisie, naviguer vers la page de modification
                    if (article != null)
                    {
                        await Navigation.PushAsync(new EditerView(article)); // Remplacez par votre page d'édition
                    }
                    break;

                case "Delete":
                    // Si l'option "Supprimer" est choisie, supprimer l'article
                    if (article != null)
                    {
                        await _viewModel.DeleteArticleAsync(); // Supprimer l'article
                        await _viewModel.LoadArticlesAsync();  // Recharger la liste des articles après suppression
                        ((CollectionView)sender).SelectedItem = null; // Réinitialiser la sélection
                    }
                    break;

                case "Cancel":
                    // Si l'utilisateur annule, ne rien faire
                    break;
            }
        }
        else
        {
            Debug.WriteLine("L'article sélectionné est null.");
        }

         ((CollectionView)sender).SelectedItem = null;
    }

}
