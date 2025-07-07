using Restaurant.Services;
using Restaurant.ViewModels;
using System.Collections.ObjectModel;

public class MenuViewModel : ViewModelBase
{
    private readonly MenuService _menuService;
    private ObservableCollection<object> _menuItems;

    public ObservableCollection<object> MenuItems
    {
        get => _menuItems;
        set
        {
            _menuItems = value;
            OnPropertyChanged(nameof(MenuItems));
        }
    }

    public MenuViewModel(MenuService menuService)
    {
        _menuService = menuService ?? throw new ArgumentNullException(nameof(menuService));
        LoadMenu();
    }

    private void LoadMenu()
    {
        try
        {
            var menuData = _menuService.GetRestaurantMenu();
            MenuItems = new ObservableCollection<object>(menuData);
        }
        catch (Exception ex)
        {
            System.Windows.MessageBox.Show($"Error loading menu: {ex.Message}", "Error",
                System.Windows.MessageBoxButton.OK, System.Windows.MessageBoxImage.Error);
        }
    }
}