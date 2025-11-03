using TangSim.Models;
using TangSim.View;
using TangSim.ViewModels;

namespace TangSim
{
    public partial class App : Application
    {
        public App()
        {
            InitializeComponent();

            // Configuration des services
            var services = new ServiceCollection();

            // Chemin de la base de données
            string dbPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "MyBusiness.db");

            // Enregistrer DBService avec le chemin de la base de données
            services.AddSingleton<DBService>(new DBService(dbPath));
            // Enregistrer les ViewModels
            services.AddTransient<panierVM>();
            services.AddTransient<ApprovisionnementVM>();
            services.AddTransient<ArticleVM>();
            services.AddTransient<HistVenteVM>();
            // Enregistrer les pages
            services.AddTransient<AchatView>();
            services.AddTransient<ArticleView>();
            services.AddTransient<PannierApproView>();
            services.AddTransient<PanierView>();
            services.AddTransient<HistoriqueView>();
            services.AddTransient<HistoriqueView>();
            services.AddTransient<HistAppView>();
            services.AddTransient<MainPage>(); // Enregistrer MainPage

            // Construire le fournisseur de services
            var serviceProvider = services.BuildServiceProvider();

            // Récupérer DBService depuis le conteneur de services
            var dbService = serviceProvider.GetRequiredService<DBService>();

            // Définir la page principale comme AppShell en passant DBService
            MainPage = new AppShell(dbService);
        }

        //protected override async void OnStart()
        //{
        //    // Durée minimale du splash screen (en ms)
        //    var minDuration = Task.Delay(300);

        //    // Chargez votre page principale
        //    var loadPage = Task.Run(() => MainPage = new MainPage());

        //    await Task.WhenAll(minDuration, loadPage);
        //}
    }

}