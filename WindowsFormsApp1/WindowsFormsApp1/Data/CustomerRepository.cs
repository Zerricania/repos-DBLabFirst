using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using Microsoft.Data.SqlClient;
using WindowsFormsApp1.Models;

namespace WindowsFormsApp1.Data
{
    public class CustomerRepository : SqlRepositoryBase, ICustomerRepository
    {
        public List<Customer> GetAll(string search)
        {
            string sql = @"
                SELECT c.Id, c.FullName, c.Phone, c.DiscountId, ISNULL(d.DName, N'—') AS DiscountName
                FROM dbo.Customers c
                LEFT JOIN dbo.Discounts d ON c.DiscountId = d.Id
                WHERE (@s = N'' OR c.FullName LIKE @like OR c.Phone LIKE @like)
                ORDER BY c.Id";

            DataTable table = ExecuteQuery(sql,
                new SqlParameter("@s", search ?? ""),
                new SqlParameter("@like", "%" + search + "%"));

            return table.Rows.Cast<DataRow>().Select(row => new Customer
            {
                Id = Convert.ToInt32(row["Id"]),
                FullName = AsString(row, "FullName"),
                Phone = AsString(row, "Phone"),
                DiscountId = AsNullableInt(row, "DiscountId"),
                DiscountName = AsString(row, "DiscountName")
            }).ToList();
        }

        public List<Customer> GetForCombo()
        {
            DataTable table = ExecuteQuery("SELECT Id, FullName FROM dbo.Customers ORDER BY FullName");

            return table.Rows.Cast<DataRow>().Select(row => new Customer
            {
                Id = Convert.ToInt32(row["Id"]),
                FullName = AsString(row, "FullName")
            }).ToList();
        }

        public Customer GetById(int id)
        {
            DataRow row = FirstRowOrNull(ExecuteQuery(
                "SELECT Id, FullName, Phone, DiscountId FROM dbo.Customers WHERE Id=@id",
                new SqlParameter("@id", id)));

            if (row == null) return null;

            return new Customer
            {
                Id = Convert.ToInt32(row["Id"]),
                FullName = AsString(row, "FullName"),
                Phone = AsString(row, "Phone"),
                DiscountId = AsNullableInt(row, "DiscountId")
            };
        }

        public void Insert(Customer customer)
        {
            ExecuteNonQuery(@"
                INSERT INTO dbo.Customers (FullName, Phone, DiscountId)
                VALUES (@f, @p, @d)",
                new SqlParameter("@f", customer.FullName),
                new SqlParameter("@p", (object)customer.Phone ?? DBNull.Value),
                new SqlParameter("@d", (object)customer.DiscountId ?? DBNull.Value));
        }

        public void Update(Customer customer)
        {
            ExecuteNonQuery(@"
                UPDATE dbo.Customers
                SET FullName=@f, Phone=@p, DiscountId=@d
                WHERE Id=@id",
                new SqlParameter("@f", customer.FullName),
                new SqlParameter("@p", (object)customer.Phone ?? DBNull.Value),
                new SqlParameter("@d", (object)customer.DiscountId ?? DBNull.Value),
                new SqlParameter("@id", customer.Id));
        }

        public void Delete(int id)
        {
            ExecuteNonQuery("DELETE FROM dbo.Customers WHERE Id=@id", new SqlParameter("@id", id));
        }
    }
}
