using System;
using System.Collections.Generic;

namespace Restaurant.Models
{
    /// <summary>
    /// Represents the relationship between menus and their dishes with specific portions
    /// </summary>
    public class MenuDish
    {
        public int MenuId { get; set; }
        public int DishId { get; set; }
        public double PortionSize { get; set; }

        public virtual Menu Menu { get; set; }
        public virtual Dish Dish { get; set; }
    }
}
