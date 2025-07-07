using System;
using System.Collections.Generic;
using System.Linq;
using Restaurant.Models;
using Restaurant.Services.Repositories;

namespace Restaurant.Services
{
    public class OrderService
    {
        private readonly OrderRepository _orderRepository;
        private readonly DishRepository _dishRepository;
        private readonly MenuRepository _menuRepository;
        private readonly UserRepository _userRepository;
        private readonly SettingRepository _settingRepository;

        public OrderService(
            OrderRepository orderRepository,
            DishRepository dishRepository,
            MenuRepository menuRepository,
            UserRepository userRepository,
            SettingRepository settingRepository)
        {
            _orderRepository = orderRepository ?? throw new ArgumentNullException(nameof(orderRepository));
            _dishRepository = dishRepository ?? throw new ArgumentNullException(nameof(dishRepository));
            _menuRepository = menuRepository ?? throw new ArgumentNullException(nameof(menuRepository));
            _userRepository = userRepository ?? throw new ArgumentNullException(nameof(userRepository));
            _settingRepository = settingRepository ?? throw new ArgumentNullException(nameof(settingRepository));
        }

        public IEnumerable<Order> GetAllOrders()
        {
            return _orderRepository.GetAll();
        }

        public IEnumerable<Order> GetActiveOrders()
        {
            return _orderRepository.GetActiveOrders();
        }

        public Order GetOrderById(int id)
        {
            return _orderRepository.GetById(id);
        }

        public IEnumerable<OrderItem> GetOrderItems(int orderId)
        {
            return _orderRepository.GetOrderItems(orderId);
        }

        public IEnumerable<Order> GetUserOrders(int userId)
        {
            return _userRepository.GetUserOrders(userId);
        }

        public int CreateOrder(Order order, List<OrderItem> orderItems)
        {
            decimal subtotal = 0;
            foreach (var item in orderItems)
            {
                if (item.DishId.HasValue)
                {
                    var dish = _dishRepository.GetById(item.DishId.Value);
                    if (dish != null && dish.IsAvailable)
                    {
                        item.UnitPrice = dish.Price;
                        item.TotalPrice = dish.Price * item.Quantity;
                        subtotal += item.TotalPrice;
                    }
                }
                else if (item.MenuId.HasValue)
                {
                    var menu = _menuRepository.GetById(item.MenuId.Value);
                    if (menu != null && menu.IsAvailable)
                    {
                        item.UnitPrice = menu.Price;
                        item.TotalPrice = menu.Price * item.Quantity;
                        subtotal += item.TotalPrice;
                    }
                }
            }

            decimal discountAmount = _orderRepository.CalculateOrderDiscount(order.UserId, subtotal);

            decimal shippingCost = _orderRepository.CalculateShippingCost(subtotal);

            decimal total = subtotal - discountAmount + shippingCost;

            order.SubTotal = subtotal;
            order.DiscountAmount = discountAmount;
            order.ShippingCost = shippingCost;
            order.Total = total;
            order.OrderStatus = "Registered";
            order.OrderDate = DateTime.Now;
            order.OrderCode = GenerateOrderCode();

            order.EstimatedDeliveryTime = DateTime.Now.AddMinutes(60);

            int orderId = _orderRepository.Add(order);

            foreach (var item in orderItems)
            {
                item.OrderId = orderId;
                _orderRepository.AddOrderItem(item);
            }

            return orderId;
        }

        public bool UpdateOrderStatus(int orderId, string newStatus)
        {
            DateTime? estimatedDeliveryTime = null;
            if (newStatus == "InPreparation")
            {
                estimatedDeliveryTime = DateTime.Now.AddMinutes(45);
            }
            else if (newStatus == "Shipped")
            {
                estimatedDeliveryTime = DateTime.Now.AddMinutes(15);
            }

            return _orderRepository.UpdateOrderStatus(orderId, newStatus, estimatedDeliveryTime);
        }

        public bool CancelOrder(int orderId)
        {
            return _orderRepository.CancelOrder(orderId);
        }

        public IEnumerable<Dish> GetLowStockDishes()
        {
            double lowStockThreshold = (double)_settingRepository.GetDecimalValue("LowStockThreshold", 5);

            return _dishRepository.GetAll()
                .Where(d => d.TotalQuantity <= lowStockThreshold)
                .ToList();
        }

        private string GenerateOrderCode()
        {
            return $"ORD-{DateTime.Now:yyyyMMdd}-{Guid.NewGuid().ToString().Substring(0, 4).ToUpper()}";
        }

        public decimal CalculateOrderDiscount(int userId, decimal subtotal)
        {
            if (userId == 0) return 0;

            try
            {
                return _orderRepository.CalculateOrderDiscount(userId, subtotal);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error calculating discount: {ex.Message}");
                return 0;
            }
        }

        public decimal CalculateShippingCost(decimal subtotal)
        {
            try
            {
                return _orderRepository.CalculateShippingCost(subtotal);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error calculating shipping: {ex.Message}");
                return 0;
            }
        }
    }
}