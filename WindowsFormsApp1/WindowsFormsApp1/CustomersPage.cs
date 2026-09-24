using System.Collections.Generic;
using System.Windows.Forms;
using WindowsFormsApp1.Data;
using WindowsFormsApp1.Models;

namespace WindowsFormsApp1
{
    public class CustomersPage : CrudPageBase<Customer>
    {
        private readonly ICustomerRepository _repository = new CustomerRepository();
        private readonly IDiscountRepository _discountRepository = new DiscountRepository();

        protected override List<Customer> LoadData(string search)
        {
            return _repository.GetAll(search);
        }

        protected override void ConfigureColumns()
        {
            if (grid.Columns["Id"] != null) grid.Columns["Id"].HeaderText = "ID";
            if (grid.Columns["FullName"] != null) grid.Columns["FullName"].HeaderText = "ФИО";
            if (grid.Columns["Phone"] != null) grid.Columns["Phone"].HeaderText = "Телефон";
            if (grid.Columns["DiscountName"] != null) grid.Columns["DiscountName"].HeaderText = "Скидка";
            if (grid.Columns["DiscountId"] != null) grid.Columns["DiscountId"].Visible = false;
        }

        protected override bool ShowEditDialog(int? id)
        {
            using (CustomerEditForm form = new CustomerEditForm(_repository, _discountRepository, id))
            {
                return form.ShowDialog() == DialogResult.OK;
            }
        }

        protected override void DeleteRow(int id)
        {
            _repository.Delete(id);
        }
    }
}
