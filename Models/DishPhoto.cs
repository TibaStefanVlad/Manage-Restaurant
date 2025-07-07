using System;
using System.Collections.Generic;

namespace Restaurant.Models
{
    /// <summary>
    /// Represents a photo of a dish
    /// </summary>
    public class DishPhoto
    {
        public int PhotoId { get; set; }
        public int DishId { get; set; }
        public byte[] PhotoData { get; set; }
        public string Description { get; set; }
        public bool IsPrimary { get; set; }

        public virtual Dish Dish { get; set; }
    }
}
