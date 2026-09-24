using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using Microsoft.Data.SqlClient;
using WindowsFormsApp1.Models;

namespace WindowsFormsApp1.Data
{
    /// <summary>
    /// SQL Server implementation of IBookRepository. All Books SQL lives here
    /// and nowhere else in the project.
    /// </summary>
    public class BookRepository : SqlRepositoryBase, IBookRepository
    {
        private const string SelectColumns =
            "Id, Title, Author, Izdatelstvo, Year, Price, Janr, Remarks";

        public List<Book> GetAll(string search)
        {
            string sql = $@"
                SELECT {SelectColumns}
                FROM dbo.Books
                WHERE (@s = N'' OR Title LIKE @like OR Author LIKE @like OR Janr LIKE @like)
                ORDER BY Id";

            DataTable table = ExecuteQuery(sql,
                new SqlParameter("@s", search ?? ""),
                new SqlParameter("@like", "%" + search + "%"));

            return MapRows(table);
        }

        public List<Book> GetForCombo()
        {
            DataTable table = ExecuteQuery("SELECT Id, Title, Price FROM dbo.Books ORDER BY Title");

            return table.Rows.Cast<DataRow>()
                .Select(row => new Book
                {
                    Id = Convert.ToInt32(row["Id"]),
                    Title = AsString(row, "Title"),
                    Price = Convert.ToDecimal(row["Price"])
                })
                .ToList();
        }

        public Book GetById(int id)
        {
            DataRow row = FirstRowOrNull(ExecuteQuery(
                $"SELECT {SelectColumns} FROM dbo.Books WHERE Id=@id",
                new SqlParameter("@id", id)));

            return row == null ? null : MapRow(row);
        }

        public void Insert(Book book)
        {
            ExecuteNonQuery(@"
                INSERT INTO dbo.Books (Title, Author, Izdatelstvo, Year, Price, Janr, Remarks)
                VALUES (@t, @a, @i, @y, @p, @j, @r)",
                new SqlParameter("@t", book.Title),
                new SqlParameter("@a", book.Author),
                new SqlParameter("@i", (object)book.Izdatelstvo ?? DBNull.Value),
                new SqlParameter("@y", book.Year),
                new SqlParameter("@p", book.Price),
                new SqlParameter("@j", (object)book.Janr ?? DBNull.Value),
                new SqlParameter("@r", (object)book.Remarks ?? DBNull.Value));
        }

        public void Update(Book book)
        {
            ExecuteNonQuery(@"
                UPDATE dbo.Books
                SET Title=@t, Author=@a, Izdatelstvo=@i, Year=@y, Price=@p, Janr=@j, Remarks=@r
                WHERE Id=@id",
                new SqlParameter("@t", book.Title),
                new SqlParameter("@a", book.Author),
                new SqlParameter("@i", (object)book.Izdatelstvo ?? DBNull.Value),
                new SqlParameter("@y", book.Year),
                new SqlParameter("@p", book.Price),
                new SqlParameter("@j", (object)book.Janr ?? DBNull.Value),
                new SqlParameter("@r", (object)book.Remarks ?? DBNull.Value),
                new SqlParameter("@id", book.Id));
        }

        public void Delete(int id)
        {
            ExecuteNonQuery("DELETE FROM dbo.Books WHERE Id=@id", new SqlParameter("@id", id));
        }

        public List<string> GetDistinctJanrs()
        {
            DataTable table = ExecuteQuery(
                "SELECT DISTINCT Janr FROM dbo.Books WHERE Janr IS NOT NULL AND Janr <> N'' ORDER BY Janr");

            return table.Rows.Cast<DataRow>().Select(r => r["Janr"].ToString()).ToList();
        }

        public List<string> GetDistinctRemarks()
        {
            DataTable table = ExecuteQuery(
                "SELECT DISTINCT Remarks FROM dbo.Books WHERE Remarks IS NOT NULL AND Remarks <> N'' ORDER BY Remarks");

            return table.Rows.Cast<DataRow>().Select(r => r["Remarks"].ToString()).ToList();
        }

        /// <summary>
        /// Multi-criteria filter, relational way: one SQL query is built from
        /// Criteria and sent to the server. Column names never come from the
        /// user directly — Criteria.SortField is restricted to a fixed
        /// whitelist of property names (see BookFilterCriteria.AllowedSortFields),
        /// so string-building the ORDER BY clause here is safe.
        /// </summary>
        public List<Book> GetFiltered(BookFilterCriteria c)
        {
            List<string> conditions = new List<string>();
            List<SqlParameter> pars = new List<SqlParameter>();

            if (c.FilterTitle && !string.IsNullOrWhiteSpace(c.Title))
            {
                conditions.Add("Title LIKE @title");
                pars.Add(new SqlParameter("@title", "%" + c.Title + "%"));
            }

            if (c.FilterAuthor && !string.IsNullOrWhiteSpace(c.Author))
            {
                conditions.Add("Author LIKE @author");
                pars.Add(new SqlParameter("@author", "%" + c.Author + "%"));
            }

            if (c.FilterIzdatelstvo && !string.IsNullOrWhiteSpace(c.Izdatelstvo))
            {
                conditions.Add("Izdatelstvo LIKE @izd");
                pars.Add(new SqlParameter("@izd", "%" + c.Izdatelstvo + "%"));
            }

            if (c.FilterYear && c.Year.HasValue)
            {
                conditions.Add("Year = @year");
                pars.Add(new SqlParameter("@year", c.Year.Value));
            }

            if (c.FilterPrice && c.PriceFrom.HasValue && c.PriceTo.HasValue)
            {
                conditions.Add("Price BETWEEN @pf AND @pt");
                pars.Add(new SqlParameter("@pf", c.PriceFrom.Value));
                pars.Add(new SqlParameter("@pt", c.PriceTo.Value));
            }

            if (c.FilterJanr && !string.IsNullOrWhiteSpace(c.Janr))
            {
                conditions.Add("Janr = @janr");
                pars.Add(new SqlParameter("@janr", c.Janr));
            }

            if (c.FilterRemarks && !string.IsNullOrWhiteSpace(c.Remarks))
            {
                conditions.Add("Remarks = @remarks");
                pars.Add(new SqlParameter("@remarks", c.Remarks));
            }

            string sql = $"SELECT {SelectColumns} FROM dbo.Books";

            if (conditions.Count > 0)
            {
                sql += " WHERE " + string.Join(" AND ", conditions);
            }

            if (!string.IsNullOrEmpty(c.SortField) && BookFilterCriteria.AllowedSortFields.Contains(c.SortField))
            {
                sql += " ORDER BY " + c.SortField + (c.SortDescending ? " DESC" : " ASC");
            }
            else
            {
                sql += " ORDER BY Id";
            }

            return MapRows(ExecuteQuery(sql, pars.ToArray()));
        }

        // ---- Bulk operations (Lab 3, including the self-practice tasks) ----

        public void ApplyDiscountToAll(decimal percent)
        {
            ExecuteNonQuery(
                "UPDATE dbo.Books SET Price = Price - Price * (@p / 100.0)",
                new SqlParameter("@p", percent));
        }

        public void ApplyDiscountAbovePrice(decimal percent, decimal priceThreshold)
        {
            ExecuteNonQuery(
                "UPDATE dbo.Books SET Price = Price - Price * (@p / 100.0) WHERE Price > @t",
                new SqlParameter("@p", percent),
                new SqlParameter("@t", priceThreshold));
        }

        public void ChangeGenreBulk(string fromGenre, string toGenre)
        {
            ExecuteNonQuery(
                "UPDATE dbo.Books SET Janr = @to WHERE Janr = @from",
                new SqlParameter("@to", toGenre),
                new SqlParameter("@from", fromGenre));
        }

        public void DeleteOlderThan(int year)
        {
            ExecuteNonQuery(
                "DELETE FROM dbo.Books WHERE Year < @year",
                new SqlParameter("@year", year));
        }

        public void DeleteByPublisher(string publisher)
        {
            ExecuteNonQuery(
                "DELETE FROM dbo.Books WHERE Izdatelstvo = @publisher",
                new SqlParameter("@publisher", publisher));
        }

        public void DeleteAll()
        {
            ExecuteNonQuery("DELETE FROM dbo.Books");
        }

        private static List<Book> MapRows(DataTable table)
        {
            return table.Rows.Cast<DataRow>().Select(MapRow).ToList();
        }

        private static Book MapRow(DataRow row)
        {
            return new Book
            {
                Id = Convert.ToInt32(row["Id"]),
                Title = AsString(row, "Title"),
                Author = AsString(row, "Author"),
                Izdatelstvo = AsString(row, "Izdatelstvo"),
                Year = Convert.ToInt32(row["Year"]),
                Price = Convert.ToDecimal(row["Price"]),
                Janr = AsString(row, "Janr"),
                Remarks = AsString(row, "Remarks")
            };
        }
    }
}
