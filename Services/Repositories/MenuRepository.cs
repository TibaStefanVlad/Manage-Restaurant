using System;
using System.Collections.Generic;
using System.Data;
using Microsoft.Data.SqlClient; 
using Restaurant.Models;

namespace Restaurant.Services.Repositories
{
    public class MenuRepository : IRepository<Menu>
    {
        private readonly DatabaseConnectionService _dbService;

        public MenuRepository(DatabaseConnectionService dbService)
        {
            _dbService = dbService ?? throw new ArgumentNullException(nameof(dbService));
        }

        public IEnumerable<Menu> GetAll()
        {
            List<Menu> menus = new List<Menu>();

            DataTable dataTable = _dbService.ExecuteReader("usp_GetAllMenus");

            foreach (DataRow row in dataTable.Rows)
            {
                menus.Add(MapRowToMenu(row));
            }

            return menus;
        }

        public Menu GetById(int id)
        {
            SqlParameter parameter = _dbService.CreateParameter("@MenuId", SqlDbType.Int, id);

            DataTable dataTable = _dbService.ExecuteReader("usp_GetMenuById", parameter);

            if (dataTable.Rows.Count > 0)
            {
                return MapRowToMenu(dataTable.Rows[0]);
            }

            return null;
        }

        public int Add(Menu menu)
        {
            SqlParameter[] parameters =
            {
                _dbService.CreateParameter("@Name", SqlDbType.NVarChar, menu.Name),
                _dbService.CreateParameter("@Description", SqlDbType.NVarChar, menu.Description),
                _dbService.CreateParameter("@CategoryId", SqlDbType.Int, menu.CategoryId),
                _dbService.CreateParameter("@IsAvailable", SqlDbType.Bit, menu.IsAvailable),
                _dbService.CreateParameter("@MenuId", SqlDbType.Int, null)
            };

            parameters[4].Direction = ParameterDirection.Output;

            _dbService.ExecuteNonQuery("usp_AddMenu", parameters);

            return (int)parameters[4].Value;
        }

        public bool Update(Menu menu)
        {
            SqlParameter[] parameters =
            {
                _dbService.CreateParameter("@MenuId", SqlDbType.Int, menu.MenuId),
                _dbService.CreateParameter("@Name", SqlDbType.NVarChar, menu.Name),
                _dbService.CreateParameter("@Description", SqlDbType.NVarChar, menu.Description),
                _dbService.CreateParameter("@CategoryId", SqlDbType.Int, menu.CategoryId),
                _dbService.CreateParameter("@IsAvailable", SqlDbType.Bit, menu.IsAvailable)
            };

            int rowsAffected = _dbService.ExecuteNonQuery("usp_UpdateMenu", parameters);

            return rowsAffected > 0;
        }

        public bool Delete(int id)
        {
            SqlParameter parameter = _dbService.CreateParameter("@MenuId", SqlDbType.Int, id);

            int rowsAffected = _dbService.ExecuteNonQuery("usp_DeleteMenu", parameter);

            return rowsAffected > 0;
        }

        private Menu MapRowToMenu(DataRow row)
        {
            return new Menu
            {
                MenuId = Convert.ToInt32(row["MenuId"]),
                Name = row["Name"].ToString(),
                Description = row["Description"] == DBNull.Value ? null : row["Description"].ToString(),
                CategoryId = Convert.ToInt32(row["CategoryId"]),
                Price = Convert.ToDecimal(row["Price"]),
                IsAvailable = Convert.ToBoolean(row["IsAvailable"])
            };
        }

        public IEnumerable<MenuDish> GetMenuDishes(int menuId)
        {
            List<MenuDish> menuDishes = new List<MenuDish>();

            SqlParameter parameter = _dbService.CreateParameter("@MenuId", SqlDbType.Int, menuId);

            DataTable dataTable = _dbService.ExecuteReader("usp_GetMenuDishes", parameter);

            foreach (DataRow row in dataTable.Rows)
            {
                menuDishes.Add(new MenuDish
                {
                    MenuId = Convert.ToInt32(row["MenuId"]),
                    DishId = Convert.ToInt32(row["DishId"]),
                    PortionSize = Convert.ToDouble(row["PortionSize"])
                });
            }

            return menuDishes;
        }

        public bool AddDishToMenu(int menuId, int dishId, double portionSize)
        {
            SqlParameter[] parameters =
            {
                _dbService.CreateParameter("@MenuId", SqlDbType.Int, menuId),
                _dbService.CreateParameter("@DishId", SqlDbType.Int, dishId),
                _dbService.CreateParameter("@PortionSize", SqlDbType.Float, portionSize)
            };

            int rowsAffected = _dbService.ExecuteNonQuery("usp_AddDishToMenu", parameters);

            return rowsAffected > 0;
        }

        public bool RemoveDishFromMenu(int menuId, int dishId)
        {
            SqlParameter[] parameters =
            {
                _dbService.CreateParameter("@MenuId", SqlDbType.Int, menuId),
                _dbService.CreateParameter("@DishId", SqlDbType.Int, dishId)
            };

            int rowsAffected = _dbService.ExecuteNonQuery("usp_RemoveDishFromMenu", parameters);

            return rowsAffected > 0;
        }
    }
}