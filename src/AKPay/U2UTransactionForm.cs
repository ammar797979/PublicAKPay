using System;
using System.Drawing;
using System.Windows.Forms;
using AKPay.Models;
using AKPay.Services;

namespace AKPay
{
    /// <summary>
    /// Form for user-to-user transfers.
    /// </summary>
    public class U2UTransactionForm : Form
    {
        private readonly User _currentUser;
        private readonly IAKPayService _service;
        private TextBox _txtRecipientEmail;
        private TextBox _txtAmount;
        private TextBox _txtDescription;
        private Label _lblCurrentBalance;

        public U2UTransactionForm(User user, IAKPayService service)
        {
            _currentUser = user;
            _service = service;
            InitializeComponent();
            LoadUserBalance();
        }

        private void InitializeComponent()
        {
            UITheme.Apply(this);

            Text = "AKPay - User to User";

            var headerPanel = new Panel
            {
                BackColor = Color.FromArgb(46, 204, 113),
                Dock = DockStyle.Top,
                Height = 160
            };

            var lblTitle = new Label
            {
                Text = "User to User Transfer",
                Font = new Font("Segoe UI", 30, FontStyle.Bold),
                ForeColor = Color.White,
                AutoSize = false,
                TextAlign = ContentAlignment.MiddleCenter,
                Dock = DockStyle.Fill
            };
            headerPanel.Controls.Add(lblTitle);

            var contentPanel = UITheme.CreateContentPanel(this, 520);
            contentPanel.Top = 220;

            _lblCurrentBalance = new Label
            {
                Text = "Your Balance: PKR 0.00",
                Location = new Point(0, 0),
                Size = new Size(contentPanel.Width, 28),
                Font = new Font("Segoe UI", 12, FontStyle.Bold),
                ForeColor = Color.FromArgb(52, 152, 219)
            };

            var lblRecipient = new Label
            {
                Text = "Recipient Email",
                Location = new Point(0, 48),
                Size = new Size(contentPanel.Width, 24),
                Font = new Font("Segoe UI", 11, FontStyle.Bold),
                ForeColor = Color.FromArgb(220, 220, 220)
            };

            _txtRecipientEmail = new TextBox
            {
                Location = new Point(0, 76),
                Size = new Size(contentPanel.Width, 40),
                Font = new Font("Segoe UI", 12),
                BackColor = UITheme.SurfaceColor,
                ForeColor = Color.White,
                BorderStyle = BorderStyle.FixedSingle
            };

            var lblAmount = new Label
            {
                Text = "Amount (PKR)",
                Location = new Point(0, 136),
                Size = new Size(contentPanel.Width, 24),
                Font = new Font("Segoe UI", 11, FontStyle.Bold),
                ForeColor = Color.FromArgb(220, 220, 220)
            };

            _txtAmount = new TextBox
            {
                Location = new Point(0, 164),
                Size = new Size(contentPanel.Width, 40),
                Font = new Font("Segoe UI", 12),
                BackColor = UITheme.SurfaceColor,
                ForeColor = Color.White,
                BorderStyle = BorderStyle.FixedSingle
            };

            var lblDescription = new Label
            {
                Text = "Description (Optional)",
                Location = new Point(0, 224),
                Size = new Size(contentPanel.Width, 24),
                Font = new Font("Segoe UI", 11, FontStyle.Bold),
                ForeColor = Color.FromArgb(220, 220, 220)
            };

            _txtDescription = new TextBox
            {
                Location = new Point(0, 252),
                Size = new Size(contentPanel.Width, 96),
                Font = new Font("Segoe UI", 12),
                BackColor = UITheme.SurfaceColor,
                ForeColor = Color.White,
                BorderStyle = BorderStyle.FixedSingle,
                Multiline = true
            };

            var btnTransfer = new Button
            {
                Text = "Transfer",
                Location = new Point(0, 360),
                Size = new Size(contentPanel.Width, 54),
                BackColor = Color.FromArgb(46, 204, 113),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Font = new Font("Segoe UI", 13, FontStyle.Bold),
                Cursor = Cursors.Hand
            };
            btnTransfer.FlatAppearance.BorderSize = 0;
            btnTransfer.Click += (_, _) => MessageBox.Show("U2U transfer not wired yet.");

            var btnCancel = new Button
            {
                Text = "Cancel",
                Location = new Point(0, 428),
                Size = new Size(contentPanel.Width, 54),
                BackColor = Color.FromArgb(231, 76, 60),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Font = new Font("Segoe UI", 13, FontStyle.Bold),
                Cursor = Cursors.Hand
            };
            btnCancel.FlatAppearance.BorderSize = 0;
            btnCancel.Click += (_, _) => Close();

            contentPanel.Controls.Add(_lblCurrentBalance);
            contentPanel.Controls.Add(lblRecipient);
            contentPanel.Controls.Add(_txtRecipientEmail);
            contentPanel.Controls.Add(lblAmount);
            contentPanel.Controls.Add(_txtAmount);
            contentPanel.Controls.Add(lblDescription);
            contentPanel.Controls.Add(_txtDescription);
            contentPanel.Controls.Add(btnTransfer);
            contentPanel.Controls.Add(btnCancel);

            Controls.Add(contentPanel);
            Controls.Add(headerPanel);
        }

        private void LoadUserBalance()
        {
            _lblCurrentBalance.Text = "Your Balance: PKR 0.00";
            // TODO: load via _service
        }
    }
}
