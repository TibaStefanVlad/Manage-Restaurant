using System;
using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Input;
using Restaurant.Commands;
using Restaurant.Models;
using Restaurant.Services;

namespace Restaurant.ViewModels
{
    public class OrderHistoryViewModel : ViewModelBase
    {
        private readonly OrderService _orderService;
        private ObservableCollection<Order> _allOrders;
        private ObservableCollection<Order> _activeOrders;
        private Order _selectedOrder;
        private ObservableCollection<OrderItem> _selectedOrderItems;

        #region Properties

        public ObservableCollection<Order> AllOrders
        {
            get => _allOrders;
            set
            {
                _allOrders = value;
                OnPropertyChanged(nameof(AllOrders));
            }
        }

        public ObservableCollection<Order> ActiveOrders
        {
            get => _activeOrders;
            set
            {
                _activeOrders = value;
                OnPropertyChanged(nameof(ActiveOrders));
            }
        }

        public Order SelectedOrder
        {
            get => _selectedOrder;
            set
            {
                _selectedOrder = value;
                OnPropertyChanged(nameof(SelectedOrder));
                LoadSelectedOrderItems();
                ((RelayCommand)CancelOrderCommand).RaiseCanExecuteChanged();
            }
        }

        public ObservableCollection<OrderItem> SelectedOrderItems
        {
            get => _selectedOrderItems;
            set
            {
                _selectedOrderItems = value;
                OnPropertyChanged(nameof(SelectedOrderItems));
            }
        }

        #endregion

        #region Commands

        public ICommand RefreshOrdersCommand { get; }
        public ICommand CancelOrderCommand { get; }

        #endregion

        public OrderHistoryViewModel(OrderService orderService)
        {
            _orderService = orderService ?? throw new ArgumentNullException(nameof(orderService));

            // Initialize collections
            AllOrders = new ObservableCollection<Order>();
            ActiveOrders = new ObservableCollection<Order>();
            SelectedOrderItems = new ObservableCollection<OrderItem>();

            // Initialize commands
            RefreshOrdersCommand = new RelayCommand(ExecuteRefreshOrders);
            CancelOrderCommand = new RelayCommand(ExecuteCancelOrder, CanCancelOrder);

            // Load orders
            LoadOrders();
        }

        private void LoadOrders()
        {
            try
            {
                if (SessionService.CurrentUser == null) return;

                // Get all orders for the current user
                var orders = _orderService.GetUserOrders(SessionService.CurrentUser.UserId);
                AllOrders = new ObservableCollection<Order>(orders);

                // Get active orders (not delivered or cancelled)
                ActiveOrders.Clear();
                foreach (var order in AllOrders)
                {
                    if (order.OrderStatus != "Delivered" && order.OrderStatus != "Cancelled")
                    {
                        ActiveOrders.Add(order);
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error loading orders: {ex.Message}",
                    "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void LoadSelectedOrderItems()
        {
            try
            {
                SelectedOrderItems.Clear();

                if (SelectedOrder != null)
                {
                    var items = _orderService.GetOrderItems(SelectedOrder.OrderId);
                    foreach (var item in items)
                    {
                        SelectedOrderItems.Add(item);
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error loading order details: {ex.Message}",
                    "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        #region Command Methods

        private void ExecuteRefreshOrders(object obj)
        {
            LoadOrders();
        }

        private bool CanCancelOrder(object obj)
        {
            return SelectedOrder != null &&
                   (SelectedOrder.OrderStatus == "Registered" ||
                    SelectedOrder.OrderStatus == "InPreparation");
        }

        private void ExecuteCancelOrder(object obj)
        {
            try
            {
                if (SelectedOrder == null) return;

                // Confirm cancellation
                MessageBoxResult result = MessageBox.Show(
                    $"Are you sure you want to cancel order {SelectedOrder.OrderCode}?",
                    "Confirm Cancellation",
                    MessageBoxButton.YesNo,
                    MessageBoxImage.Question);

                if (result == MessageBoxResult.Yes)
                {
                    // Cancel the order
                    bool success = _orderService.CancelOrder(SelectedOrder.OrderId);

                    if (success)
                    {
                        MessageBox.Show("Order cancelled successfully.",
                            "Order Cancelled", MessageBoxButton.OK, MessageBoxImage.Information);

                        // Refresh orders
                        LoadOrders();
                    }
                    else
                    {
                        MessageBox.Show("Failed to cancel order. Please try again.",
                            "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error cancelling order: {ex.Message}",
                    "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        #endregion
    }
}