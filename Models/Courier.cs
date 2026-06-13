using System;
using System.Collections.Generic;

namespace LogisticsSystem.Models
{
    public partial class Courier
    {
        public int CourierId { get; set; }
        
        public string Name { get; set; } = null!;
        
        
        public string Username { get; set; } = null!;
        
        public string? Phone { get; set; }
        
        public string? Zone { get; set; }

        
    }
}