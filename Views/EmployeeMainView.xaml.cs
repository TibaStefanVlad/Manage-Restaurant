using System.Windows;
using Restaurant.Services;
using Restaurant.Services.Repositories;
using Restaurant.ViewModels;

namespace Restaurant.Views
{
    /// <summary>
    /// Interaction logic for EmployeeMainView.xaml
    /// </summary>
    public partial class EmployeeMainView : Window
    {
        private readonly EmployeeMainViewModel _viewModel;

        public EmployeeMainView()
        {
            InitializeComponent();

            // Create database connection service
            var dbService = new DatabaseConnectionService();

            // Create repositories
            var categoryRepository = new CategoryRepository(dbService);
            var dishRepository = new DishRepository(dbService);
            var menuRepository = new MenuRepository(dbService);
            var allergenRepository = new AllergenRepository(dbService);
            var orderRepository = new OrderRepository(dbService);
            var userRepository = new UserRepository(dbService);
            var settingRepository = new SettingRepository(dbService);

            // Create services
            var menuService = new MenuService(menuRepository, dishRepository, categoryRepository, settingRepository, allergenRepository);
            var userService = new UserService(userRepository);
            var orderService = new OrderService(orderRepository, dishRepository, menuRepository, userRepository, settingRepository);

            // Create view model
            _viewModel = new EmployeeMainViewModel(
                menuService,
                orderService,
                userService,
                dishRepository,
                categoryRepository,
                menuRepository,
                allergenRepository,
                settingRepository);

            DataContext = _viewModel;

            // Handle logout event
            _viewModel.LogoutRequested += (s, e) => Close();
        }
    }
}