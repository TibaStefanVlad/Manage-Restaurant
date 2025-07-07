using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows;
using System.Windows.Input;
using Restaurant.Commands;
using Restaurant.Models;
using Restaurant.Services;
using Restaurant.Services.Repositories;

namespace Restaurant.ViewModels
{
    public class MenuManagementViewModel : ViewModelBase
    {
        private readonly MenuService _menuService;
        private readonly MenuRepository _menuRepository;
        private readonly DishRepository _dishRepository;
        private readonly CategoryRepository _categoryRepository;

        // Observable collections for binding
        private ObservableCollection<Menu> _menus;
        private ObservableCollection<Category> _categories;
        private ObservableCollection<Dish> _availableDishes;
        private ObservableCollection<MenuDish> _selectedMenuDishes;

        // Selected items
        private Menu _selectedMenu;
        private Category _selectedCategory;
        private Dish _selectedDish;
        private MenuDish _selectedMenuDish;
        private double _selectedDishPortionSize;

        // New menu properties
        private string _menuName;
        private string _menuDescription;
        private int _selectedCategoryId;
        private bool _isMenuAvailable;

        // Editing state
        private bool _isEditing;
        private bool _isNewMenu;
        private decimal _calculatedPrice;

        #region Properties

        public ObservableCollection<Menu> Menus
        {
            get => _menus;
            set
            {
                _menus = value;
                OnPropertyChanged(nameof(Menus));
            }
        }

        public ObservableCollection<Category> Categories
        {
            get => _categories;
            set
            {
                _categories = value;
                OnPropertyChanged(nameof(Categories));
            }
        }

        public ObservableCollection<Dish> AvailableDishes
        {
            get => _availableDishes;
            set
            {
                _availableDishes = value;
                OnPropertyChanged(nameof(AvailableDishes));
            }
        }

        public ObservableCollection<MenuDish> SelectedMenuDishes
        {
            get => _selectedMenuDishes;
            set
            {
                _selectedMenuDishes = value;
                OnPropertyChanged(nameof(SelectedMenuDishes));
                CalculateMenuPrice();
            }
        }

        public Menu SelectedMenu
        {
            get => _selectedMenu;
            set
            {
                _selectedMenu = value;
                OnPropertyChanged(nameof(SelectedMenu));

                if (value != null)
                {
                    LoadMenuDetails();
                }
                else
                {
                    SelectedMenuDishes?.Clear();
                }

                UpdateCommandStates();
            }
        }

        public Category SelectedCategory
        {
            get => _selectedCategory;
            set
            {
                _selectedCategory = value;
                OnPropertyChanged(nameof(SelectedCategory));

                if (value != null)
                {
                    LoadDishesByCategory(value.CategoryId);
                    SelectedCategoryId = value.CategoryId;
                }
            }
        }

        public Dish SelectedDish
        {
            get => _selectedDish;
            set
            {
                _selectedDish = value;
                OnPropertyChanged(nameof(SelectedDish));

                if (value != null)
                {
                    // Default the portion size to the dish's standard portion
                    SelectedDishPortionSize = value.PortionSize;
                }

                UpdateCommandStates();
            }
        }

        public MenuDish SelectedMenuDish
        {
            get => _selectedMenuDish;
            set
            {
                _selectedMenuDish = value;
                OnPropertyChanged(nameof(SelectedMenuDish));
                UpdateCommandStates();
            }
        }

        public double SelectedDishPortionSize
        {
            get => _selectedDishPortionSize;
            set
            {
                _selectedDishPortionSize = value;
                OnPropertyChanged(nameof(SelectedDishPortionSize));
            }
        }

        public string MenuName
        {
            get => _menuName;
            set
            {
                _menuName = value;
                OnPropertyChanged(nameof(MenuName));
                UpdateCommandStates();
            }
        }

        public string MenuDescription
        {
            get => _menuDescription;
            set
            {
                _menuDescription = value;
                OnPropertyChanged(nameof(MenuDescription));
            }
        }

        public int SelectedCategoryId
        {
            get => _selectedCategoryId;
            set
            {
                _selectedCategoryId = value;
                OnPropertyChanged(nameof(SelectedCategoryId));
                UpdateCommandStates();
            }
        }

        public bool IsMenuAvailable
        {
            get => _isMenuAvailable;
            set
            {
                _isMenuAvailable = value;
                OnPropertyChanged(nameof(IsMenuAvailable));
            }
        }

        public bool IsEditing
        {
            get => _isEditing;
            set
            {
                _isEditing = value;
                OnPropertyChanged(nameof(IsEditing));
                OnPropertyChanged(nameof(IsNotEditing));
            }
        }

        public bool IsNotEditing => !IsEditing;

        public bool IsNewMenu
        {
            get => _isNewMenu;
            set
            {
                _isNewMenu = value;
                OnPropertyChanged(nameof(IsNewMenu));
            }
        }

        public decimal CalculatedPrice
        {
            get => _calculatedPrice;
            set
            {
                _calculatedPrice = value;
                OnPropertyChanged(nameof(CalculatedPrice));
            }
        }

        #endregion

        #region Commands

        public ICommand LoadMenusCommand { get; private set; }
        public ICommand NewMenuCommand { get; private set; }
        public ICommand EditMenuCommand { get; private set; }
        public ICommand DeleteMenuCommand { get; private set; }
        public ICommand SaveMenuCommand { get; private set; }
        public ICommand CancelEditCommand { get; private set; }
        public ICommand AddDishToMenuCommand { get; private set; }
        public ICommand RemoveDishFromMenuCommand { get; private set; }

        #endregion

        // Constructor
        public MenuManagementViewModel(
            MenuService menuService,
            MenuRepository menuRepository,
            DishRepository dishRepository,
            CategoryRepository categoryRepository)
        {
            _menuService = menuService ?? throw new ArgumentNullException(nameof(menuService));
            _menuRepository = menuRepository ?? throw new ArgumentNullException(nameof(menuRepository));
            _dishRepository = dishRepository ?? throw new ArgumentNullException(nameof(dishRepository));
            _categoryRepository = categoryRepository ?? throw new ArgumentNullException(nameof(categoryRepository));

            // Initialize collections
            Menus = new ObservableCollection<Menu>();
            Categories = new ObservableCollection<Category>();
            AvailableDishes = new ObservableCollection<Dish>();
            SelectedMenuDishes = new ObservableCollection<MenuDish>();

            // Initialize commands
            LoadMenusCommand = new RelayCommand(ExecuteLoadMenus);
            NewMenuCommand = new RelayCommand(ExecuteNewMenu);
            EditMenuCommand = new RelayCommand(ExecuteEditMenu, CanExecuteEditMenu);
            DeleteMenuCommand = new RelayCommand(ExecuteDeleteMenu, CanExecuteDeleteMenu);
            SaveMenuCommand = new RelayCommand(ExecuteSaveMenu, CanExecuteSaveMenu);
            CancelEditCommand = new RelayCommand(ExecuteCancelEdit);
            AddDishToMenuCommand = new RelayCommand(ExecuteAddDishToMenu, CanExecuteAddDishToMenu);
            RemoveDishFromMenuCommand = new RelayCommand(ExecuteRemoveDishFromMenu, CanExecuteRemoveDishFromMenu);

            // Initial load
            LoadCategories();
            ExecuteLoadMenus(null);
        }

        #region Private Methods

        private void LoadCategories()
        {
            try
            {
                var categories = _categoryRepository.GetAll();
                Categories.Clear();

                foreach (var category in categories)
                {
                    Categories.Add(category);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error loading categories: {ex.Message}", "Error",
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void LoadDishesByCategory(int categoryId)
        {
            try
            {
                var dishes = _dishRepository.GetByCategory(categoryId);
                AvailableDishes.Clear();

                foreach (var dish in dishes)
                {
                    AvailableDishes.Add(dish);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error loading dishes: {ex.Message}", "Error",
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void LoadMenuDetails()
        {
            if (SelectedMenu == null) return;

            try
            {
                var menuDishes = _menuRepository.GetMenuDishes(SelectedMenu.MenuId);
                SelectedMenuDishes.Clear();

                foreach (var menuDish in menuDishes)
                {
                    // Load the dish details
                    menuDish.Dish = _dishRepository.GetById(menuDish.DishId);
                    SelectedMenuDishes.Add(menuDish);
                }

                // Update the form fields
                MenuName = SelectedMenu.Name;
                MenuDescription = SelectedMenu.Description;
                SelectedCategoryId = SelectedMenu.CategoryId;
                IsMenuAvailable = SelectedMenu.IsAvailable;
                CalculatedPrice = SelectedMenu.Price;

                // Select the menu's category
                SelectedCategory = Categories.FirstOrDefault(c => c.CategoryId == SelectedMenu.CategoryId);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error loading menu details: {ex.Message}", "Error",
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void CalculateMenuPrice()
        {
            try
            {
                // Let the service calculate the price based on the dishes and the discount
                CalculatedPrice = _menuService.CalculateMenuPrice(SelectedMenuDishes);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error calculating menu price: {ex.Message}");
                CalculatedPrice = 0;
            }
        }

        private void UpdateCommandStates()
        {
            // Update command CanExecute states
            ((RelayCommand)EditMenuCommand)?.RaiseCanExecuteChanged();
            ((RelayCommand)DeleteMenuCommand)?.RaiseCanExecuteChanged();
            ((RelayCommand)SaveMenuCommand)?.RaiseCanExecuteChanged();
            ((RelayCommand)AddDishToMenuCommand)?.RaiseCanExecuteChanged();
            ((RelayCommand)RemoveDishFromMenuCommand)?.RaiseCanExecuteChanged();
        }

        #endregion

        #region Command Execution Methods

        private void ExecuteLoadMenus(object obj)
        {
            try
            {
                var menus = _menuService.GetAllMenus();
                Menus.Clear();

                foreach (var menu in menus)
                {
                    Menus.Add(menu);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error loading menus: {ex.Message}", "Error",
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void ExecuteNewMenu(object obj)
        {
            // Clear form for a new menu
            SelectedMenu = null;
            MenuName = string.Empty;
            MenuDescription = string.Empty;
            SelectedCategory = Categories.FirstOrDefault();
            SelectedCategoryId = SelectedCategory?.CategoryId ?? 0;
            IsMenuAvailable = true;
            SelectedMenuDishes.Clear();
            CalculatedPrice = 0;

            IsNewMenu = true;
            IsEditing = true;
        }

        private bool CanExecuteEditMenu(object obj)
        {
            return SelectedMenu != null && !IsEditing;
        }

        private void ExecuteEditMenu(object obj)
        {
            if (SelectedMenu == null) return;

            IsNewMenu = false;
            IsEditing = true;
        }

        private bool CanExecuteDeleteMenu(object obj)
        {
            return SelectedMenu != null && !IsEditing;
        }

        private void ExecuteDeleteMenu(object obj)
        {
            if (SelectedMenu == null) return;

            // Confirm deletion
            var result = MessageBox.Show($"Are you sure you want to delete the menu '{SelectedMenu.Name}'?",
                "Confirm Deletion", MessageBoxButton.YesNo, MessageBoxImage.Question);

            if (result == MessageBoxResult.Yes)
            {
                try
                {
                    bool success = _menuRepository.Delete(SelectedMenu.MenuId);

                    if (success)
                    {
                        Menus.Remove(SelectedMenu);
                        SelectedMenu = null;
                        MessageBox.Show("Menu deleted successfully.", "Success",
                            MessageBoxButton.OK, MessageBoxImage.Information);
                    }
                    else
                    {
                        MessageBox.Show("Failed to delete menu. Please try again.", "Error",
                            MessageBoxButton.OK, MessageBoxImage.Error);
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Error deleting menu: {ex.Message}", "Error",
                        MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }

        private bool CanExecuteSaveMenu(object obj)
        {
            // Validate menu data
            bool isValid = !string.IsNullOrWhiteSpace(MenuName) &&
                          SelectedCategoryId > 0 &&
                          SelectedMenuDishes.Count > 0 &&
                          IsEditing;

            return isValid;
        }

        private void ExecuteSaveMenu(object obj)
        {
            try
            {
                // Create menu object
                Menu menu = IsNewMenu ? new Menu() : SelectedMenu;
                menu.Name = MenuName;
                menu.Description = MenuDescription;
                menu.CategoryId = SelectedCategoryId;
                menu.IsAvailable = IsMenuAvailable;

                // Get the menu dishes
                var menuDishes = SelectedMenuDishes.ToList();

                if (IsNewMenu)
                {
                    // Add new menu
                    int menuId = _menuService.CreateMenu(menu, menuDishes);

                    if (menuId > 0)
                    {
                        menu.MenuId = menuId;
                        Menus.Add(menu);
                        SelectedMenu = menu;

                        MessageBox.Show("Menu created successfully.", "Success",
                            MessageBoxButton.OK, MessageBoxImage.Information);
                    }
                    else
                    {
                        MessageBox.Show("Failed to create menu. Please try again.", "Error",
                            MessageBoxButton.OK, MessageBoxImage.Error);
                        return;
                    }
                }
                else
                {
                    // Update existing menu
                    bool success = _menuService.UpdateMenu(menu, menuDishes);

                    if (success)
                    {
                        // Refresh the menu in the list
                        int index = Menus.IndexOf(SelectedMenu);
                        if (index >= 0)
                        {
                            Menus[index] = menu;
                        }

                        MessageBox.Show("Menu updated successfully.", "Success",
                            MessageBoxButton.OK, MessageBoxImage.Information);
                    }
                    else
                    {
                        MessageBox.Show("Failed to update menu. Please try again.", "Error",
                            MessageBoxButton.OK, MessageBoxImage.Error);
                        return;
                    }
                }

                // Exit editing mode
                IsEditing = false;
                IsNewMenu = false;

                // Reload menus to get updated data
                ExecuteLoadMenus(null);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error saving menu: {ex.Message}", "Error",
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void ExecuteCancelEdit(object obj)
        {
            // Cancel editing
            IsEditing = false;
            IsNewMenu = false;

            if (SelectedMenu != null)
            {
                // Reload the selected menu's details
                LoadMenuDetails();
            }
        }

        private bool CanExecuteAddDishToMenu(object obj)
        {
            return IsEditing && SelectedDish != null && SelectedDishPortionSize > 0;
        }

        private void ExecuteAddDishToMenu(object obj)
        {
            if (SelectedDish == null) return;

            // Check if dish is already in the menu
            var existingDish = SelectedMenuDishes.FirstOrDefault(md => md.DishId == SelectedDish.DishId);

            if (existingDish != null)
            {
                // Update portion size
                existingDish.PortionSize = SelectedDishPortionSize;

                // Force refresh of the collection
                var temp = SelectedMenuDishes.ToList();
                SelectedMenuDishes.Clear();
                foreach (var dish in temp)
                {
                    SelectedMenuDishes.Add(dish);
                }
            }
            else
            {
                // Add new menu dish
                var menuDish = new MenuDish
                {
                    DishId = SelectedDish.DishId,
                    Dish = SelectedDish,
                    PortionSize = SelectedDishPortionSize
                };

                SelectedMenuDishes.Add(menuDish);
            }

            // Recalculate the menu price
            CalculateMenuPrice();

            // Clear selection
            SelectedDish = null;
            SelectedDishPortionSize = 0;
        }

        private bool CanExecuteRemoveDishFromMenu(object obj)
        {
            return IsEditing && SelectedMenuDish != null;
        }

        private void ExecuteRemoveDishFromMenu(object obj)
        {
            if (SelectedMenuDish == null) return;

            SelectedMenuDishes.Remove(SelectedMenuDish);
            SelectedMenuDish = null;

            // Recalculate the menu price
            CalculateMenuPrice();
        }

        #endregion
    }
}