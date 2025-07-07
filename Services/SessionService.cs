using System.ComponentModel;
using System.Runtime.CompilerServices;
using Restaurant.Models;

namespace Restaurant.Services
{
    public static class SessionService
    {
        private static User _currentUser;

        public static event PropertyChangedEventHandler CurrentUserChanged;

        public static User CurrentUser
        {
            get => _currentUser;
            set
            {
                _currentUser = value;
                OnCurrentUserChanged();
            }
        }

        private static void OnCurrentUserChanged()
        {
            CurrentUserChanged?.Invoke(null, new PropertyChangedEventArgs(nameof(CurrentUser)));
        }
    }
}