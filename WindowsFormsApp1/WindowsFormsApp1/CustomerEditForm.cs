using System;
using System.Drawing;
using System.Windows.Forms;
using WindowsFormsApp1.Data;
using WindowsFormsApp1.Models;

namespace WindowsFormsApp1
{
    public class CustomerEditForm : Form
    {
        private readonly ICustomerRepository _repository;
        private readonly IDiscountRepository _discountRepository;
        private readonly int? _id;

        private TextBox txtFullName;
        private TextBox txtPhone;
        private ComboBox cmbDiscount;
        private Button btnSave;
        private Button btnCancel;

        public CustomerEditForm(ICustomerRepository repository, IDiscountRepository discountRepository, int? id)
        {
            _repository = repository;
            _discountRepository = discountRepository;
            _id = id;
            BuildLayout();
            LoadDiscounts();

            if (_id.HasValue)
            {
                Text = "Изменить клиента";
                LoadExisting(_id.Value);
            }
            else
            {
                Text = "Добавить клиента";
                cmbDiscount.SelectedIndex = 0;
            }
        }

        private void BuildLayout()
        {
            ClientSize = new Size(360, 190);
            StartPosition = FormStartPosition.CenterParent;
            FormBorderStyle = FormBorderStyle.FixedDialog;
            MaximizeBox = false;
            MinimizeBox = false;

            const int labelWidth = 90;
            const int fieldWidth = 230;

            Controls.Add(new Label { Text = "ФИО:", Location = new Point(15, 18), AutoSize = true });
            txtFullName = new TextBox { Location = new Point(15 + labelWidth, 15), Width = fieldWidth };
            Controls.Add(txtFullName);

            Controls.Add(new Label { Text = "Телефон:", Location = new Point(15, 53), AutoSize = true });
            txtPhone = new TextBox { Location = new Point(15 + labelWidth, 50), Width = fieldWidth };
            Controls.Add(txtPhone);

            Controls.Add(new Label { Text = "Скидка:", Location = new Point(15, 88), AutoSize = true });
            cmbDiscount = new ComboBox
            {
                Location = new Point(15 + labelWidth, 85),
                Width = fieldWidth,
                DropDownStyle = ComboBoxStyle.DropDownList
            };
            Controls.Add(cmbDiscount);

            btnSave = new Button { Text = "Сохранить", Location = new Point(15 + labelWidth, 135), Width = 110 };
            btnCancel = new Button { Text = "Отмена", Location = new Point(135 + labelWidth, 135), Width = 100 };

            btnSave.Click += BtnSave_Click;
            btnCancel.Click += (s, e) => { DialogResult = DialogResult.Cancel; Close(); };

            Controls.Add(btnSave);
            Controls.Add(btnCancel);

            AcceptButton = btnSave;
            CancelButton = btnCancel;
        }

        private void LoadDiscounts()
        {
            cmbDiscount.Items.Clear();
            cmbDiscount.Items.Add(new ComboItem(null, "Без скидки"));

            foreach (Discount discount in _discountRepository.GetForCombo())
            {
                cmbDiscount.Items.Add(new ComboItem(discount.Id, discount.DName));
            }

            cmbDiscount.DisplayMember = "Text";
        }

        private void LoadExisting(int id)
        {
            Customer customer = _repository.GetById(id);
            if (customer == null)
            {
                MessageBox.Show("Запись не найдена.", "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
                DialogResult = DialogResult.Cancel;
                Close();
                return;
            }

            txtFullName.Text = customer.FullName;
            txtPhone.Text = customer.Phone ?? "";

            cmbDiscount.SelectedIndex = 0;
            for (int i = 0; i < cmbDiscount.Items.Count; i++)
            {
                ComboItem item = (ComboItem)cmbDiscount.Items[i];
                if (item.Id == customer.DiscountId)
                {
                    cmbDiscount.SelectedIndex = i;
                    break;
                }
            }
        }

        private void BtnSave_Click(object sender, EventArgs e)
        {
            if (string.IsNullOrWhiteSpace(txtFullName.Text))
            {
                MessageBox.Show("Укажите ФИО клиента.", "Проверка данных",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            int? discountId = ((ComboItem)cmbDiscount.SelectedItem).Id;

            Customer customer = new Customer
            {
                Id = _id ?? 0,
                FullName = txtFullName.Text.Trim(),
                Phone = txtPhone.Text.Trim(),
                DiscountId = discountId
            };

            try
            {
                if (_id.HasValue) _repository.Update(customer);
                else _repository.Insert(customer);

                DialogResult = DialogResult.OK;
                Close();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Не удалось сохранить клиента:\n\n" + ex.Message, "Ошибка",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        /// <summary>Combo item: pair of (database Id, display text).</summary>
        private class ComboItem
        {
            public int? Id { get; }
            public string Text { get; }

            public ComboItem(int? id, string text)
            {
                Id = id;
                Text = text;
            }

            public override string ToString() => Text;
        }
    }
}
