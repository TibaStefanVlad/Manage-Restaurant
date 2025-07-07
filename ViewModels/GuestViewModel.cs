using System;
using System.Windows;
using System.Windows.Input;
using Restaurant.Commands;
using Restaurant.Services;
using Restaurant.Views;

namespace Restaurant.ViewModels
{
    public class GuestViewModel : ViewModelBase
    {
        private readonly MenuService _menuService;
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

        // View models for guest functionality
        public MenuViewModel MenuViewModel { get; private set; }
        public SearchViewModel SearchViewModel { get; private set; }

        // Commands
        public ICommand LoginCommand { get; }
        public ICommand RegisterCommand { get; }

        public GuestViewModel(MenuService menuService)
        {
            _menuService = menuService ?? throw new ArgumentNullException(nameof(menuService));

            // Initialize commands
            LoginCommand = new RelayCommand(ExecuteLogin);
            RegisterCommand = new RelayCommand(ExecuteRegister);

            // Initialize sub-ViewModels
            InitializeViewModels();

            // Set default tab
            SelectedTabIndex = 0;
        }

        private void InitializeViewModels()
        {
            // Initialize each tab's view model
            MenuViewModel = new MenuViewModel(_menuService);
            SearchViewModel = new SearchViewModel(_menuService);
        }

        private void ExecuteLogin(object obj)
        {
            // Show login view
            LoginView loginView = new LoginView();
            loginView.Show();

            // Close current window
            if (obj is Window window)
            {
                window.Close();
            }
        }

        private void ExecuteRegister(object obj)
        {
            // Show register view
            RegisterView registerView = new RegisterView();
            registerView.Show();

            // Close current window
            if (obj is Window window)
            {
                window.Close();
            }
        }
    }
}