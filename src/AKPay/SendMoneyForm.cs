using System;
using System.Drawing;
using System.Threading.Tasks;
using System.Windows.Forms;
using AKPay.Models;
using AKPay.Services;

namespace AKPay
{
    public partial class SendMoneyForm : Form
    {
        private readonly IAKPayService _service;
        private readonly User _currentUser;
        private readonly BeneficiaryDisplayDto? _prefill;
        private TextBox txtUsername;
        private TextBox txtAmount;
        private Button btnSend;
        private Button btnCancel;
        private int? _toUserId;

        public SendMoneyForm(User currentUser, IAKPayService service, BeneficiaryDisplayDto? prefill)
        {
            _currentUser = currentUser;
            _service = service;
            _prefill = prefill;
            InitializeComponent();
            this.Load += async (_, _) => await PrefillAsync();
        }

        private void InitializeComponent()
        {
            UITheme.Apply(this);

            Text = "Send Money";

            var headerPanel = new Panel
            {
                BackColor = Color.FromArgb(0, 200, 150),
                Dock = DockStyle.Top,
                Height = 140
            };

            var lblTitle = new Label
            {
                Text = "Send Money",
                Font = new Font("Segoe UI", 30, FontStyle.Bold),
                ForeColor = Color.White,
                AutoSize = false,
                TextAlign = ContentAlignment.MiddleCenter,
                Dock = DockStyle.Fill
            };
            headerPanel.Controls.Add(lblTitle);

            var contentPanel = UITheme.CreateContentPanel(this, 320);
            contentPanel.Top = 220;

            var lblUsername = new Label
            {
                Text = "Beneficiary Username",
                Location = new Point(0, 0),
                Size = new Size(contentPanel.Width, 24),
                Font = new Font("Segoe UI", 11, FontStyle.Bold),
                ForeColor = Color.FromArgb(220, 220, 220)
            };

            txtUsername = new TextBox
            {
                Location = new Point(0, 28),
                Size = new Size(contentPanel.Width, 40),
                Font = new Font("Segoe UI", 12),
                BackColor = UITheme.SurfaceColor,
                ForeColor = Color.White,
                BorderStyle = BorderStyle.FixedSingle
            };

            var lblAmount = new Label
            {
                Text = "Amount (PKR)",
                Location = new Point(0, 88),
                Size = new Size(contentPanel.Width, 24),
                Font = new Font("Segoe UI", 11, FontStyle.Bold),
                ForeColor = Color.FromArgb(220, 220, 220)
            };

            txtAmount = new TextBox
            {
                Location = new Point(0, 116),
                Size = new Size(contentPanel.Width, 40),
                Font = new Font("Segoe UI", 12),
                BackColor = UITheme.SurfaceColor,
                ForeColor = Color.White,
                BorderStyle = BorderStyle.FixedSingle
            };

            btnSend = new Button
            {
                Text = "Pay",
                Location = new Point(0, 188),
                Size = new Size(contentPanel.Width, 52),
                BackColor = Color.FromArgb(0, 200, 150),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Font = new Font("Segoe UI", 13, FontStyle.Bold),
                Cursor = Cursors.Hand
            };
            btnSend.FlatAppearance.BorderSize = 0;
            btnSend.Click += BtnSend_Click;

            btnCancel = new Button
            {
                Text = "Cancel",
                Location = new Point(0, 256),
                Size = new Size(contentPanel.Width, 52),
                BackColor = Color.FromArgb(70, 70, 75),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Font = new Font("Segoe UI", 13, FontStyle.Bold),
                Cursor = Cursors.Hand
            };
            btnCancel.FlatAppearance.BorderSize = 0;
            btnCancel.Click += (_, _) => this.Close();

            contentPanel.Controls.Add(lblUsername);
            contentPanel.Controls.Add(txtUsername);
            contentPanel.Controls.Add(lblAmount);
            contentPanel.Controls.Add(txtAmount);
            contentPanel.Controls.Add(btnSend);
            contentPanel.Controls.Add(btnCancel);

            Controls.Add(contentPanel);
            Controls.Add(headerPanel);
        }

        private async Task PrefillAsync()
        {
            if (_prefill == null)
            {
                txtUsername.Text = string.Empty;
                txtAmount.Text = string.Empty;
                _toUserId = null;
                return;
            }

            txtUsername.Text = _prefill.UserName;
            txtAmount.Text = string.Empty;
            try
            {
                var details = await _service.FetchBenfDetails(_prefill.UserName);
                _toUserId = details?.UserId;
            }
            catch
            {
                _toUserId = null;
            }
        }

        private async void BtnSend_Click(object? sender, EventArgs e)
        {
            var username = txtUsername.Text.Trim();
            if (string.IsNullOrWhiteSpace(username))
            {
                MessageBox.Show("Please enter a beneficiary username.", "Validation Error",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            if (string.IsNullOrWhiteSpace(txtAmount.Text) || !decimal.TryParse(txtAmount.Text, out decimal amount) || amount <= 0)
            {
                MessageBox.Show("Please enter a valid amount.", "Validation Error",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            btnSend.Enabled = false;
            try
            {
                // Resolve beneficiary id if not already
                if (!_toUserId.HasValue)
                {
                    var details = await _service.FetchBenfDetails(username);
                    _toUserId = details?.UserId;
                }

                if (!_toUserId.HasValue)
                {
                    MessageBox.Show("Could not find beneficiary user.", "Validation Error",
                        MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }

                var ok = await _service.AsyncU2UTx(amount, _toUserId.Value, _currentUser.UserId);
                if (!ok)
                {
                    MessageBox.Show("Payment failed. Please try again.", "Failed",
                        MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return;
                }

                MessageBox.Show($"Money sent successfully!\nAmount: PKR {amount:N2}", "Success",
                    MessageBoxButtons.OK, MessageBoxIcon.Information);
                this.Close();
            }
            catch (NotImplementedException)
            {
                MessageBox.Show("Selected backend has not implemented U2U payments yet.", "Not Implemented",
                    MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Payment failed: {ex.Message}", "Error",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            finally
            {
                btnSend.Enabled = true;
            }
        }
    }
}
