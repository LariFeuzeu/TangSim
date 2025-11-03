using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;
using System.Collections.ObjectModel;
using TangSim.Models;
using static TangSim.ViewModels.ArticleVM;


namespace TangSim.ViewModels
{
    public partial class ApprovisionnementVM : ObservableObject
    {
        private readonly DBService _dbservice;

        public ObservableCollection<Article> Articles { get; set; } = new ObservableCollection<Article>(); // Liste d'articles

        [ObservableProperty]
        private Article _selectedArticle;  // L'article sélectionné

        [ObservableProperty]
        private int _prixApprov;  // Prix d'approvisionnement

        [ObservableProperty]
        private int _qteApprov;  // Quantité d'approvisionnement

        [ObservableProperty]
        private DateTime _dateApprov = DateTime.Now;
        private int _montantApprov;

        public int MontantApprov
        {
            get => _montantApprov;
            set => SetProperty(ref _montantApprov, value);
        }

        // Méthode pour calculer dynamiquement le montant de l'approvisionnement
        partial void OnPrixApprovChanged(int value)
        {
            UpdateMontantApprov();
        }

        partial void OnQteApprovChanged(int value)
        {
            UpdateMontantApprov();
        }

        // Mise à jour du montant de l'approvisionnement
        public void UpdateMontantApprov()
        {
            MontantApprov = PrixApprov * QteApprov;
        }




        // Commande pour valider l'approvisionnement
        public IAsyncRelayCommand ValiderApprovCommand { get; }

        public ApprovisionnementVM(DBService dbService)
        {
            _dbservice = dbService;
            LoadArticlesAsync();
            ValiderApprovCommand = new AsyncRelayCommand(ValiderApprovAsync);

            // Recevoir le message d'article sélectionné
            WeakReferenceMessenger.Default.Register<ArticleSelectionneMessage>(this, async (r, m) =>
            {
                SelectedArticle = m.Article;
                await LoadArticlesAsync();
            });
            WeakReferenceMessenger.Default.Register<ArticleDeplaceVersApprovisionnementMessag>(this, async (r, m) =>
            {
                await MainThread.InvokeOnMainThreadAsync(async () =>
                {
                    if (m.Article.QteStock == 0 && !Articles.Any(a => a.IdProd == m.Article.IdProd))
                    {
                        Articles.Add(m.Article);
                        // Optionnel : trier la liste si nécessaire
                        Articles = new ObservableCollection<Article>(Articles.OrderBy(a => a.Nom));
                    }
                });
            });

            WeakReferenceMessenger.Default.Register<ArticleModifieMessage>(this, (r, m) =>
            {
                MainThread.BeginInvokeOnMainThread(() =>
                {
                    // Supprimer l'article si son stock est >0
                    var articleToRemove = Articles.FirstOrDefault(a => a.IdProd == m.SelectedArticle.IdProd);
                    if (articleToRemove != null && m.SelectedArticle.QteStock > 0)
                    {
                        Articles.Remove(articleToRemove);
                    }
                });
            });
        }


        // Méthode pour charger les articles
        public async Task LoadArticlesAsync()
        {
            try
            {
                var articlesList = await _dbservice.GetArticlesAsync();

                // Filtrer les articles dont le stock est inférieur ou égal à 5
                var filteredArticles = articlesList.Where(a => a.QteStock == 0).ToList();

                Articles.Clear();
                foreach (var article in filteredArticles)
                {
                    Articles.Add(article);
                }
            }
            catch (Exception ex)
            {
                await App.Current.MainPage.DisplayAlert("Erreur", "Impossible de charger les articles : " + ex.Message, "OK");
            }
        }

        // Méthode pour valider l'approvisionnement
        // Méthode pour valider l'approvisionnement
        // Dans ApprovisionnementVM.cs

        public async Task ValiderApprovAsync()
        {
            try
            {
                // Vérifier la quantité et le prix
                if (QteApprov <= 0 || PrixApprov <= 0)
                {
                    await App.Current.MainPage.DisplayAlert("Erreur", "Veuillez entrer une quantité et un prix valides.", "OK");
                    return;
                }

                // Calculer le montant d'approvisionnement dynamiquement
                UpdateMontantApprov();

                // Créer un nouvel approvisionnement
                var approv = new Approvisionnement
                {
                    IdProd = SelectedArticle.IdProd,
                    QteApprov = QteApprov,
                    PrixApprov = PrixApprov,
                    MontantApprovPersiste = MontantApprov,
                    DateApprov = DateApprov
                };

                // Ajouter l'approvisionnement à la base de données
                await _dbservice.CreateApprovisionnementAsync(approv);

                // Mettre à jour le stock de l'article
                SelectedArticle.QteStock += QteApprov;

                // Mettre à jour l'article dans la base de données
                await _dbservice.UpdateArticleAsync(SelectedArticle);
                Articles.Remove(SelectedArticle);

                await LoadArticlesAsync();
                // Envoyer les messages de notification
                WeakReferenceMessenger.Default.Send(new ArticleModifieMessage { SelectedArticle = SelectedArticle });
                WeakReferenceMessenger.Default.Send(new ArticleRefreshMessenge());

                if (SelectedArticle.QteStock > 0)
                {
                    WeakReferenceMessenger.Default.Send(new ArticleDeplaceVersAchatMessage { Article = SelectedArticle });
                    // Si le stock est maintenant > 0, déplacer l'article vers la vue d'achat
                    if (SelectedArticle.QteStock > 0)
                    {

                        WeakReferenceMessenger.Default.Send(new ArticleDeplaceVersAchatMessage { Article = SelectedArticle });
                    }

                    // Réinitialiser les champs
                    PrixApprov = 0;
                    QteApprov = 0;

                    // Confirmer l'ajout de l'approvisionnement
                    await App.Current.MainPage.DisplayAlert("Succès", "Approvisionnement effectué avec succès.", "OK");

                    //// Si le stock est maintenant > 0, déplacer l'article vers la vue d'achat
                    //if (SelectedArticle.QteStock > 0)
                    //{
                    //    WeakReferenceMessenger.Default.Send(new ArticleDeplaceVersAchatMessage { Article = SelectedArticle });
                    //}
                }
            }
            catch (Exception ex)
            {
                await App.Current.MainPage.DisplayAlert("Erreur", "Une erreur est survenue : " + ex.Message, "OK");
            }
        }


        public class ArticleRefreshMessenge();
        public class ArticleDeplaceVersAchatMessage
        {
            public Article Article { get; set; }
        }

        // Message envoyé après modification de l'article
        public class ArticleModifieMessages
        {
            public Article SelectedArticle { get; set; }
        }

        // Ce message est utilisé pour notifier la vue de la mise à jour des articles à faible stock
        public class ArticlesAvecStockFaibleMessage
        {
            // Aucun besoin d'arguments supplémentaires ici, car c'est juste une notification pour réactualiser les articles
        }
        // À placer dans le même namespace que ApprovisionnementVM
        public class ArticleDeplaceVersApprovisionnementMessag
        {
            public Article Article { get; set; }
        }

    }
}
