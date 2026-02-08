using System;
using System.Drawing;
using System.Windows.Forms;
using AKPay.Models;
using AKPay.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace AKPay
{
    /// <summary>
    /// Login form for user authentication.
    /// </summary>
    public partial class LoginForm : Form
    {
        private readonly IAKPayService _service;
        private TextBox txtEmail = null!;
        private TextBox txtPassword = null!;
        private Button btnTogglePassword = null!;
        private LinkLabel lnkForgotPassword = null!;
        private Button btnLogin = null!;
        private Button btnRegister = null!;
        private Button btnBack = null!;

        public LoginForm(IAKPayService service)
        {
            _service = service;
            InitializeComponent();
        }

        private void InitializeComponent()
        {
            UITheme.Apply(this);

            Text = "AKPay - Login";

            var headerPanel = new Panel
            {
                BackColor = Color.FromArgb(0, 200, 150),
                Dock = DockStyle.Top,
                Height = 160
            };

            var lblTitle = new Label
            {
                Text = "Welcome Back",
                Font = new Font("Segoe UI", 32, FontStyle.Bold),
                ForeColor = Color.White,
                AutoSize = false,
                TextAlign = ContentAlignment.MiddleCenter,
                Dock = DockStyle.Fill
            };
            headerPanel.Controls.Add(lblTitle);

            var contentPanel = UITheme.CreateContentPanel(this, 540);
            contentPanel.Top = 220;

            var lblEmail = new Label
            {
                Text = "Email",
                Location = new Point(0, 0),
                Size = new Size(contentPanel.Width, 24),
                Font = new Font("Segoe UI", 11, FontStyle.Bold),
                ForeColor = Color.FromArgb(220, 220, 220)
            };

            txtEmail = new TextBox
            {
                Location = new Point(0, 30),
                Size = new Size(contentPanel.Width, 44),
                Font = new Font("Segoe UI", 12),
                BackColor = UITheme.SurfaceColor,
                ForeColor = Color.White,
                BorderStyle = BorderStyle.FixedSingle
            };

            var lblPassword = new Label
            {
                Text = "Password",
                Location = new Point(0, 92),
                Size = new Size(contentPanel.Width, 24),
                Font = new Font("Segoe UI", 11, FontStyle.Bold),
                ForeColor = Color.FromArgb(220, 220, 220)
            };

            var passwordPanel = new Panel
            {
                Location = new Point(0, 122),
                Size = new Size(contentPanel.Width, 46)
            };

            txtPassword = new TextBox
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

            btnTogglePassword = new Button
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
            btnTogglePassword.FlatAppearance.BorderSize = 0;
            btnTogglePassword.Click += (_, _) => TogglePasswordVisibility();

            passwordPanel.Controls.Add(txtPassword);
            passwordPanel.Controls.Add(btnTogglePassword);

            lnkForgotPassword = new LinkLabel
            {
                Text = "Forgot Password?",
                Location = new Point(0, 182),
                Size = new Size(contentPanel.Width, 24),
                Font = new Font("Segoe UI", 9),
                TextAlign = ContentAlignment.MiddleCenter,
                LinkColor = Color.FromArgb(0, 200, 150),
                ActiveLinkColor = Color.FromArgb(0, 150, 100),
                VisitedLinkColor = Color.FromArgb(0, 200, 150)
            };
            lnkForgotPassword.Click += LnkForgotPassword_Click;

            btnLogin = new Button
            {
                Text = "Login",
                Location = new Point(0, 222),
                Size = new Size(contentPanel.Width, 54),
                BackColor = Color.FromArgb(0, 200, 150),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Font = new Font("Segoe UI", 13, FontStyle.Bold),
                Cursor = Cursors.Hand
            };
            btnLogin.FlatAppearance.BorderSize = 0;
            btnLogin.Click += BtnLogin_Click;

            btnRegister = new Button
            {
                Text = "Create New Account",
                Location = new Point(0, 290),
                Size = new Size(contentPanel.Width, 54),
                BackColor = Color.FromArgb(70, 70, 75),
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
                Location = new Point(0, 358),
                Size = new Size(contentPanel.Width, 50),
                BackColor = UITheme.SurfaceColor,
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Font = new Font("Segoe UI", 12, FontStyle.Bold),
                Cursor = Cursors.Hand
            };
            btnBack.FlatAppearance.BorderSize = 0;
            btnBack.Click += BtnBack_Click;

            contentPanel.Controls.Add(lblEmail);
            contentPanel.Controls.Add(txtEmail);
            contentPanel.Controls.Add(lblPassword);
            contentPanel.Controls.Add(passwordPanel);
            contentPanel.Controls.Add(lnkForgotPassword);
            contentPanel.Controls.Add(btnLogin);
            contentPanel.Controls.Add(btnRegister);
            contentPanel.Controls.Add(btnBack);

            Controls.Add(contentPanel);
            Controls.Add(headerPanel);
        }

        private async void BtnLogin_Click(object? sender, EventArgs e)
        {
            if (string.IsNullOrWhiteSpace(txtEmail.Text))
            {
                MessageBox.Show("Please enter your email.", "Validation Error",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            if (string.IsNullOrWhiteSpace(txtPassword.Text))
            {
                MessageBox.Show("Please enter your password.", "Validation Error",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            ToggleInputs(false);
            try
            {
                var user = await _service.AuthUser(txtEmail.Text.Trim(), txtPassword.Text);

                if (user == null)
                {
                    MessageBox.Show("Invalid email or password.", "Login Failed",
                        MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return;
                }

                if (user.UserAccount == null)
                {
                    try
                    {
                        using var scope = HostAccessor.Current?.Services.CreateScope();
                        if (scope != null)
                        {
                            var db = scope.ServiceProvider.GetRequiredService<AkpayDbContext>();
                            var account = await db.UserAccounts.AsNoTracking().FirstOrDefaultAsync(a => a.UserId == user.UserId);
                            if (account != null)
                            {
                                user.UserAccount = account;
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"Could not preload account: {ex.Message}");
                    }
                }

                using var dashboard = new DashboardForm(user, _service);
                this.Hide();
                dashboard.ShowDialog();
                this.Close();
            }
            catch (NotImplementedException)
            {
                MessageBox.Show("Selected backend has not implemented login yet.", "Not Implemented",
                    MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Login failed: {ex.Message}", "Error",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            finally
            {
                ToggleInputs(true);
            }
        }

        private void BtnRegister_Click(object? sender, EventArgs e)
        {
            using var registerForm = new RegisterForm(_service);
            this.Hide();
            registerForm.ShowDialog();
            this.Close();
        }

        private void BtnBack_Click(object? sender, EventArgs e)
        {
            var host = HostAccessor.Current;
            if (host == null)
            {
                this.Close();
                return;
            }

            using var selection = new ModeSelectionForm(host);
            this.Hide();
            selection.ShowDialog();
            this.Close();
        }

        private void LnkForgotPassword_Click(object? sender, EventArgs e)
        {
            using var forgotPasswordForm = new ForgotPasswordForm();
            forgotPasswordForm.ShowDialog();
        }

        public void PrefillEmail(string email)
        {
            txtEmail.Text = email;
        }

        private void ToggleInputs(bool enabled)
        {
            txtEmail.Enabled = enabled;
            txtPassword.Enabled = enabled;
            btnTogglePassword.Enabled = enabled;
            btnLogin.Enabled = enabled;
            btnRegister.Enabled = enabled;
            btnBack.Enabled = enabled;
        }

        private void TogglePasswordVisibility()
        {
            txtPassword.UseSystemPasswordChar = !txtPassword.UseSystemPasswordChar;
            txtPassword.PasswordChar = txtPassword.UseSystemPasswordChar ? '•' : '\0';
            btnTogglePassword.Text = txtPassword.UseSystemPasswordChar ? "Show" : "Hide";
        }
    }
}
