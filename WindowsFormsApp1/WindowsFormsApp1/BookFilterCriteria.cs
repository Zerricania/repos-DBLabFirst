using System;
using System.Collections.Generic;
using System.Linq;
using WindowsFormsApp1.Models;

namespace WindowsFormsApp1
{
    /// <summary>
    /// Criteria for the multi-criteria book filter: which fields are enabled,
    /// what to search for, how to sort, and which access method to demonstrate
    /// (relational — SQL query on the server, or navigational — LINQ over an
    /// already-loaded in-memory list).
    /// </summary>
    public class BookFilterCriteria
    {
        /// <summary>
        /// Only these property names are allowed in a hand-built ORDER BY clause
        /// (see BookRepository.GetFiltered). The UI only ever offers these four
        /// via radio buttons, but the whitelist is enforced here — in the data
        /// layer — rather than trusted from the caller.
        /// </summary>
        public static readonly HashSet<string> AllowedSortFields =
            new HashSet<string> { "Title", "Author", "Year", "Price" };

        public bool FilterTitle;
        public string Title;

        public bool FilterAuthor;
        public string Author;

        public bool FilterIzdatelstvo;
        public string Izdatelstvo;

        public bool FilterYear;
        public int? Year;

        public bool FilterPrice;
        public decimal? PriceFrom;
        public decimal? PriceTo;

        public bool FilterJanr;
        public string Janr;

        public bool FilterRemarks;
        public string Remarks;

        /// <summary>One of: null, "Title", "Author", "Year", "Price".</summary>
        public string SortField;
        public bool SortDescending;

        /// <summary>true = navigational (LINQ in memory), false = relational (SQL).</summary>
        public bool UseNavigational;

        /// <summary>
        /// Navigational-mode predicate: does this book pass every enabled filter?
        /// Kept here, next to the criteria it evaluates, so BooksPage does not
        /// need to know how filtering works — it just calls Matches.
        /// </summary>
        public bool Matches(Book book)
        {
            if (FilterTitle && !string.IsNullOrWhiteSpace(Title) &&
                (book.Title == null || book.Title.IndexOf(Title, StringComparison.OrdinalIgnoreCase) < 0))
                return false;

            if (FilterAuthor && !string.IsNullOrWhiteSpace(Author) &&
                (book.Author == null || book.Author.IndexOf(Author, StringComparison.OrdinalIgnoreCase) < 0))
                return false;

            if (FilterIzdatelstvo && !string.IsNullOrWhiteSpace(Izdatelstvo) &&
                (book.Izdatelstvo == null || book.Izdatelstvo.IndexOf(Izdatelstvo, StringComparison.OrdinalIgnoreCase) < 0))
                return false;

            if (FilterYear && Year.HasValue && book.Year != Year.Value)
                return false;

            if (FilterPrice && PriceFrom.HasValue && PriceTo.HasValue &&
                (book.Price < PriceFrom.Value || book.Price > PriceTo.Value))
                return false;

            if (FilterJanr && !string.IsNullOrWhiteSpace(Janr) && book.Janr != Janr)
                return false;

            if (FilterRemarks && !string.IsNullOrWhiteSpace(Remarks) && book.Remarks != Remarks)
                return false;

            return true;
        }

        /// <summary>Applies Matches + sorting to an in-memory list (navigational mode).</summary>
        public List<Book> ApplyTo(IEnumerable<Book> books)
        {
            IEnumerable<Book> filtered = books.Where(Matches);

            switch (SortField)
            {
                case "Title": filtered = SortDescending ? filtered.OrderByDescending(b => b.Title) : filtered.OrderBy(b => b.Title); break;
                case "Author": filtered = SortDescending ? filtered.OrderByDescending(b => b.Author) : filtered.OrderBy(b => b.Author); break;
                case "Year": filtered = SortDescending ? filtered.OrderByDescending(b => b.Year) : filtered.OrderBy(b => b.Year); break;
                case "Price": filtered = SortDescending ? filtered.OrderByDescending(b => b.Price) : filtered.OrderBy(b => b.Price); break;
                default: filtered = filtered.OrderBy(b => b.Id); break;
            }

            return filtered.ToList();
        }
    }
}
