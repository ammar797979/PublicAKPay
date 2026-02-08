using System;
using System.Diagnostics;
using System.Drawing;
using System.Windows.Forms;
using AKPay.Models;
using AKPay.Services;

namespace AKPay
{
    public partial class ProfileSettingsForm : Form
    {
        private readonly User _user;
        private readonly IAKPayService _service;
        private TextBox txtUserName = null!;
        private TextBox txtPhone = null!;
        private TextBox txtFullName = null!;
        private Label lblEmailSuffix = null!;
        private Label lblDateCreated = null!;

        public ProfileSettingsForm(User user, IAKPayService service)
        {
            _user = user;
            _service = service;
            InitializeComponent();
        }

        private void InitializeComponent()
        {
            UITheme.Apply(this);

            Text = "Profile / Settings";

            var headerPanel = new Panel
            {
                BackColor = Color.FromArgb(0, 200, 150),
                Dock = DockStyle.Top,
                Height = 160
            };

            var lblTitle = new Label
            {
                Text = "Profile / Settings",
                Font = new Font("Segoe UI", 30, FontStyle.Bold),
                ForeColor = Color.White,
                AutoSize = false,
                TextAlign = ContentAlignment.MiddleCenter,
                Dock = DockStyle.Fill
            };
            headerPanel.Controls.Add(lblTitle);

            var contentPanel = UITheme.CreateContentPanel(this, 520);
            contentPanel.Top = 220;
            var lblUsername = new Label
            {
                Text = "Username",
                Location = new Point(0, 0),
                Size = new Size(contentPanel.Width, 24),
                Font = new Font("Segoe UI", 11, FontStyle.Bold),
                ForeColor = Color.FromArgb(220, 220, 220)
            };

            var usernamePanel = new Panel
            {
                Location = new Point(0, 28),
                Size = new Size(contentPanel.Width, 46)
            };

            txtUserName = new TextBox
            {
                Location = new Point(0, 0),
                Size = new Size(260, 38),
                Font = new Font("Segoe UI", 12),
                Text = ResolveUserName(),
                ReadOnly = true,
                TabStop = false,
                BackColor = UITheme.SurfaceColor,
                ForeColor = Color.White,
                BorderStyle = BorderStyle.FixedSingle
            };

            lblEmailSuffix = new Label
            {
                Text = "(@lums.edu.pk)",
                Location = new Point(txtUserName.Right + 12, 8),
                Size = new Size(usernamePanel.Width - txtUserName.Width - 12, 24),
                Font = new Font("Segoe UI", 11, FontStyle.Regular),
                ForeColor = Color.FromArgb(200, 200, 200)
            };

            usernamePanel.Controls.Add(txtUserName);
            usernamePanel.Controls.Add(lblEmailSuffix);

            var lblPhone = new Label
            {
                Text = "Phone Number",
                Location = new Point(0, 92),
                Size = new Size(contentPanel.Width, 24),
                Font = new Font("Segoe UI", 11, FontStyle.Bold),
                ForeColor = Color.FromArgb(220, 220, 220)
            };

            txtPhone = new TextBox
            {
                Location = new Point(0, 120),
                Size = new Size(contentPanel.Width, 40),
                Font = new Font("Segoe UI", 12),
                Text = ResolvePhoneNumber(),
                ReadOnly = true,
                TabStop = false,
                BackColor = UITheme.SurfaceColor,
                ForeColor = Color.White,
                BorderStyle = BorderStyle.FixedSingle
            };

            var lblFullName = new Label
            {
                Text = "Full Name",
                Location = new Point(0, 184),
                Size = new Size(contentPanel.Width, 24),
                Font = new Font("Segoe UI", 11, FontStyle.Bold),
                ForeColor = Color.FromArgb(220, 220, 220)
            };

            txtFullName = new TextBox
            {
                Location = new Point(0, 212),
                Size = new Size(contentPanel.Width, 40),
                Font = new Font("Segoe UI", 12),
                Text = _user.FullName,
                ReadOnly = true,
                TabStop = false,
                BackColor = UITheme.SurfaceColor,
                ForeColor = Color.White,
                BorderStyle = BorderStyle.FixedSingle
            };

            var btnChangePassword = new Button
            {
                Text = "Change Password",
                Location = new Point(0, 280),
                Size = new Size(contentPanel.Width, 52),
                BackColor = Color.FromArgb(0, 200, 150),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Font = new Font("Segoe UI", 13, FontStyle.Bold),
                Cursor = Cursors.Hand
            };
            btnChangePassword.FlatAppearance.BorderSize = 0;
            btnChangePassword.Click += (_, _) => OpenChangePassword();

            var btnSupport = new Button
            {
                Text = "Help / Support",
                Location = new Point(0, 344),
                Size = new Size(contentPanel.Width, 52),
                BackColor = Color.FromArgb(60, 130, 200),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Font = new Font("Segoe UI", 13, FontStyle.Bold),
                Cursor = Cursors.Hand
            };
            btnSupport.FlatAppearance.BorderSize = 0;
            btnSupport.Click += (_, _) => OpenSupport();

            var btnDeleteAccount = new Button
            {
                Text = "Delete Account",
                Location = new Point(0, 408),
                Size = new Size(contentPanel.Width, 52),
                BackColor = Color.FromArgb(200, 50, 50),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Font = new Font("Segoe UI", 13, FontStyle.Bold),
                Cursor = Cursors.Hand
            };
            btnDeleteAccount.FlatAppearance.BorderSize = 0;
            btnDeleteAccount.Click += (_, _) => MessageBox.Show("Account deletion will be available soon.");

            lblDateCreated = new Label
            {
                Text = $"Member since {_user.DateCreated:MMMM dd, yyyy}",
                Location = new Point(0, 478),
                Size = new Size(contentPanel.Width, 24),
                Font = new Font("Segoe UI", 9, FontStyle.Italic),
                ForeColor = Color.FromArgb(180, 180, 180),
                TextAlign = ContentAlignment.MiddleCenter
            };

            contentPanel.Controls.Add(lblUsername);
            contentPanel.Controls.Add(usernamePanel);
            contentPanel.Controls.Add(lblPhone);
            contentPanel.Controls.Add(txtPhone);
            contentPanel.Controls.Add(lblFullName);
            contentPanel.Controls.Add(txtFullName);
            contentPanel.Controls.Add(btnChangePassword);
            contentPanel.Controls.Add(btnSupport);
            contentPanel.Controls.Add(btnDeleteAccount);
            contentPanel.Controls.Add(lblDateCreated);

            Controls.Add(contentPanel);
            Controls.Add(headerPanel);

            AdjustSuffixPosition();
        }

        private void AdjustSuffixPosition()
        {
            lblEmailSuffix.Left = txtUserName.Right + 12;
            lblEmailSuffix.Top = 8;
        }

        private void OpenSupport()
        {
            try
            {
                Process.Start(new ProcessStartInfo
                {
                    FileName = "https://chatgpt.com",
                    UseShellExecute = true
                });
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Could not open support: {ex.Message}", "Error",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private string ResolveUserName()
        {
            if (!string.IsNullOrWhiteSpace(_user.UserName))
            {
                return _user.UserName.Trim();
            }

            if (!string.IsNullOrWhiteSpace(_user.Email))
            {
                var at = _user.Email.IndexOf('@');
                if (at > 0)
                {
                    return _user.Email[..at];
                }
            }

            return string.Empty;
        }

        private string ResolvePhoneNumber()
        {
            var digits = _user.Phone?.Trim() ?? string.Empty;
            if (digits.StartsWith("0", StringComparison.Ordinal))
            {
                return digits;
            }

            return "0" + digits;
        }

        private void OpenChangePassword()
        {
            using var form = new ChangePasswordForm(_user, _service);
            form.ShowDialog(this);
        }
    }
}
