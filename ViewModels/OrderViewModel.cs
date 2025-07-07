using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows;
using System.Windows.Input;
using Restaurant.Commands;
using Restaurant.Models;
using Restaurant.Services;

namespace Restaurant.ViewModels
{
    public class OrderViewModel : ViewModelBase
    {
        private readonly MenuService _menuService;
        private readonly OrderService _orderService;

        private ObservableCollection<object> _menuCategories;
        private ObservableCollection<OrderItem> _cartItems;
        private Dish _selectedDish;
        private Menu _selectedMenu;
        private int _quantity = 1;
        private decimal _subtotal;
        private decimal _discount;
        private decimal _shipping;
        private decimal _total;

        #region Properties

        public ObservableCollection<object> MenuCategories
        {
            get => _menuCategories;
            set
            {
                _menuCategories = value;
                OnPropertyChanged(nameof(MenuCategories));
            }
        }

        public ObservableCollection<OrderItem> CartItems
        {
            get => _cartItems;
            set
            {
                _cartItems = value;
                OnPropertyChanged(nameof(CartItems));
                UpdateTotals();
            }
        }

        public Dish SelectedDish
        {
            get => _selectedDish;
            set
            {
                _selectedDish = value;
                OnPropertyChanged(nameof(SelectedDish));
                ((RelayCommand)AddDishToCartCommand).RaiseCanExecuteChanged();
            }
        }

        public Menu SelectedMenu
        {
            get => _selectedMenu;
            set
            {
                _selectedMenu = value;
                OnPropertyChanged(nameof(SelectedMenu));
                ((RelayCommand)AddMenuToCartCommand).RaiseCanExecuteChanged();
            }
        }

        public int Quantity
        {
            get => _quantity;
            set
            {
                if (value >= 1)
                {
                    _quantity = value;
                    OnPropertyChanged(nameof(Quantity));

                    // Add these lines to notify commands when quantity changes
                    var addDishCommand = AddDishToCartCommand as RelayCommand;
                    addDishCommand?.RaiseCanExecuteChanged();

                    var addMenuCommand = AddMenuToCartCommand as RelayCommand;
                    addMenuCommand?.RaiseCanExecuteChanged();
                }
            }
        }

        public decimal Subtotal
        {
            get => _subtotal;
            set
            {
                _subtotal = value;
                OnPropertyChanged(nameof(Subtotal));
            }
        }

        public decimal Discount
        {
            get => _discount;
            set
            {
                _discount = value;
                OnPropertyChanged(nameof(Discount));
            }
        }

        public decimal Shipping
        {
            get => _shipping;
            set
            {
                _shipping = value;
                OnPropertyChanged(nameof(Shipping));
            }
        }

        public decimal Total
        {
            get => _total;
            set
            {
                _total = value;
                OnPropertyChanged(nameof(Total));
            }
        }

        #endregion

        #region Commands

        public ICommand AddDishToCartCommand { get; }
        public ICommand AddMenuToCartCommand { get; }
        public ICommand RemoveFromCartCommand { get; }
        public ICommand IncreaseQuantityCommand { get; }
        public ICommand DecreaseQuantityCommand { get; }
        public ICommand ClearCartCommand { get; }
        public ICommand PlaceOrderCommand { get; }

        #endregion

        public OrderViewModel(MenuService menuService, OrderService orderService)
        {
            _menuService = menuService ?? throw new ArgumentNullException(nameof(menuService));
            _orderService = orderService ?? throw new ArgumentNullException(nameof(orderService));

            // Initialize collections
            CartItems = new ObservableCollection<OrderItem>();

            // Initialize commands
            AddDishToCartCommand = new RelayCommand(ExecuteAddDishToCart, CanAddDishToCart);
            AddMenuToCartCommand = new RelayCommand(ExecuteAddMenuToCart, CanAddMenuToCart);
            RemoveFromCartCommand = new RelayCommand(ExecuteRemoveFromCart);
            IncreaseQuantityCommand = new RelayCommand(ExecuteIncreaseQuantity);
            DecreaseQuantityCommand = new RelayCommand(ExecuteDecreaseQuantity);
            ClearCartCommand = new RelayCommand(ExecuteClearCart, CanClearCart);
            PlaceOrderCommand = new RelayCommand(ExecutePlaceOrder, CanPlaceOrder);

            _quantity = 1;  // Make sure this is set
            OnPropertyChanged(nameof(Quantity));  // Force notification

            Subtotal = 0;
            Discount = 0;
            Shipping = 0;
            Total = 0;

            // Load menu data
            LoadMenu();
        }

        private void LoadMenu()
        {
            try
            {
                var menuData = _menuService.GetRestaurantMenu();
                MenuCategories = new ObservableCollection<object>(menuData);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error loading menu: {ex.Message}", "Error",
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        #region Command Methods

        private bool CanAddDishToCart(object obj)
        {
            var dish = obj as Dish;
            // If Quantity is 0 (perhaps from an empty TextBox), default to 1
            int qty = Quantity <= 0 ? 1 : Quantity;
            return dish != null && dish.IsAvailable && qty > 0;
        }

        private void ExecuteAddDishToCart(object obj)
        {
            var dish = obj as Dish;
            if (dish == null) return;

            // Check if already in cart
            var existingItem = CartItems.FirstOrDefault(item =>
                item.DishId == dish.DishId && item.MenuId == null);

            if (existingItem != null)
            {
                // Increase quantity if already in cart
                existingItem.Quantity += Quantity;
                existingItem.TotalPrice = existingItem.UnitPrice * existingItem.Quantity;
            }
            else
            {
                // Add new item to cart
                var newItem = new OrderItem
                {
                    DishId = dish.DishId,
                    Quantity = Quantity,
                    UnitPrice = dish.Price,
                    TotalPrice = dish.Price * Quantity,
                    Dish = dish
                };

                CartItems.Add(newItem);
            }

            // Reset quantity
            Quantity = 1;

            // Update cart totals
            UpdateTotals();
        }

        private bool CanAddMenuToCart(object obj)
        {
            var menu = obj as Menu;
            // If Quantity is 0 (perhaps from an empty TextBox), default to 1
            int qty = Quantity <= 0 ? 1 : Quantity;
            return menu != null && menu.IsAvailable && qty > 0;
        }

        private void ExecuteAddMenuToCart(object obj)
        {
            var menu = obj as Menu;
            if (menu == null) return;

            // Check if already in cart
            var existingItem = CartItems.FirstOrDefault(item =>
                item.MenuId == menu.MenuId);

            if (existingItem != null)
            {
                // Increase quantity if already in cart
                existingItem.Quantity += Quantity;
                existingItem.TotalPrice = existingItem.UnitPrice * existingItem.Quantity;
            }
            else
            {
                // Add new item to cart
                var newItem = new OrderItem
                {
                    MenuId = menu.MenuId,
                    Quantity = Quantity,
                    UnitPrice = menu.Price,
                    TotalPrice = menu.Price * Quantity,
                    Menu = menu
                };

                CartItems.Add(newItem);
            }

            // Reset quantity
            Quantity = 1;

            // Update cart totals
            UpdateTotals();
        }

        private void ExecuteRemoveFromCart(object obj)
        {
            if (obj is OrderItem item)
            {
                CartItems.Remove(item);
                UpdateTotals();
            }
        }

        private void ExecuteIncreaseQuantity(object obj)
        {
            if (obj is OrderItem item)
            {
                item.Quantity++;
                item.TotalPrice = item.UnitPrice * item.Quantity;
                UpdateTotals();
            }
        }

        private void ExecuteDecreaseQuantity(object obj)
        {
            if (obj is OrderItem item && item.Quantity > 1)
            {
                item.Quantity--;
                item.TotalPrice = item.UnitPrice * item.Quantity;
                UpdateTotals();
            }
        }

        private bool CanClearCart(object obj)
        {
            return CartItems.Count > 0;
        }

        private void ExecuteClearCart(object obj)
        {
            CartItems.Clear();
            UpdateTotals();
        }

        private bool CanPlaceOrder(object obj)
        {
            return CartItems.Count > 0 && SessionService.CurrentUser != null;
        }

        private void ExecutePlaceOrder(object obj)
        {
            try
            {
                // Create a new order
                Order order = new Order
                {
                    UserId = SessionService.CurrentUser.UserId,
                    OrderDate = DateTime.Now,
                    OrderStatus = "Registered"
                };

                // Convert cart items to order items
                var orderItems = CartItems.ToList();

                // Place the order
                int orderId = _orderService.CreateOrder(order, orderItems);

                if (orderId > 0)
                {
                    MessageBox.Show($"Order placed successfully! Your order code is: {order.OrderCode}",
                        "Order Placed", MessageBoxButton.OK, MessageBoxImage.Information);

                    // Clear the cart
                    CartItems.Clear();
                    UpdateTotals();

                    // Raise event to switch to order history tab
                    OrderPlaced?.Invoke(this, EventArgs.Empty);
                }
                else
                {
                    MessageBox.Show("Failed to place order. Please try again.",
                        "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error placing order: {ex.Message}",
                    "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        #endregion

        private void UpdateTotals()
        {
            try
            {
                // Guard clause to prevent null reference
                if (CartItems == null)
                {
                    Subtotal = 0;
                    Discount = 0;
                    Shipping = 0;
                    Total = 0;
                    return;
                }

                // Calculate subtotal
                Subtotal = CartItems.Sum(item => item.TotalPrice);

                // Safely get current user ID
                int userId = SessionService.CurrentUser?.UserId ?? 0;

                // Safely calculate discount and shipping
                try
                {
                    // Calculate discount - add null checks
                    Discount = _orderService?.CalculateOrderDiscount(userId, Subtotal) ?? 0;

                    // Calculate shipping - add null checks
                    Shipping = _orderService?.CalculateShippingCost(Subtotal) ?? 0;
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Error calculating discounts or shipping: {ex.Message}");
                    Discount = 0;
                    Shipping = 0;
                }

                // Calculate total
                Total = Subtotal - Discount + Shipping;

                // Update commands that depend on cart status - add null checks
                var clearCartCommand = ClearCartCommand as RelayCommand;
                clearCartCommand?.RaiseCanExecuteChanged();

                var placeOrderCommand = PlaceOrderCommand as RelayCommand;
                placeOrderCommand?.RaiseCanExecuteChanged();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error in UpdateTotals: {ex.Message}");
                // Set defaults if calculation fails
                Subtotal = 0;
                Discount = 0;
                Shipping = 0;
                Total = 0;
            }
        }

        public event EventHandler OrderPlaced;
    }
}