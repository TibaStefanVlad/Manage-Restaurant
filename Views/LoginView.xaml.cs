using System;
using System.Windows;
using System.Windows.Controls;
using Restaurant.ViewModels;
using Restaurant.Services;
using Restaurant.Services.Repositories;
using Microsoft.Win32;

namespace Restaurant.Views
{
    /// <summary>
    /// Interaction logic for LoginView.xaml
    /// </summary>
    public partial class LoginView : Window
    {
        private readonly LoginViewModel _viewModel;

        public LoginView()
        {
            InitializeComponent();

            var dbService = new DatabaseConnectionService();
            var userRepository = new UserRepository(dbService);
            var userService = new UserService(userRepository);

            _viewModel = new LoginViewModel(userService);
            DataContext = _viewModel;

            PasswordBox.PasswordChanged += PasswordBox_PasswordChanged;

            _viewModel.CloseRequested += (s, e) =>
            {
                this.Close();
            };

            _viewModel.RegisterRequested += OnRegisterRequested;
        }
        private void PasswordBox_PasswordChanged(object sender, RoutedEventArgs e)
        {
            if (DataContext is LoginViewModel viewModel)
            {
                viewModel.Password = PasswordBox.Password;
            }
        }

        private void OnRegisterRequested(object sender, EventArgs e)
        {
            var registerView = new RegisterView();
            registerView.Show();
            Close();
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {

        }

        private void Button_Click_1(object sender, RoutedEventArgs e)
        {

        }
    }
}
