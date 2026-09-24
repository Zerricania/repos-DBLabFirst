using System.Collections.Generic;
using System.Windows.Forms;
using WindowsFormsApp1.Data;
using WindowsFormsApp1.Models;

namespace WindowsFormsApp1
{
    public class DiscountsPage : CrudPageBase<Discount>
    {
        private readonly IDiscountRepository _repository = new DiscountRepository();

        protected override List<Discount> LoadData(string search)
        {
            return _repository.GetAll(search);
        }

        protected override void ConfigureColumns()
        {
            if (grid.Columns["Id"] != null) grid.Columns["Id"].HeaderText = "ID";
            if (grid.Columns["DName"] != null) grid.Columns["DName"].HeaderText = "Название скидки";
            if (grid.Columns["Percents"] != null) grid.Columns["Percents"].HeaderText = "Процент";
        }

        protected override bool ShowEditDialog(int? id)
        {
            using (DiscountEditForm form = new DiscountEditForm(_repository, id))
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
