using System;
using System.Drawing;
using System.Windows.Forms;
using AKPay.Services;

namespace AKPay
{
    public partial class BeneficiaryListForm : Form
    {
        private readonly IAKPayService _service;
        private ListView listBeneficiaries;
        private Button btnClose;

        public BeneficiaryListForm(IAKPayService service)
        {
            _service = service;
            InitializeComponent();
        }

        private void InitializeComponent()
        {
            UITheme.Apply(this);

            Text = "Beneficiary List";

            var headerPanel = new Panel
            {
                BackColor = Color.FromArgb(0, 200, 150),
                Dock = DockStyle.Top,
                Height = 140
            };

            var lblTitle = new Label
            {
                Text = "Beneficiary List",
                Font = new Font("Segoe UI", 30, FontStyle.Bold),
                ForeColor = Color.White,
                AutoSize = false,
                TextAlign = ContentAlignment.MiddleCenter,
                Dock = DockStyle.Fill
            };
            headerPanel.Controls.Add(lblTitle);

            var contentPanel = UITheme.CreateContentPanel(this, 420);
            contentPanel.Top = 220;

            listBeneficiaries = new ListView
            {
                Location = new Point(0, 0),
                Size = new Size(contentPanel.Width, 300),
                View = View.Details,
                FullRowSelect = true,
                GridLines = true,
                BackColor = UITheme.SurfaceColor,
                ForeColor = Color.White,
                Font = new Font("Segoe UI", 10)
            };
            listBeneficiaries.Columns.Add("Name", 190);
            listBeneficiaries.Columns.Add("Account Number", 170);
            listBeneficiaries.Columns.Add("Bank", 60);

            listBeneficiaries.Items.Add(new ListViewItem(new[] { "Alice", "123456789", "Bank A" }));
            listBeneficiaries.Items.Add(new ListViewItem(new[] { "Bob", "987654321", "Bank B" }));

            btnClose = new Button
            {
                Text = "Close",
                Location = new Point(0, 320),
                Size = new Size(contentPanel.Width, 46),
                BackColor = Color.FromArgb(70, 70, 75),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Font = new Font("Segoe UI", 12, FontStyle.Bold),
                Cursor = Cursors.Hand
            };
            btnClose.FlatAppearance.BorderSize = 0;
            btnClose.Click += (_, _) => this.Close();

            contentPanel.Controls.Add(listBeneficiaries);
            contentPanel.Controls.Add(btnClose);

            Controls.Add(contentPanel);
            Controls.Add(headerPanel);
        }
    }
}
