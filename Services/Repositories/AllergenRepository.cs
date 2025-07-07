using System;
using System.Collections.Generic;
using System.Data;
using Microsoft.Data.SqlClient;
using Restaurant.Models;

namespace Restaurant.Services.Repositories
{
    public class AllergenRepository : IRepository<Allergen>
    {
        private readonly DatabaseConnectionService _dbService;

        public AllergenRepository(DatabaseConnectionService dbService)
        {
            _dbService = dbService ?? throw new ArgumentNullException(nameof(dbService));
        }

        public IEnumerable<Allergen> GetAll()
        {
            List<Allergen> allergens = new List<Allergen>();

            DataTable dataTable = _dbService.ExecuteReader("usp_GetAllAllergens");

            foreach (DataRow row in dataTable.Rows)
            {
                allergens.Add(MapRowToAllergen(row));
            }

            return allergens;
        }

        public Allergen GetById(int id)
        {
            SqlParameter parameter = _dbService.CreateParameter("@AllergenId", SqlDbType.Int, id);

            DataTable dataTable = _dbService.ExecuteReader("usp_GetAllergenById", parameter);

            if (dataTable.Rows.Count > 0)
            {
                return MapRowToAllergen(dataTable.Rows[0]);
            }

            return null;
        }

        public int Add(Allergen allergen)
        {
            SqlParameter[] parameters =
            {
                _dbService.CreateParameter("@Name", SqlDbType.NVarChar, allergen.Name),
                _dbService.CreateParameter("@Description", SqlDbType.NVarChar, allergen.Description),
                _dbService.CreateParameter("@AllergenId", SqlDbType.Int, null)
            };

            parameters[2].Direction = ParameterDirection.Output;

            _dbService.ExecuteNonQuery("usp_AddAllergen", parameters);

            return (int)parameters[2].Value;
        }

        public bool Update(Allergen allergen)
        {
            SqlParameter[] parameters =
            {
                _dbService.CreateParameter("@AllergenId", SqlDbType.Int, allergen.AllergenId),
                _dbService.CreateParameter("@Name", SqlDbType.NVarChar, allergen.Name),
                _dbService.CreateParameter("@Description", SqlDbType.NVarChar, allergen.Description)
            };

            int rowsAffected = _dbService.ExecuteNonQuery("usp_UpdateAllergen", parameters);

            return rowsAffected > 0;
        }

        public bool Delete(int id)
        {
            SqlParameter parameter = _dbService.CreateParameter("@AllergenId", SqlDbType.Int, id);

            int rowsAffected = _dbService.ExecuteNonQuery("usp_DeleteAllergen", parameter);

            return rowsAffected > 0;
        }

        public IEnumerable<Dish> GetDishesByAllergen(int allergenId)
        {
            List<Dish> dishes = new List<Dish>();

            SqlParameter parameter = _dbService.CreateParameter("@AllergenId", SqlDbType.Int, allergenId);

            DataTable dataTable = _dbService.ExecuteReader("usp_GetDishesByAllergen", parameter);

            foreach (DataRow row in dataTable.Rows)
            {
                dishes.Add(new Dish
                {
                    DishId = Convert.ToInt32(row["DishId"]),
                    Name = row["Name"].ToString(),
                    CategoryId = Convert.ToInt32(row["CategoryId"]),
                    Price = Convert.ToDecimal(row["Price"]),
                    PortionSize = Convert.ToDouble(row["PortionSize"]),
                    PortionUnit = row["PortionUnit"].ToString(),
                    TotalQuantity = Convert.ToDouble(row["TotalQuantity"]),
                    IsAvailable = Convert.ToBoolean(row["IsAvailable"]),
                    Description = row["Description"] == DBNull.Value ? null : row["Description"].ToString()
                });
            }

            return dishes;
        }

        public bool AddAllergenToDish(int dishId, int allergenId)
        {
            SqlParameter[] parameters =
            {
                _dbService.CreateParameter("@DishId", SqlDbType.Int, dishId),
                _dbService.CreateParameter("@AllergenId", SqlDbType.Int, allergenId)
            };

            int rowsAffected = _dbService.ExecuteNonQuery("usp_AddAllergenToDish", parameters);

            return rowsAffected > 0;
        }

        public bool RemoveAllergenFromDish(int dishId, int allergenId)
        {
            SqlParameter[] parameters =
            {
                _dbService.CreateParameter("@DishId", SqlDbType.Int, dishId),
                _dbService.CreateParameter("@AllergenId", SqlDbType.Int, allergenId)
            };

            int rowsAffected = _dbService.ExecuteNonQuery("usp_RemoveAllergenFromDish", parameters);

            return rowsAffected > 0;
        }

        public IEnumerable<Dish> GetDishesWithoutAllergen(int allergenId)
        {
            List<Dish> dishes = new List<Dish>();

            SqlParameter parameter = _dbService.CreateParameter("@AllergenId", SqlDbType.Int, allergenId);

            DataTable dataTable = _dbService.ExecuteReader("usp_GetDishesWithoutAllergen", parameter);

            foreach (DataRow row in dataTable.Rows)
            {
                dishes.Add(new Dish
                {
                    DishId = Convert.ToInt32(row["DishId"]),
                    Name = row["Name"].ToString(),
                    CategoryId = Convert.ToInt32(row["CategoryId"]),
                    Price = Convert.ToDecimal(row["Price"]),
                    PortionSize = Convert.ToDouble(row["PortionSize"]),
                    PortionUnit = row["PortionUnit"].ToString(),
                    TotalQuantity = Convert.ToDouble(row["TotalQuantity"]),
                    IsAvailable = Convert.ToBoolean(row["IsAvailable"]),
                    Description = row["Description"] == DBNull.Value ? null : row["Description"].ToString()
                });
            }

            return dishes;
        }

        private Allergen MapRowToAllergen(DataRow row)
        {
            return new Allergen
            {
                AllergenId = Convert.ToInt32(row["AllergenId"]),
                Name = row["Name"].ToString(),
                Description = row["Description"] == DBNull.Value ? null : row["Description"].ToString()
            };
        }
    }
}