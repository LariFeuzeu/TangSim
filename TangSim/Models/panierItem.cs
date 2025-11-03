namespace TangSim.Models
{
    public class PanierItem
    {
        public Article Article { get; set; }
        public int Quantite { get; set; }
        public decimal PrixTotal => Article.PrixU * Quantite;
    }
}
