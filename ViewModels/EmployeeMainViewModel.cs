using System;
using System.ComponentModel;
using System.Windows;
using System.Windows.Input;
using Restaurant.Commands;
using Restaurant.Models;
using Restaurant.Services;
using Restaurant.Services.Repositories;
using Restaurant.Views;

namespace Restaurant.ViewModels
{
    public class EmployeeMainViewModel : ViewModelBase
    {
        private readonly MenuService _menuService;
        private readonly OrderService _orderService;
        private readonly UserService _userService;
        private readonly DishRepository _dishRepository;
        private readonly CategoryRepository _categoryRepository;
        private readonly MenuRepository _menuRepository;
        private readonly AllergenRepository _allergenRepository;
        private readonly SettingRepository _settingRepository;

        private int _selectedTabIndex;
        private object _currentView;

        #region Properties

        public int SelectedTabIndex
        {
            get => _selectedTabIndex;
            set
            {
                _selectedTabIndex = value;
                OnPropertyChanged(nameof(SelectedTabIndex));
            }
        }

        public object CurrentView
        {
            get => _currentView;
            set
            {
                _currentView = value;
                OnPropertyChanged(nameof(CurrentView));
            }
        }

        // Current user and admin status
        public User CurrentUser => SessionService.CurrentUser;
        public bool IsAdminUser => SessionService.CurrentUser?.Role == "Admin";

        // Sub-ViewModels for each management section
        public CategoryManagementViewModel CategoryManagementViewModel { get; private set; }
        public DishManagementViewModel DishManagementViewModel { get; private set; }
        public MenuManagementViewModel MenuManagementViewModel { get; private set; }
        public AllergenManagementViewModel AllergenManagementViewModel { get; private set; }
        public OrderManagementViewModel AllOrdersViewModel { get; private set; }
        public OrderManagementViewModel ActiveOrdersViewModel { get; private set; }
        public InventoryManagementViewModel InventoryManagementViewModel { get; private set; }
        public UserManagementViewModel UserManagementViewModel { get; private set; }

        #endregion

        #region Commands

        public ICommand LogoutCommand { get; }

        #endregion

        public EmployeeMainViewModel(
            MenuService menuService,
            OrderService orderService,
            UserService userService,
            DishRepository dishRepository,
            CategoryRepository categoryRepository,
            MenuRepository menuRepository,
            AllergenRepository allergenRepository,
            SettingRepository settingRepository)
        {
            _menuService = menuService ?? throw new ArgumentNullException(nameof(menuService));
            _orderService = orderService ?? throw new ArgumentNullException(nameof(orderService));
            _userService = userService ?? throw new ArgumentNullException(nameof(userService));
            _dishRepository = dishRepository ?? throw new ArgumentNullException(nameof(dishRepository));
            _categoryRepository = categoryRepository ?? throw new ArgumentNullException(nameof(categoryRepository));
            _menuRepository = menuRepository ?? throw new ArgumentNullException(nameof(menuRepository));
            _allergenRepository = allergenRepository ?? throw new ArgumentNullException(nameof(allergenRepository));
            _settingRepository = settingRepository ?? throw new ArgumentNullException(nameof(settingRepository));

            // Initialize commands
            LogoutCommand = new RelayCommand(ExecuteLogout);

            // Initialize sub-ViewModels
            InitializeViewModels();

            // Set default tab
            SelectedTabIndex = 0;

            // Force notify properties that depend on SessionService
            OnPropertyChanged(nameof(CurrentUser));
            OnPropertyChanged(nameof(IsAdminUser));
        }

        private void InitializeViewModels()
        {
            // Category Management
            CategoryManagementViewModel = new CategoryManagementViewModel(_categoryRepository);

            // Dish Management
            DishManagementViewModel = new DishManagementViewModel(
                _dishRepository,
                _categoryRepository,
                _allergenRepository);

            // Menu Management
            MenuManagementViewModel = new MenuManagementViewModel(
                _menuService,
                _menuRepository,
                _dishRepository,
                _categoryRepository);

            // Allergen Management
            AllergenManagementViewModel = new AllergenManagementViewModel(_allergenRepository);

            // Orders Management
            AllOrdersViewModel = new OrderManagementViewModel(
                _orderService,
                _userService,
                false); // false = all orders

            ActiveOrdersViewModel = new OrderManagementViewModel(
                _orderService,
                _userService,
                true);  // true = active orders only

            // Inventory Management
            InventoryManagementViewModel = new InventoryManagementViewModel(
                _dishRepository,
                _orderService);

            // User Management (Admin only)
            if (IsAdminUser)
            {
                UserManagementViewModel = new UserManagementViewModel(_userService);
            }
        }

        private void ExecuteLogout(object obj)
        {
            // Clear current user
            SessionService.CurrentUser = null;

            // Show login view
            LoginView loginView = new LoginView();
            loginView.Show();

            // Close current window (from command parameter)
            if (obj is Window window)
            {
                window.Close();
            }

            // Also raise event for subscribers
            LogoutRequested?.Invoke(this, EventArgs.Empty);
        }

        public event EventHandler LogoutRequested;
    }

    // Placeholder ViewModels - you would create full implementations of these

    public class CategoryManagementViewModel : ViewModelBase
    {
        private readonly CategoryRepository _categoryRepository;

        public CategoryManagementViewModel(CategoryRepository categoryRepository)
        {
            _categoryRepository = categoryRepository ?? throw new ArgumentNullException(nameof(categoryRepository));
            // Initialize and load categories
        }
    }

    public class AllergenManagementViewModel : ViewModelBase
    {
        private readonly AllergenRepository _allergenRepository;

        public AllergenManagementViewModel(AllergenRepository allergenRepository)
        {
            _allergenRepository = allergenRepository ?? throw new ArgumentNullException(nameof(allergenRepository));
            // Initialize and load allergens
        }
    }

    public class OrderManagementViewModel : ViewModelBase
    {
        private readonly OrderService _orderService;
        private readonly UserService _userService;
        private readonly bool _activeOnly;

        public OrderManagementViewModel(
            OrderService orderService,
            UserService userService,
            bool activeOnly)
        {
            _orderService = orderService ?? throw new ArgumentNullException(nameof(orderService));
            _userService = userService ?? throw new ArgumentNullException(nameof(userService));
            _activeOnly = activeOnly;
            // Initialize and load orders
        }
    }

    public class InventoryManagementViewModel : ViewModelBase
    {
        private readonly DishRepository _dishRepository;
        private readonly OrderService _orderService;

        public InventoryManagementViewModel(
            DishRepository dishRepository,
            OrderService orderService)
        {
            _dishRepository = dishRepository ?? throw new ArgumentNullException(nameof(dishRepository));
            _orderService = orderService ?? throw new ArgumentNullException(nameof(orderService));
            // Initialize and load inventory
        }
    }

    public class UserManagementViewModel : ViewModelBase
    {
        private readonly UserService _userService;

        public UserManagementViewModel(UserService userService)
        {
            _userService = userService ?? throw new ArgumentNullException(nameof(userService));
            // Initialize and load users
        }
    }
}