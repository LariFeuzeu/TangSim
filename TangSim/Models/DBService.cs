
using SQLite;


namespace TangSim.Models
{
    public class DBService
    {
        private readonly string _dbPath;
        //private const string Db_Name = "MyBusiness.db";
        public readonly SQLiteAsyncConnection _connection; // utilisée pour gérer les connexions à une base de données SQLite de manière asynchrone.


        // Constructeur pour initialiser la connexion SQLite et créer les tables si elles n'existent pas
        public DBService(string dbPath)
        {
            _dbPath = dbPath ?? throw new ArgumentNullException(nameof(dbPath), "Mybusiness.db");
            _connection = new SQLiteAsyncConnection(dbPath);
            _connection.ExecuteAsync("PRAGMA foreign_keys=ON;"); // Activer les clés étrangères
            _connection.CreateTableAsync<Article>().Wait(); // creation de la table Article
            _connection.CreateTableAsync<Approvisionnement>().Wait();
            _connection.CreateTableAsync<Achat>().Wait();
        }
        // Exemple de méthode qui retourne la base de données SQLite
        public SQLiteAsyncConnection GetDatabase()
        {
            return _connection;
        }


        // Methode pour l'ajout dans la base de donnee
        public async Task<List<Article>> GetArticlesAsync()
        {
            return await _connection.Table<Article>().ToListAsync();
        }
        public async Task<Article> GetByIdAsync(int id)
        {
            return await _connection.Table<Article>().Where(x => x.IdProd == id).FirstOrDefaultAsync();

        }
        public async Task CreateArticleAsync(Article article)
        {
            await _connection.InsertAsync(article);
        }

        public async Task UpdateArticleAsync(Article article)
        {
            await _connection.UpdateAsync(article);
        }

        public async Task DeleteArticleAsync(Article article)
        {
            await _connection.DeleteAsync(article);
        }


        //Merhodes pours les approvisionnements

        public async Task<List<Approvisionnement>> GetApprovisionnementsAsync()
        {
            return await _connection.Table<Approvisionnement>().ToListAsync();
        }

        public async Task<Approvisionnement> GetApprovisionnementByIdAsync(int id)
        {
            return await _connection.Table<Approvisionnement>().Where(x => x.IdApprov == id).FirstOrDefaultAsync();
        }

        public async Task CreateApprovisionnementAsync(Approvisionnement approvisionnement)
        {
            await _connection.InsertAsync(approvisionnement);
        }

        public async Task UpdateApprovisionnementAsync(Approvisionnement approv)
        {
            await _connection.InsertOrReplaceAsync(approv);  // Insère ou remplace l'approvisionnement dans la DB
        }


        //public async Task DeleteApprovisionnementAsync(Approvisionnement approvisionnement)
        //{
        //    await _connection.DeleteAsync(approvisionnement);
        //}

        public async Task<List<Approvisionnement>> GetApprovisionnementsByArticleIdAsync(int articleId)
        {
            return await _connection.Table<Approvisionnement>().Where(x => x.IdProd == articleId).ToListAsync();
        }

        public async Task<int> GetTotalApprovisionnementAsync()
        {
            return await _connection.Table<Approvisionnement>().CountAsync();
        }

        public async Task CreateAchatAsync(Achat achat)
        {
            await _connection.InsertAsync(achat);
        }

        public async Task<List<Achat>> GetAchatAsync()
        {
            return await _connection.Table<Achat>().ToListAsync();

        }




    }


}



