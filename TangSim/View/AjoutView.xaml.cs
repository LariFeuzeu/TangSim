using CommunityToolkit.Mvvm.Messaging;
using TangSim.Models;
using TangSim.ViewModels;
using static TangSim.ViewModels.ArticleVM;
namespace TangSim.View;

public partial class AjoutView : ContentPage
{
    public AjoutView(DBService dbService)
    {
        InitializeComponent();

        // Définir le BindingContext de la page à l'instance de ArticleVM
        BindingContext = new ArticleVM(dbService);

        // S'abonner au message "ArticleAjoute" pour actualiser la vue
        WeakReferenceMessenger.Default.Register<ArticleAjouteMessage>(this, async (r, m) =>
        {
            if (BindingContext is ArticleVM viewModel)
            {
                // Ajouter directement le nouvel article plutôt que recharger
                if (m.NewArticle != null)
                {
                    viewModel.Articles.Add(m.NewArticle);
                }
                else
                {
                    await viewModel.LoadArticlesAsync();
                }
            }
        });
        // S'abonner au message JSON envoyé via WeakReferenceMessenger
        // Vous avez déjà ce code pour recevoir le message, donc la mise à jour de l'image devrait fonctionner
        //WeakReferenceMessenger.Default.Register<ImageUpdatedMessage>(this, (recipient, message) =>
        //{
        //    // Mettez à jour l'image de l'interface utilisateur avec le chemin de l'image

        //    MyImage.Source = ImageSource.FromFile(message.ImagePath);
        //});



        //// S'abonner au message "ArticleModifieMessage" pour actualiser la vue
        //WeakReferenceMessenger.Default.Register<ApprovisionnementVM.ArticleModifieMessage>(this, async (r, m) =>
        //{
        //    if (BindingContext is ArticleVM viewModel)
        //    {
        //        // Recharger les articles
        //        await viewModel.LoadArticlesAsync();
        //    }
        //});

        //// S'abonner au message "ArticlesAvecStockFaibleMessage" pour actualiser la vue
        //WeakReferenceMessenger.Default.Register<ArticlesAvecStockFaibleMessage>(this, async (r, m) =>
        //{
        //    if (BindingContext is ArticleVM viewModel)
        //    {
        //        // Recharger les articles
        //        await viewModel.LoadArticlesAsync();
        //    }
        //});
        //WeakReferenceMessenger.Default.Register<ArticleDeplaceVersApprovisionnementMessage>(this, async (r, m) =>
        //{
        //    if (BindingContext is ArticleVM viewModel)
        //    {
        //        await viewModel.LoadArticlesAsync(); // Recharger les articles disponibles
        //    }
        //});

        //WeakReferenceMessenger.Default.Register<ArticleDeplaceVersAchatMessage>(this, async (r, m) =>
        //{
        //    if (BindingContext is ArticleVM viewModel)
        //    {
        //        await viewModel.LoadArticlesAsync();
        //    }
        //});
    }




    // N'oubliez pas de vous désabonner lorsque la page disparaît

    private async void OnPickImageClicked(object sender, EventArgs e)
    {
        // Appel de la méthode AddImageAsync dans le ViewModel
        var viewModel = (ArticleVM)BindingContext;
        await viewModel.AddImageAsync();
    }



    // Désabonnement du message lorsque la page disparaît
    // Désabonnement du message lorsque la page disparaît
    protected override void OnDisappearing()
    {
        WeakReferenceMessenger.Default.Unregister<ArticleAjouteMessage>(this);
        //WeakReferenceMessenger.Default.Unregister<ArticlesAvecStockFaibleMessage>(this);
        //WeakReferenceMessenger.Default.Unregister<ApprovisionnementVM.ArticleModifieMessage>(this);
        //WeakReferenceMessenger.Default.Unregister<ArticleDeplaceVersAchatMessage>(this);
        base.OnDisappearing();
    }

    private async void AddArticle(object sender, EventArgs e)
    {
        // Appel de la méthode AddArticleAsync du ViewModel
        var viewModel = (ArticleVM)BindingContext;
        await viewModel.AddArticleAsync();
        await viewModel.AddImageAsync();
    }
}
