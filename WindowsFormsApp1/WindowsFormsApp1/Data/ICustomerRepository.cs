using System.Collections.Generic;
using WindowsFormsApp1.Models;

namespace WindowsFormsApp1.Data
{
    public interface ICustomerRepository
    {
        List<Customer> GetAll(string search);
        List<Customer> GetForCombo();
        Customer GetById(int id);
        void Insert(Customer customer);
        void Update(Customer customer);
        void Delete(int id);
    }
}
