using System;
using System.ComponentModel.DataAnnotations;
using System.Windows;
using System.Windows.Input;
using Microsoft.Data.SqlClient;
using Restaurant.Commands;
using Restaurant.Models;
using Restaurant.Services;
using Restaurant.Views;

namespace Restaurant.ViewModels
{
    internal class LoginViewModel : ViewModelBase
    {
        private readonly UserService _userService;
        private string _email;
        private string _password;
        private string _errorMessage;

        public string Email
        {
            get => _email;
            set
            {
                _email = value;
                OnPropertyChanged(nameof(Email));
                ((RelayCommand)LoginCommand).RaiseCanExecuteChanged();
            }
        }

        public string Password
        {
            get => _password;
            set
            {
                _password = value;
                OnPropertyChanged(nameof(Password));
                ((RelayCommand)LoginCommand).RaiseCanExecuteChanged();
            }
        }

        public string ErrorMessage
        {
            get => _errorMessage;
            set
            {
                _errorMessage = value;
                OnPropertyChanged(nameof(ErrorMessage));
            }
        }

        public ICommand LoginCommand { get; }
        public ICommand OpenRegisterCommand { get; }
        public ICommand ContinueAsGuestCommand { get; }

        public LoginViewModel(UserService userService)
        {
            _userService = userService ?? throw new ArgumentNullException(nameof(userService));
            LoginCommand = new RelayCommand(ExecuteLogin, CanExecuteLogin);
            OpenRegisterCommand = new RelayCommand(ExecuteOpenRegister);
            ContinueAsGuestCommand = new RelayCommand(ExecuteContinueAsGuest);
        }
        private bool CanExecuteLogin(object obj)
        {
            return !string.IsNullOrWhiteSpace(Email) && !string.IsNullOrWhiteSpace(Password);
        }

        private void ExecuteLogin(object obj)
        {
            try
            {
                User user = _userService.Authenticate(Email, Password);
                if (user != null)
                {
                    SessionService.CurrentUser = user;

                    MessageBox.Show($"Login successful. Welcome {user.FirstName} {user.LastName}! (Role: {user.Role})",
                        "Success", MessageBoxButton.OK, MessageBoxImage.Information);

                    try
                    {
                        if (user.Role == "Admin" || user.Role == "Employee")
                        {
                            Console.WriteLine($"Opening EmployeeMainView for role: {user.Role}");
                            EmployeeMainView employeeView = new EmployeeMainView();
                            employeeView.Show();
                        }
                        else if (user.Role == "Customer") 
                        {
                            Console.WriteLine($"Opening CustomerMainView for role: {user.Role}");
                            CustomerMainView customerView = new CustomerMainView();
                            customerView.Show();
                        }
                        else
                        {
                            MessageBox.Show($"Unknown role: {user.Role}. Using customer view by default.",
                                "Role Warning", MessageBoxButton.OK, MessageBoxImage.Warning);

                            Console.WriteLine($"Using default CustomerMainView for unknown role: {user.Role}");
                            CustomerMainView customerView = new CustomerMainView();
                            customerView.Show();
                        }

                        CloseRequested?.Invoke(this, EventArgs.Empty);
                    }
                    catch (Exception navEx)
                    {
                        string errorMsg = $"Error navigating to view for role '{user.Role}': {navEx.Message}\n\n";
                        errorMsg += $"Stack trace: {navEx.StackTrace}";

                        Console.WriteLine(errorMsg);
                        MessageBox.Show(errorMsg, "Navigation Error", MessageBoxButton.OK, MessageBoxImage.Error);
                    }
                }
                else
                {
                    ErrorMessage = "Invalid email or password.";
                }
            }
            catch (SqlException ex)
            {
                ErrorMessage = "Database connection error. Please make sure the database is available.";
                Console.WriteLine($"SQL Exception: {ex.Message}");
            }
            catch (Exception ex)
            {
                ErrorMessage = "An unexpected error occurred. Please try again later.";
                Console.WriteLine($"Exception details: {ex.Message}");
                Console.WriteLine($"Stack trace: {ex.StackTrace}");
            }
        }

        private void ExecuteContinueAsGuest(object obj)
        {
            try
            {
                Console.WriteLine("Continue as Guest button clicked");
                GuestView guestView = new GuestView();
                guestView.Show();
                Console.WriteLine("GuestView shown successfully");
                CloseRequested?.Invoke(this, EventArgs.Empty);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error opening Guest View: {ex.Message}");
                Console.WriteLine($"Stack trace: {ex.StackTrace}");
                MessageBox.Show($"Error opening Guest View: {ex.Message}\n\nStack trace: {ex.StackTrace}",
                    "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void ExecuteOpenRegister(object obj)
        {
            RegisterRequested?.Invoke(this, EventArgs.Empty);
        }

        public event EventHandler CloseRequested;
        public event EventHandler RegisterRequested;
    }
}
