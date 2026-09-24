using System;

namespace WindowsFormsApp1.Models
{
    /// <summary>Plain data holder for one row of dbo.SoldBooks.</summary>
    public class SoldBook
    {
        public int Id { get; set; }
        public int BookId { get; set; }
        public int CustomerId { get; set; }
        public DateTime SaleDate { get; set; }
        public int Quantity { get; set; }
        public decimal Price { get; set; }

        /// <summary>Populated only by queries that join Books (grid listing).</summary>
        public string BookTitle { get; set; }

        /// <summary>Populated only by queries that join Customers (grid listing).</summary>
        public string CustomerName { get; set; }
    }
}
