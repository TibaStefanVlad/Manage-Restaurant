using System;
using System.Collections.Generic;


namespace Restaurant.Models
{
    /// <summary>
    /// Represents an order placed by a customer
    /// </summary>
    public class Order
    {
        public int OrderId { get; set; }
        public int UserId { get; set; }
        public DateTime OrderDate { get; set; }
        public string OrderStatus { get; set; }  // "Registered", "InPreparation", "Shipped", "Delivered", "Cancelled"
        public string OrderCode { get; set; }  // Unique code for the order
        public decimal SubTotal { get; set; }  // Price of all items before discounts/shipping
        public decimal DiscountAmount { get; set; }
        public decimal ShippingCost { get; set; }
        public decimal Total { get; set; }  // Final price after discounts and shipping
        public DateTime? EstimatedDeliveryTime { get; set; }

        // Navigation properties
        public virtual User User { get; set; }
        public virtual ICollection<OrderItem> OrderItems { get; set; }

        public Order()
        {
            OrderItems = new HashSet<OrderItem>();
            OrderDate = DateTime.Now;
            OrderStatus = "Registered";
            OrderCode = GenerateOrderCode();
        }

        private string GenerateOrderCode()
        {
            // Generate a unique order code like "ORD-20250507-XXXX"
            return $"ORD-{DateTime.Now:yyyyMMdd}-{Guid.NewGuid().ToString().Substring(0, 4).ToUpper()}";
        }
    }
}
