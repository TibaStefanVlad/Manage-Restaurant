using System;
using System.Collections.Generic;

namespace Restaurant.Models
{
    /// <summary>
    /// Represents a food allergen (e.g., gluten, nuts)
    /// </summary>
    public class Allergen
    {
        public int AllergenId { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }

        public virtual ICollection<DishAllergen> DishAllergens { get; set; }

        public Allergen()
        {
            DishAllergens = new HashSet<DishAllergen>();
        }
    }
}
