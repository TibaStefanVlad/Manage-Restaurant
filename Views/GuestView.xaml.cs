using System.Windows;
using Restaurant.Services;
using Restaurant.Services.Repositories;
using Restaurant.ViewModels;

namespace Restaurant.Views
{
    /// <summary>
    /// Interaction logic for GuestView.xaml
    /// </summary>
    public partial class GuestView : Window
    {
        private readonly GuestViewModel _viewModel;

        public GuestView()
        {
            InitializeComponent();

            var dbService = new DatabaseConnectionService();

            var categoryRepository = new CategoryRepository(dbService);
            var dishRepository = new DishRepository(dbService);
            var menuRepository = new MenuRepository(dbService);
            var allergenRepository = new AllergenRepository(dbService);
            var settingRepository = new SettingRepository(dbService);

            var menuService = new MenuService(menuRepository, dishRepository, categoryRepository, settingRepository, allergenRepository);

            _viewModel = new GuestViewModel(menuService);
            DataContext = _viewModel;
        }

        private void SearchView_Loaded(object sender, RoutedEventArgs e)
        {

        }
    }
}