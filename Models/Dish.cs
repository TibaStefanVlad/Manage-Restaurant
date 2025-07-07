using System;
using System.Collections.Generic;

namespace Restaurant.Models
{
    /// <summary>
    /// Represents a single dish/food item in the restaurant
    /// </summary>
    public class Dish
    {
        public int DishId { get; set; }
        public string Name { get; set; }
        public decimal Price { get; set; }
        public double PortionSize { get; set; }  
        public string PortionUnit { get; set; }  
        public double TotalQuantity { get; set; } 
        public int CategoryId { get; set; }
        public bool IsAvailable { get; set; }
        public string Description { get; set; }

        public virtual Category Category { get; set; }
        public virtual ICollection<DishPhoto> Photos { get; set; }
        public virtual ICollection<DishAllergen> DishAllergens { get; set; }
        public virtual ICollection<MenuDish> MenuDishes { get; set; }

        public Dish()
        {
            Photos = new HashSet<DishPhoto>();
            DishAllergens = new HashSet<DishAllergen>();
            MenuDishes = new HashSet<MenuDish>();
            IsAvailable = true;
        }
    }
}