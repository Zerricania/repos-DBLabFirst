using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;
using WindowsFormsApp1.Data;
using WindowsFormsApp1.Models;

namespace WindowsFormsApp1
{
    public class BooksPage : CrudPageBase<Book>
    {
        private readonly IBookRepository _repository = new BookRepository();

        /// <summary>
        /// Set by the filter dialog; while it's non-null the grid shows the
        /// filtered result instead of a plain search. Cleared by "Сбросить"
        /// (ResetExtraState) or by "Очистить фильтр" inside the filter dialog.
        /// </summary>
        private BookFilterCriteria _activeFilter;

        protected override int ExtraTopPanelHeight => 36;

        protected override void BuildExtraControls(Panel extraRow)
        {
            Button btnFilter = new Button
            {
                Text = "Фильтр / сортировка...",
                Location = new Point(0, 2),
                Width = 180
            };
            btnFilter.Click += BtnFilter_Click;

            Button btnBulk = new Button
            {
                Text = "Массовые операции...",
                Location = new Point(190, 2),
                Width = 170
            };
            btnBulk.Click += BtnBulk_Click;

            extraRow.Controls.Add(btnFilter);
            extraRow.Controls.Add(btnBulk);
        }

        private void BtnFilter_Click(object sender, System.EventArgs e)
        {
            using (BookFilterForm form = new BookFilterForm(_repository))
            {
                if (form.ShowDialog() != DialogResult.OK) return;

                // null Criteria means "Очистить фильтр" was pressed inside the dialog.
                _activeFilter = form.Criteria;
                txtSearch.Text = "";
                ReloadData();
            }
        }

        private void BtnBulk_Click(object sender, System.EventArgs e)
        {
            using (BulkUpdateForm form = new BulkUpdateForm(_repository))
            {
                if (form.ShowDialog() == DialogResult.OK && form.ChangesMade)
                {
                    ReloadData();
                }
            }
        }

        protected override void ResetExtraState()
        {
            _activeFilter = null;
        }

        protected override List<Book> LoadData(string search)
        {
            if (_activeFilter != null)
            {
                return _activeFilter.UseNavigational
                    ? _activeFilter.ApplyTo(_repository.GetAll(""))   // load all, filter/sort in memory
                    : _repository.GetFiltered(_activeFilter);          // filter/sort on the server
            }

            return _repository.GetAll(search);
        }

        protected override void ConfigureColumns()
        {
            if (grid.Columns["Id"] != null) grid.Columns["Id"].HeaderText = "ID";
            if (grid.Columns["Title"] != null) grid.Columns["Title"].HeaderText = "Название";
            if (grid.Columns["Author"] != null) grid.Columns["Author"].HeaderText = "Автор";
            if (grid.Columns["Izdatelstvo"] != null) grid.Columns["Izdatelstvo"].HeaderText = "Издательство";
            if (grid.Columns["Year"] != null) grid.Columns["Year"].HeaderText = "Год";
            if (grid.Columns["Price"] != null) grid.Columns["Price"].HeaderText = "Цена";
            if (grid.Columns["Janr"] != null) grid.Columns["Janr"].HeaderText = "Жанр";
            if (grid.Columns["Remarks"] != null) grid.Columns["Remarks"].HeaderText = "Примечания";
        }

        protected override bool ShowEditDialog(int? id)
        {
            using (BookEditForm form = new BookEditForm(_repository, id))
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
