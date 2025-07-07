using System;
using System.Collections.Generic;
using System.Data;
using Microsoft.Data.SqlClient;
using Restaurant.Models;

namespace Restaurant.Services.Repositories
{
    public class DishRepository : IRepository<Dish>
    {
        private readonly DatabaseConnectionService _dbService;

        public DishRepository(DatabaseConnectionService dbService)
        {
            _dbService = dbService ?? throw new ArgumentNullException(nameof(dbService));
        }

        public IEnumerable<Dish> GetAll()
        {
            List<Dish> dishes = new List<Dish>();

            DataTable dataTable = _dbService.ExecuteReader("usp_GetAllDishes");

            foreach (DataRow row in dataTable.Rows)
            {
                dishes.Add(MapRowToDish(row));
            }

            return dishes;
        }

        public Dish GetById(int id)
        {
            SqlParameter parameter = _dbService.CreateParameter("@DishId", SqlDbType.Int, id);

            DataTable dataTable = _dbService.ExecuteReader("usp_GetDishById", parameter);

            if (dataTable.Rows.Count > 0)
            {
                return MapRowToDish(dataTable.Rows[0]);
            }

            return null;
        }

        public int Add(Dish dish)
        {
            SqlParameter[] parameters =
            {
                _dbService.CreateParameter("@Name", SqlDbType.NVarChar, dish.Name),
                _dbService.CreateParameter("@Price", SqlDbType.Decimal, dish.Price),
                _dbService.CreateParameter("@PortionSize", SqlDbType.Float, dish.PortionSize),
                _dbService.CreateParameter("@PortionUnit", SqlDbType.NVarChar, dish.PortionUnit),
                _dbService.CreateParameter("@TotalQuantity", SqlDbType.Float, dish.TotalQuantity),
                _dbService.CreateParameter("@CategoryId", SqlDbType.Int, dish.CategoryId),
                _dbService.CreateParameter("@IsAvailable", SqlDbType.Bit, dish.IsAvailable),
                _dbService.CreateParameter("@Description", SqlDbType.NVarChar, dish.Description),
                _dbService.CreateParameter("@DishId", SqlDbType.Int, null)
            };

            parameters[8].Direction = ParameterDirection.Output;

            _dbService.ExecuteNonQuery("usp_AddDish", parameters);

            return (int)parameters[8].Value;
        }

        public bool Update(Dish dish)
        {
            SqlParameter[] parameters =
            {
                _dbService.CreateParameter("@DishId", SqlDbType.Int, dish.DishId),
                _dbService.CreateParameter("@Name", SqlDbType.NVarChar, dish.Name),
                _dbService.CreateParameter("@Price", SqlDbType.Decimal, dish.Price),
                _dbService.CreateParameter("@PortionSize", SqlDbType.Float, dish.PortionSize),
                _dbService.CreateParameter("@PortionUnit", SqlDbType.NVarChar, dish.PortionUnit),
                _dbService.CreateParameter("@TotalQuantity", SqlDbType.Float, dish.TotalQuantity),
                _dbService.CreateParameter("@CategoryId", SqlDbType.Int, dish.CategoryId),
                _dbService.CreateParameter("@IsAvailable", SqlDbType.Bit, dish.IsAvailable),
                _dbService.CreateParameter("@Description", SqlDbType.NVarChar, dish.Description)
            };

            int rowsAffected = _dbService.ExecuteNonQuery("usp_UpdateDish", parameters);

            return rowsAffected > 0;
        }

        public bool Delete(int id)
        {
            SqlParameter parameter = _dbService.CreateParameter("@DishId", SqlDbType.Int, id);

            int rowsAffected = _dbService.ExecuteNonQuery("usp_DeleteDish", parameter);

            return rowsAffected > 0;
        }

        private Dish MapRowToDish(DataRow row)
        {
            return new Dish
            {
                DishId = Convert.ToInt32(row["DishId"]),
                Name = row["Name"].ToString(),
                Price = Convert.ToDecimal(row["Price"]),
                PortionSize = Convert.ToDouble(row["PortionSize"]),
                PortionUnit = row["PortionUnit"].ToString(),
                TotalQuantity = Convert.ToDouble(row["TotalQuantity"]),
                CategoryId = Convert.ToInt32(row["CategoryId"]),
                IsAvailable = Convert.ToBoolean(row["IsAvailable"]),
                Description = row["Description"] == DBNull.Value ? null : row["Description"].ToString()
            };
        }

        public IEnumerable<Dish> GetByCategory(int categoryId)
        {
            List<Dish> dishes = new List<Dish>();

            SqlParameter parameter = _dbService.CreateParameter("@CategoryId", SqlDbType.Int, categoryId);

            DataTable dataTable = _dbService.ExecuteReader("usp_GetDishesByCategory", parameter);

            foreach (DataRow row in dataTable.Rows)
            {
                dishes.Add(MapRowToDish(row));
            }

            return dishes;
        }

        public IEnumerable<DishAllergen> GetDishAllergens(int dishId)
        {
            List<DishAllergen> dishAllergens = new List<DishAllergen>();

            SqlParameter parameter = _dbService.CreateParameter("@DishId", SqlDbType.Int, dishId);

            DataTable dataTable = _dbService.ExecuteReader("usp_GetDishAllergens", parameter);

            foreach (DataRow row in dataTable.Rows)
            {
                dishAllergens.Add(new DishAllergen
                {
                    DishId = Convert.ToInt32(row["DishId"]),
                    AllergenId = Convert.ToInt32(row["AllergenId"])
                });
            }

            return dishAllergens;
        }

        public IEnumerable<DishPhoto> GetDishPhotos(int dishId)
        {
            List<DishPhoto> dishPhotos = new List<DishPhoto>();

            SqlParameter parameter = _dbService.CreateParameter("@DishId", SqlDbType.Int, dishId);

            DataTable dataTable = _dbService.ExecuteReader("usp_GetDishPhotos", parameter);

            foreach (DataRow row in dataTable.Rows)
            {
                dishPhotos.Add(new DishPhoto
                {
                    PhotoId = Convert.ToInt32(row["PhotoId"]),
                    DishId = Convert.ToInt32(row["DishId"]),
                    PhotoData = (byte[])row["PhotoData"],
                    Description = row["Description"] == DBNull.Value ? null : row["Description"].ToString(),
                    IsPrimary = Convert.ToBoolean(row["IsPrimary"])
                });
            }

            return dishPhotos;
        }

        public IEnumerable<DishAllergen> GetDishAllergensWithDetails(int dishId)
        {
            List<DishAllergen> dishAllergens = new List<DishAllergen>();

            SqlParameter parameter = _dbService.CreateParameter("@DishId", SqlDbType.Int, dishId);

            DataTable dataTable = _dbService.ExecuteReader("usp_GetDishAllergensWithDetails", parameter);

            foreach (DataRow row in dataTable.Rows)
            {
                dishAllergens.Add(new DishAllergen
                {
                    DishId = Convert.ToInt32(row["DishId"]),
                    AllergenId = Convert.ToInt32(row["AllergenId"]),
                    Allergen = new Allergen
                    {
                        AllergenId = Convert.ToInt32(row["AllergenId"]),
                        Name = row["AllergenName"].ToString(),
                        Description = row["AllergenDescription"] == DBNull.Value ? null : row["AllergenDescription"].ToString()
                    }
                });
            }

            return dishAllergens;
        }
    }
}