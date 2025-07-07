using System;
using System.Collections.ObjectModel;
using System.Windows.Input;
using Restaurant.Commands;
using Restaurant.Models;
using Restaurant.Services;

namespace Restaurant.ViewModels
{
    public class SearchViewModel : ViewModelBase
    {
        private readonly MenuService _menuService;
        private string _searchKeyword;
        private bool _searchInName = true;
        private bool _excludeAllergen;
        private ObservableCollection<Dish> _searchResultDishes;
        private ObservableCollection<Menu> _searchResultMenus;

        public string SearchKeyword
        {
            get => _searchKeyword;
            set
            {
                _searchKeyword = value;
                OnPropertyChanged(nameof(SearchKeyword));
            }
        }

        public bool SearchInName
        {
            get => _searchInName;
            set
            {
                _searchInName = value;
                OnPropertyChanged(nameof(SearchInName));
            }
        }

        public bool ExcludeAllergen
        {
            get => _excludeAllergen;
            set
            {
                _excludeAllergen = value;
                OnPropertyChanged(nameof(ExcludeAllergen));
            }
        }

        public ObservableCollection<Dish> SearchResultDishes
        {
            get => _searchResultDishes;
            set
            {
                _searchResultDishes = value;
                OnPropertyChanged(nameof(SearchResultDishes));
            }
        }

        public ObservableCollection<Menu> SearchResultMenus
        {
            get => _searchResultMenus;
            set
            {
                _searchResultMenus = value;
                OnPropertyChanged(nameof(SearchResultMenus));
            }
        }

        public ICommand SearchCommand { get; }

        public SearchViewModel(MenuService menuService)
        {
            _menuService = menuService ?? throw new ArgumentNullException(nameof(menuService));
            SearchCommand = new RelayCommand(ExecuteSearch, CanExecuteSearch);

            SearchResultDishes = new ObservableCollection<Dish>();
            SearchResultMenus = new ObservableCollection<Menu>();
        }

        private bool CanExecuteSearch(object obj)
        {
            return !string.IsNullOrWhiteSpace(SearchKeyword);
        }

        private void ExecuteSearch(object obj)
        {
            try
            {
                SearchResultDishes.Clear();
                SearchResultMenus.Clear();

                if (string.IsNullOrWhiteSpace(SearchKeyword))
                {
                    return;
                }

                if (SearchInName)
                {
                    // Search by name
                    var dishes = _menuService.SearchDishesByName(SearchKeyword);
                    var menus = _menuService.SearchMenusByName(SearchKeyword);

                    foreach (var dish in dishes)
                    {
                        SearchResultDishes.Add(dish);
                    }

                    foreach (var menu in menus)
                    {
                        SearchResultMenus.Add(menu);
                    }
                }
                else
                {
                    // Search by allergen
                    var dishes = _menuService.SearchDishesByAllergen(SearchKeyword, ExcludeAllergen);
                    var menus = _menuService.SearchMenusByAllergen(SearchKeyword, ExcludeAllergen);

                    foreach (var dish in dishes)
                    {
                        SearchResultDishes.Add(dish);
                    }

                    foreach (var menu in menus)
                    {
                        SearchResultMenus.Add(menu);
                    }
                }
            }
            catch (Exception ex)
            {
                System.Windows.MessageBox.Show($"Error searching menu: {ex.Message}", "Error",
                    System.Windows.MessageBoxButton.OK, System.Windows.MessageBoxImage.Error);
            }
        }
    }
}