using SQLite;

namespace TangSim.Models
{

    public class Article
    {
        [PrimaryKey, AutoIncrement]
        public int IdProd { get; set; }

        public string Nom { get; set; }
        public int PrixU { get; set; }
        public int QteStock { get; set; }

        public string ImagePath { get; set; }

    }
}
