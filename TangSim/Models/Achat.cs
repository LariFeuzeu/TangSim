using SQLite;
using System.ComponentModel.DataAnnotations.Schema;

namespace TangSim.Models
{
    public class Achat
    {
        [PrimaryKey, AutoIncrement]
        public int IdAchat { get; set; }
        [ForeignKey("IdProd")]
        public int IdProd { get; set; }
        public int Quantite { get; set; }
        public decimal PrixTotal { get; set; }
        public DateTime DateVente { get; set; }
        public string NomArticle { get; set; } // Non mappée en base de données
    }
}
