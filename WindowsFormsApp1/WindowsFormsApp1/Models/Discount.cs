namespace WindowsFormsApp1.Models
{
    /// <summary>Plain data holder for one row of dbo.Discounts.</summary>
    public class Discount
    {
        public int Id { get; set; }
        public string DName { get; set; }
        public int Percents { get; set; }
    }
}
