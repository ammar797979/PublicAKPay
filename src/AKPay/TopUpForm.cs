using System;
using System.Drawing;
using System.Threading.Tasks;
using System.Windows.Forms;
using AKPay.Models;
using AKPay.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace AKPay
{
    public class TopUpForm : Form
    {
        private readonly User _currentUser;
        private readonly IAKPayService _service;
        private TextBox _txtAmount = null!;
        private ComboBox _cmbSource = null!;
        private TextBox _txtSourceDetails = null!;
        private Label _lblStatus = null!;

        public TopUpForm(User user, IAKPayService service)
        {
            _currentUser = user;
            _service = service;
            InitializeComponent();
            this.Load += async (_, _) => await LoadSourcesAsync();
        }

        private void InitializeComponent()
        {
            UITheme.Apply(this);

            Text = "AKPay - Top Up Account";

            var headerPanel = new Panel
            {
                BackColor = Color.FromArgb(155, 89, 182),
                Dock = DockStyle.Top,
                Height = 140
            };

            var lblTitle = new Label
            {
                Text = "Top Up Account",
                Font = new Font("Segoe UI", 30, FontStyle.Bold),
                ForeColor = Color.White,
                AutoSize = false,
                TextAlign = ContentAlignment.MiddleCenter,
                Dock = DockStyle.Fill
            };
            headerPanel.Controls.Add(lblTitle);

            var contentPanel = UITheme.CreateContentPanel(this, 480);
            contentPanel.Top = 220;

            var lblSource = new Label
            {
                Text = "Top Up Source",
                Location = new Point(0, 0),
                Size = new Size(contentPanel.Width, 24),
                Font = new Font("Segoe UI", 11, FontStyle.Bold),
                ForeColor = Color.FromArgb(220, 220, 220)
            };

            _cmbSource = new ComboBox
            {
                Location = new Point(0, 26),
                Size = new Size(contentPanel.Width, 40),
                Font = new Font("Segoe UI", 12),
                DropDownStyle = ComboBoxStyle.DropDownList,
                BackColor = Color.FromArgb(45, 45, 48),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat
            };

            var lblSourceDetails = new Label
            {
                Text = "Source Details (Card/Account Number)",
                Location = new Point(0, 80),
                Size = new Size(contentPanel.Width, 24),
                Font = new Font("Segoe UI", 11, FontStyle.Bold),
                ForeColor = Color.FromArgb(220, 220, 220)
            };

            _txtSourceDetails = new TextBox
            {
                Location = new Point(0, 106),
                Size = new Size(contentPanel.Width, 40),
                Font = new Font("Segoe UI", 12),
                BackColor = Color.FromArgb(45, 45, 48),
                ForeColor = Color.Gray,
                Text = "notImplemented(third-party service)",
                BorderStyle = BorderStyle.FixedSingle
            };

            var lblAmount = new Label
            {
                Text = "Amount (PKR)",
                Location = new Point(0, 156),
                Size = new Size(contentPanel.Width, 24),
                Font = new Font("Segoe UI", 11, FontStyle.Bold),
                ForeColor = Color.FromArgb(220, 220, 220)
            };

            _txtAmount = new TextBox
            {
                Location = new Point(0, 182),
                Size = new Size(contentPanel.Width, 40),
                Font = new Font("Segoe UI", 12),
                BackColor = Color.FromArgb(45, 45, 48),
                ForeColor = Color.White,
                BorderStyle = BorderStyle.FixedSingle
            };

            var lblInfo = new Label
            {
                Text = "Minimum top up: PKR 100 | Maximum: PKR 100,000",
                Location = new Point(0, 232),
                Size = new Size(contentPanel.Width, 24),
                Font = new Font("Segoe UI", 9),
                ForeColor = Color.Gray
            };

            var btnTopUp = new Button
            {
                Text = "Top Up",
                Location = new Point(0, 280),
                Size = new Size(contentPanel.Width, 52),
                BackColor = Color.FromArgb(155, 89, 182),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Font = new Font("Segoe UI", 13, FontStyle.Bold),
                Cursor = Cursors.Hand
            };
            btnTopUp.FlatAppearance.BorderSize = 0;
            btnTopUp.Click += BtnTopUp_Click;

            var btnCancel = new Button
            {
                Text = "Cancel",
                Location = new Point(0, 348),
                Size = new Size(contentPanel.Width, 52),
                BackColor = Color.FromArgb(231, 76, 60),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Font = new Font("Segoe UI", 13, FontStyle.Bold),
                Cursor = Cursors.Hand
            };
            btnCancel.FlatAppearance.BorderSize = 0;
            btnCancel.Click += (_, _) => Close();

            _lblStatus = new Label
            {
                Text = string.Empty,
                Location = new Point(0, 416),
                Size = new Size(contentPanel.Width, 48),
                ForeColor = Color.FromArgb(200, 200, 200),
                Font = new Font("Segoe UI", 9),
                AutoEllipsis = true
            };

            contentPanel.Controls.Add(lblSource);
            contentPanel.Controls.Add(_cmbSource);
            contentPanel.Controls.Add(lblSourceDetails);
            contentPanel.Controls.Add(_txtSourceDetails);
            contentPanel.Controls.Add(lblAmount);
            contentPanel.Controls.Add(_txtAmount);
            contentPanel.Controls.Add(lblInfo);
            contentPanel.Controls.Add(btnTopUp);
            contentPanel.Controls.Add(btnCancel);
            contentPanel.Controls.Add(_lblStatus);

            Controls.Add(contentPanel);
            Controls.Add(headerPanel);
        }

        private async Task LoadSourcesAsync()
        {
            try
            {
                using var scope = HostAccessor.Current?.Services.CreateScope();
                if (scope == null)
                {
                    _lblStatus.Text = "Service scope unavailable; cannot load top-up sources.";
                    return;
                }

                var db = scope.ServiceProvider.GetRequiredService<AkpayDbContext>();
                var sources = await db.TopUpSources.AsNoTracking().ToListAsync();
                _cmbSource.Items.Clear();
                foreach (var s in sources)
                {
                    _cmbSource.Items.Add(s.SourceName);
                }
                if (_cmbSource.Items.Count > 0)
                {
                    _cmbSource.SelectedIndex = 0;
                }
                _lblStatus.Text = _cmbSource.Items.Count == 0 ? "No top-up sources available." : string.Empty;
            }
            catch (Exception ex)
            {
                _lblStatus.Text = $"Could not load sources: {ex.Message}";
            }
        }

        private async void BtnTopUp_Click(object? sender, EventArgs e)
        {
            if (string.IsNullOrWhiteSpace(_txtAmount.Text) || !decimal.TryParse(_txtAmount.Text, out var amount) || amount < 100 || amount > 100000)
            {
                MessageBox.Show("Amount must be between 100 and 100,000.", "Validation Error",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            try
            {
                using var scope = HostAccessor.Current?.Services.CreateScope();
                if (scope == null)
                {
                    MessageBox.Show("Unable to create service scope; top-up not saved.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return;
                }

                var db = scope.ServiceProvider.GetRequiredService<AkpayDbContext>();
                var account = await db.UserAccounts.FirstOrDefaultAsync(a => a.UserId == _currentUser.UserId);

                if (account == null)
                {
                    account = new UserAccount
                    {
                        UserId = _currentUser.UserId,
                        UserBalance = 0,
                        IsActive = true,
                        LastUpdateTime = DateTime.UtcNow
                    };
                    db.UserAccounts.Add(account);
                }

                account.UserBalance += amount;
                account.LastUpdateTime = DateTime.UtcNow;
                await db.SaveChangesAsync();

                // keep in-memory model in sync for the dashboard
                _currentUser.UserAccount = account;

                MessageBox.Show($"Top-up of PKR {amount:N0} recorded.", "Success",
                    MessageBoxButtons.OK, MessageBoxIcon.Information);
                Close();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Could not save top-up: {ex.Message}", "Error",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
    }
}
