using System;
using System.Collections.Generic;

namespace Restaurant.Models
{
    /// <summary>
    /// Represents a user of the system (customer or employee)
    /// </summary>
    public class User
    {
        public int UserId { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string Email { get; set; }
        public string PasswordHash { get; set; }
        public string PhoneNumber { get; set; }
        public string DeliveryAddress { get; set; }
        public string Role { get; set; }  // "Customer" or "Employee"
        public DateTime RegistrationDate { get; set; }

        // Navigation property
        public virtual ICollection<Order> Orders { get; set; }

        public User()
        {
            Orders = new HashSet<Order>();
            RegistrationDate = DateTime.Now;
        }

        public string FullName => $"{FirstName ?? ""} {LastName ?? ""}".Trim();
    }
}
