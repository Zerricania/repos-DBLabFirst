using System;
using System.Drawing;
using System.Windows.Forms;
using Microsoft.Data.SqlClient;

namespace WindowsFormsApp1
{
    /// <summary>
    /// Non-generic UI shell shared by every "list + search + add/edit/delete" page.
    /// Kept non-generic on purpose so Form1 can hold Books/Customers/Discounts/
    /// SoldBooks pages in a single Dictionary&lt;string, CrudPageBase&gt; even though
    /// each one is strongly typed to a different model (see CrudPageBase&lt;T&gt; below).
    /// </summary>
    public abstract class CrudPageBase : UserControl
    {
        protected TextBox txtSearch;
        protected Button btnSearch;
        protected Button btnRefresh;
        protected Button btnAdd;
        protected Button btnEdit;
        protected Button btnDelete;
        protected DataGridView grid;
        protected Panel topPanel;

        /// <summary>
        /// Height reserved for a second row of page-specific buttons.
        /// Override together with BuildExtraControls to add commands
        /// beyond the standard add/edit/delete set (e.g. BooksPage's
        /// filter and bulk-operations buttons).
        /// </summary>
        protected virtual int ExtraTopPanelHeight => 0;

        protected CrudPageBase()
        {
            Dock = DockStyle.Fill;
            BuildLayout();
            WireEvents();
        }

        private void BuildLayout()
        {
            topPanel = new Panel
            {
                Dock = DockStyle.Top,
                Height = 46 + ExtraTopPanelHeight,
                Padding = new Padding(10, 8, 10, 8)
            };

            Label lblSearch = new Label { Text = "Поиск:", AutoSize = true, Location = new Point(10, 14) };
            txtSearch = new TextBox { Location = new Point(60, 10), Width = 220 };
            btnSearch = new Button { Text = "Найти", Location = new Point(290, 8), Width = 80 };
            btnRefresh = new Button { Text = "Сбросить", Location = new Point(375, 8), Width = 90 };
            btnAdd = new Button { Text = "Добавить", Location = new Point(490, 8), Width = 100 };
            btnEdit = new Button { Text = "Изменить", Location = new Point(595, 8), Width = 100 };
            btnDelete = new Button { Text = "Удалить", Location = new Point(700, 8), Width = 100 };

            topPanel.Controls.Add(lblSearch);
            topPanel.Controls.Add(txtSearch);
            topPanel.Controls.Add(btnSearch);
            topPanel.Controls.Add(btnRefresh);
            topPanel.Controls.Add(btnAdd);
            topPanel.Controls.Add(btnEdit);
            topPanel.Controls.Add(btnDelete);

            if (ExtraTopPanelHeight > 0)
            {
                Panel extraRow = new Panel
                {
                    Location = new Point(0, 44),
                    Width = 900,
                    Height = ExtraTopPanelHeight
                };
                topPanel.Controls.Add(extraRow);
                BuildExtraControls(extraRow);
            }

            grid = new DataGridView
            {
                Dock = DockStyle.Fill,
                ReadOnly = true,
                AllowUserToAddRows = false,
                AllowUserToDeleteRows = false,
                AutoGenerateColumns = true,
                AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill,
                SelectionMode = DataGridViewSelectionMode.FullRowSelect,
                MultiSelect = false,
                RowHeadersWidth = 40
            };

            Controls.Add(grid);
            Controls.Add(topPanel);
        }

        /// <summary>Hook for pages that need extra commands beyond add/edit/delete.</summary>
        protected virtual void BuildExtraControls(Panel extraRow)
        {
        }

        /// <summary>
        /// Hook called when the user clicks "Сбросить" (before ReloadData).
        /// Pages that keep extra state alongside the search box (e.g. an
        /// active advanced filter) override this to clear it.
        /// </summary>
        protected virtual void ResetExtraState()
        {
        }

        private void WireEvents()
        {
            btnSearch.Click += (s, e) => ReloadData();

            txtSearch.KeyDown += (s, e) =>
            {
                if (e.KeyCode == Keys.Enter)
                {
                    e.SuppressKeyPress = true;
                    ReloadData();
                }
            };

            btnRefresh.Click += (s, e) =>
            {
                txtSearch.Text = "";
                ResetExtraState();
                ReloadData();
            };

            btnAdd.Click += (s, e) =>
            {
                if (ShowEditDialog(null)) ReloadData();
            };

            btnEdit.Click += (s, e) =>
            {
                int? id = GetSelectedId();
                if (id == null)
                {
                    MessageBox.Show("Сначала выберите строку в таблице.", "Изменение",
                        MessageBoxButtons.OK, MessageBoxIcon.Information);
                    return;
                }

                if (ShowEditDialog(id)) ReloadData();
            };

            btnDelete.Click += (s, e) =>
            {
                int? id = GetSelectedId();
                if (id == null)
                {
                    MessageBox.Show("Сначала выберите строку в таблице.", "Удаление",
                        MessageBoxButtons.OK, MessageBoxIcon.Information);
                    return;
                }

                DialogResult confirm = MessageBox.Show(
                    "Удалить выбранную запись?", "Подтверждение",
                    MessageBoxButtons.YesNo, MessageBoxIcon.Question);

                if (confirm != DialogResult.Yes) return;

                try
                {
                    DeleteRow(id.Value);
                    ReloadData();
                }
                catch (SqlException)
                {
                    MessageBox.Show(
                        "Не удалось удалить запись: она используется в другой таблице " +
                        "(например, книга есть в продажах, или клиент/скидка на неё ссылается).",
                        "Ошибка удаления", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            };
        }

        /// <summary>
        /// Reloads the grid. Called from outside (page switch in Form1) and
        /// from inside (after add/edit/delete/filter/bulk operation).
        /// </summary>
        public abstract void ReloadData();

        protected int? GetSelectedId()
        {
            if (grid.CurrentRow == null) return null;
            DataGridViewCell cell = grid.CurrentRow.Cells["Id"];
            if (cell == null || cell.Value == null || cell.Value == DBNull.Value) return null;
            return Convert.ToInt32(cell.Value);
        }

        /// <summary>Show add (id == null) or edit (id given) dialog. Returns true if data changed.</summary>
        protected abstract bool ShowEditDialog(int? id);

        /// <summary>Delete a record by Id.</summary>
        protected abstract void DeleteRow(int id);
    }

    /// <summary>
    /// Strongly-typed half of the CRUD page: knows how to load a List&lt;T&gt;
    /// and bind it to the grid. Concrete pages (BooksPage, CustomersPage, ...)
    /// derive from this and only implement LoadData/ConfigureColumns/
    /// ShowEditDialog/DeleteRow — everything else (buttons, search box,
    /// delete confirmation, FK-violation handling) is inherited.
    /// </summary>
    public abstract class CrudPageBase<T> : CrudPageBase where T : class
    {
        protected abstract System.Collections.Generic.List<T> LoadData(string search);
        protected abstract void ConfigureColumns();

        public override void ReloadData()
        {
            try
            {
                grid.DataSource = LoadData(txtSearch.Text.Trim());
                ConfigureColumns();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Ошибка загрузки данных:\n\n" + ex.Message, "Ошибка",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
    }
}
