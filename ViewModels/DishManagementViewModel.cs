using System;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Input;
using Microsoft.Win32;
using Restaurant.Commands;
using Restaurant.Models;
using Restaurant.Services.Repositories;

namespace Restaurant.ViewModels
{
    public class DishManagementViewModel : ViewModelBase
    {
        private readonly DishRepository _dishRepository;
        private readonly CategoryRepository _categoryRepository;
        private readonly AllergenRepository _allergenRepository;

        private ObservableCollection<Dish> _dishes;
        private ObservableCollection<Category> _categories;
        private ObservableCollection<Allergen> _allergens;
        private ObservableCollection<Allergen> _selectedAllergens;
        private ObservableCollection<DishPhoto> _dishPhotos;

        private Dish _selectedDish;
        private Category _selectedCategory;
        private Allergen _selectedAllergen;
        private DishPhoto _selectedPhoto;

        private string _dishName;
        private decimal _dishPrice;
        private double _portionSize;
        private string _portionUnit;
        private double _totalQuantity;
        private int _selectedCategoryId;
        private string _dishDescription;
        private bool _isDishAvailable;

        private bool _isEditing;
        private bool _isNewDish;

        #region Properties

        public ObservableCollection<Dish> Dishes
        {
            get => _dishes;
            set
            {
                _dishes = value;
                OnPropertyChanged(nameof(Dishes));
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

        public ObservableCollection<Allergen> Allergens
        {
            get => _allergens;
            set
            {
                _allergens = value;
                OnPropertyChanged(nameof(Allergens));
            }
        }

        public ObservableCollection<Allergen> SelectedAllergens
        {
            get => _selectedAllergens;
            set
            {
                _selectedAllergens = value;
                OnPropertyChanged(nameof(SelectedAllergens));
            }
        }

        public ObservableCollection<DishPhoto> DishPhotos
        {
            get => _dishPhotos;
            set
            {
                _dishPhotos = value;
                OnPropertyChanged(nameof(DishPhotos));
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
                    LoadDishDetails();
                }
                else
                {
                    SelectedAllergens?.Clear();
                    DishPhotos?.Clear();
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
                    SelectedCategoryId = value.CategoryId;
                }
            }
        }

        public Allergen SelectedAllergen
        {
            get => _selectedAllergen;
            set
            {
                _selectedAllergen = value;
                OnPropertyChanged(nameof(SelectedAllergen));
                UpdateCommandStates();
            }
        }

        public DishPhoto SelectedPhoto
        {
            get => _selectedPhoto;
            set
            {
                _selectedPhoto = value;
                OnPropertyChanged(nameof(SelectedPhoto));
                UpdateCommandStates();
            }
        }

        public string DishName
        {
            get => _dishName;
            set
            {
                _dishName = value;
                OnPropertyChanged(nameof(DishName));
                UpdateCommandStates();
            }
        }

        public decimal DishPrice
        {
            get => _dishPrice;
            set
            {
                _dishPrice = value;
                OnPropertyChanged(nameof(DishPrice));
            }
        }

        public double PortionSize
        {
            get => _portionSize;
            set
            {
                _portionSize = value;
                OnPropertyChanged(nameof(PortionSize));
            }
        }

        public string PortionUnit
        {
            get => _portionUnit;
            set
            {
                _portionUnit = value;
                OnPropertyChanged(nameof(PortionUnit));
            }
        }

        public double TotalQuantity
        {
            get => _totalQuantity;
            set
            {
                _totalQuantity = value;
                OnPropertyChanged(nameof(TotalQuantity));
            }
        }

        public int SelectedCategoryId
        {
            get => _selectedCategoryId;
            set
            {
                _selectedCategoryId = value;
                OnPropertyChanged(nameof(SelectedCategoryId));
            }
        }

        public string DishDescription
        {
            get => _dishDescription;
            set
            {
                _dishDescription = value;
                OnPropertyChanged(nameof(DishDescription));
            }
        }

        public bool IsDishAvailable
        {
            get => _isDishAvailable;
            set
            {
                _isDishAvailable = value;
                OnPropertyChanged(nameof(IsDishAvailable));
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

        public bool IsNewDish
        {
            get => _isNewDish;
            set
            {
                _isNewDish = value;
                OnPropertyChanged(nameof(IsNewDish));
            }
        }

        #endregion

        #region Commands

        public ICommand LoadDishesCommand { get; private set; }
        public ICommand NewDishCommand { get; private set; }
        public ICommand EditDishCommand { get; private set; }
        public ICommand DeleteDishCommand { get; private set; }
        public ICommand SaveDishCommand { get; private set; }
        public ICommand CancelEditCommand { get; private set; }
        public ICommand AddAllergenCommand { get; private set; }
        public ICommand RemoveAllergenCommand { get; private set; }
        public ICommand AddPhotoCommand { get; private set; }
        public ICommand RemovePhotoCommand { get; private set; }
        public ICommand SetPrimaryPhotoCommand { get; private set; }

        #endregion

        public DishManagementViewModel(
            DishRepository dishRepository,
            CategoryRepository categoryRepository,
            AllergenRepository allergenRepository)
        {
            _dishRepository = dishRepository ?? throw new ArgumentNullException(nameof(dishRepository));
            _categoryRepository = categoryRepository ?? throw new ArgumentNullException(nameof(categoryRepository));
            _allergenRepository = allergenRepository ?? throw new ArgumentNullException(nameof(allergenRepository));

            Dishes = new ObservableCollection<Dish>();
            Categories = new ObservableCollection<Category>();
            Allergens = new ObservableCollection<Allergen>();
            SelectedAllergens = new ObservableCollection<Allergen>();
            DishPhotos = new ObservableCollection<DishPhoto>();

            LoadDishesCommand = new RelayCommand(ExecuteLoadDishes);
            NewDishCommand = new RelayCommand(ExecuteNewDish);
            EditDishCommand = new RelayCommand(ExecuteEditDish, CanExecuteEditDish);
            DeleteDishCommand = new RelayCommand(ExecuteDeleteDish, CanExecuteDeleteDish);
            SaveDishCommand = new RelayCommand(ExecuteSaveDish, CanExecuteSaveDish);
            CancelEditCommand = new RelayCommand(ExecuteCancelEdit);
            AddAllergenCommand = new RelayCommand(ExecuteAddAllergen, CanExecuteAddAllergen);
            RemoveAllergenCommand = new RelayCommand(ExecuteRemoveAllergen, CanExecuteRemoveAllergen);
            AddPhotoCommand = new RelayCommand(ExecuteAddPhoto, CanExecuteAddPhoto);
            RemovePhotoCommand = new RelayCommand(ExecuteRemovePhoto, CanExecuteRemovePhoto);
            SetPrimaryPhotoCommand = new RelayCommand(ExecuteSetPrimaryPhoto, CanExecuteSetPrimaryPhoto);

            LoadCategories();
            LoadAllergens();
            ExecuteLoadDishes(null);
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

        private void LoadAllergens()
        {
            try
            {
                var allergens = _allergenRepository.GetAll();
                Allergens.Clear();

                foreach (var allergen in allergens)
                {
                    Allergens.Add(allergen);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error loading allergens: {ex.Message}", "Error",
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void LoadDishDetails()
        {
            if (SelectedDish == null) return;

            try
            {
                var photos = _dishRepository.GetDishPhotos(SelectedDish.DishId);
                DishPhotos.Clear();

                foreach (var photo in photos)
                {
                    DishPhotos.Add(photo);
                }

                var dishAllergens = _dishRepository.GetDishAllergens(SelectedDish.DishId);
                SelectedAllergens.Clear();

                foreach (var dishAllergen in dishAllergens)
                {
                    var allergen = _allergenRepository.GetById(dishAllergen.AllergenId);
                    if (allergen != null)
                    {
                        SelectedAllergens.Add(allergen);
                    }
                }

                DishName = SelectedDish.Name;
                DishPrice = SelectedDish.Price;
                PortionSize = SelectedDish.PortionSize;
                PortionUnit = SelectedDish.PortionUnit;
                TotalQuantity = SelectedDish.TotalQuantity;
                SelectedCategoryId = SelectedDish.CategoryId;
                DishDescription = SelectedDish.Description;
                IsDishAvailable = SelectedDish.IsAvailable;

                SelectedCategory = Categories.FirstOrDefault(c => c.CategoryId == SelectedDish.CategoryId);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error loading dish details: {ex.Message}", "Error",
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void UpdateCommandStates()
        {
            ((RelayCommand)EditDishCommand)?.RaiseCanExecuteChanged();
            ((RelayCommand)DeleteDishCommand)?.RaiseCanExecuteChanged();
            ((RelayCommand)SaveDishCommand)?.RaiseCanExecuteChanged();
            ((RelayCommand)AddAllergenCommand)?.RaiseCanExecuteChanged();
            ((RelayCommand)RemoveAllergenCommand)?.RaiseCanExecuteChanged();
            ((RelayCommand)AddPhotoCommand)?.RaiseCanExecuteChanged();
            ((RelayCommand)RemovePhotoCommand)?.RaiseCanExecuteChanged();
            ((RelayCommand)SetPrimaryPhotoCommand)?.RaiseCanExecuteChanged();
        }

        #endregion

        #region Command Execution Methods

        private void ExecuteLoadDishes(object obj)
        {
            try
            {
                var dishes = _dishRepository.GetAll();
                Dishes.Clear();

                foreach (var dish in dishes)
                {
                    dish.Category = Categories.FirstOrDefault(c => c.CategoryId == dish.CategoryId);
                    Dishes.Add(dish);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error loading dishes: {ex.Message}", "Error",
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void ExecuteNewDish(object obj)
        {
            SelectedDish = null;
            DishName = string.Empty;
            DishPrice = 0;
            PortionSize = 0;
            PortionUnit = "g"; // Default unit
            TotalQuantity = 0;
            SelectedCategory = Categories.FirstOrDefault();
            SelectedCategoryId = SelectedCategory?.CategoryId ?? 0;
            DishDescription = string.Empty;
            IsDishAvailable = true;
            SelectedAllergens.Clear();
            DishPhotos.Clear();

            IsNewDish = true;
            IsEditing = true;
        }

        private bool CanExecuteEditDish(object obj)
        {
            return SelectedDish != null && !IsEditing;
        }

        private void ExecuteEditDish(object obj)
        {
            if (SelectedDish == null) return;

            IsNewDish = false;
            IsEditing = true;
        }

        private bool CanExecuteDeleteDish(object obj)
        {
            return SelectedDish != null && !IsEditing;
        }

        private void ExecuteDeleteDish(object obj)
        {
            if (SelectedDish == null) return;

            var result = MessageBox.Show($"Are you sure you want to delete the dish '{SelectedDish.Name}'?",
                "Confirm Deletion", MessageBoxButton.YesNo, MessageBoxImage.Question);

            if (result == MessageBoxResult.Yes)
            {
                try
                {
                    bool success = _dishRepository.Delete(SelectedDish.DishId);

                    if (success)
                    {
                        Dishes.Remove(SelectedDish);
                        SelectedDish = null;
                        MessageBox.Show("Dish deleted successfully.", "Success",
                            MessageBoxButton.OK, MessageBoxImage.Information);
                    }
                    else
                    {
                        MessageBox.Show("Failed to delete dish. Please try again.", "Error",
                            MessageBoxButton.OK, MessageBoxImage.Error);
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Error deleting dish: {ex.Message}", "Error",
                        MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }

        private bool CanExecuteSaveDish(object obj)
        {
            bool isValid = !string.IsNullOrWhiteSpace(DishName) &&
                          DishPrice > 0 &&
                          PortionSize > 0 &&
                          !string.IsNullOrWhiteSpace(PortionUnit) &&
                          TotalQuantity >= 0 &&
                          SelectedCategoryId > 0 &&
                          IsEditing;

            return isValid;
        }

        private void ExecuteSaveDish(object obj)
        {
            try
            {
                Dish dish = IsNewDish ? new Dish() : SelectedDish;
                dish.Name = DishName;
                dish.Price = DishPrice;
                dish.PortionSize = PortionSize;
                dish.PortionUnit = PortionUnit;
                dish.TotalQuantity = TotalQuantity;
                dish.CategoryId = SelectedCategoryId;
                dish.Description = DishDescription;
                dish.IsAvailable = IsDishAvailable;

                if (IsNewDish)
                {
                    int dishId = _dishRepository.Add(dish);

                    if (dishId > 0)
                    {
                        dish.DishId = dishId;
                        dish.Category = Categories.FirstOrDefault(c => c.CategoryId == dish.CategoryId);

                        SaveDishAllergens(dishId);

                        SaveDishPhotos(dishId);

                        Dishes.Add(dish);
                        SelectedDish = dish;

                        MessageBox.Show("Dish created successfully.", "Success",
                            MessageBoxButton.OK, MessageBoxImage.Information);
                    }
                    else
                    {
                        MessageBox.Show("Failed to create dish. Please try again.", "Error",
                            MessageBoxButton.OK, MessageBoxImage.Error);
                        return;
                    }
                }
                else
                {
                    bool success = _dishRepository.Update(dish);

                    if (success)
                    {
                        dish.Category = Categories.FirstOrDefault(c => c.CategoryId == dish.CategoryId);

                        SaveDishAllergens(dish.DishId);

                        SaveDishPhotos(dish.DishId);

                        int index = Dishes.IndexOf(SelectedDish);
                        if (index >= 0)
                        {
                            Dishes[index] = dish;
                        }

                        MessageBox.Show("Dish updated successfully.", "Success",
                            MessageBoxButton.OK, MessageBoxImage.Information);
                    }
                    else
                    {
                        MessageBox.Show("Failed to update dish. Please try again.", "Error",
                            MessageBoxButton.OK, MessageBoxImage.Error);
                        return;
                    }
                }

                IsEditing = false;
                IsNewDish = false;

                ExecuteLoadDishes(null);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error saving dish: {ex.Message}", "Error",
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void SaveDishAllergens(int dishId)
        {
            var currentAllergens = _dishRepository.GetDishAllergens(dishId)
                .Select(da => da.AllergenId)
                .ToList();

            var selectedAllergens = SelectedAllergens
                .Select(a => a.AllergenId)
                .ToList();

            foreach (var allergenId in currentAllergens)
            {
                if (!selectedAllergens.Contains(allergenId))
                {
                    _allergenRepository.RemoveAllergenFromDish(dishId, allergenId);
                }
            }

            foreach (var allergenId in selectedAllergens)
            {
                if (!currentAllergens.Contains(allergenId))
                {
                    _allergenRepository.AddAllergenToDish(dishId, allergenId);
                }
            }
        }

        private void SaveDishPhotos(int dishId)
        {
            // For new dishes or when implementing photo uploads
            // This is a placeholder for the actual photo saving logic
            // Photos would be saved to the database via the repository
        }

        private void ExecuteCancelEdit(object obj)
        {
            IsEditing = false;
            IsNewDish = false;

            if (SelectedDish != null)
            {
                LoadDishDetails();
            }
        }

        private bool CanExecuteAddAllergen(object obj)
        {
            return IsEditing && SelectedAllergen != null && !SelectedAllergens.Contains(SelectedAllergen);
        }

        private void ExecuteAddAllergen(object obj)
        {
            if (SelectedAllergen == null || SelectedAllergens.Contains(SelectedAllergen)) return;

            SelectedAllergens.Add(SelectedAllergen);
            SelectedAllergen = null;
        }

        private bool CanExecuteRemoveAllergen(object obj)
        {
            return IsEditing && obj is Allergen allergen && SelectedAllergens.Contains(allergen);
        }

        private void ExecuteRemoveAllergen(object obj)
        {
            if (obj is Allergen allergen)
            {
                SelectedAllergens.Remove(allergen);
            }
        }

        private bool CanExecuteAddPhoto(object obj)
        {
            return IsEditing;
        }

        private void ExecuteAddPhoto(object obj)
        {
            var openFileDialog = new OpenFileDialog
            {
                Filter = "Image files (*.jpg;*.jpeg;*.png;*.bmp)|*.jpg;*.jpeg;*.png;*.bmp",
                Multiselect = false
            };

            if (openFileDialog.ShowDialog() == true)
            {
                try
                {
                    byte[] photoData = File.ReadAllBytes(openFileDialog.FileName);

                    var photo = new DishPhoto
                    {
                        PhotoData = photoData,
                        Description = Path.GetFileName(openFileDialog.FileName),
                        IsPrimary = DishPhotos.Count == 0 // First photo is primary by default
                    };

                    DishPhotos.Add(photo);
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Error loading image: {ex.Message}", "Error",
                        MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }

        private bool CanExecuteRemovePhoto(object obj)
        {
            return IsEditing && SelectedPhoto != null;
        }

        private void ExecuteRemovePhoto(object obj)
        {
            if (SelectedPhoto == null) return;

            if (SelectedPhoto.IsPrimary && DishPhotos.Count > 1)
            {
                var nextPhoto = DishPhotos.FirstOrDefault(p => p != SelectedPhoto);
                if (nextPhoto != null)
                {
                    nextPhoto.IsPrimary = true;
                }
            }

            DishPhotos.Remove(SelectedPhoto);
            SelectedPhoto = null;
        }

        private bool CanExecuteSetPrimaryPhoto(object obj)
        {
            return IsEditing && SelectedPhoto != null && !SelectedPhoto.IsPrimary;
        }

        private void ExecuteSetPrimaryPhoto(object obj)
        {
            if (SelectedPhoto == null || SelectedPhoto.IsPrimary) return;

            var currentPrimary = DishPhotos.FirstOrDefault(p => p.IsPrimary);
            if (currentPrimary != null)
            {
                currentPrimary.IsPrimary = false;
            }

            SelectedPhoto.IsPrimary = true;

            var temp = DishPhotos.ToList();
            DishPhotos.Clear();
            foreach (var photo in temp)
            {
                DishPhotos.Add(photo);
            }
        }

        #endregion
    }
}