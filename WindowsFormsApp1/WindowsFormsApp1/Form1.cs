using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;

namespace WindowsFormsApp1
{
    public class Form1 : Form
    {
        private Panel pnlMenu;
        private Panel pnlContent;

        private readonly Dictionary<string, CrudPageBase> _pages = new Dictionary<string, CrudPageBase>();
        private CrudPageBase _currentPage;

        public Form1()
        {
            BuildLayout();
            BuildPages();
            ShowPage("Books");
        }

        private void BuildLayout()
        {
            Text = "Библиотека книг";
            ClientSize = new Size(1000, 600);
            StartPosition = FormStartPosition.CenterScreen;
            MinimumSize = new Size(800, 500);

            pnlMenu = new Panel
            {
                Dock = DockStyle.Left,
                Width = 190,
                BackColor = Color.FromArgb(45, 45, 48)
            };

            pnlContent = new Panel
            {
                Dock = DockStyle.Fill
            };

            Controls.Add(pnlContent);
            Controls.Add(pnlMenu);

            AddMenuButton("Книги", 20, "Books");
            AddMenuButton("Клиенты", 70, "Customers");
            AddMenuButton("Скидки", 120, "Discounts");
            AddMenuButton("Продажи", 170, "SoldBooks");
        }

        private void AddMenuButton(string text, int top, string pageKey)
        {
            Button button = new Button
            {
                Text = text,
                Location = new Point(15, top),
                Size = new Size(160, 42),
                FlatStyle = FlatStyle.Flat,
                ForeColor = Color.White,
                BackColor = Color.FromArgb(62, 62, 66),
                TextAlign = ContentAlignment.MiddleCenter
            };
            button.FlatAppearance.BorderSize = 0;
            button.Click += (s, e) => ShowPage(pageKey);

            pnlMenu.Controls.Add(button);
        }

        private void BuildPages()
        {
            _pages["Books"] = new BooksPage();
            _pages["Customers"] = new CustomersPage();
            _pages["Discounts"] = new DiscountsPage();
            _pages["SoldBooks"] = new SoldBooksPage();

            foreach (CrudPageBase page in _pages.Values)
            {
                page.Visible = false;
                pnlContent.Controls.Add(page);
            }
        }

        private void ShowPage(string key)
        {
            if (_currentPage != null) _currentPage.Visible = false;

            CrudPageBase page = _pages[key];
            page.Visible = true;
            page.BringToFront();
            page.ReloadData();

            _currentPage = page;
        }
    }
}
