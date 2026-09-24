using System;
using System.Drawing;
using System.Windows.Forms;
using WindowsFormsApp1.Data;
using WindowsFormsApp1.Models;

namespace WindowsFormsApp1
{
    public class BookEditForm : Form
    {
        private readonly IBookRepository _repository;
        private readonly int? _id;

        private TextBox txtTitle;
        private TextBox txtAuthor;
        private TextBox txtIzdatelstvo;
        private NumericUpDown numYear;
        private NumericUpDown numPrice;
        private TextBox txtJanr;
        private TextBox txtRemarks;
        private Button btnSave;
        private Button btnCancel;

        public BookEditForm(IBookRepository repository, int? id)
        {
            _repository = repository;
            _id = id;
            BuildLayout();

            if (_id.HasValue)
            {
                Text = "Изменить книгу";
                LoadExisting(_id.Value);
            }
            else
            {
                Text = "Добавить книгу";
            }
        }

        private void BuildLayout()
        {
            ClientSize = new Size(380, 340);
            StartPosition = FormStartPosition.CenterParent;
            FormBorderStyle = FormBorderStyle.FixedDialog;
            MaximizeBox = false;
            MinimizeBox = false;

            int y = 15;
            const int labelWidth = 110;
            const int fieldWidth = 230;
            const int rowHeight = 32;

            txtTitle = AddRow("Название:", y, labelWidth, fieldWidth); y += rowHeight;
            txtAuthor = AddRow("Автор:", y, labelWidth, fieldWidth); y += rowHeight;
            txtIzdatelstvo = AddRow("Издательство:", y, labelWidth, fieldWidth); y += rowHeight;

            Controls.Add(new Label { Text = "Год:", Location = new Point(15, y + 4), AutoSize = true });
            numYear = new NumericUpDown
            {
                Location = new Point(15 + labelWidth, y),
                Width = fieldWidth,
                Minimum = 1000,
                Maximum = 2100,
                Value = DateTime.Now.Year
            };
            Controls.Add(numYear);
            y += rowHeight;

            Controls.Add(new Label { Text = "Цена:", Location = new Point(15, y + 4), AutoSize = true });
            numPrice = new NumericUpDown
            {
                Location = new Point(15 + labelWidth, y),
                Width = fieldWidth,
                Minimum = 0,
                Maximum = 1000000,
                DecimalPlaces = 2
            };
            Controls.Add(numPrice);
            y += rowHeight;

            txtJanr = AddRow("Жанр:", y, labelWidth, fieldWidth); y += rowHeight;
            txtRemarks = AddRow("Примечания:", y, labelWidth, fieldWidth); y += rowHeight + 10;

            btnSave = new Button { Text = "Сохранить", Location = new Point(15 + labelWidth, y), Width = 110 };
            btnCancel = new Button { Text = "Отмена", Location = new Point(135 + labelWidth, y), Width = 100 };

            btnSave.Click += BtnSave_Click;
            btnCancel.Click += (s, e) => { DialogResult = DialogResult.Cancel; Close(); };

            Controls.Add(btnSave);
            Controls.Add(btnCancel);

            AcceptButton = btnSave;
            CancelButton = btnCancel;
        }

        private TextBox AddRow(string label, int y, int labelWidth, int fieldWidth)
        {
            Controls.Add(new Label { Text = label, Location = new Point(15, y + 3), AutoSize = true });
            TextBox tb = new TextBox { Location = new Point(15 + labelWidth, y), Width = fieldWidth };
            Controls.Add(tb);
            return tb;
        }

        private void LoadExisting(int id)
        {
            Book book = _repository.GetById(id);
            if (book == null)
            {
                MessageBox.Show("Запись не найдена.", "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
                DialogResult = DialogResult.Cancel;
                Close();
                return;
            }

            txtTitle.Text = book.Title;
            txtAuthor.Text = book.Author;
            txtIzdatelstvo.Text = book.Izdatelstvo ?? "";
            numYear.Value = book.Year;
            numPrice.Value = book.Price;
            txtJanr.Text = book.Janr ?? "";
            txtRemarks.Text = book.Remarks ?? "";
        }

        private void BtnSave_Click(object sender, EventArgs e)
        {
            if (string.IsNullOrWhiteSpace(txtTitle.Text))
            {
                MessageBox.Show("Укажите название книги.", "Проверка данных",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            if (string.IsNullOrWhiteSpace(txtAuthor.Text))
            {
                MessageBox.Show("Укажите автора.", "Проверка данных",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            Book book = new Book
            {
                Id = _id ?? 0,
                Title = txtTitle.Text.Trim(),
                Author = txtAuthor.Text.Trim(),
                Izdatelstvo = txtIzdatelstvo.Text.Trim(),
                Year = (int)numYear.Value,
                Price = numPrice.Value,
                Janr = txtJanr.Text.Trim(),
                Remarks = txtRemarks.Text.Trim()
            };

            try
            {
                if (_id.HasValue) _repository.Update(book);
                else _repository.Insert(book);

                DialogResult = DialogResult.OK;
                Close();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Не удалось сохранить книгу:\n\n" + ex.Message, "Ошибка",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
    }
}
