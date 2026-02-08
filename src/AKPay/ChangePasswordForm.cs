using System;
using System.Drawing;
using System.Threading.Tasks;
using System.Windows.Forms;
using AKPay.Models;
using AKPay.Services;

namespace AKPay
{
    public partial class ChangePasswordForm : Form
    {
        private readonly User _user;
        private readonly IAKPayService _service;
        private TextBox _txtCurrentPassword = null!;
        private TextBox _txtNewPassword = null!;
        private TextBox _txtConfirmPassword = null!;
        private Button _btnToggleCurrent = null!;
        private Button _btnToggleNew = null!;
        private Button _btnToggleConfirm = null!;
        private Button _btnConfirm = null!;
        private Button _btnBack = null!;

        public ChangePasswordForm(User user, IAKPayService service)
        {
            _user = user;
            _service = service;
            InitializeComponent();
        }

        private void InitializeComponent()
        {
            UITheme.Apply(this);

            Text = "Change Password";

            var headerPanel = new Panel
            {
                BackColor = Color.FromArgb(0, 200, 150),
                Dock = DockStyle.Top,
                Height = 160
            };

            var lblTitle = new Label
            {
                Text = "Change Password",
                Font = new Font("Segoe UI", 30, FontStyle.Bold),
                ForeColor = Color.White,
                AutoSize = false,
                TextAlign = ContentAlignment.MiddleCenter,
                Dock = DockStyle.Fill
            };
            headerPanel.Controls.Add(lblTitle);

            var contentPanel = UITheme.CreateContentPanel(this, 520);
            contentPanel.Top = 220;

            var lblCurrent = new Label
            {
                Text = "Current Password",
                Location = new Point(0, 0),
                Size = new Size(contentPanel.Width, 24),
                Font = new Font("Segoe UI", 11, FontStyle.Bold),
                ForeColor = Color.FromArgb(220, 220, 220)
            };

            var currentPanel = CreatePasswordPanel(out _txtCurrentPassword, out _btnToggleCurrent, 28);
            _btnToggleCurrent.Click += (_, _) => TogglePasswordVisibility(_txtCurrentPassword, _btnToggleCurrent);

            var lblNew = new Label
            {
                Text = "New Password",
                Location = new Point(0, 96),
                Size = new Size(contentPanel.Width, 24),
                Font = new Font("Segoe UI", 11, FontStyle.Bold),
                ForeColor = Color.FromArgb(220, 220, 220)
            };

            var newPanel = CreatePasswordPanel(out _txtNewPassword, out _btnToggleNew, 124);
            _btnToggleNew.Click += (_, _) => TogglePasswordVisibility(_txtNewPassword, _btnToggleNew);

            var lblConfirm = new Label
            {
                Text = "Confirm New Password",
                Location = new Point(0, 192),
                Size = new Size(contentPanel.Width, 24),
                Font = new Font("Segoe UI", 11, FontStyle.Bold),
                ForeColor = Color.FromArgb(220, 220, 220)
            };

            var confirmPanel = CreatePasswordPanel(out _txtConfirmPassword, out _btnToggleConfirm, 220);
            _btnToggleConfirm.Click += (_, _) => TogglePasswordVisibility(_txtConfirmPassword, _btnToggleConfirm);

            _btnConfirm = new Button
            {
                Text = "Confirm",
                Location = new Point(0, 300),
                Size = new Size(contentPanel.Width, 54),
                BackColor = Color.FromArgb(0, 200, 150),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Font = new Font("Segoe UI", 13, FontStyle.Bold),
                Cursor = Cursors.Hand
            };
            _btnConfirm.FlatAppearance.BorderSize = 0;
            _btnConfirm.Click += async (_, _) => await SubmitAsync();

            _btnBack = new Button
            {
                Text = "Back",
                Location = new Point(0, 368),
                Size = new Size(contentPanel.Width, 54),
                BackColor = Color.FromArgb(70, 70, 75),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Font = new Font("Segoe UI", 13, FontStyle.Bold),
                Cursor = Cursors.Hand
            };
            _btnBack.FlatAppearance.BorderSize = 0;
            _btnBack.Click += (_, _) => Close();

            contentPanel.Controls.Add(lblCurrent);
            contentPanel.Controls.Add(currentPanel);
            contentPanel.Controls.Add(lblNew);
            contentPanel.Controls.Add(newPanel);
            contentPanel.Controls.Add(lblConfirm);
            contentPanel.Controls.Add(confirmPanel);
            contentPanel.Controls.Add(_btnConfirm);
            contentPanel.Controls.Add(_btnBack);

            Controls.Add(contentPanel);
            Controls.Add(headerPanel);
        }

        private Panel CreatePasswordPanel(out TextBox textBox, out Button toggleButton, int top)
        {
            var panel = new Panel
            {
                Location = new Point(0, top),
                Size = new Size(UITheme.ContentWidth, 46)
            };

            textBox = new TextBox
            {
                Dock = DockStyle.Fill,
                Margin = Padding.Empty,
                Font = new Font("Segoe UI", 12),
                UseSystemPasswordChar = true,
                PasswordChar = '•',
                BackColor = UITheme.SurfaceColor,
                ForeColor = Color.White,
                BorderStyle = BorderStyle.FixedSingle
            };

            toggleButton = new Button
            {
                Text = "Show",
                Dock = DockStyle.Right,
                Width = 80,
                FlatStyle = FlatStyle.Flat,
                BackColor = Color.FromArgb(70, 70, 75),
                ForeColor = Color.White,
                Font = new Font("Segoe UI", 10, FontStyle.Bold),
                Cursor = Cursors.Hand
            };
            toggleButton.FlatAppearance.BorderSize = 0;

            panel.Controls.Add(textBox);
            panel.Controls.Add(toggleButton);
            return panel;
        }

        private void TogglePasswordVisibility(TextBox textBox, Button toggleButton)
        {
            textBox.UseSystemPasswordChar = !textBox.UseSystemPasswordChar;
            textBox.PasswordChar = textBox.UseSystemPasswordChar ? '•' : '\0';
            toggleButton.Text = textBox.UseSystemPasswordChar ? "Show" : "Hide";
        }

        private async Task SubmitAsync()
        {
            var current = _txtCurrentPassword.Text.Trim();
            var next = _txtNewPassword.Text.Trim();
            var confirm = _txtConfirmPassword.Text.Trim();

            if (string.IsNullOrEmpty(current))
            {
                MessageBox.Show("Enter your current password.", "Validation", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                _txtCurrentPassword.Focus();
                return;
            }

            if (string.IsNullOrEmpty(next))
            {
                MessageBox.Show("Enter a new password.", "Validation", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                _txtNewPassword.Focus();
                return;
            }

            if (string.IsNullOrEmpty(confirm))
            {
                MessageBox.Show("Re-enter your new password.", "Validation", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                _txtConfirmPassword.Focus();
                return;
            }

            if (!string.Equals(next, confirm, StringComparison.Ordinal))
            {
                MessageBox.Show("New passwords do not match.", "Validation", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                _txtConfirmPassword.Focus();
                return;
            }

            if (string.Equals(current, next, StringComparison.Ordinal))
            {
                MessageBox.Show("New password must be different from the current password.", "Validation", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                _txtNewPassword.Focus();
                return;
            }

            ToggleInputs(false);
            try
            {
                var success = await _service.ChangePassword(_user.Email, current, next, confirm);
                if (!success)
                {
                    MessageBox.Show("Password update failed. Please check your credentials and try again.", "Update Failed", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return;
                }

                MessageBox.Show("Your password has been updated successfully.", "Success", MessageBoxButtons.OK, MessageBoxIcon.Information);
                DialogResult = DialogResult.OK;
                Close();
            }
            catch (NotImplementedException)
            {
                MessageBox.Show("Change password is not implemented for the selected backend.", "Not Implemented", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Password update failed: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            finally
            {
                ToggleInputs(true);
            }
        }

        private void ToggleInputs(bool enabled)
        {
            _txtCurrentPassword.Enabled = enabled;
            _txtNewPassword.Enabled = enabled;
            _txtConfirmPassword.Enabled = enabled;
            _btnToggleCurrent.Enabled = enabled;
            _btnToggleNew.Enabled = enabled;
            _btnToggleConfirm.Enabled = enabled;
            _btnConfirm.Enabled = enabled;
            _btnBack.Enabled = enabled;
        }
    }
}
