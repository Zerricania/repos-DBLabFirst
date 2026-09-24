using System;
using System.Drawing;
using System.Windows.Forms;
using WindowsFormsApp1.Data;
using WindowsFormsApp1.Models;

namespace WindowsFormsApp1
{
    public class DiscountEditForm : Form
    {
        private readonly IDiscountRepository _repository;
        private readonly int? _id;

        private TextBox txtName;
        private NumericUpDown numPercents;
        private Button btnSave;
        private Button btnCancel;

        public DiscountEditForm(IDiscountRepository repository, int? id)
        {
            _repository = repository;
            _id = id;
            BuildLayout();

            if (_id.HasValue)
            {
                Text = "Изменить скидку";
                LoadExisting(_id.Value);
            }
            else
            {
                Text = "Добавить скидку";
            }
        }

        private void BuildLayout()
        {
            ClientSize = new Size(320, 150);
            StartPosition = FormStartPosition.CenterParent;
            FormBorderStyle = FormBorderStyle.FixedDialog;
            MaximizeBox = false;
            MinimizeBox = false;

            const int labelWidth = 130;
            const int fieldWidth = 150;

            Controls.Add(new Label { Text = "Название скидки:", Location = new Point(15, 18), AutoSize = true });
            txtName = new TextBox { Location = new Point(15 + labelWidth, 15), Width = fieldWidth };
            Controls.Add(txtName);

            Controls.Add(new Label { Text = "Процент:", Location = new Point(15, 53), AutoSize = true });
            numPercents = new NumericUpDown
            {
                Location = new Point(15 + labelWidth, 50),
                Width = fieldWidth,
                Minimum = 0,
                Maximum = 100
            };
            Controls.Add(numPercents);

            btnSave = new Button { Text = "Сохранить", Location = new Point(15 + labelWidth, 95), Width = 110 };
            btnCancel = new Button { Text = "Отмена", Location = new Point(135 + labelWidth - 20, 95), Width = 90 };

            btnSave.Click += BtnSave_Click;
            btnCancel.Click += (s, e) => { DialogResult = DialogResult.Cancel; Close(); };

            Controls.Add(btnSave);
            Controls.Add(btnCancel);

            AcceptButton = btnSave;
            CancelButton = btnCancel;
        }

        private void LoadExisting(int id)
        {
            Discount discount = _repository.GetById(id);
            if (discount == null)
            {
                MessageBox.Show("Запись не найдена.", "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
                DialogResult = DialogResult.Cancel;
                Close();
                return;
            }

            txtName.Text = discount.DName;
            numPercents.Value = discount.Percents;
        }

        private void BtnSave_Click(object sender, EventArgs e)
        {
            if (string.IsNullOrWhiteSpace(txtName.Text))
            {
                MessageBox.Show("Укажите название скидки.", "Проверка данных",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            Discount discount = new Discount
            {
                Id = _id ?? 0,
                DName = txtName.Text.Trim(),
                Percents = (int)numPercents.Value
            };

            try
            {
                if (_id.HasValue) _repository.Update(discount);
                else _repository.Insert(discount);

                DialogResult = DialogResult.OK;
                Close();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Не удалось сохранить скидку:\n\n" + ex.Message, "Ошибка",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
    }
}
