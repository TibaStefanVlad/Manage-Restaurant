using System;
using System.Windows.Input;
using Restaurant.Commands;
using Restaurant.Models;
using Restaurant.Services;
using Restaurant.Views;

namespace Restaurant.ViewModels
{
    public class CustomerMainViewModel : ViewModelBase
    {
        private readonly MenuService _menuService;
        private readonly OrderService _orderService;
        private readonly UserService _userService;

        private int _selectedTabIndex;

        public int SelectedTabIndex
        {
            get => _selectedTabIndex;
            set
            {
                _selectedTabIndex = value;
                OnPropertyChanged(nameof(SelectedTabIndex));
            }
        }

        public User CurrentUser => SessionService.CurrentUser;

        public MenuViewModel MenuViewModel { get; private set; }
        public SearchViewModel SearchViewModel { get; private set; }
        public OrderViewModel OrderViewModel { get; private set; }
        public OrderHistoryViewModel OrderHistoryViewModel { get; private set; }

        public ICommand LogoutCommand { get; }

        public CustomerMainViewModel(
            MenuService menuService,
            OrderService orderService,
            UserService userService)
        {
            _menuService = menuService ?? throw new ArgumentNullException(nameof(menuService));
            _orderService = orderService ?? throw new ArgumentNullException(nameof(orderService));
            _userService = userService ?? throw new ArgumentNullException(nameof(userService));

            LogoutCommand = new RelayCommand(ExecuteLogout);

            InitializeViewModels();

            SelectedTabIndex = 0;

            OnPropertyChanged(nameof(CurrentUser));
        }

        private void InitializeViewModels()
        {
            MenuViewModel = new MenuViewModel(_menuService);
            SearchViewModel = new SearchViewModel(_menuService);
            OrderViewModel = new OrderViewModel(_menuService, _orderService);
            OrderHistoryViewModel = new OrderHistoryViewModel(_orderService);

            OrderViewModel.OrderPlaced += (s, e) => {
                SelectedTabIndex = 3; // Switch to order history tab
                OrderHistoryViewModel.RefreshOrdersCommand.Execute(null);
            };
        }

        private void ExecuteLogout(object obj)
        {
            SessionService.CurrentUser = null;

            LoginView loginView = new LoginView();
            loginView.Show();

            if (obj is System.Windows.Window window)
            {
                window.Close();
            }

            LogoutRequested?.Invoke(this, EventArgs.Empty);
        }

        public event EventHandler LogoutRequested;
    }
}