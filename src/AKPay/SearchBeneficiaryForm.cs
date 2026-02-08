using System;
using System.Drawing;
using System.Windows.Forms;
using AKPay.Services;

namespace AKPay
{
    public partial class SearchBeneficiaryForm : Form
    {
        private readonly IAKPayService _service;
        private TextBox txtSearch;
        private Button btnSearch;
        private ListView listResults;
        private Button btnClose;

        public SearchBeneficiaryForm(IAKPayService service)
        {
            _service = service;
            InitializeComponent();
        }

        private void InitializeComponent()
        {
            UITheme.Apply(this);

            Text = "Search Beneficiary";

            var headerPanel = new Panel
            {
                BackColor = Color.FromArgb(0, 200, 150),
                Dock = DockStyle.Top,
                Height = 140
            };

            var lblTitle = new Label
            {
                Text = "Search Beneficiary",
                Font = new Font("Segoe UI", 30, FontStyle.Bold),
                ForeColor = Color.White,
                AutoSize = false,
                TextAlign = ContentAlignment.MiddleCenter,
                Dock = DockStyle.Fill
            };
            headerPanel.Controls.Add(lblTitle);

            var contentPanel = UITheme.CreateContentPanel(this, 420);
            contentPanel.Top = 220;

            txtSearch = new TextBox
            {
                Location = new Point(0, 0),
                Size = new Size(contentPanel.Width, 40),
                Font = new Font("Segoe UI", 12),
                BackColor = UITheme.SurfaceColor,
                ForeColor = Color.White,
                BorderStyle = BorderStyle.FixedSingle
            };

            btnSearch = new Button
            {
                Text = "Search",
                Location = new Point(0, 56),
                Size = new Size(contentPanel.Width, 46),
                BackColor = Color.FromArgb(0, 200, 150),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Font = new Font("Segoe UI", 12, FontStyle.Bold),
                Cursor = Cursors.Hand
            };
            btnSearch.FlatAppearance.BorderSize = 0;
            btnSearch.Click += BtnSearch_Click;

            listResults = new ListView
            {
                Location = new Point(0, 118),
                Size = new Size(contentPanel.Width, 220),
                View = View.Details,
                FullRowSelect = true,
                GridLines = true,
                BackColor = UITheme.SurfaceColor,
                ForeColor = Color.White,
                Font = new Font("Segoe UI", 10)
            };
            listResults.Columns.Add("Name", 190);
            listResults.Columns.Add("Account Number", 170);
            listResults.Columns.Add("Bank", 60);

            btnClose = new Button
            {
                Text = "Close",
                Location = new Point(0, 352),
                Size = new Size(contentPanel.Width, 46),
                BackColor = Color.FromArgb(70, 70, 75),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Font = new Font("Segoe UI", 12, FontStyle.Bold),
                Cursor = Cursors.Hand
            };
            btnClose.FlatAppearance.BorderSize = 0;
            btnClose.Click += (_, _) => this.Close();

            contentPanel.Controls.Add(txtSearch);
            contentPanel.Controls.Add(btnSearch);
            contentPanel.Controls.Add(listResults);
            contentPanel.Controls.Add(btnClose);

            Controls.Add(contentPanel);
            Controls.Add(headerPanel);
        }

        private void BtnSearch_Click(object? sender, EventArgs e)
        {
            listResults.Items.Clear();
            if (string.IsNullOrWhiteSpace(txtSearch.Text))
            {
                MessageBox.Show("Enter a name to search.", "Validation Error",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            listResults.Items.Add(new ListViewItem(new[] { "Alice", "123456789", "Bank A" }));
            listResults.Items.Add(new ListViewItem(new[] { "Bob", "987654321", "Bank B" }));
        }
    }
}
