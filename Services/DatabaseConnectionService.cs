using System;
using System.Configuration;
using System.Data;
using Microsoft.Data.SqlClient; 

namespace Restaurant.Services
{
    public class DatabaseConnectionService
    {
        private readonly string _connectionString;

        public DatabaseConnectionService()
        {
            _connectionString = ConfigurationManager.ConnectionStrings["RestaurantDB"].ConnectionString;
        }

        public SqlConnection CreateConnection()
        {
            return new SqlConnection(_connectionString);
        }

        public SqlCommand CreateCommand(string storedProcedureName, SqlConnection connection)
        {
            SqlCommand command = new SqlCommand(storedProcedureName, connection)
            {
                CommandType = CommandType.StoredProcedure
            };
            return command;
        }

        public SqlParameter CreateParameter(string name, SqlDbType type, object value)
        {
            return new SqlParameter
            {
                ParameterName = name,
                SqlDbType = type,
                Value = value ?? DBNull.Value
            };
        }

        public DataTable ExecuteReader(string storedProcedureName, params SqlParameter[] parameters)
        {
            DataTable dataTable = new DataTable();

            using (SqlConnection connection = CreateConnection())
            {
                using (SqlCommand command = CreateCommand(storedProcedureName, connection))
                {
                    if (parameters != null)
                    {
                        command.Parameters.AddRange(parameters);
                    }

                    try
                    {
                        connection.Open();
                        using (SqlDataReader reader = command.ExecuteReader())
                        {
                            dataTable.Load(reader);
                        }
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"Database error: {ex.Message}");
                        throw;
                    }
                }
            }

            return dataTable;
        }

        public object ExecuteScalar(string storedProcedureName, params SqlParameter[] parameters)
        {
            using (SqlConnection connection = CreateConnection())
            {
                using (SqlCommand command = CreateCommand(storedProcedureName, connection))
                {
                    if (parameters != null)
                    {
                        command.Parameters.AddRange(parameters);
                    }

                    try
                    {
                        connection.Open();
                        return command.ExecuteScalar();
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"Database error: {ex.Message}");
                        throw;
                    }
                }
            }
        }

        public int ExecuteNonQuery(string storedProcedureName, params SqlParameter[] parameters)
        {
            using (SqlConnection connection = CreateConnection())
            {
                using (SqlCommand command = CreateCommand(storedProcedureName, connection))
                {
                    if (parameters != null)
                    {
                        command.Parameters.AddRange(parameters);
                    }

                    try
                    {
                        connection.Open();
                        return command.ExecuteNonQuery();
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"Database error: {ex.Message}");
                        throw;
                    }
                }
            }
        }
    }
}