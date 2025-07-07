using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Restaurant.Models;
using Restaurant.Services.Repositories;

namespace Restaurant.Services
{
    public class MenuService
    {
        private readonly MenuRepository _menuRepository;
        private readonly DishRepository _dishRepository;
        private readonly CategoryRepository _categoryRepository;
        private readonly SettingRepository _settingRepository;
        private readonly AllergenRepository _allergenRepository;

        public MenuService(
            MenuRepository menuRepository,
            DishRepository dishRepository,
            CategoryRepository categoryRepository,
            SettingRepository settingRepository,
            AllergenRepository allergenRepository)
        {
            _menuRepository = menuRepository ?? throw new ArgumentNullException(nameof(menuRepository));
            _dishRepository = dishRepository ?? throw new ArgumentNullException(nameof(dishRepository));
            _categoryRepository = categoryRepository ?? throw new ArgumentNullException(nameof(categoryRepository));
            _settingRepository = settingRepository ?? throw new ArgumentNullException(nameof(settingRepository));
            _allergenRepository = allergenRepository ?? throw new ArgumentNullException(nameof(allergenRepository));

        }

        public IEnumerable<Menu> GetAllMenus()
        {
            return _menuRepository.GetAll();
        }

        public Menu GetMenuById(int id)
        {
            return _menuRepository.GetById(id);
        }

        public IEnumerable<MenuDish> GetMenuDishes(int menuId)
        {
            return _menuRepository.GetMenuDishes(menuId);
        }

        public IEnumerable<Dish> GetDishesByCategory(int categoryId)
        {
            return _dishRepository.GetByCategory(categoryId);
        }

        public IEnumerable<Category> GetAllCategories()
        {
            return _categoryRepository.GetAll();
        }

        public decimal CalculateMenuPrice(IEnumerable<MenuDish> menuDishes)
        {
            decimal totalPrice = 0;

            foreach (var menuDish in menuDishes)
            {
                var dish = _dishRepository.GetById(menuDish.DishId);
                if (dish != null)
                {
                    totalPrice += dish.Price;
                }
            }

            decimal discountPercentage = _settingRepository.GetDecimalValue("MenuDiscountPercentage", 10);
            decimal discountFactor = 1 - (discountPercentage / 100);

            return Math.Round(totalPrice * discountFactor, 2);
        }

        public int CreateMenu(Menu menu, IEnumerable<MenuDish> menuDishes)
        {
            menu.Price = CalculateMenuPrice(menuDishes);

            int menuId = _menuRepository.Add(menu);

            foreach (var menuDish in menuDishes)
            {
                menuDish.MenuId = menuId;
                _menuRepository.AddDishToMenu(menuId, menuDish.DishId, menuDish.PortionSize);
            }

            return menuId;
        }

        public bool UpdateMenu(Menu menu, IEnumerable<MenuDish> menuDishes)
        {
            menu.Price = CalculateMenuPrice(menuDishes);

            bool result = _menuRepository.Update(menu);

            if (result)
            {
                var existingDishes = _menuRepository.GetMenuDishes(menu.MenuId).ToList();

                foreach (var existingDish in existingDishes)
                {
                    if (!menuDishes.Any(d => d.DishId == existingDish.DishId))
                    {
                        _menuRepository.RemoveDishFromMenu(menu.MenuId, existingDish.DishId);
                    }
                }

                foreach (var menuDish in menuDishes)
                {
                    var existingDish = existingDishes.FirstOrDefault(d => d.DishId == menuDish.DishId);

                    if (existingDish == null)
                    {
                        _menuRepository.AddDishToMenu(menu.MenuId, menuDish.DishId, menuDish.PortionSize);
                    }
                    else if (Math.Abs(existingDish.PortionSize - menuDish.PortionSize) > 0.001)
                    {
                        _menuRepository.RemoveDishFromMenu(menu.MenuId, menuDish.DishId);
                        _menuRepository.AddDishToMenu(menu.MenuId, menuDish.DishId, menuDish.PortionSize);
                    }
                }
            }

            return result;
        }

        public bool DeleteMenu(int menuId)
        {
            return _menuRepository.Delete(menuId);
        }

        public IEnumerable<object> GetRestaurantMenu()
        {
            var categories = _categoryRepository.GetAll().ToList();
            var dishes = _dishRepository.GetAll().ToList();
            var menus = _menuRepository.GetAll().ToList();

            var result = new List<object>();

            foreach (var category in categories)
            {
                var categoryDishes = dishes.Where(d => d.CategoryId == category.CategoryId).ToList();

                foreach (var dish in categoryDishes)
                {
                    try
                    {
                        dish.Photos = new HashSet<DishPhoto>(_dishRepository.GetDishPhotos(dish.DishId));

                        dish.DishAllergens = new HashSet<DishAllergen>(_dishRepository.GetDishAllergens(dish.DishId));

                        foreach (var allergen in dish.DishAllergens.ToList())
                        {
                            var allergenDetails = _allergenRepository.GetById(allergen.AllergenId);
                            if (allergenDetails != null)
                            {
                                allergen.Allergen = allergenDetails;
                            }
                            else
                            {
                                allergen.Allergen = new Allergen
                                {
                                    AllergenId = allergen.AllergenId,
                                    Name = "Unknown Allergen"
                                };
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"Error loading dish details: {ex.Message}");
                    }
                }

                var categoryMenus = menus.Where(m => m.CategoryId == category.CategoryId).ToList();

                foreach (var menu in categoryMenus)
                {
                    menu.MenuDishes = new HashSet<MenuDish>(_menuRepository.GetMenuDishes(menu.MenuId));

                    foreach (var menuDish in menu.MenuDishes)
                    {
                        menuDish.Dish = _dishRepository.GetById(menuDish.DishId);
                    }
                }

                if (categoryDishes.Any() || categoryMenus.Any())
                {
                    result.Add(new
                    {
                        Category = category,
                        Dishes = categoryDishes,
                        Menus = categoryMenus
                    });
                }
            }

            return result;
        }

        public IEnumerable<Dish> SearchDishesByName(string keyword)
        {
            var dishes = _dishRepository.GetAll()
                .Where(d => d.Name.Contains(keyword, StringComparison.OrdinalIgnoreCase))
                .ToList();

            foreach (var dish in dishes)
            {
                dish.DishAllergens = new HashSet<DishAllergen>(_dishRepository.GetDishAllergens(dish.DishId));

                foreach (var allergen in dish.DishAllergens.ToList())
                {
                    var allergenDetails = _allergenRepository.GetById(allergen.AllergenId);
                    if (allergenDetails != null)
                    {
                        allergen.Allergen = allergenDetails;
                    }
                }

                dish.Photos = new HashSet<DishPhoto>(_dishRepository.GetDishPhotos(dish.DishId));
            }

            return dishes;
        }

        public IEnumerable<Menu> SearchMenusByName(string keyword)
        {
            var menus = _menuRepository.GetAll()
                .Where(m => m.Name.Contains(keyword, StringComparison.OrdinalIgnoreCase))
                .ToList();

            foreach (var menu in menus)
            {
                menu.MenuDishes = new HashSet<MenuDish>(_menuRepository.GetMenuDishes(menu.MenuId));

                foreach (var menuDish in menu.MenuDishes)
                {
                    menuDish.Dish = _dishRepository.GetById(menuDish.DishId);
                    if (menuDish.Dish != null)
                    {
                        menuDish.Dish.DishAllergens = new HashSet<DishAllergen>(_dishRepository.GetDishAllergens(menuDish.DishId));

                        foreach (var allergen in menuDish.Dish.DishAllergens.ToList())
                        {
                            var allergenDetails = _allergenRepository.GetById(allergen.AllergenId);
                            if (allergenDetails != null)
                            {
                                allergen.Allergen = allergenDetails;
                            }
                        }
                    }
                }
            }

            return menus;
        }

        public IEnumerable<Dish> SearchDishesByAllergen(string allergenName, bool exclude)
        {
            var allDishes = _dishRepository.GetAll().ToList();

            foreach (var dish in allDishes)
            {
                dish.DishAllergens = new HashSet<DishAllergen>(_dishRepository.GetDishAllergens(dish.DishId));

                foreach (var allergen in dish.DishAllergens.ToList())
                {
                    var allergenDetails = _allergenRepository.GetById(allergen.AllergenId);
                    if (allergenDetails != null)
                    {
                        allergen.Allergen = allergenDetails;
                    }
                }
            }

            if (exclude)
            {
                return allDishes.Where(d =>
                    !d.DishAllergens.Any(a =>
                        a.Allergen?.Name.Contains(allergenName, StringComparison.OrdinalIgnoreCase) == true
                    )
                ).ToList();
            }
            else
            {
                return allDishes.Where(d =>
                    d.DishAllergens.Any(a =>
                        a.Allergen?.Name.Contains(allergenName, StringComparison.OrdinalIgnoreCase) == true
                    )
                ).ToList();
            }
        }

        public IEnumerable<Menu> SearchMenusByAllergen(string allergenName, bool exclude)
        {
            var allMenus = _menuRepository.GetAll().ToList();

            foreach (var menu in allMenus)
            {
                menu.MenuDishes = new HashSet<MenuDish>(_menuRepository.GetMenuDishes(menu.MenuId));

                foreach (var menuDish in menu.MenuDishes)
                {
                    menuDish.Dish = _dishRepository.GetById(menuDish.DishId);
                    if (menuDish.Dish != null)
                    {
                        menuDish.Dish.DishAllergens = new HashSet<DishAllergen>(_dishRepository.GetDishAllergens(menuDish.DishId));

                        foreach (var allergen in menuDish.Dish.DishAllergens.ToList())
                        {
                            var allergenDetails = _allergenRepository.GetById(allergen.AllergenId);
                            if (allergenDetails != null)
                            {
                                allergen.Allergen = allergenDetails;
                            }
                        }
                    }
                }
            }

            if (exclude)
            {
                return allMenus.Where(m =>
                    !m.MenuDishes.Any(md =>
                        md.Dish?.DishAllergens.Any(a =>
                            a.Allergen?.Name.Contains(allergenName, StringComparison.OrdinalIgnoreCase) == true
                        ) == true
                    )
                ).ToList();
            }
            else
            {
                return allMenus.Where(m =>
                    m.MenuDishes.Any(md =>
                        md.Dish?.DishAllergens.Any(a =>
                            a.Allergen?.Name.Contains(allergenName, StringComparison.OrdinalIgnoreCase) == true
                        ) == true
                    )
                ).ToList();
            }
        }
    }
}