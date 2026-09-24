using System.Collections.Generic;
using WindowsFormsApp1.Models;

namespace WindowsFormsApp1.Data
{
    public interface IDiscountRepository
    {
        List<Discount> GetAll(string search);
        List<Discount> GetForCombo();
        Discount GetById(int id);
        void Insert(Discount discount);
        void Update(Discount discount);
        void Delete(int id);
    }
}
