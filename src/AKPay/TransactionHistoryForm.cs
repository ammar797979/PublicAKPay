using System;
using System.Drawing;
using System.Windows.Forms;
using AKPay.Models;
using AKPay.Services;

namespace AKPay
{
    public class TransactionHistoryForm : Form
    {
        private readonly User _currentUser;
        private readonly IAKPayService _service;
        private DataGridView _dgvTransactions;
        private ComboBox _cmbTransactionType;
        private DateTimePicker _dtpFromDate;
        private DateTimePicker _dtpToDate;

        public TransactionHistoryForm(User user, IAKPayService service)
        {
            _currentUser = user;
            _service = service;
            InitializeComponent();
            LoadTransactionHistory();
        }

        private void InitializeComponent()
        {
            Text = "AKPay - Transaction History";
            Size = new Size(900, 700);
            StartPosition = FormStartPosition.CenterScreen;

            var headerPanel = new Panel
            {
                BackColor = Color.FromArgb(52, 152, 219),
                Dock = DockStyle.Top,
                Height = 80
            };

            var lblTitle = new Label
            {
                Text = "Transaction History",
                Font = new Font("Segoe UI", 20, FontStyle.Bold),
                ForeColor = Color.White,
                AutoSize = false,
                TextAlign = ContentAlignment.MiddleCenter,
                Dock = DockStyle.Fill
            };
            headerPanel.Controls.Add(lblTitle);

            var filterPanel = new Panel
            {
                Location = new Point(20, 95),
                Size = new Size(840, 80),
                BorderStyle = BorderStyle.FixedSingle,
                BackColor = Color.FromArgb(236, 240, 241)
            };

            var lblType = new Label { Text = "Type:", Location = new Point(10, 15), Size = new Size(50, 25), Font = new Font("Segoe UI", 9) };
            _cmbTransactionType = new ComboBox
            {
                Location = new Point(10, 40),
                Size = new Size(150, 25),
                Font = new Font("Segoe UI", 9),
                DropDownStyle = ComboBoxStyle.DropDownList
            };
            _cmbTransactionType.Items.AddRange(new object[] { "All Transactions", "Transfers Sent", "Transfers Received", "Top Ups", "Vendor Payments" });
            _cmbTransactionType.SelectedIndex = 0;

            var lblFrom = new Label { Text = "From:", Location = new Point(180, 15), Size = new Size(50, 25), Font = new Font("Segoe UI", 9) };
            _dtpFromDate = new DateTimePicker { Location = new Point(180, 40), Size = new Size(200, 25), Font = new Font("Segoe UI", 9), Format = DateTimePickerFormat.Short, Value = DateTime.Now.AddMonths(-1) };

            var lblTo = new Label { Text = "To:", Location = new Point(400, 15), Size = new Size(50, 25), Font = new Font("Segoe UI", 9) };
            _dtpToDate = new DateTimePicker { Location = new Point(400, 40), Size = new Size(200, 25), Font = new Font("Segoe UI", 9), Format = DateTimePickerFormat.Short };

            var btnFilter = new Button
            {
                Text = "Apply Filter",
                Location = new Point(620, 35),
                Size = new Size(100, 30),
                BackColor = Color.FromArgb(52, 152, 219),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Font = new Font("Segoe UI", 9, FontStyle.Bold),
                Cursor = Cursors.Hand
            };
            btnFilter.Click += (_, _) => MessageBox.Show("Filter not wired yet.");

            var btnExport = new Button
            {
                Text = "Export",
                Location = new Point(730, 35),
                Size = new Size(100, 30),
                BackColor = Color.FromArgb(46, 204, 113),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Font = new Font("Segoe UI", 9, FontStyle.Bold),
                Cursor = Cursors.Hand
            };
            btnExport.Click += (_, _) => MessageBox.Show("Export not wired yet.");

            filterPanel.Controls.Add(lblType);
            filterPanel.Controls.Add(_cmbTransactionType);
            filterPanel.Controls.Add(lblFrom);
            filterPanel.Controls.Add(_dtpFromDate);
            filterPanel.Controls.Add(lblTo);
            filterPanel.Controls.Add(_dtpToDate);
            filterPanel.Controls.Add(btnFilter);
            filterPanel.Controls.Add(btnExport);

            _dgvTransactions = new DataGridView
            {
                Location = new Point(20, 190),
                Size = new Size(840, 410),
                BackgroundColor = Color.White,
                BorderStyle = BorderStyle.Fixed3D,
                AllowUserToAddRows = false,
                AllowUserToDeleteRows = false,
                ReadOnly = true,
                SelectionMode = DataGridViewSelectionMode.FullRowSelect,
                AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill,
                ColumnHeadersHeightSizeMode = DataGridViewColumnHeadersHeightSizeMode.AutoSize
            };
            _dgvTransactions.Columns.Add("Date", "Date");
            _dgvTransactions.Columns.Add("Type", "Type");
            _dgvTransactions.Columns.Add("Description", "Description");
            _dgvTransactions.Columns.Add("Amount", "Amount");
            _dgvTransactions.Columns.Add("Status", "Status");
            _dgvTransactions.Columns["Date"].FillWeight = 20;
            _dgvTransactions.Columns["Type"].FillWeight = 20;
            _dgvTransactions.Columns["Description"].FillWeight = 35;
            _dgvTransactions.Columns["Amount"].FillWeight = 15;
            _dgvTransactions.Columns["Status"].FillWeight = 10;

            var btnClose = new Button
            {
                Text = "Close",
                Location = new Point(760, 615),
                Size = new Size(100, 35),
                BackColor = Color.FromArgb(231, 76, 60),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Font = new Font("Segoe UI", 10, FontStyle.Bold),
                Cursor = Cursors.Hand
            };
            btnClose.Click += (_, _) => Close();

            Controls.Add(headerPanel);
            Controls.Add(filterPanel);
            Controls.Add(_dgvTransactions);
            Controls.Add(btnClose);
        }

        private void LoadTransactionHistory()
        {
            // TODO: load from _service when available
        }
    }
}
