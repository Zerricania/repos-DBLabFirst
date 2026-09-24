using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using Microsoft.Data.SqlClient;
using WindowsFormsApp1.Models;

namespace WindowsFormsApp1.Data
{
    public class SoldBookRepository : SqlRepositoryBase, ISoldBookRepository
    {
        public List<SoldBook> GetAll(string search)
        {
            string sql = @"
                SELECT sb.Id, sb.BookId, sb.CustomerId, b.Title AS BookTitle, c.FullName AS CustomerName,
                       sb.SaleDate, sb.Quantity, sb.Price
                FROM dbo.SoldBooks sb
                JOIN dbo.Books b ON sb.BookId = b.Id
                JOIN dbo.Customers c ON sb.CustomerId = c.Id
                WHERE (@s = N'' OR b.Title LIKE @like OR c.FullName LIKE @like)
                ORDER BY sb.Id";

            DataTable table = ExecuteQuery(sql,
                new SqlParameter("@s", search ?? ""),
                new SqlParameter("@like", "%" + search + "%"));

            return table.Rows.Cast<DataRow>().Select(row => new SoldBook
            {
                Id = Convert.ToInt32(row["Id"]),
                BookId = Convert.ToInt32(row["BookId"]),
                CustomerId = Convert.ToInt32(row["CustomerId"]),
                BookTitle = AsString(row, "BookTitle"),
                CustomerName = AsString(row, "CustomerName"),
                SaleDate = Convert.ToDateTime(row["SaleDate"]),
                Quantity = Convert.ToInt32(row["Quantity"]),
                Price = Convert.ToDecimal(row["Price"])
            }).ToList();
        }

        public SoldBook GetById(int id)
        {
            DataRow row = FirstRowOrNull(ExecuteQuery(
                "SELECT Id, BookId, CustomerId, SaleDate, Quantity, Price FROM dbo.SoldBooks WHERE Id=@id",
                new SqlParameter("@id", id)));

            if (row == null) return null;

            return new SoldBook
            {
                Id = Convert.ToInt32(row["Id"]),
                BookId = Convert.ToInt32(row["BookId"]),
                CustomerId = Convert.ToInt32(row["CustomerId"]),
                SaleDate = Convert.ToDateTime(row["SaleDate"]),
                Quantity = Convert.ToInt32(row["Quantity"]),
                Price = Convert.ToDecimal(row["Price"])
            };
        }

        public void Insert(SoldBook soldBook)
        {
            ExecuteNonQuery(@"
                INSERT INTO dbo.SoldBooks (BookId, CustomerId, SaleDate, Quantity, Price)
                VALUES (@b, @c, @d, @q, @p)",
                new SqlParameter("@b", soldBook.BookId),
                new SqlParameter("@c", soldBook.CustomerId),
                new SqlParameter("@d", soldBook.SaleDate),
                new SqlParameter("@q", soldBook.Quantity),
                new SqlParameter("@p", soldBook.Price));
        }

        public void Update(SoldBook soldBook)
        {
            ExecuteNonQuery(@"
                UPDATE dbo.SoldBooks
                SET BookId=@b, CustomerId=@c, SaleDate=@d, Quantity=@q, Price=@p
                WHERE Id=@id",
                new SqlParameter("@b", soldBook.BookId),
                new SqlParameter("@c", soldBook.CustomerId),
                new SqlParameter("@d", soldBook.SaleDate),
                new SqlParameter("@q", soldBook.Quantity),
                new SqlParameter("@p", soldBook.Price),
                new SqlParameter("@id", soldBook.Id));
        }

        public void Delete(int id)
        {
            ExecuteNonQuery("DELETE FROM dbo.SoldBooks WHERE Id=@id", new SqlParameter("@id", id));
        }
    }
}
