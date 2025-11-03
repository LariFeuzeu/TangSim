using System.Collections.ObjectModel; // Ajoutez cette ligne
using TangSim.Models;

namespace TangSim.ViewModels
{
    public class Panier : VMBase
    {
        private ObservableCollection<PanierItem> _items = new ObservableCollection<PanierItem>();

        public ObservableCollection<PanierItem> Items
        {
            get => _items;
            set
            {
                _items = value;
                OnPropertyChanged();
                OnPropertyChanged(nameof(Total));
            }
        }

        public decimal Total => Items.Sum(item => item.PrixTotal);



        public void RetirerArticle(int articleId)
        {
            var item = Items.FirstOrDefault(item => item.Article?.IdProd == articleId);  // Vérification de null sur Article
            if (item != null)
            {
                Items.Remove(item);
                OnPropertyChanged(nameof(Items));
                OnPropertyChanged(nameof(Total));
            }
        }

        public void ViderPanier()
        {
            Items.Clear();
            OnPropertyChanged(nameof(Items));
            OnPropertyChanged(nameof(Total));
        }
    }

    public class PanierItem : VMBase
    {
        private Article _article;
        private int _quantite;

        public Article Article
        {
            get => _article;
            set
            {
                _article = value;
                OnPropertyChanged();
                OnPropertyChanged(nameof(PrixTotal));
            }
        }

        public int Quantite
        {
            get => _quantite;
            set
            {
                _quantite = value;
                OnPropertyChanged();
                OnPropertyChanged(nameof(PrixTotal));
            }
        }

        public decimal PrixTotal => Article?.PrixU * Quantite ?? 0m;
    }
}