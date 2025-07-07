using System;
using System.Collections.Generic;
using System.Data;
using Microsoft.Data.SqlClient;
using Restaurant.Models;
using System.Security.Cryptography;
using System.Text;

namespace Restaurant.Services.Repositories
{
    public class UserRepository : IRepository<User>
    {
        private readonly DatabaseConnectionService _dbService;

        public UserRepository(DatabaseConnectionService dbService)
        {
            _dbService = dbService ?? throw new ArgumentNullException(nameof(dbService));
        }

        public IEnumerable<User> GetAll()
        {
            List<User> users = new List<User>();

            DataTable dataTable = _dbService.ExecuteReader("usp_GetAllUsers");

            foreach (DataRow row in dataTable.Rows)
            {
                users.Add(MapRowToUser(row));
            }

            return users;
        }

        public User GetById(int id)
        {
            SqlParameter parameter = _dbService.CreateParameter("@UserId", SqlDbType.Int, id);

            DataTable dataTable = _dbService.ExecuteReader("usp_GetUserById", parameter);

            if (dataTable.Rows.Count > 0)
            {
                return MapRowToUser(dataTable.Rows[0]);
            }

            return null;
        }

        public int Add(User user)
        {
            // Hash the password before storing
            string passwordHash = HashPassword(user.PasswordHash);

            SqlParameter[] parameters =
            {
                _dbService.CreateParameter("@FirstName", SqlDbType.NVarChar, user.FirstName),
                _dbService.CreateParameter("@LastName", SqlDbType.NVarChar, user.LastName),
                _dbService.CreateParameter("@Email", SqlDbType.NVarChar, user.Email),
                _dbService.CreateParameter("@PasswordHash", SqlDbType.NVarChar, passwordHash),
                _dbService.CreateParameter("@PhoneNumber", SqlDbType.NVarChar, user.PhoneNumber),
                _dbService.CreateParameter("@DeliveryAddress", SqlDbType.NVarChar, user.DeliveryAddress),
                _dbService.CreateParameter("@Role", SqlDbType.NVarChar, user.Role),
                _dbService.CreateParameter("@RegistrationDate", SqlDbType.DateTime, DateTime.Now),
                _dbService.CreateParameter("@UserId", SqlDbType.Int, null)
            };

            parameters[8].Direction = ParameterDirection.Output;

            _dbService.ExecuteNonQuery("usp_AddUser", parameters);

            return (int)parameters[8].Value;
        }

        public bool Update(User user)
        {
            SqlParameter[] parameters =
            {
                _dbService.CreateParameter("@UserId", SqlDbType.Int, user.UserId),
                _dbService.CreateParameter("@FirstName", SqlDbType.NVarChar, user.FirstName),
                _dbService.CreateParameter("@LastName", SqlDbType.NVarChar, user.LastName),
                _dbService.CreateParameter("@Email", SqlDbType.NVarChar, user.Email),
                _dbService.CreateParameter("@PhoneNumber", SqlDbType.NVarChar, user.PhoneNumber),
                _dbService.CreateParameter("@DeliveryAddress", SqlDbType.NVarChar, user.DeliveryAddress),
                _dbService.CreateParameter("@Role", SqlDbType.NVarChar, user.Role)
            };

            int rowsAffected = _dbService.ExecuteNonQuery("usp_UpdateUser", parameters);

            return rowsAffected > 0;
        }

        public bool UpdatePassword(int userId, string newPassword)
        {
            string passwordHash = HashPassword(newPassword);

            SqlParameter[] parameters =
            {
                _dbService.CreateParameter("@UserId", SqlDbType.Int, userId),
                _dbService.CreateParameter("@PasswordHash", SqlDbType.NVarChar, passwordHash)
            };

            int rowsAffected = _dbService.ExecuteNonQuery("usp_UpdateUserPassword", parameters);

            return rowsAffected > 0;
        }

        public bool Delete(int id)
        {
            SqlParameter parameter = _dbService.CreateParameter("@UserId", SqlDbType.Int, id);

            int rowsAffected = _dbService.ExecuteNonQuery("usp_DeleteUser", parameter);

            return rowsAffected > 0;
        }

        public User Authenticate(string email, string password)
        {
            SqlParameter[] parameters =
            {
                _dbService.CreateParameter("@Email", SqlDbType.NVarChar, email)
            };

            DataTable dataTable = _dbService.ExecuteReader("usp_GetUserByEmail", parameters);

            if (dataTable.Rows.Count > 0)
            {
                User user = MapRowToUser(dataTable.Rows[0]);

                // Verify the password
                if (VerifyPassword(password, user.PasswordHash))
                {
                    return user;
                }
            }

            return null;
        }

        public User GetByEmail(string email)
        {
            SqlParameter parameter = _dbService.CreateParameter("@Email", SqlDbType.NVarChar, email);

            DataTable dataTable = _dbService.ExecuteReader("usp_GetUserByEmail", parameter);

            if (dataTable.Rows.Count > 0)
            {
                return MapRowToUser(dataTable.Rows[0]);
            }

            return null;
        }

        public IEnumerable<Order> GetUserOrders(int userId)
        {
            List<Order> orders = new List<Order>();

            SqlParameter parameter = _dbService.CreateParameter("@UserId", SqlDbType.Int, userId);

            DataTable dataTable = _dbService.ExecuteReader("usp_GetUserOrders", parameter);

            foreach (DataRow row in dataTable.Rows)
            {
                orders.Add(new Order
                {
                    OrderId = Convert.ToInt32(row["OrderId"]),
                    UserId = Convert.ToInt32(row["UserId"]),
                    OrderDate = Convert.ToDateTime(row["OrderDate"]),
                    OrderStatus = row["OrderStatus"].ToString(),
                    OrderCode = row["OrderCode"].ToString(),
                    SubTotal = Convert.ToDecimal(row["SubTotal"]),
                    DiscountAmount = Convert.ToDecimal(row["DiscountAmount"]),
                    ShippingCost = Convert.ToDecimal(row["ShippingCost"]),
                    Total = Convert.ToDecimal(row["Total"]),
                    EstimatedDeliveryTime = row["EstimatedDeliveryTime"] == DBNull.Value ?
                        (DateTime?)null : Convert.ToDateTime(row["EstimatedDeliveryTime"])
                });
            }

            return orders;
        }

        private User MapRowToUser(DataRow row)
        {
            return new User
            {
                UserId = Convert.ToInt32(row["UserId"]),
                FirstName = row["FirstName"].ToString(),
                LastName = row["LastName"].ToString(),
                Email = row["Email"].ToString(),
                PasswordHash = row["PasswordHash"].ToString(),
                PhoneNumber = row["PhoneNumber"] == DBNull.Value ? null : row["PhoneNumber"].ToString(),
                DeliveryAddress = row["DeliveryAddress"] == DBNull.Value ? null : row["DeliveryAddress"].ToString(),
                Role = row["Role"].ToString(),
                RegistrationDate = Convert.ToDateTime(row["RegistrationDate"])
            };
        }

        private string HashPassword(string password)
        {
            using (SHA256 sha256 = SHA256.Create())
            {
                byte[] bytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(password));

                StringBuilder builder = new StringBuilder();
                for (int i = 0; i < bytes.Length; i++)
                {
                    builder.Append(bytes[i].ToString("x2"));
                }

                return builder.ToString();
            }
        }

        private bool VerifyPassword(string password, string storedHash)
        {
            string passwordHash = HashPassword(password);
            return passwordHash.Equals(storedHash, StringComparison.OrdinalIgnoreCase);
        }
    }
}