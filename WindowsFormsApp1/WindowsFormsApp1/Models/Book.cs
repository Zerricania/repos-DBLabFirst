namespace WindowsFormsApp1.Models
{
    /// <summary>
    /// Plain data holder for one row of dbo.Books. Carries no database
    /// or UI logic — repositories map DataRow -> Book, and forms bind to
    /// its properties instead of indexing a DataRow by column name.
    /// </summary>
    public class Book
    {
        public int Id { get; set; }
        public string Title { get; set; }
        public string Author { get; set; }
        public string Izdatelstvo { get; set; }
        public int Year { get; set; }
        public decimal Price { get; set; }
        public string Janr { get; set; }
        public string Remarks { get; set; }
    }
}
