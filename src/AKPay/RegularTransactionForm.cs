using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;
using AKPay.Models;
using AKPay.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace AKPay
{
    public partial class RegularTransactionForm : Form
    {
        private readonly User _currentUser;
        private readonly IAKPayService _service;
        private ComboBox cmbVendor;
        private TextBox txtAmount;
        private Button btnPay;
        private Button btnScanQR;
        private Button btnCancel;
        private List<Vendor> _vendors = new();

        public RegularTransactionForm(User user, IAKPayService service)
        {
            _currentUser = user;
            _service = service;
            InitializeComponent();
            this.Load += async (_, _) => await LoadVendorsAsync();
        }

        private void InitializeComponent()
        {
            UITheme.Apply(this);

            Text = "Regular Transaction";

            var headerPanel = new Panel
            {
                BackColor = Color.FromArgb(0, 200, 150),
                Dock = DockStyle.Top,
                Height = 160
            };

            var lblTitle = new Label
            {
                Text = "Regular Transaction",
                Font = new Font("Segoe UI", 30, FontStyle.Bold),
                ForeColor = Color.White,
                AutoSize = false,
                TextAlign = ContentAlignment.MiddleCenter,
                Dock = DockStyle.Fill
            };
            headerPanel.Controls.Add(lblTitle);

            var contentPanel = UITheme.CreateContentPanel(this, 520);
            contentPanel.Top = 220;

            var lblVendor = new Label
            {
                Text = "Select Vendor",
                Location = new Point(0, 0),
                Size = new Size(contentPanel.Width, 24),
                Font = new Font("Segoe UI", 12, FontStyle.Bold),
                ForeColor = Color.FromArgb(220, 220, 220)
            };

            cmbVendor = new ComboBox
            {
                Location = new Point(0, 32),
                Size = new Size(contentPanel.Width, 40),
                Font = new Font("Segoe UI", 12),
                BackColor = UITheme.SurfaceColor,
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                DropDownStyle = ComboBoxStyle.DropDownList
            };

            var lblAmount = new Label
            {
                Text = "Amount (PKR)",
                Location = new Point(0, 96),
                Size = new Size(contentPanel.Width, 24),
                Font = new Font("Segoe UI", 12, FontStyle.Bold),
                ForeColor = Color.FromArgb(220, 220, 220)
            };

            txtAmount = new TextBox
            {
                Location = new Point(0, 128),
                Size = new Size(contentPanel.Width, 40),
                Font = new Font("Segoe UI", 12),
                BackColor = UITheme.SurfaceColor,
                ForeColor = Color.White,
                BorderStyle = BorderStyle.FixedSingle,
                ReadOnly = true,
                TabStop = false
            };

            btnScanQR = new Button
            {
                Text = "📷 Scan QR Code",
                Location = new Point(0, 190),
                Size = new Size(contentPanel.Width, 56),
                BackColor = Color.FromArgb(100, 100, 105),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Font = new Font("Segoe UI", 13, FontStyle.Bold),
                Cursor = Cursors.Hand
            };
            btnScanQR.FlatAppearance.BorderSize = 0;
            btnScanQR.Click += BtnScanQR_Click;

            btnPay = new Button
            {
                Text = "Pay Now",
                Location = new Point(0, 268),
                Size = new Size(contentPanel.Width, 54),
                BackColor = Color.FromArgb(0, 200, 150),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Font = new Font("Segoe UI", 13, FontStyle.Bold),
                Cursor = Cursors.Hand
            };
            btnPay.FlatAppearance.BorderSize = 0;
            btnPay.Click += BtnPay_Click;

            btnCancel = new Button
            {
                Text = "Cancel",
                Location = new Point(0, 336),
                Size = new Size(contentPanel.Width, 54),
                BackColor = Color.FromArgb(70, 70, 75),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Font = new Font("Segoe UI", 13, FontStyle.Bold),
                Cursor = Cursors.Hand
            };
            btnCancel.FlatAppearance.BorderSize = 0;
            btnCancel.Click += (_, _) => this.Close();

            contentPanel.Controls.Add(lblVendor);
            contentPanel.Controls.Add(cmbVendor);
            contentPanel.Controls.Add(lblAmount);
            contentPanel.Controls.Add(txtAmount);
            contentPanel.Controls.Add(btnScanQR);
            contentPanel.Controls.Add(btnPay);
            contentPanel.Controls.Add(btnCancel);

            Controls.Add(contentPanel);
            Controls.Add(headerPanel);
        }

        private async Task LoadVendorsAsync()
        {
            try
            {
                using var scope = HostAccessor.Current?.Services.CreateScope();
                if (scope == null)
                {
                    MessageBox.Show("Service host unavailable.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return;
                }

                var db = scope.ServiceProvider.GetRequiredService<AkpayDbContext>();
                _vendors = await db.Vendors.AsNoTracking()
                    .OrderBy(v => v.VendorName)
                    .ToListAsync();

                cmbVendor.DataSource = _vendors;
                cmbVendor.DisplayMember = nameof(Vendor.VendorName);
                cmbVendor.ValueMember = nameof(Vendor.VendorId);
                if (_vendors.Count > 0)
                {
                    cmbVendor.SelectedIndex = 0;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Could not load vendors: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void BtnScanQR_Click(object? sender, EventArgs e)
        {
            Random rand = new Random();
            if (_vendors.Count == 0)
            {
                MessageBox.Show("No vendors available to scan.", "Info", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            decimal randomAmount = Math.Round((decimal)rand.Next(50, 5000), 2);

            var selectedVendor = _vendors[rand.Next(_vendors.Count)];
            cmbVendor.SelectedValue = selectedVendor.VendorId;
            txtAmount.Text = randomAmount.ToString("0.00");

            MessageBox.Show($"QR Code Scanned!\n\nVendor: {selectedVendor.VendorName}\nAmount: PKR {randomAmount:0.00}",
                "QR Code", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }

        private async void BtnPay_Click(object? sender, EventArgs e)
        {
            if (cmbVendor.SelectedIndex == -1)
            {
                MessageBox.Show("Please select a vendor.", "Validation Error",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            if (string.IsNullOrWhiteSpace(txtAmount.Text))
            {
                MessageBox.Show("Scan a QR code to populate the amount.", "Validation Error",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            if (!decimal.TryParse(txtAmount.Text, out decimal amount) || amount <= 0)
            {
                MessageBox.Show("Please enter a valid amount.", "Validation Error",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            int vendorId;
            string vendorName;
            if (cmbVendor.SelectedValue is int id)
            {
                vendorId = id;
                vendorName = (cmbVendor.Text ?? string.Empty).Trim();
            }
            else
            {
                MessageBox.Show("Unable to resolve the selected vendor.", "Error",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            btnPay.Enabled = false;
            btnScanQR.Enabled = false;
            try
            {
                var ok = await _service.AsyncRegularTx(amount, _currentUser.UserId, vendorId);
                if (!ok)
                {
                    MessageBox.Show("Payment failed. Please try again.", "Failed",
                        MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return;
                }

                await RefreshUserAccountAsync();

                MessageBox.Show($"Payment Successful!\n\nPaid to: {vendorName}\nAmount: PKR {amount:N2}",
                    "Success", MessageBoxButtons.OK, MessageBoxIcon.Information);
                this.Close();
            }
            catch (NotImplementedException)
            {
                MessageBox.Show("Selected backend has not implemented regular transactions yet.", "Not Implemented",
                    MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Payment failed: {ex.Message}", "Error",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            finally
            {
                btnPay.Enabled = true;
                btnScanQR.Enabled = true;
            }
        }

        private async Task RefreshUserAccountAsync()
        {
            try
            {
                using var scope = HostAccessor.Current?.Services.CreateScope();
                if (scope == null)
                {
                    return;
                }

                var db = scope.ServiceProvider.GetRequiredService<AkpayDbContext>();
                var refreshed = await db.UserAccounts.AsNoTracking()
                    .FirstOrDefaultAsync(a => a.UserId == _currentUser.UserId);
                if (refreshed != null)
                {
                    _currentUser.UserAccount = refreshed;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Could not refresh user account after regular tx: {ex.Message}");
            }
        }
    }
}
