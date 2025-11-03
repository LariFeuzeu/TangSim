using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System.Collections.ObjectModel;
using System.Diagnostics;
using TangSim.Models;

namespace TangSim.ViewModels
{
    public partial class HistAppVM : ObservableObject
    {
        private DBService _dbservice;

        [ObservableProperty]
        private DateTime _dateDebut = DateTime.Now.AddMonths(-1);

        [ObservableProperty]
        private DateTime _dateFin = DateTime.Now;

        [ObservableProperty]
        private ObservableCollection<Approvisionnement> _approvisionnementsFiltres = new ObservableCollection<Approvisionnement>();

        [ObservableProperty]
        private decimal _montantTotal;

        public HistAppVM()
        {
            FiltrerCommand = new RelayCommand(Filtrer);
        }

        public IRelayCommand FiltrerCommand { get; }

        public void Initialize(DBService dbService)
        {
            _dbservice = dbService ?? throw new ArgumentNullException(nameof(dbService));
        }

        private async Task<string> GetNomArticleAsync(int id)
        {
            var article = await _dbservice.GetByIdAsync(id);
            return article?.Nom ?? "Article inconnu";
        }

        private async void Filtrer()
        {
            try
            {
                Debug.WriteLine($"Filtrage des approvisionnements entre {DateDebut:dd/MM/yyyy} et {DateFin:dd/MM/yyyy}");

                var approvisionnements = (await _dbservice.GetApprovisionnementsAsync())
                    .Where(a => a.DateApprov.Date >= DateDebut.Date && a.DateApprov.Date <= DateFin.Date)
                    .ToList();

                ApprovisionnementsFiltres.Clear();
                MontantTotal = 0;

                foreach (var approv in approvisionnements)
                {
                    approv.NomArticle = await GetNomArticleAsync(approv.IdProd);
                    ApprovisionnementsFiltres.Add(approv);
                    MontantTotal += approv.MontantApprovPersiste;
                }

                Debug.WriteLine($"Nombre d'approvisionnements trouvés : {approvisionnements.Count}");
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Erreur lors du filtrage : {ex.Message}");
                await App.Current.MainPage.DisplayAlert("Erreur", ex.Message, "OK");
            }
        }
    }
}