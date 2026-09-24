namespace WindowsFormsApp1.Models
{
    /// <summary>Plain data holder for one row of dbo.Customers.</summary>
    public class Customer
    {
        public int Id { get; set; }
        public string FullName { get; set; }
        public string Phone { get; set; }
        public int? DiscountId { get; set; }

        /// <summary>
        /// Populated only by queries that join Discounts (e.g. the grid listing).
        /// Not persisted — this is display data, not a column on Customers.
        /// </summary>
        public string DiscountName { get; set; }
    }
}
