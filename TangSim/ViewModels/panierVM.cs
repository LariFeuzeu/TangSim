using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;
using System.Collections.ObjectModel;
using System.Diagnostics;
using TangSim.Models;
using TangSim.View;
using static TangSim.ViewModels.ArticleVM;

namespace TangSim.ViewModels
{
    public partial class panierVM : ObservableObject, IDisposable
    {
        private readonly DBService _dbService;
        private Panier _panier;
        private SemaphoreSlim _operationLock = new SemaphoreSlim(1, 1);

        public DateTime DateVente => DateTime.Now;

        public Panier Panier
        {
            get => _panier ?? (_panier = new Panier());
            set
            {
                _panier = value;
                OnPropertyChanged();
                OnPropertyChanged(nameof(Total));
            }
        }
        public void Dispose()
        {
            WeakReferenceMessenger.Default.Unregister<ArticleVM.ArticleModifiedMessage>(this);
          
        }

        public decimal Total => Panier?.Total ?? 0m;

        [ObservableProperty]
        private int _nombreArticlesDansPanier;

        public INavigation Navigation { get; }

        public IAsyncRelayCommand LoadArticlesCommand { get; }
        public IRelayCommand<Article> AjouterAuPanierCommand { get; }
        public IAsyncRelayCommand VoirPanierCommand { get; }
        public IAsyncRelayCommand ValiderAchatCommand { get; }
        public IAsyncRelayCommand AnnulerPanierCommand { get; }
        public IAsyncRelayCommand<Article> DiminuerPanierCommand { get; }
        public IAsyncRelayCommand<Article> AugmenterPanierCommand { get; }

        public panierVM()
        {
            Articles = new ObservableCollection<Article>();
        }

        public panierVM(DBService dbService, INavigation navigation)
        {
            _dbService = dbService;
            _panier = new Panier();
            Navigation = navigation;

            LoadArticlesCommand = new AsyncRelayCommand(LoadArticlesAsync);
            AjouterAuPanierCommand = new RelayCommand<Article>(AjouterAuPanier);
            VoirPanierCommand = new AsyncRelayCommand(VoirPanierAsync);
            ValiderAchatCommand = new AsyncRelayCommand(ValiderAchatAsync);
            AnnulerPanierCommand = new AsyncRelayCommand(AnnulerPanierAsync);
            DiminuerPanierCommand = new AsyncRelayCommand<Article>(DiminuerPanierAsync);
            AugmenterPanierCommand = new AsyncRelayCommand<Article>(AugmenterPanierAsync);
            WeakReferenceMessenger.Default.Register<ArticleModifiedMessage>(this, async (r, m) =>
            {
                if (m.ShouldRefreshAll || m.Action == "DELETE" || (m.Action == "UPDATE" && m.Article.QteStock <= 0))
                {
                    await LoadArticlesAsync();
                }
            });

            // Dans le constructeur :
            // Gestion des abonnements aux messages
            WeakReferenceMessenger.Default.Unregister<ArticleVM.ArticleModifiedMessage>(this); // Désinscription préalable
            WeakReferenceMessenger.Default.Register<ArticleVM.ArticleModifiedMessage>(this, async (r, m) =>
            {
                MainThread.BeginInvokeOnMainThread(() =>
               {
                   switch (m.Action)
                   {
                       case "ADD" when m.Article.QteStock > 0:
                           if (!Articles.Contains(m.Article))
                               Articles.Add(m.Article);
                           break;

                       case "UPDATE":
                           var existing = Articles.FirstOrDefault(a => a.IdProd == m.Article.IdProd);
                           if (existing != null)
                           {
                               if (m.Article.QteStock > 0)
                               {
                                   existing.Nom = m.Article.Nom;
                                   existing.PrixU = m.Article.PrixU;
                                   existing.QteStock = m.Article.QteStock;
                               }
                               else
                               {
                                   Articles.Remove(existing);
                               }
                           }
                           break;

                       case "DELETE":
                           var toRemove = Articles.FirstOrDefault(a => a.IdProd == m.Article.IdProd);
                           if (toRemove != null)
                               Articles.Remove(toRemove);
                           break;
                   }
                   // Forcer la mise à jour de l'UI
                   OnPropertyChanged(nameof(Articles));
               });
            });
        }

     
        public Article SelectedArticles { get; set; }

        [ObservableProperty]
        private Article _selectedArticle;

        public ObservableCollection<Article> Articles { get; set; } = new ObservableCollection<Article>();
            
        private async Task VoirPanierAsync()
        {
            var panierView = new PanierView
            {
                BindingContext = this // toujours réutiliser l'instance si nécessaire
            };
            await Navigation.PushAsync(panierView);

        }

        private async Task ValiderAchatAsync()
        {
            await _operationLock.WaitAsync();
            try
            {
                foreach (var item in Panier.Items.ToList()) // Utilisation de ToList pour éviter les modifications pendant l'itération
                {
                    var article = await _dbService.GetByIdAsync(item.Article.IdProd);
                    if (article.QteStock >= item.Quantite)
                    {
                        article.QteStock -= item.Quantite;
                        await _dbService.UpdateArticleAsync(article);

                        var achat = new Achat
                        {
                            IdProd = article.IdProd,
                            Quantite = item.Quantite,
                            PrixTotal = item.PrixTotal,
                            DateVente = DateTime.Now
                        };
                        await _dbService.CreateAchatAsync(achat);

                        // Notification forte pour mise à jour immédiate
                        WeakReferenceMessenger.Default.Send(new ArticleVM.ArticleModifiedMessage
                        {
                            Action = article.QteStock > 0 ? "UPDATE" : "DELETE",
                            Article = article,
                            ShouldRefreshAll = true
                        });
                    }
                }

                Panier.ViderPanier();
                NombreArticlesDansPanier = 0;
                OnPropertyChanged(nameof(Panier));
                OnPropertyChanged(nameof(Total));
                await LoadArticlesAsync(); // Recharger les articles pour s'assurer que ceux à stock zéro sont bien retirés
                await Shell.Current.DisplayAlert("Succès", "Achat validé avec succès !", "OK");
            }
            finally
            {
                _operationLock.Release();
            }
        }

        public class ArticleDeplaceVersApprovisionnementMessage
        {
            public Article Article { get; set; }
        }

        public void AjouterAuPanier(Article article)
        {
            if (article == null) return;

            _operationLock.WaitAsync().ContinueWith(t =>
            {
                try
                {
                    // Vérification critique avant toute opération
                    if (article.QteStock <= 0)
                    {
                        MainThread.BeginInvokeOnMainThread(async () =>
                        {
                            await RetirerArticleSiNecessaire(article);
                            await Shell.Current.DisplayAlert("Erreur",
                                $"Impossible d'ajouter {article.Nom}: stock épuisé", "OK");
                        });
                        return;
                    }

                    var existingItem = Panier.Items.FirstOrDefault(i => i.Article?.IdProd == article.IdProd);

                    if (existingItem != null)
                    {
                        existingItem.Quantite++;
                    }
                    else
                    {
                        Panier.Items.Add(new PanierItem { Article = article, Quantite = 1 });
                    }
                    article.QteStock--;

                    MainThread.BeginInvokeOnMainThread(async () =>
                    {
                        // Mise à jour immédiate de l'UI
                        NombreArticlesDansPanier = Panier.Items.Sum(i => i.Quantite);
                        OnPropertyChanged(nameof(Panier));
                        OnPropertyChanged(nameof(Panier.Items));
                        OnPropertyChanged(nameof(NombreArticlesDansPanier));

                        // Contrôle final absolu
                        if (article.QteStock <= 0)
                        {
                            await RetirerArticleSiNecessaire(article);
                            await Shell.Current.DisplayAlert("Information",
                                $"Dernier {article.Nom} ajouté - stock épuisé", "OK");
                        }
                    });
                }
                finally
                {
                    _operationLock.Release();
                }
            });
        }

        private async Task RetirerArticleSiNecessaire(Article article)
        {
            if (Articles.Contains(article))
            {
                Articles.Remove(article);
                // Forcer le recalcul des propriétés
                OnPropertyChanged(nameof(Articles));
            }
        }

        private async Task AnnulerPanierAsync()
        {
            bool confirm = await Shell.Current.DisplayAlert(
                "Confirmation",
                "Voulez-vous vraiment annuler le panier ?",
                "Oui", "Non");

            if (!confirm) return;

            await _operationLock.WaitAsync();
            try
            {
                foreach (var panierItem in Panier.Items)
                {
                    var article = panierItem.Article;
                    article.QteStock += panierItem.Quantite;

                    if (!Articles.Contains(article))
                    {
                        Articles.Add(article);
                    }
                }

                Panier.Items.Clear();
                NombreArticlesDansPanier = 0;
                OnPropertyChanged(nameof(Panier));
                OnPropertyChanged(nameof(Panier.Items));
                OnPropertyChanged(nameof(Total));
                OnPropertyChanged(nameof(NombreArticlesDansPanier));

                await Shell.Current.DisplayAlert("Information", "Le panier a été annulé et les articles ont été réintégrés.", "OK");
            }
            finally
            {
                _operationLock.Release();
            }
        }

        public async Task LoadArticlesAsync()
        {
            var articlesList = await _dbService.GetArticlesAsync();
            var filteredArticles = articlesList.Where(a => a.QteStock > 0).ToList();

            await MainThread.InvokeOnMainThreadAsync(() =>
            {
                Articles.Clear();
                foreach (var article in filteredArticles)
                {
                    Articles.Add(article);
                    OnPropertyChanged(nameof(Articles));
                }
            });
        }

        private async Task DiminuerPanierAsync(Article article)
        {
            if (article == null) return;

            await _operationLock.WaitAsync();
            try
            {
                var item = Panier.Items.FirstOrDefault(i => i.Article?.IdProd == article.IdProd);
                if (item != null)
                {
                    // Décrémenter la quantité dans le panier
                    item.Quantite--;
                    // Réintégrer le stock
                    article.QteStock++;

                    // Si quantité arrive à 0, retirer l'article du panier
                    if (item.Quantite <= 0)
                    {
                        Panier.RetirerArticle(article.IdProd);
                    }

                    await MainThread.InvokeOnMainThreadAsync(() =>
                    {
                        // Mettre à jour l'affichage
                        NombreArticlesDansPanier = Panier.Items.Sum(i => i.Quantite);
                        OnPropertyChanged(nameof(Panier));
                        OnPropertyChanged(nameof(Total));
                        OnPropertyChanged(nameof(Panier.Items));

                        // Réintégrer l'article dans la liste si nécessaire
                        if (article.QteStock > 0 && !Articles.Contains(article))
                        {
                            Articles.Add(article);
                            // Tri si nécessaire pour remettre l'article à sa place
                            Articles = new ObservableCollection<Article>(Articles.OrderBy(a => a.Nom));
                        }
                    });

                    // Debug
                    Debug.WriteLine($"Après diminution - Stock: {article.QteStock}, Panier: {item?.Quantite ?? 0}");
                }
            }
            finally
            {
                _operationLock.Release();
            }
        }
        private async Task AugmenterPanierAsync(Article article)
        {
            if (article == null) return;

            await _operationLock.WaitAsync();
            try
            {
                // Vérification initiale du stock
                if (article.QteStock <= 0)
                {
                    await MainThread.InvokeOnMainThreadAsync(async () =>
                    {
                        if (Articles.Contains(article))
                        {
                            Articles.Remove(article);
                        }
                        await Shell.Current.DisplayAlert("Stock épuisé",
                            "Cet article n'est plus disponible.", "OK");
                    });
                    return;
                }

                var item = Panier.Items.FirstOrDefault(i => i.Article?.IdProd == article.IdProd);

                // Modification du stock
                if (item != null)
                {
                    item.Quantite++;
                }
                else
                {
                    Panier.Items.Add(new PanierItem { Article = article, Quantite = 1 });
                }
                article.QteStock--;

                // Mise à jour de l'UI
                await MainThread.InvokeOnMainThreadAsync(async () =>
                {
                    NombreArticlesDansPanier = Panier.Items.Sum(i => i.Quantite);
                    OnPropertyChanged(nameof(Panier));
                    OnPropertyChanged(nameof(Total));
                    OnPropertyChanged(nameof(Panier.Items));
                    OnPropertyChanged(nameof(NombreArticlesDansPanier));

                    // Gestion de l'article dans la liste
                    if (article.QteStock <= 0)
                    {
                        if (Articles.Contains(article))
                        {
                            Articles.Remove(article);
                            await Shell.Current.DisplayAlert("Information",
                                $"Le stock de {article.Nom} est épuisé.", "OK");
                        }
                    }
                    else if (!Articles.Contains(article))
                    {
                        Articles.Add(article);
                        Articles = new ObservableCollection<Article>(Articles.OrderBy(a => a.Nom));
                    }
                });
            }
            finally
            {
                _operationLock.Release();
            }
        }
    }
}
