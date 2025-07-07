using System;
using System.Windows;
using System.Windows.Controls;
using Restaurant.ViewModels;
using Restaurant.Services;
using Restaurant.Services.Repositories;

namespace Restaurant.Views
{
    /// <summary>
    /// Interaction logic for RegisterView.xaml
    /// </summary>
    public partial class RegisterView : Window
    {
        private readonly RegisterViewModel _viewModel;

        public RegisterView()
        {
            InitializeComponent();

            var dbService = new DatabaseConnectionService();
            var userRepository = new UserRepository(dbService);
            var userService = new UserService(userRepository);

            _viewModel = new RegisterViewModel(userService);
            DataContext = _viewModel;

            PasswordBox.PasswordChanged += PasswordBox_PasswordChanged;
            ConfirmPasswordBox.PasswordChanged += ConfirmPasswordBox_PasswordChanged;

            _viewModel.CloseRequested += (s, e) => Close();
            _viewModel.LoginRequested += OnLoginRequested;
        }
        private void PasswordBox_PasswordChanged(object sender, RoutedEventArgs e)
        {
            if (DataContext is RegisterViewModel viewModel)
            {
                viewModel.Password = PasswordBox.Password;
            }
        }

        private void ConfirmPasswordBox_PasswordChanged(object sender, RoutedEventArgs e)
        {
            if (DataContext is RegisterViewModel viewModel)
            {
                viewModel.ConfirmPassword = ConfirmPasswordBox.Password;
            }
        }

        private void OnLoginRequested(object sender, EventArgs e)
        {
            var loginView = new LoginView();
            loginView.Show();
            Close();
        }
    }
}
