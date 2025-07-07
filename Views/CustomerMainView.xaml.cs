using System.Windows;
using Restaurant.Services;
using Restaurant.Services.Repositories;
using Restaurant.ViewModels;

namespace Restaurant.Views
{
    /// <summary>
    /// Interaction logic for CustomerMainView.xaml
    /// </summary>
    public partial class CustomerMainView : Window
    {
        private readonly CustomerMainViewModel _viewModel;

        public CustomerMainView()
        {
            InitializeComponent();

            var dbService = new DatabaseConnectionService();

            var categoryRepository = new CategoryRepository(dbService);
            var dishRepository = new DishRepository(dbService);
            var menuRepository = new MenuRepository(dbService);
            var allergenRepository = new AllergenRepository(dbService);
            var orderRepository = new OrderRepository(dbService);
            var userRepository = new UserRepository(dbService);
            var settingRepository = new SettingRepository(dbService);

            var menuService = new MenuService(menuRepository, dishRepository, categoryRepository, settingRepository, allergenRepository);
            var userService = new UserService(userRepository);
            var orderService = new OrderService(orderRepository, dishRepository, menuRepository, userRepository, settingRepository);

            _viewModel = new CustomerMainViewModel(menuService, orderService, userService);
            DataContext = _viewModel;

            _viewModel.LogoutRequested += (s, e) => Close();
        }
    }
}