using System.Collections.Generic;
using WindowsFormsApp1.Models;

namespace WindowsFormsApp1.Data
{
    /// <summary>
    /// Everything the UI is allowed to do with the Books table, expressed as
    /// an interface so forms depend on this contract instead of on ADO.NET
    /// or on a concrete class. This is also the seam a unit test would use
    /// (a fake IBookRepository, no real SQL Server needed).
    /// </summary>
    public interface IBookRepository
    {
        List<Book> GetAll(string search);
        List<Book> GetForCombo();
        Book GetById(int id);
        void Insert(Book book);
        void Update(Book book);
        void Delete(int id);

        // Multi-criteria filter, relational way (one SQL query built from Criteria).
        List<Book> GetFiltered(BookFilterCriteria criteria);

        List<string> GetDistinctJanrs();
        List<string> GetDistinctRemarks();

        // Bulk operations (Lab 3 + its self-practice tasks).
        void ApplyDiscountToAll(decimal percent);
        void ApplyDiscountAbovePrice(decimal percent, decimal priceThreshold);
        void ChangeGenreBulk(string fromGenre, string toGenre);
        void DeleteOlderThan(int year);
        void DeleteByPublisher(string publisher);
        void DeleteAll();
    }
}
