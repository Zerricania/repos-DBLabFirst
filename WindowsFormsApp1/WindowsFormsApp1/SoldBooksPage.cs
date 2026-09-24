using System.Collections.Generic;
using System.Windows.Forms;
using WindowsFormsApp1.Data;
using WindowsFormsApp1.Models;

namespace WindowsFormsApp1
{
    public class SoldBooksPage : CrudPageBase<SoldBook>
    {
        private readonly ISoldBookRepository _repository = new SoldBookRepository();
        private readonly IBookRepository _bookRepository = new BookRepository();
        private readonly ICustomerRepository _customerRepository = new CustomerRepository();

        protected override List<SoldBook> LoadData(string search)
        {
            return _repository.GetAll(search);
        }

        protected override void ConfigureColumns()
        {
            if (grid.Columns["Id"] != null) grid.Columns["Id"].HeaderText = "ID";
            if (grid.Columns["BookTitle"] != null) grid.Columns["BookTitle"].HeaderText = "Книга";
            if (grid.Columns["CustomerName"] != null) grid.Columns["CustomerName"].HeaderText = "Клиент";
            if (grid.Columns["SaleDate"] != null)
            {
                grid.Columns["SaleDate"].HeaderText = "Дата продажи";
                grid.Columns["SaleDate"].DefaultCellStyle.Format = "dd.MM.yyyy";
            }
            if (grid.Columns["Quantity"] != null) grid.Columns["Quantity"].HeaderText = "Кол-во";
            if (grid.Columns["Price"] != null) grid.Columns["Price"].HeaderText = "Цена";
            if (grid.Columns["BookId"] != null) grid.Columns["BookId"].Visible = false;
            if (grid.Columns["CustomerId"] != null) grid.Columns["CustomerId"].Visible = false;
        }

        protected override bool ShowEditDialog(int? id)
        {
            using (SoldBookEditForm form = new SoldBookEditForm(_repository, _bookRepository, _customerRepository, id))
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
