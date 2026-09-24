using System;
using System.Drawing;
using System.Windows.Forms;
using WindowsFormsApp1.Data;

namespace WindowsFormsApp1
{
    /// <summary>
    /// Multi-criteria filter, search and sort window for books. Lets the user
    /// pick the access method: relational (SQL, via IBookRepository.GetFiltered)
    /// or navigational (BindingSource-style, via BookFilterCriteria.ApplyTo over
    /// an in-memory list) — demonstrating both approaches, per Lab 4.
    /// </summary>
    public class BookFilterForm : Form
    {
        private readonly IBookRepository _repository;

        /// <summary>
        /// The result: filled in when "Применить" is pressed, left null when
        /// the user presses "Очистить фильтр" (meaning: go back to plain search)
        /// or cancels the dialog entirely (DialogResult stays Cancel).
        /// </summary>
        public BookFilterCriteria Criteria { get; private set; }

        private CheckBox chkTitle; private TextBox txtTitle;
        private CheckBox chkAuthor; private TextBox txtAuthor;
        private CheckBox chkIzd; private TextBox txtIzd;
        private CheckBox chkYear; private NumericUpDown numYear;
        private CheckBox chkPrice; private NumericUpDown numPriceFrom; private NumericUpDown numPriceTo;
        private CheckBox chkJanr; private ComboBox cmbJanr;
        private CheckBox chkRemarks; private ComboBox cmbRemarks;

        private RadioButton rbSortNone, rbSortTitle, rbSortAuthor, rbSortYear, rbSortPrice;
        private RadioButton rbAsc, rbDesc;
        private RadioButton rbRelational, rbNavigational;

        public BookFilterForm(IBookRepository repository)
        {
            _repository = repository;
            BuildLayout();
            LoadCombos();
        }

        private void BuildLayout()
        {
            Text = "Фильтрация, поиск и сортировка книг";
            StartPosition = FormStartPosition.CenterParent;
            FormBorderStyle = FormBorderStyle.FixedDialog;
            MaximizeBox = false;
            MinimizeBox = false;

            int y = 10;

            GroupBox gbFilter = new GroupBox { Text = "Фильтрация", Location = new Point(10, y), Size = new Size(400, 240) };

            chkTitle = new CheckBox { Text = "Название содержит:", Location = new Point(10, 25), AutoSize = true };
            txtTitle = new TextBox { Location = new Point(180, 22), Width = 200 };
            gbFilter.Controls.Add(chkTitle); gbFilter.Controls.Add(txtTitle);

            chkAuthor = new CheckBox { Text = "Автор содержит:", Location = new Point(10, 55), AutoSize = true };
            txtAuthor = new TextBox { Location = new Point(180, 52), Width = 200 };
            gbFilter.Controls.Add(chkAuthor); gbFilter.Controls.Add(txtAuthor);

            chkIzd = new CheckBox { Text = "Издательство содержит:", Location = new Point(10, 85), AutoSize = true };
            txtIzd = new TextBox { Location = new Point(180, 82), Width = 200 };
            gbFilter.Controls.Add(chkIzd); gbFilter.Controls.Add(txtIzd);

            chkYear = new CheckBox { Text = "Год выпуска =", Location = new Point(10, 115), AutoSize = true };
            numYear = new NumericUpDown
            {
                Location = new Point(180, 112),
                Width = 100,
                Minimum = 1000,
                Maximum = 2100,
                Value = DateTime.Now.Year
            };
            gbFilter.Controls.Add(chkYear); gbFilter.Controls.Add(numYear);

            chkPrice = new CheckBox { Text = "Цена от:", Location = new Point(10, 145), AutoSize = true };
            numPriceFrom = new NumericUpDown { Location = new Point(180, 142), Width = 90, Minimum = 0, Maximum = 1000000, DecimalPlaces = 2 };
            Label lblTo = new Label { Text = "до:", Location = new Point(280, 145), AutoSize = true };
            numPriceTo = new NumericUpDown { Location = new Point(310, 142), Width = 90, Minimum = 0, Maximum = 1000000, DecimalPlaces = 2, Value = 100000 };
            gbFilter.Controls.Add(chkPrice); gbFilter.Controls.Add(numPriceFrom);
            gbFilter.Controls.Add(lblTo); gbFilter.Controls.Add(numPriceTo);

            chkJanr = new CheckBox { Text = "Жанр:", Location = new Point(10, 175), AutoSize = true };
            cmbJanr = new ComboBox { Location = new Point(180, 172), Width = 200, DropDownStyle = ComboBoxStyle.DropDownList };
            gbFilter.Controls.Add(chkJanr); gbFilter.Controls.Add(cmbJanr);

            chkRemarks = new CheckBox { Text = "Примечания:", Location = new Point(10, 205), AutoSize = true };
            cmbRemarks = new ComboBox { Location = new Point(180, 202), Width = 200, DropDownStyle = ComboBoxStyle.DropDownList };
            gbFilter.Controls.Add(chkRemarks); gbFilter.Controls.Add(cmbRemarks);

            Controls.Add(gbFilter);
            y += 250;

            GroupBox gbSort = new GroupBox { Text = "Сортировка", Location = new Point(10, y), Size = new Size(400, 105) };

            rbSortNone = new RadioButton { Text = "Без сортировки", Location = new Point(10, 20), AutoSize = true, Checked = true };
            rbSortTitle = new RadioButton { Text = "По названию", Location = new Point(150, 20), AutoSize = true };
            rbSortAuthor = new RadioButton { Text = "По автору", Location = new Point(280, 20), AutoSize = true };
            rbSortYear = new RadioButton { Text = "По году", Location = new Point(10, 45), AutoSize = true };
            rbSortPrice = new RadioButton { Text = "По цене", Location = new Point(150, 45), AutoSize = true };
            gbSort.Controls.Add(rbSortNone); gbSort.Controls.Add(rbSortTitle);
            gbSort.Controls.Add(rbSortAuthor); gbSort.Controls.Add(rbSortYear);
            gbSort.Controls.Add(rbSortPrice);

            rbAsc = new RadioButton { Text = "По возрастанию", Location = new Point(10, 75), AutoSize = true, Checked = true };
            rbDesc = new RadioButton { Text = "По убыванию", Location = new Point(180, 75), AutoSize = true };
            gbSort.Controls.Add(rbAsc); gbSort.Controls.Add(rbDesc);

            Controls.Add(gbSort);
            y += 115;

            GroupBox gbAccess = new GroupBox { Text = "Способ доступа к данным", Location = new Point(10, y), Size = new Size(400, 65) };
            rbRelational = new RadioButton
            {
                Text = "Реляционный (SQL-запрос на сервер)",
                Location = new Point(10, 20),
                AutoSize = true,
                Checked = true
            };
            rbNavigational = new RadioButton
            {
                Text = "Навигационный (в памяти, LINQ)",
                Location = new Point(10, 40),
                AutoSize = true
            };
            gbAccess.Controls.Add(rbRelational); gbAccess.Controls.Add(rbNavigational);
            Controls.Add(gbAccess);
            y += 75;

            Button btnApply = new Button { Text = "Применить", Location = new Point(10, y), Width = 120 };
            btnApply.Click += BtnApply_Click;

            Button btnClear = new Button { Text = "Очистить фильтр", Location = new Point(140, y), Width = 140 };
            btnClear.Click += BtnClear_Click;

            Button btnClose = new Button { Text = "Закрыть", Location = new Point(290, y), Width = 120 };
            btnClose.Click += (s, e) => { DialogResult = DialogResult.Cancel; Close(); };

            Controls.Add(btnApply);
            Controls.Add(btnClear);
            Controls.Add(btnClose);

            AcceptButton = btnApply;
            CancelButton = btnClose;

            ClientSize = new Size(430, y + 45);
        }

        private void LoadCombos()
        {
            cmbJanr.Items.Clear();
            foreach (string janr in _repository.GetDistinctJanrs())
            {
                cmbJanr.Items.Add(janr);
            }
            if (cmbJanr.Items.Count > 0) cmbJanr.SelectedIndex = 0;

            cmbRemarks.Items.Clear();
            foreach (string remark in _repository.GetDistinctRemarks())
            {
                cmbRemarks.Items.Add(remark);
            }
            if (cmbRemarks.Items.Count > 0) cmbRemarks.SelectedIndex = 0;
        }

        private void BtnClear_Click(object sender, EventArgs e)
        {
            chkTitle.Checked = false; txtTitle.Clear();
            chkAuthor.Checked = false; txtAuthor.Clear();
            chkIzd.Checked = false; txtIzd.Clear();
            chkYear.Checked = false; numYear.Value = DateTime.Now.Year;
            chkPrice.Checked = false; numPriceFrom.Value = 0; numPriceTo.Value = 100000;
            chkJanr.Checked = false;
            chkRemarks.Checked = false;

            rbSortNone.Checked = true;
            rbAsc.Checked = true;
            rbRelational.Checked = true;

            // Criteria stays null: BooksPage reads this as "go back to plain search".
            Criteria = null;
            DialogResult = DialogResult.OK;
            Close();
        }

        private void BtnApply_Click(object sender, EventArgs e)
        {
            if (chkPrice.Checked && numPriceFrom.Value > numPriceTo.Value)
            {
                MessageBox.Show("Цена \"от\" не может быть больше цены \"до\".", "Проверка данных",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            Criteria = new BookFilterCriteria
            {
                FilterTitle = chkTitle.Checked,
                Title = txtTitle.Text.Trim(),

                FilterAuthor = chkAuthor.Checked,
                Author = txtAuthor.Text.Trim(),

                FilterIzdatelstvo = chkIzd.Checked,
                Izdatelstvo = txtIzd.Text.Trim(),

                FilterYear = chkYear.Checked,
                Year = (int)numYear.Value,

                FilterPrice = chkPrice.Checked,
                PriceFrom = numPriceFrom.Value,
                PriceTo = numPriceTo.Value,

                FilterJanr = chkJanr.Checked,
                Janr = cmbJanr.SelectedItem as string,

                FilterRemarks = chkRemarks.Checked,
                Remarks = cmbRemarks.SelectedItem as string,

                SortField = GetSortField(),
                SortDescending = rbDesc.Checked,

                UseNavigational = rbNavigational.Checked
            };

            DialogResult = DialogResult.OK;
            Close();
        }

        private string GetSortField()
        {
            if (rbSortTitle.Checked) return "Title";
            if (rbSortAuthor.Checked) return "Author";
            if (rbSortYear.Checked) return "Year";
            if (rbSortPrice.Checked) return "Price";
            return null;
        }
    }
}
