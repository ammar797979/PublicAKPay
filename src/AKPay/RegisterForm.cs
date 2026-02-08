using System;
using System.Drawing;
using System.Threading.Tasks;
using System.Windows.Forms;
using AKPay.Models;
using AKPay.Services;

namespace AKPay
{
    /// <summary>
    /// Registration form for new user sign-up
    /// </summary>
    public partial class RegisterForm : Form
    {
        private readonly IAKPayService _service;
        private TextBox txtFullName = null!;
        private TextBox txtEmail = null!;
        private TextBox txtPhone = null!;
        private TextBox txtPassword = null!;
        private TextBox txtConfirmPassword = null!;
        private Button btnTogglePassword = null!;
        private Button btnToggleConfirm = null!;
        private Button btnRegister = null!;
        private Button btnBack = null!;
        private Label lblTitle = null!;
        private Panel headerPanel = null!;

        public RegisterForm(IAKPayService service)
        {
            _service = service;
            InitializeComponent();
        }

        /// <summary>
        /// Initialize all UI components for the registration form
        /// </summary>
        private void InitializeComponent()
        {
            UITheme.Apply(this);

            this.Text = "AKPay - Register";

            headerPanel = new Panel
            {
                BackColor = UITheme.SurfaceColor,
                Dock = DockStyle.Top,
                Height = 140
            };

            lblTitle = new Label
            {
                Text = "Create Account",
                Font = new Font("Segoe UI", 30, FontStyle.Bold),
                ForeColor = Color.FromArgb(0, 200, 150),
                AutoSize = false,
                TextAlign = ContentAlignment.MiddleCenter,
                Dock = DockStyle.Fill
            };
            headerPanel.Controls.Add(lblTitle);

            var contentPanel = UITheme.CreateContentPanel(this, 580);
            contentPanel.Top = 200;

            Label lblFullName = new Label
            {
                Text = "Full Name",
                Location = new Point(0, 0),
                Size = new Size(contentPanel.Width, 24),
                Font = new Font("Segoe UI", 11, FontStyle.Bold),
                ForeColor = Color.FromArgb(220, 220, 220)
            };

            txtFullName = new TextBox
            {
                Location = new Point(0, 26),
                Size = new Size(contentPanel.Width, 40),
                Font = new Font("Segoe UI", 12),
                BackColor = Color.FromArgb(45, 45, 48),
                ForeColor = Color.White,
                BorderStyle = BorderStyle.FixedSingle
            };

            Label lblEmail = new Label
            {
                Text = "Email",
                Location = new Point(0, 76),
                Size = new Size(contentPanel.Width, 24),
                Font = new Font("Segoe UI", 11, FontStyle.Bold),
                ForeColor = Color.FromArgb(220, 220, 220)
            };

            txtEmail = new TextBox
            {
                Location = new Point(0, 102),
                Size = new Size(contentPanel.Width, 40),
                Font = new Font("Segoe UI", 12),
                BackColor = Color.FromArgb(45, 45, 48),
                ForeColor = Color.White,
                BorderStyle = BorderStyle.FixedSingle
            };

            Label lblPhone = new Label
            {
                Text = "Phone Number",
                Location = new Point(0, 152),
                Size = new Size(contentPanel.Width, 24),
                Font = new Font("Segoe UI", 11, FontStyle.Bold),
                ForeColor = Color.FromArgb(220, 220, 220)
            };

            txtPhone = new TextBox
            {
                Location = new Point(0, 178),
                Size = new Size(contentPanel.Width, 40),
                Font = new Font("Segoe UI", 12),
                BackColor = Color.FromArgb(45, 45, 48),
                ForeColor = Color.White,
                BorderStyle = BorderStyle.FixedSingle,
                MaxLength = 11
            };

            Label lblPhoneHint = new Label
            {
                Text = "Format: 3xxxxxxxxx (10 digits)",
                Location = new Point(0, 222),
                Size = new Size(contentPanel.Width, 20),
                Font = new Font("Segoe UI", 9),
                ForeColor = Color.FromArgb(140, 140, 140)
            };

            Label lblPassword = new Label
            {
                Text = "Password",
                Location = new Point(0, 252),
                Size = new Size(contentPanel.Width, 24),
                Font = new Font("Segoe UI", 11, FontStyle.Bold),
                ForeColor = Color.FromArgb(220, 220, 220)
            };

            var passwordPanel = new Panel
            {
                Location = new Point(0, 278),
                Size = new Size(contentPanel.Width, 42)
            };

            txtPassword = new TextBox
            {
                Dock = DockStyle.Fill,
                Margin = Padding.Empty,
                Font = new Font("Segoe UI", 12),
                PasswordChar = '•',
                UseSystemPasswordChar = true,
                BackColor = Color.FromArgb(45, 45, 48),
                ForeColor = Color.White,
                BorderStyle = BorderStyle.FixedSingle
            };

            btnTogglePassword = new Button
            {
                Text = "Show",
                Dock = DockStyle.Right,
                Width = 80,
                BackColor = Color.FromArgb(45, 45, 48),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Font = new Font("Segoe UI", 10, FontStyle.Bold),
                Cursor = Cursors.Hand,
                TabStop = false
            };
            btnTogglePassword.FlatAppearance.BorderSize = 0;
            btnTogglePassword.Click += (_, _) => TogglePasswordVisibility(txtPassword);

            passwordPanel.Controls.Add(txtPassword);
            passwordPanel.Controls.Add(btnTogglePassword);

            Label lblConfirmPassword = new Label
            {
                Text = "Confirm Password",
                Location = new Point(0, 330),
                Size = new Size(contentPanel.Width, 24),
                Font = new Font("Segoe UI", 11, FontStyle.Bold),
                ForeColor = Color.FromArgb(220, 220, 220)
            };

            var confirmPanel = new Panel
            {
                Location = new Point(0, 356),
                Size = new Size(contentPanel.Width, 42)
            };

            txtConfirmPassword = new TextBox
            {
                Dock = DockStyle.Fill,
                Margin = Padding.Empty,
                Font = new Font("Segoe UI", 12),
                PasswordChar = '•',
                UseSystemPasswordChar = true,
                BackColor = Color.FromArgb(45, 45, 48),
                ForeColor = Color.White,
                BorderStyle = BorderStyle.FixedSingle
            };

            btnToggleConfirm = new Button
            {
                Text = "Show",
                Dock = DockStyle.Right,
                Width = 80,
                BackColor = Color.FromArgb(45, 45, 48),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Font = new Font("Segoe UI", 10, FontStyle.Bold),
                Cursor = Cursors.Hand,
                TabStop = false
            };
            btnToggleConfirm.FlatAppearance.BorderSize = 0;
            btnToggleConfirm.Click += (_, _) => TogglePasswordVisibility(txtConfirmPassword);

            confirmPanel.Controls.Add(txtConfirmPassword);
            confirmPanel.Controls.Add(btnToggleConfirm);

            btnRegister = new Button
            {
                Text = "Register",
                Location = new Point(0, 424),
                Size = new Size(contentPanel.Width, 54),
                BackColor = Color.FromArgb(0, 200, 150),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Font = new Font("Segoe UI", 13, FontStyle.Bold),
                Cursor = Cursors.Hand
            };
            btnRegister.FlatAppearance.BorderSize = 0;
            btnRegister.Click += BtnRegister_Click;

            btnBack = new Button
            {
                Text = "Back",
                Location = new Point(0, 492),
                Size = new Size(contentPanel.Width, 50),
                BackColor = Color.FromArgb(45, 45, 48),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Font = new Font("Segoe UI", 12, FontStyle.Bold),
                Cursor = Cursors.Hand
            };
            btnBack.FlatAppearance.BorderSize = 0;
            btnBack.Click += BtnBack_Click;

            contentPanel.Controls.Add(lblFullName);
            contentPanel.Controls.Add(txtFullName);
            contentPanel.Controls.Add(lblEmail);
            contentPanel.Controls.Add(txtEmail);
            contentPanel.Controls.Add(lblPhone);
            contentPanel.Controls.Add(txtPhone);
            contentPanel.Controls.Add(lblPhoneHint);
            contentPanel.Controls.Add(lblPassword);
            contentPanel.Controls.Add(passwordPanel);
            contentPanel.Controls.Add(lblConfirmPassword);
            contentPanel.Controls.Add(confirmPanel);
            contentPanel.Controls.Add(btnRegister);
            contentPanel.Controls.Add(btnBack);

            this.Controls.Add(contentPanel);
            this.Controls.Add(headerPanel);
        }

        private async void BtnRegister_Click(object? sender, EventArgs e)
        {
            var fullName = txtFullName.Text.Trim();
            var email = txtEmail.Text.Trim();
            var phone = txtPhone.Text.Trim();
            var password = txtPassword.Text;
            var confirm = txtConfirmPassword.Text;

            if (string.IsNullOrWhiteSpace(fullName) ||
                string.IsNullOrWhiteSpace(email) ||
                string.IsNullOrWhiteSpace(phone) ||
                string.IsNullOrWhiteSpace(password))
            {
                MessageBox.Show("Please fill all fields.", "Validation Error",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            if (!string.Equals(password, confirm, StringComparison.Ordinal))
            {
                MessageBox.Show("Passwords do not match.", "Validation Error",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            ToggleInputs(false);
            try
            {
                var status = await _service.IsUserRegistered(email);
                if (status == 1)
                {
                    MessageBox.Show("Account already exists. Please login instead.", "Account Exists",
                        MessageBoxButtons.OK, MessageBoxIcon.Information);
                    return;
                }
                if (status == -1)
                {
                    MessageBox.Show("This account is deleted. Contact support.", "Account Inactive",
                        MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }
                if (status == -2)
                {
                    MessageBox.Show("Invalid email format.", "Validation Error",
                        MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }

                var user = await _service.CreateUser(email, phone, fullName, password);
                if (user == null)
                {
                    MessageBox.Show("Could not create account. Please check inputs or try again.", "Registration Failed",
                        MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return;
                }

                MessageBox.Show("Account created. Please login.", "Success",
                    MessageBoxButtons.OK, MessageBoxIcon.Information);

                using var login = new LoginForm(_service);
                login.PrefillEmail(email);
                this.Hide();
                login.ShowDialog();
                this.Close();
            }
            catch (NotImplementedException)
            {
                MessageBox.Show("Selected backend has not implemented registration yet.", "Not Implemented",
                    MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Registration failed: {ex.Message}", "Error",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            finally
            {
                ToggleInputs(true);
            }
        }

        private void BtnBack_Click(object? sender, EventArgs e)
        {
            this.Hide();
            using var preAuth = new PreAuthForm(_service);
            preAuth.ShowDialog();
            this.Close();
        }

        public void PrefillEmail(string email)
        {
            txtEmail.Text = email;
        }

        private void ToggleInputs(bool enabled)
        {
            txtFullName.Enabled = enabled;
            txtEmail.Enabled = enabled;
            txtPhone.Enabled = enabled;
            txtPassword.Enabled = enabled;
            txtConfirmPassword.Enabled = enabled;
            btnTogglePassword.Enabled = enabled;
            btnToggleConfirm.Enabled = enabled;
            btnRegister.Enabled = enabled;
            btnBack.Enabled = enabled;
        }

        private void TogglePasswordVisibility(TextBox box)
        {
            box.UseSystemPasswordChar = !box.UseSystemPasswordChar;
            box.PasswordChar = box.UseSystemPasswordChar ? '•' : '\0';
        }
    }
}
