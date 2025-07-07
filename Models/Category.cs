using System;
using System.Collections.Generic;
using System.Windows.Controls;

namespace Restaurant.Models
{
    /// <summary>
    /// Represents a category of dishes or menus in the restaurant
    /// </summary>
    public class Category
    {
        public int CategoryId { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }

        // Navigation properties
        public virtual ICollection<Dish> Dishes { get; set; }
        public virtual ICollection<Menu> Menus { get; set; }

        public Category()
        {
            Dishes = new HashSet<Dish>();
            Menus = new HashSet<Menu>();
        }
    }
}
