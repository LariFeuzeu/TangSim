using TangSim.Models;

namespace TangSim
{
    public partial class AppShell : Shell
    {
        private readonly DBService _dbService;

        public AppShell(DBService dbService)
        {
            InitializeComponent();
            _dbService = dbService;
        }
    }
}