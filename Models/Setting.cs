using System;
using System.Collections.Generic;

namespace Restaurant.Models
{
    /// <summary>
    /// Represents application configuration settings
    /// </summary>
    public class Setting
    {
        public int SettingId { get; set; }
        public string Key { get; set; }
        public string Value { get; set; }
        public string Description { get; set; }
    }
}
