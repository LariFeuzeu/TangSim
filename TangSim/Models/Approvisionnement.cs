using SQLite;
using System.ComponentModel.DataAnnotations.Schema;

namespace TangSim.Models
{
    public class Approvisionnement
    {
        //internal Article SelectedArticle;

        [PrimaryKey, AutoIncrement]
        public int IdApprov { get; set; }

        [ForeignKey("IdProd")]
        public int IdProd { get; set; }
        public int QteApprov { get; set; }
        public int PrixApprov { get; set; }
        public DateTime DateApprov { get; set; }
        public string NomArticle { get; set; } // Non mappée en base de données
        // Ajout de la colonne MontantApprov pour le stocker dans la base de données
        public int MontantApprov { get; set; }  // Stocker le montant calculé
                                                // Calcul dynamique du montant
                                                // Colonne pour persister le montant dans la base de données
        public int MontantApprovPersiste { get; set; }  // Utilisé pour le stockage en base de données
    }
}
