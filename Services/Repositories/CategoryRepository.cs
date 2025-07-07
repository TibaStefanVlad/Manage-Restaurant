using System;
using System.Collections.Generic;
using System.Data;
using Microsoft.Data.SqlClient;
using Restaurant.Models;

namespace Restaurant.Services.Repositories
{
    public class CategoryRepository : IRepository<Category>
    {
        private readonly DatabaseConnectionService _dbService;

        public CategoryRepository(DatabaseConnectionService dbService)
        {
            _dbService = dbService ?? throw new ArgumentNullException(nameof(dbService));
        }

        public IEnumerable<Category> GetAll()
        {
            List<Category> categories = new List<Category>();

            DataTable dataTable = _dbService.ExecuteReader("usp_GetAllCategories");

            foreach (DataRow row in dataTable.Rows)
            {
                categories.Add(MapRowToCategory(row));
            }

            return categories;
        }

        public Category GetById(int id)
        {
            SqlParameter parameter = _dbService.CreateParameter("@CategoryId", SqlDbType.Int, id);

            DataTable dataTable = _dbService.ExecuteReader("usp_GetCategoryById", parameter);

            if (dataTable.Rows.Count > 0)
            {
                return MapRowToCategory(dataTable.Rows[0]);
            }

            return null;
        }

        public int Add(Category category)
        {
            SqlParameter[] parameters =
            {
                _dbService.CreateParameter("@Name", SqlDbType.NVarChar, category.Name),
                _dbService.CreateParameter("@Description", SqlDbType.NVarChar, category.Description),
                _dbService.CreateParameter("@CategoryId", SqlDbType.Int, null)
            };

            parameters[2].Direction = ParameterDirection.Output;

            _dbService.ExecuteNonQuery("usp_AddCategory", parameters);

            return (int)parameters[2].Value;
        }

        public bool Update(Category category)
        {
            SqlParameter[] parameters =
            {
                _dbService.CreateParameter("@CategoryId", SqlDbType.Int, category.CategoryId),
                _dbService.CreateParameter("@Name", SqlDbType.NVarChar, category.Name),
                _dbService.CreateParameter("@Description", SqlDbType.NVarChar, category.Description)
            };

            int rowsAffected = _dbService.ExecuteNonQuery("usp_UpdateCategory", parameters);

            return rowsAffected > 0;
        }

        public bool Delete(int id)
        {
            SqlParameter parameter = _dbService.CreateParameter("@CategoryId", SqlDbType.Int, id);

            int rowsAffected = _dbService.ExecuteNonQuery("usp_DeleteCategory", parameter);

            return rowsAffected > 0;
        }

        private Category MapRowToCategory(DataRow row)
        {
            return new Category
            {
                CategoryId = Convert.ToInt32(row["CategoryId"]),
                Name = row["Name"].ToString(),
                Description = row["Description"] == DBNull.Value ? null : row["Description"].ToString()
            };
        }
    }
}