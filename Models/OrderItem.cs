using System;
using System.Collections.Generic;

namespace Restaurant.Models
{
    /// <summary>
    /// Represents an item in an order (dish or menu)
    /// </summary>
    public class OrderItem
    {
        public int OrderItemId { get; set; }
        public int OrderId { get; set; }
        public int? DishId { get; set; }  // Nullable because it could be a menu
        public int? MenuId { get; set; }  // Nullable because it could be a dish
        public int Quantity { get; set; }
        public decimal UnitPrice { get; set; }
        public decimal TotalPrice { get; set; }  // UnitPrice * Quantity

        // Navigation properties
        public virtual Order Order { get; set; }
        public virtual Dish Dish { get; set; }
        public virtual Menu Menu { get; set; }
    }
}
