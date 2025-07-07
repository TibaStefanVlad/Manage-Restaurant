using System;
using System.Collections.Generic;

namespace Restaurant.Models
{
    /// <summary>
    /// Represents a special menu composed of multiple dishes
    /// </summary>
    public class Menu
    {
        public int MenuId { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public decimal Price { get; set; }  
        public bool IsAvailable { get; set; }
        public int CategoryId { get; set; }

        public virtual Category Category { get; set; }
        public virtual ICollection<MenuDish> MenuDishes { get; set; }

        public Menu()
        {
            MenuDishes = new HashSet<MenuDish>();
            IsAvailable = true;
        }
    }
}
