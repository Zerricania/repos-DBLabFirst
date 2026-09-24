using System.Collections.Generic;
using WindowsFormsApp1.Models;

namespace WindowsFormsApp1.Data
{
    public interface ISoldBookRepository
    {
        List<SoldBook> GetAll(string search);
        SoldBook GetById(int id);
        void Insert(SoldBook soldBook);
        void Update(SoldBook soldBook);
        void Delete(int id);
    }
}
