using System;
using System.Windows;
using System.Windows.Input;
using Restaurant.Commands;
using Restaurant.Models;
using Restaurant.Services;

namespace Restaurant.ViewModels
{
    internal class RegisterViewModel: ViewModelBase
    {
        private readonly UserService _userService;
        private string _firstName;
        private string _lastName;
        private string _email;
        private string _phoneNumber;
        private string _password;
        private string _deliveryAddress;
        private string _errorMessage;
        private string _confirmPassword;

        public string FirstName
        {
            get => _firstName;
            set
            {
                _firstName = value;
                OnPropertyChanged(nameof(FirstName));
                ((RelayCommand)RegisterCommand).RaiseCanExecuteChanged();
            }
        }

        public string LastName
        {
            get => _lastName;
            set
            {
                _lastName = value;
                OnPropertyChanged(nameof(LastName));
                ((RelayCommand)RegisterCommand).RaiseCanExecuteChanged();
            }
        }

        public string Email
        {
            get => _email;
            set
            {
                _email = value;
                OnPropertyChanged(nameof(Email));
                ((RelayCommand)RegisterCommand).RaiseCanExecuteChanged();
            }
        }

        public string PhoneNumber
        {
            get => _phoneNumber;
            set
            {
                _phoneNumber = value;
                OnPropertyChanged(nameof(PhoneNumber));
            }
        }

        public string DeliveryAddress
        {
            get => _deliveryAddress;
            set
            {
                _deliveryAddress = value;
                OnPropertyChanged(nameof(DeliveryAddress));
            }
        }

        public string Password
        {
            get => _password;
            set
            {
                _password = value;
                OnPropertyChanged(nameof(Password));
                ((RelayCommand)RegisterCommand).RaiseCanExecuteChanged();
            }
        }

        public string ConfirmPassword
        {
            get => _confirmPassword;
            set
            {
                _confirmPassword = value;
                OnPropertyChanged(nameof(ConfirmPassword));
                ((RelayCommand)RegisterCommand).RaiseCanExecuteChanged();
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

        public ICommand RegisterCommand { get; }
        public ICommand BackToLoginCommand { get; }

        public RegisterViewModel(UserService userService)
        {
            _userService = userService ?? throw new ArgumentNullException(nameof(userService));
            RegisterCommand = new RelayCommand(ExecuteRegister, CanExecuteRegister);
            BackToLoginCommand = new RelayCommand(ExecuteBackToLogin);
        }

        private bool CanExecuteRegister(object obj)
        {
            return !string.IsNullOrWhiteSpace(FirstName) &&
                   !string.IsNullOrWhiteSpace(LastName) &&
                   !string.IsNullOrWhiteSpace(Email) &&
                   !string.IsNullOrWhiteSpace(Password) &&
                   !string.IsNullOrWhiteSpace(ConfirmPassword) &&
                   Password == ConfirmPassword;
        }

        private void ExecuteRegister(object obj)
        {
            try
            {
                User existingUser = _userService.GetUserByEmail(Email);

                if (existingUser != null)
                {
                    ErrorMessage = "A user with this email already exists.";
                    return;
                }

                User user = new User
                {
                    FirstName = FirstName,
                    LastName = LastName,
                    Email = Email,
                    PhoneNumber = PhoneNumber,
                    DeliveryAddress = DeliveryAddress,
                    PasswordHash = Password,
                    Role = "Customer"
                };

                int userId = _userService.RegisterUser(user);

                if (userId > 0)
                {
                    MessageBox.Show("Registration successful. You can now login.", "Success", MessageBoxButton.OK, MessageBoxImage.Information);

                    ExecuteBackToLogin(null);
                }
                else
                {
                    ErrorMessage = "Registration failed";
                }
            }
            catch (Exception ex)
            {
                ErrorMessage = $"Registration error: {ex.Message}";
            }
        }
        private void ExecuteBackToLogin(object obj)
        {
            LoginRequested?.Invoke(this, EventArgs.Empty);
        }

        public event EventHandler CloseRequested;
        public event EventHandler LoginRequested;
    }
}
