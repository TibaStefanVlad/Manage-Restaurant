using System.Windows;
using Restaurant.Models;
using Restaurant.Services.Repositories;
using Restaurant.Services;
using Restaurant.Views;

namespace Restaurant
{
    public partial class App : Application
    {
        protected override void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);
            EnsureAdminExists();
            EnsureSampleData();

            var loginView = new LoginView();
            loginView.Show();
        }

        private void EnsureAdminExists()
        {
            try
            {
                var dbService = new DatabaseConnectionService();
                var userRepository = new UserRepository(dbService);
                var userService = new UserService(userRepository);

                User adminUser = userService.GetUserByEmail("admin@restaurant.com");

                if (adminUser == null)
                {
                    User newAdmin = new User
                    {
                        FirstName = "System",
                        LastName = "Administrator",
                        Email = "admin@restaurant.com",
                        PasswordHash = "admin123",
                        PhoneNumber = "0000000000",
                        DeliveryAddress = "Restaurant HQ",
                        Role = "Admin",
                        RegistrationDate = DateTime.Now
                    };

                    userService.RegisterUser(newAdmin);
                    Console.WriteLine("Admin user created successfully.");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Failed to create admin user: {ex.Message}");
            }
        }

        private void EnsureSampleData()
        {
            var dbService = new DatabaseConnectionService();
            var categoryRepo = new CategoryRepository(dbService);
            var dishRepo = new DishRepository(dbService);
            var allergenRepo = new AllergenRepository(dbService);

            // Check if categories exist
            if (!categoryRepo.GetAll().Any())
            {
                // Add sample categories
                var breakfastCategoryId = categoryRepo.Add(new Category { Name = "Breakfast", Description = "Morning meals" });
                var mainCourseId = categoryRepo.Add(new Category { Name = "Main Course", Description = "Lunch and dinner" });
                var dessertsId = categoryRepo.Add(new Category { Name = "Desserts", Description = "Sweet treats" });
                var drinksId = categoryRepo.Add(new Category { Name = "Drinks", Description = "Beverages" });

                // Add sample allergens
                var glutenId = allergenRepo.Add(new Allergen { Name = "Gluten", Description = "Contains gluten" });
                var nutsId = allergenRepo.Add(new Allergen { Name = "Nuts", Description = "Contains nuts" });
                var dairyId = allergenRepo.Add(new Allergen { Name = "Dairy", Description = "Contains dairy" });

                // Add sample dishes
                var omeletteId = dishRepo.Add(new Dish
                {
                    Name = "Omelette",
                    Description = "Fluffy egg omelette with vegetables",
                    Price = 8.99m,
                    PortionSize = 250,
                    PortionUnit = "g",
                    TotalQuantity = 5000,
                    CategoryId = breakfastCategoryId,
                    IsAvailable = true
                });

                var pastaId = dishRepo.Add(new Dish
                {
                    Name = "Spaghetti Carbonara",
                    Description = "Classic Italian pasta with eggs, cheese and bacon",
                    Price = 12.99m,
                    PortionSize = 350,
                    PortionUnit = "g",
                    TotalQuantity = 7000,
                    CategoryId = mainCourseId,
                    IsAvailable = true
                });

                var tiramisu = dishRepo.Add(new Dish
                {
                    Name = "Tiramisu",
                    Description = "Italian coffee-flavored dessert",
                    Price = 6.99m,
                    PortionSize = 150,
                    PortionUnit = "g",
                    TotalQuantity = 3000,
                    CategoryId = dessertsId,
                    IsAvailable = true
                });

                var coffee = dishRepo.Add(new Dish
                {
                    Name = "Espresso",
                    Description = "Strong Italian coffee",
                    Price = 2.99m,
                    PortionSize = 50,
                    PortionUnit = "ml",
                    TotalQuantity = 10000,
                    CategoryId = drinksId,
                    IsAvailable = true
                });

                // Add allergens to dishes
                allergenRepo.AddAllergenToDish(pastaId, glutenId);
                allergenRepo.AddAllergenToDish(pastaId, dairyId);
                allergenRepo.AddAllergenToDish(tiramisu, dairyId);
                allergenRepo.AddAllergenToDish(tiramisu, glutenId);
            }
        }
    }
}