using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using Microsoft.Data.SqlClient;
using WindowsFormsApp1.Models;

namespace WindowsFormsApp1.Data
{
    public class DiscountRepository : SqlRepositoryBase, IDiscountRepository
    {
        public List<Discount> GetAll(string search)
        {
            string sql = @"
                SELECT Id, DName, Percents
                FROM dbo.Discounts
                WHERE (@s = N'' OR DName LIKE @like)
                ORDER BY Id";

            DataTable table = ExecuteQuery(sql,
                new SqlParameter("@s", search ?? ""),
                new SqlParameter("@like", "%" + search + "%"));

            return MapRows(table);
        }

        public List<Discount> GetForCombo()
        {
            return MapRows(ExecuteQuery("SELECT Id, DName, Percents FROM dbo.Discounts ORDER BY DName"));
        }

        public Discount GetById(int id)
        {
            DataRow row = FirstRowOrNull(ExecuteQuery(
                "SELECT Id, DName, Percents FROM dbo.Discounts WHERE Id=@id",
                new SqlParameter("@id", id)));

            return row == null ? null : MapRow(row);
        }

        public void Insert(Discount discount)
        {
            ExecuteNonQuery("INSERT INTO dbo.Discounts (DName, Percents) VALUES (@n, @p)",
                new SqlParameter("@n", discount.DName),
                new SqlParameter("@p", discount.Percents));
        }

        public void Update(Discount discount)
        {
            ExecuteNonQuery("UPDATE dbo.Discounts SET DName=@n, Percents=@p WHERE Id=@id",
                new SqlParameter("@n", discount.DName),
                new SqlParameter("@p", discount.Percents),
                new SqlParameter("@id", discount.Id));
        }

        public void Delete(int id)
        {
            ExecuteNonQuery("DELETE FROM dbo.Discounts WHERE Id=@id", new SqlParameter("@id", id));
        }

        private static List<Discount> MapRows(DataTable table)
        {
            return table.Rows.Cast<DataRow>().Select(MapRow).ToList();
        }

        private static Discount MapRow(DataRow row)
        {
            return new Discount
            {
                Id = Convert.ToInt32(row["Id"]),
                DName = AsString(row, "DName"),
                Percents = Convert.ToInt32(row["Percents"])
            };
        }
    }
}
