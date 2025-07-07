using System;
using System.Collections.Generic;
using Restaurant.Models;
using Restaurant.Services.Repositories;

namespace Restaurant.Services
{
    public class UserService
    {
        private readonly UserRepository _userRepository;

        public UserService(UserRepository userRepository)
        {
            _userRepository = userRepository ?? throw new ArgumentNullException(nameof(userRepository));
        }

        public User Authenticate(string email, string password)
        {
            return _userRepository.Authenticate(email, password);
        }

        public User GetUserById(int userId)
        {
            return _userRepository.GetById(userId);
        }

        public User GetUserByEmail(string email)
        {
            return _userRepository.GetByEmail(email);
        }

        public IEnumerable<User> GetAllUsers()
        {
            return _userRepository.GetAll();
        }

        public int RegisterUser(User user)
        {
            if (_userRepository.GetByEmail(user.Email) != null)
            {
                throw new InvalidOperationException("A user with this email already exists.");
            }

            if (string.IsNullOrEmpty(user.Role))
            {
                user.Role = "Customer";
            }

            user.RegistrationDate = DateTime.Now;

            return _userRepository.Add(user);
        }

        public bool UpdateUser(User user)
        {
            return _userRepository.Update(user);
        }

        public bool ChangePassword(int userId, string newPassword)
        {
            return _userRepository.UpdatePassword(userId, newPassword);
        }

        public bool DeleteUser(int userId)
        {
            return _userRepository.Delete(userId);
        }
    }
}