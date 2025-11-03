using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Text.Json;
using TangSim.Models;
using static TangSim.ViewModels.ApprovisionnementVM;

namespace TangSim.ViewModels
{
    public partial class ArticleVM : ObservableObject
    {
        private readonly DBService _dbservice;

        // Liste des articles
        
        public ObservableCollection<Article> Articles { get; set; } = new ObservableCollection<Article>();

        // Propriétés observables
        [ObservableProperty]
        private Article _selectedArticle;

        [ObservableProperty]
        private string _nom;

        [ObservableProperty]
        private int _prixU;

        [ObservableProperty]
        private int _qteStock;

        [ObservableProperty]
        private string _imagePath;

        private ImageSource _imageSource;

        public string ImagePaths
        {
            get => string.IsNullOrEmpty(_imagePath) ? "Resources/Images/ts.png" : _imagePath;
            set => _imagePath = value;
        }

        public ImageSource ImageSource
        {
            get => _imageSource ??= ImageSource.FromFile("Resources/Images/ts.png");
            set => SetProperty(ref _imageSource, value);
        }

        // Commandes
        public IAsyncRelayCommand LoadArticlesCommand { get; }
        public IAsyncRelayCommand AddArticlesCommand { get; }
        public IAsyncRelayCommand UpdateArticleCommand { get; }
        public IAsyncRelayCommand DeleteArticleCommand { get; }
        public IAsyncRelayCommand AddImageCommand { get; }

        // Constructeur
        public ArticleVM(DBService dbService)
        {
            _dbservice = dbService;

            // Initialisation des commandes
            LoadArticlesCommand = new AsyncRelayCommand(LoadArticlesAsync);
            AddArticlesCommand = new AsyncRelayCommand(AddArticleAsync);
            UpdateArticleCommand = new AsyncRelayCommand(UpdateArticleAsync);
            DeleteArticleCommand = new AsyncRelayCommand(DeleteArticleAsync);
            AddImageCommand = new AsyncRelayCommand(AddImageAsync);

            // S'abonner aux messages
            WeakReferenceMessenger.Default.Register<ArticleModifiedMessage>(this, (r, m) =>
            {
                MainThread.BeginInvokeOnMainThread(async () =>
                {
                    if (m.ShouldRefreshAll)
                        await LoadArticlesAsync();
                });
            });
            WeakReferenceMessenger.Default.Register<ImageUpdatedMessage>(this, (r, m) =>
            {
                MainThread.BeginInvokeOnMainThread(() =>
                {
                    ImagePath = m.ImagePath;
                    ImageSource = ImageSource.FromFile(m.ImagePath);
                });
            }); 
            //WeakReferenceMessenger.Default.Register<ArticleModifiedMessage>(this, (r, m) =>
            //{
            //    MainThread.BeginInvokeOnMainThread(async () =>
            //    {
            //        if (m.ShouldRefreshAll)
            //            await LoadArticlesAsync();
            //    });
            //});
            // Initialisation de SelectedArticle
            SelectedArticle = new Article();
        }

        // Méthode pour charger les articles
        public async Task LoadArticlesAsync()
        {
            try
            {
                var articlesList = await _dbservice.GetArticlesAsync();
                await MainThread.InvokeOnMainThreadAsync(() =>
                {
                    Articles.Clear();
                    foreach (var article in articlesList.Where(a => a.QteStock > 0).OrderBy(a => a.Nom))
                    {
                        Articles.Add(article);
                        OnPropertyChanged(nameof(Articles));
                    }
                });
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"ERREUR LoadArticles: {ex.Message}");
            }
        }

        // Méthode pour ajouter un article
        public async Task AddArticleAsync()
        {
            try
            {
                var art = new Article
                {
                    Nom = Nom,
                    PrixU = PrixU,
                    QteStock = QteStock,
                    ImagePath = string.IsNullOrEmpty(ImagePath) ? "Resources/Images/ts.png" : ImagePath
                };

                await _dbservice.CreateArticleAsync(art);

                // Envoyer deux messages distincts
                WeakReferenceMessenger.Default.Send(new ArticleModifiedMessage
                {
                    Action = "ADD",
                    Article = art,
                    ShouldRefreshAll = true
                });

                // Puis modifiez l'envoi du message :
                if (art.QteStock == 0)
                {
                    WeakReferenceMessenger.Default.Send(new ArticleDeplaceVersApprovisionnementMessag
                    {
                        Article = art
                    });
                }

                ClearFields();
            }
            catch (Exception ex)
            {
                await App.Current.MainPage.DisplayAlert("Erreur", ex.Message, "OK");
            }
        }
        // Méthode pour mettre à jour un article
        public async Task UpdateArticleAsync()
        {
            try
            {
                if (SelectedArticle != null)
                {
                    var updatedArticle = new Article
                    {
                        IdProd = SelectedArticle.IdProd,
                        Nom = Nom,
                        PrixU = PrixU,
                        QteStock = QteStock,
                        ImagePath = string.IsNullOrEmpty(ImagePath) ? "Resources/Images/ts.png" : ImagePath
                    };

                    await _dbservice.UpdateArticleAsync(updatedArticle);

                    // Envoyer un message de mise à jour
                    WeakReferenceMessenger.Default.Send(new ArticleModifiedMessage
                    {
                        Action = "UPDATE",
                        Article = updatedArticle,
                        ShouldRefreshAll = true
                    });
                }
            }
            catch (Exception ex)
            {
                await App.Current.MainPage.DisplayAlert("Erreur", ex.Message, "OK");
            }
        }

        // Méthode pour supprimer un article
        public async Task DeleteArticleAsync()
        {
            if (SelectedArticle != null)
            {
                bool confirmed = await App.Current.MainPage.DisplayAlert("Confirmation", "Voulez-vous vraiment supprimer cet article ?", "Oui", "Non");
                if (confirmed)
                {
                    await _dbservice.DeleteArticleAsync(SelectedArticle);

                    // Envoyer un message de suppression
                    WeakReferenceMessenger.Default.Send(new ArticleModifiedMessage
                    {
                        Action = "DELETE",
                        Article = SelectedArticle,
                        ShouldRefreshAll = true
                    });
                }
            }
        }
        public class ArticleModifiedMessage
        {
            public string Action { get; set; } // "ADD", "UPDATE" ou "DELETE"
            public Article Article { get; set; }
            public bool ShouldRefreshAll { get; set; } // Si vrai, force le rechargement complet
        }
        // Méthode pour ajouter ou prendre une photo
        public async Task AddImageAsync()
        {
            try
            {
                var result = await App.Current.MainPage.DisplayActionSheet("Choisir une image", "Annuler", null, "Prendre une photo", "Choisir depuis la galerie");

                if (result == "Prendre une photo")
                {
                    var photo = await MediaPicker.CapturePhotoAsync();
                    if (photo != null)
                    {
                        ImagePath = photo.FullPath;
                        ImageSource = ImageSource.FromFile(ImagePath);
                    }
                }

                if (result == "Choisir depuis la galerie")
                {
                    var photo = await MediaPicker.PickPhotoAsync();
                    if (photo != null)
                    {
                        ImagePath = photo.FullPath;
                        ImageSource = ImageSource.FromFile(ImagePath);
                    }
                }

                if (!string.IsNullOrEmpty(ImagePath))
                {
                    await App.Current.MainPage.DisplayAlert("Succès", "Image sélectionnée avec succès.", "OK");
                }
                else
                {
                    await App.Current.MainPage.DisplayAlert("Aucune image sélectionnée", "Veuillez sélectionner une image", "OK");
                }
            }
            catch (Exception ex)
            {
                await App.Current.MainPage.DisplayAlert("Erreur", $"Une erreur est survenue : {ex.Message}", "OK");
            }
        }

        // Méthode pour réinitialiser les champs
        private void ClearFields()
        {
            Nom = string.Empty;
            PrixU = 0;
            QteStock = 0;
            ImagePath = string.Empty;
            ImageSource = ImageSource.FromFile("Resources/Images/ts.png");
        }

        // Messages
        public class ArticleAjouteMessage
        {
            public Article NewArticle { get; set; } // Optionnel
        }
        public class ArticleModifieMessage
        {
            public Article SelectedArticle { get; set; }
        }
        public class ArticleSelectionneMessage
        {
            public Article Article { get; set; }
        }
        public class ImageUpdatedMessage
        {
            public string ImagePath { get; set; }
            public ImageUpdatedMessage(string imagePath) => ImagePath = imagePath;
            public string ToJson() => JsonSerializer.Serialize(this);
            public static ImageUpdatedMessage FromJson(string json) => JsonSerializer.Deserialize<ImageUpdatedMessage>(json);
        }
    }
}