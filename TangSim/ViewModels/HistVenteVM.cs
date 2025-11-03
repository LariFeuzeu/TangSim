using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System.Collections.ObjectModel;
using System.Diagnostics;
using TangSim.Models;

namespace TangSim.ViewModels
{
    public partial class HistVenteVM : ObservableObject
    {
        private DBService _dbservice;

        [ObservableProperty]
        private DateTime _dateDebut = DateTime.Now;

        [ObservableProperty]
        private DateTime _dateFin = DateTime.Now;

        [ObservableProperty]
        private ObservableCollection<Achat> _ventesFiltrees = new ObservableCollection<Achat>();

        [ObservableProperty]
        private decimal _montantTotal;

        public HistVenteVM()
        {

            FiltrerCommand = new RelayCommand(Filtrer);
            // Initialisation des commandes

        }

        public ObservableCollection<Article> Articles { get; } = new ObservableCollection<Article>();
        // Commandes pour changer la date

        public IRelayCommand FiltrerCommand { get; }
        // Méthode pour initialiser le DBService après que le ViewModel ait été créé

        private async Task<string> GetNomArticleAsync(int id)
        {
            var article = await _dbservice.GetByIdAsync(id); // Supposant que cette méthode existe dans DBService
            return article?.Nom ?? "Article inconnu";
        }
        public void Initialize(DBService dbService)
        {
            _dbservice = dbService ?? throw new ArgumentNullException(nameof(dbService));
        }
        private async void Filtrer()
        {
            Debug.WriteLine("Méthode Filtrer appelée"); // Ajoutez ce log
            Debug.WriteLine($"DateDébut: {DateDebut.ToString("dd/MM/yyyy")}, DateFin: {DateFin.ToString("dd/MM/yyyy")}");
            var ventes = (await _dbservice.GetAchatAsync())
                .Where(a => a.DateVente.Date >= DateDebut.Date && a.DateVente.Date <= DateFin.Date)
                .ToList();
            Debug.WriteLine($"Nombre de ventes filtrées : {ventes.Count}");

            VentesFiltrees.Clear();
            MontantTotal = 0;

            foreach (var vente in ventes)
            {
                vente.NomArticle = await GetNomArticleAsync(vente.IdProd);
                VentesFiltrees.Add(vente);
                MontantTotal += vente.PrixTotal;
            }

            if (ventes.Count == 0)
            {
                Debug.WriteLine("Aucune vente correspondante trouvée");
            }
        }


    }
}