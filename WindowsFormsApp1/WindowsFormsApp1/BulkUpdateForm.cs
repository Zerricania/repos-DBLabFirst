using System;
using System.Drawing;
using System.Windows.Forms;
using Microsoft.Data.SqlClient;
using WindowsFormsApp1.Data;

namespace WindowsFormsApp1
{
    /// <summary>
    /// Bulk operations on the Books table: discount all, discount above a price
    /// threshold, bulk genre change, delete older than a year, delete by
    /// publisher, delete everything. All six are the Lab 3 task list, including
    /// its "self-practice" items.
    /// </summary>
    public class BulkUpdateForm : Form
    {
        private readonly IBookRepository _repository;

        public bool ChangesMade { get; private set; }

        private NumericUpDown numFlatDiscount;
        private NumericUpDown numCondDiscount;
        private NumericUpDown numCondPrice;
        private TextBox txtGenreFrom;
        private TextBox txtGenreTo;
        private NumericUpDown numDeleteYear;
        private TextBox txtDeletePublisher;

        public BulkUpdateForm(IBookRepository repository)
        {
            _repository = repository;
            BuildLayout();
        }

        private void BuildLayout()
        {
            Text = "Массовые операции над книгами";
            StartPosition = FormStartPosition.CenterParent;
            FormBorderStyle = FormBorderStyle.FixedDialog;
            MaximizeBox = false;
            MinimizeBox = false;

            int y = 15;

            GroupBox gbFlat = new GroupBox { Text = "Скидка на все книги", Location = new Point(15, y), Size = new Size(395, 70) };
            gbFlat.Controls.Add(new Label { Text = "Процент скидки:", Location = new Point(10, 28), AutoSize = true });
            numFlatDiscount = new NumericUpDown { Location = new Point(140, 25), Width = 80, Minimum = 1, Maximum = 100, Value = 10 };
            gbFlat.Controls.Add(numFlatDiscount);
            Button btnFlat = new Button { Text = "Применить", Location = new Point(250, 23), Width = 120 };
            btnFlat.Click += BtnFlat_Click;
            gbFlat.Controls.Add(btnFlat);
            Controls.Add(gbFlat);
            y += 85;

            GroupBox gbCond = new GroupBox { Text = "Скидка для книг дороже указанной цены", Location = new Point(15, y), Size = new Size(395, 95) };
            gbCond.Controls.Add(new Label { Text = "Процент скидки:", Location = new Point(10, 28), AutoSize = true });
            numCondDiscount = new NumericUpDown { Location = new Point(140, 25), Width = 80, Minimum = 1, Maximum = 100, Value = 5 };
            gbCond.Controls.Add(numCondDiscount);
            gbCond.Controls.Add(new Label { Text = "Цена превышает:", Location = new Point(10, 60), AutoSize = true });
            numCondPrice = new NumericUpDown { Location = new Point(140, 57), Width = 80, Minimum = 0, Maximum = 1000000, Value = 2500 };
            gbCond.Controls.Add(numCondPrice);
            Button btnCond = new Button { Text = "Применить", Location = new Point(250, 55), Width = 120 };
            btnCond.Click += BtnCond_Click;
            gbCond.Controls.Add(btnCond);
            Controls.Add(gbCond);
            y += 110;

            GroupBox gbGenre = new GroupBox { Text = "Массовая смена жанра", Location = new Point(15, y), Size = new Size(395, 95) };
            gbGenre.Controls.Add(new Label { Text = "Из жанра:", Location = new Point(10, 28), AutoSize = true });
            txtGenreFrom = new TextBox { Location = new Point(90, 25), Width = 120, Text = "Учебник" };
            gbGenre.Controls.Add(txtGenreFrom);
            gbGenre.Controls.Add(new Label { Text = "В жанр:", Location = new Point(220, 28), AutoSize = true });
            txtGenreTo = new TextBox { Location = new Point(275, 25), Width = 110, Text = "Пособие" };
            gbGenre.Controls.Add(txtGenreTo);
            Button btnGenre = new Button { Text = "Применить", Location = new Point(250, 58), Width = 120 };
            btnGenre.Click += BtnGenre_Click;
            gbGenre.Controls.Add(btnGenre);
            Controls.Add(gbGenre);
            y += 110;

            GroupBox gbDeleteYear = new GroupBox { Text = "Удалить книги, выпущенные ранее указанного года", Location = new Point(15, y), Size = new Size(395, 70) };
            gbDeleteYear.Controls.Add(new Label { Text = "Год:", Location = new Point(10, 28), AutoSize = true });
            numDeleteYear = new NumericUpDown { Location = new Point(140, 25), Width = 80, Minimum = 1000, Maximum = 2100, Value = 2000 };
            gbDeleteYear.Controls.Add(numDeleteYear);
            Button btnDeleteYear = new Button { Text = "Удалить", Location = new Point(250, 23), Width = 120 };
            btnDeleteYear.Click += BtnDeleteYear_Click;
            gbDeleteYear.Controls.Add(btnDeleteYear);
            Controls.Add(gbDeleteYear);
            y += 85;

            GroupBox gbDeletePub = new GroupBox { Text = "Удалить книги указанного издательства", Location = new Point(15, y), Size = new Size(395, 70) };
            gbDeletePub.Controls.Add(new Label { Text = "Издательство:", Location = new Point(10, 28), AutoSize = true });
            txtDeletePublisher = new TextBox { Location = new Point(120, 25), Width = 120, Text = "Наука" };
            gbDeletePub.Controls.Add(txtDeletePublisher);
            Button btnDeletePub = new Button { Text = "Удалить", Location = new Point(250, 23), Width = 120 };
            btnDeletePub.Click += BtnDeletePublisher_Click;
            gbDeletePub.Controls.Add(btnDeletePub);
            Controls.Add(gbDeletePub);
            y += 85;

            GroupBox gbDeleteAll = new GroupBox { Text = "Удалить все данные", Location = new Point(15, y), Size = new Size(395, 60) };
            Button btnDeleteAll = new Button { Text = "Удалить ВСЕ книги", Location = new Point(10, 22), Width = 160 };
            btnDeleteAll.Click += BtnDeleteAll_Click;
            gbDeleteAll.Controls.Add(btnDeleteAll);
            Controls.Add(gbDeleteAll);
            y += 75;

            Button btnClose = new Button { Text = "Закрыть", Location = new Point(300, y), Width = 110 };
            btnClose.Click += (s, e) =>
            {
                DialogResult = ChangesMade ? DialogResult.OK : DialogResult.Cancel;
                Close();
            };
            Controls.Add(btnClose);
            CancelButton = btnClose;

            ClientSize = new Size(430, y + 45);
        }

        private bool Confirm(string message)
        {
            return MessageBox.Show(message, "Подтверждение", MessageBoxButtons.YesNo, MessageBoxIcon.Warning)
                   == DialogResult.Yes;
        }

        private void RunGuarded(Action action, string doneMessage)
        {
            try
            {
                action();
                ChangesMade = true;
                MessageBox.Show(doneMessage, "Готово", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            catch (SqlException)
            {
                MessageBox.Show(
                    "Операция не выполнена: часть затронутых книг используется в продажах " +
                    "(таблица SoldBooks на них ссылается).",
                    "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            catch (Exception ex)
            {
                MessageBox.Show("Ошибка:\n\n" + ex.Message, "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void BtnFlat_Click(object sender, EventArgs e)
        {
            decimal percent = numFlatDiscount.Value;
            if (!Confirm($"Применить скидку {percent}% ко ВСЕМ книгам? Действие необратимо.")) return;

            RunGuarded(() => _repository.ApplyDiscountToAll(percent), "Скидка применена ко всем книгам.");
        }

        private void BtnCond_Click(object sender, EventArgs e)
        {
            decimal percent = numCondDiscount.Value;
            decimal threshold = numCondPrice.Value;
            if (!Confirm($"Применить скидку {percent}% для книг дороже {threshold}? Действие необратимо.")) return;

            RunGuarded(() => _repository.ApplyDiscountAbovePrice(percent, threshold), "Скидка применена.");
        }

        private void BtnGenre_Click(object sender, EventArgs e)
        {
            string from = txtGenreFrom.Text.Trim();
            string to = txtGenreTo.Text.Trim();

            if (string.IsNullOrWhiteSpace(from) || string.IsNullOrWhiteSpace(to))
            {
                MessageBox.Show("Укажите оба жанра.", "Проверка данных", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            if (!Confirm($"Заменить жанр \"{from}\" на \"{to}\" у всех подходящих книг?")) return;

            RunGuarded(() => _repository.ChangeGenreBulk(from, to), "Жанр обновлён.");
        }

        private void BtnDeleteYear_Click(object sender, EventArgs e)
        {
            int year = (int)numDeleteYear.Value;
            if (!Confirm($"Удалить все книги, выпущенные ранее {year} года? Действие необратимо.")) return;

            RunGuarded(() => _repository.DeleteOlderThan(year), "Книги удалены.");
        }

        private void BtnDeletePublisher_Click(object sender, EventArgs e)
        {
            string publisher = txtDeletePublisher.Text.Trim();
            if (string.IsNullOrWhiteSpace(publisher))
            {
                MessageBox.Show("Укажите издательство.", "Проверка данных", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            if (!Confirm($"Удалить все книги издательства \"{publisher}\"? Действие необратимо.")) return;

            RunGuarded(() => _repository.DeleteByPublisher(publisher), "Книги удалены.");
        }

        private void BtnDeleteAll_Click(object sender, EventArgs e)
        {
            if (!Confirm("Удалить ВСЕ книги без исключения? Это действие необратимо.")) return;
            // Second confirmation on the most destructive action in the form —
            // one "Yes" click is easy to hit by accident on a full wipe.
            if (!Confirm("Вы точно уверены? Все записи в таблице Books будут удалены безвозвратно.")) return;

            RunGuarded(() => _repository.DeleteAll(), "Все книги удалены.");
        }
    }
}
