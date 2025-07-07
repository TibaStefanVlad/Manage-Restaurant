using System;
using System.Collections.Generic;

namespace Restaurant.Models
{
    /// <summary>
    /// Represents the relationship between dishes and allergens
    /// </summary>
    public class DishAllergen
    {
        public int DishId { get; set; }
        public int AllergenId { get; set; }

        public virtual Dish Dish { get; set; }
        public virtual Allergen Allergen { get; set; }
    }
}
