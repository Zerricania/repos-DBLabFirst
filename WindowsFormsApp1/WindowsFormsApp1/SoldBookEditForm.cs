using System;
using System.Drawing;
using System.Windows.Forms;
using WindowsFormsApp1.Data;
using WindowsFormsApp1.Models;

namespace WindowsFormsApp1
{
    public class SoldBookEditForm : Form
    {
        private readonly ISoldBookRepository _repository;
        private readonly IBookRepository _bookRepository;
        private readonly ICustomerRepository _customerRepository;
        private readonly int? _id;

        private ComboBox cmbBook;
        private ComboBox cmbCustomer;
        private DateTimePicker dtpSaleDate;
        private NumericUpDown numQuantity;
        private NumericUpDown numPrice;
        private Button btnSave;
        private Button btnCancel;

        public SoldBookEditForm(
            ISoldBookRepository repository,
            IBookRepository bookRepository,
            ICustomerRepository customerRepository,
            int? id)
        {
            _repository = repository;
            _bookRepository = bookRepository;
            _customerRepository = customerRepository;
            _id = id;
            BuildLayout();
            LoadCombos();

            if (_id.HasValue)
            {
                Text = "Изменить продажу";
                LoadExisting(_id.Value);
            }
            else
            {
                Text = "Добавить продажу";
                dtpSaleDate.Value = DateTime.Today;
                numQuantity.Value = 1;
                if (cmbBook.Items.Count > 0) cmbBook.SelectedIndex = 0;
                if (cmbCustomer.Items.Count > 0) cmbCustomer.SelectedIndex = 0;
            }
        }

        private void BuildLayout()
        {
            ClientSize = new Size(360, 260);
            StartPosition = FormStartPosition.CenterParent;
            FormBorderStyle = FormBorderStyle.FixedDialog;
            MaximizeBox = false;
            MinimizeBox = false;

            const int labelWidth = 90;
            const int fieldWidth = 230;
            int y = 15;

            Controls.Add(new Label { Text = "Книга:", Location = new Point(15, y + 3), AutoSize = true });
            cmbBook = new ComboBox
            {
                Location = new Point(15 + labelWidth, y),
                Width = fieldWidth,
                DropDownStyle = ComboBoxStyle.DropDownList
            };
            cmbBook.SelectedIndexChanged += CmbBook_SelectedIndexChanged;
            Controls.Add(cmbBook);
            y += 35;

            Controls.Add(new Label { Text = "Клиент:", Location = new Point(15, y + 3), AutoSize = true });
            cmbCustomer = new ComboBox
            {
                Location = new Point(15 + labelWidth, y),
                Width = fieldWidth,
                DropDownStyle = ComboBoxStyle.DropDownList
            };
            Controls.Add(cmbCustomer);
            y += 35;

            Controls.Add(new Label { Text = "Дата продажи:", Location = new Point(15, y + 3), AutoSize = true });
            dtpSaleDate = new DateTimePicker
            {
                Location = new Point(15 + labelWidth, y),
                Width = fieldWidth,
                Format = DateTimePickerFormat.Short
            };
            Controls.Add(dtpSaleDate);
            y += 35;

            Controls.Add(new Label { Text = "Количество:", Location = new Point(15, y + 3), AutoSize = true });
            numQuantity = new NumericUpDown
            {
                Location = new Point(15 + labelWidth, y),
                Width = fieldWidth,
                Minimum = 1,
                Maximum = 100000
            };
            Controls.Add(numQuantity);
            y += 35;

            Controls.Add(new Label { Text = "Цена:", Location = new Point(15, y + 3), AutoSize = true });
            numPrice = new NumericUpDown
            {
                Location = new Point(15 + labelWidth, y),
                Width = fieldWidth,
                Minimum = 0,
                Maximum = 1000000,
                DecimalPlaces = 2
            };
            Controls.Add(numPrice);
            y += 45;

            btnSave = new Button { Text = "Сохранить", Location = new Point(15 + labelWidth, y), Width = 110 };
            btnCancel = new Button { Text = "Отмена", Location = new Point(135 + labelWidth, y), Width = 100 };

            btnSave.Click += BtnSave_Click;
            btnCancel.Click += (s, e) => { DialogResult = DialogResult.Cancel; Close(); };

            Controls.Add(btnSave);
            Controls.Add(btnCancel);

            AcceptButton = btnSave;
            CancelButton = btnCancel;
        }

        private void LoadCombos()
        {
            cmbBook.Items.Clear();
            foreach (Book book in _bookRepository.GetForCombo())
            {
                cmbBook.Items.Add(new BookItem(book.Id, book.Title, book.Price));
            }

            cmbCustomer.Items.Clear();
            foreach (Customer customer in _customerRepository.GetForCombo())
            {
                cmbCustomer.Items.Add(new IdTextItem(customer.Id, customer.FullName));
            }
        }

        private void CmbBook_SelectedIndexChanged(object sender, EventArgs e)
        {
            // Selecting a book pre-fills its price — the user can still edit it by hand.
            if (cmbBook.SelectedItem is BookItem item)
            {
                numPrice.Value = item.Price;
            }
        }

        private void LoadExisting(int id)
        {
            SoldBook soldBook = _repository.GetById(id);
            if (soldBook == null)
            {
                MessageBox.Show("Запись не найдена.", "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
                DialogResult = DialogResult.Cancel;
                Close();
                return;
            }

            SelectById(cmbBook, soldBook.BookId);
            SelectById(cmbCustomer, soldBook.CustomerId);

            dtpSaleDate.Value = soldBook.SaleDate;
            numQuantity.Value = soldBook.Quantity;
            numPrice.Value = soldBook.Price;
        }

        private static void SelectById(ComboBox combo, int id)
        {
            for (int i = 0; i < combo.Items.Count; i++)
            {
                if (combo.Items[i] is BookItem book && book.Id == id) { combo.SelectedIndex = i; return; }
                if (combo.Items[i] is IdTextItem item && item.Id == id) { combo.SelectedIndex = i; return; }
            }
        }

        private void BtnSave_Click(object sender, EventArgs e)
        {
            if (cmbBook.SelectedItem == null)
            {
                MessageBox.Show("Выберите книгу.", "Проверка данных", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            if (cmbCustomer.SelectedItem == null)
            {
                MessageBox.Show("Выберите клиента.", "Проверка данных", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            SoldBook soldBook = new SoldBook
            {
                Id = _id ?? 0,
                BookId = ((BookItem)cmbBook.SelectedItem).Id,
                CustomerId = ((IdTextItem)cmbCustomer.SelectedItem).Id,
                SaleDate = dtpSaleDate.Value.Date,
                Quantity = (int)numQuantity.Value,
                Price = numPrice.Value
            };

            try
            {
                if (_id.HasValue) _repository.Update(soldBook);
                else _repository.Insert(soldBook);

                DialogResult = DialogResult.OK;
                Close();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Не удалось сохранить продажу:\n\n" + ex.Message, "Ошибка",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private class BookItem
        {
            public int Id { get; }
            public string Title { get; }
            public decimal Price { get; }

            public BookItem(int id, string title, decimal price)
            {
                Id = id;
                Title = title;
                Price = price;
            }

            public override string ToString() => Title;
        }

        private class IdTextItem
        {
            public int Id { get; }
            public string Text { get; }

            public IdTextItem(int id, string text)
            {
                Id = id;
                Text = text;
            }

            public override string ToString() => Text;
        }
    }
}
