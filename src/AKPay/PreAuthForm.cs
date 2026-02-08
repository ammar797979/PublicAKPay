using System;
using System.Drawing;
using System.Threading.Tasks;
using System.Windows.Forms;
using AKPay.Services;

namespace AKPay
{
    public class PreAuthForm : Form
    {
        private readonly IAKPayService _service;
        private TextBox _txtEmail = null!;
        private Button _btnContinue = null!;
        private Button _btnBack = null!;
        private Label _lblTitle = null!;
        private Label _lblSubtitle = null!;

        public PreAuthForm(IAKPayService service)
        {
            _service = service;
            InitializeComponent();
        }

        private void InitializeComponent()
        {
            UITheme.Apply(this);

            this.Text = "AKPay - Start";

            _lblTitle = new Label
            {
                Text = "Welcome",
                Font = new Font("Segoe UI", 28, FontStyle.Bold),
                ForeColor = Color.FromArgb(0, 200, 150),
                AutoSize = false,
                TextAlign = ContentAlignment.MiddleCenter,
                Dock = DockStyle.Top,
                Height = 140
            };

            _lblSubtitle = new Label
            {
                Text = "Enter your LUMS email to continue.",
                Font = new Font("Segoe UI", 12),
                ForeColor = Color.FromArgb(200, 200, 200),
                AutoSize = false,
                TextAlign = ContentAlignment.MiddleCenter,
                Dock = DockStyle.Top,
                Height = 40
            };

            var contentPanel = UITheme.CreateContentPanel(this, 260);
            contentPanel.Top = 240;

            _txtEmail = new TextBox
            {
                Location = new Point(0, 0),
                Size = new Size(contentPanel.Width, 44),
                Font = new Font("Segoe UI", 12),
                BackColor = Color.FromArgb(45, 45, 48),
                ForeColor = Color.White,
                BorderStyle = BorderStyle.FixedSingle
            };

            _btnContinue = new Button
            {
                Text = "Continue",
                Location = new Point(0, 70),
                Size = new Size(contentPanel.Width, 50),
                BackColor = Color.FromArgb(0, 200, 150),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Font = new Font("Segoe UI", 12, FontStyle.Bold),
                Cursor = Cursors.Hand
            };
            _btnContinue.FlatAppearance.BorderSize = 0;
            _btnContinue.Click += BtnContinue_Click;

            _btnBack = new Button
            {
                Text = "Back",
                Location = new Point(0, 140),
                Size = new Size(contentPanel.Width, 48),
                BackColor = Color.FromArgb(45, 45, 48),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Font = new Font("Segoe UI", 12, FontStyle.Bold),
                Cursor = Cursors.Hand
            };
            _btnBack.FlatAppearance.BorderSize = 0;
            _btnBack.Click += BtnBack_Click;

            contentPanel.Controls.Add(_txtEmail);
            contentPanel.Controls.Add(_btnContinue);
            contentPanel.Controls.Add(_btnBack);

            this.Controls.Add(contentPanel);
            this.Controls.Add(_lblSubtitle);
            this.Controls.Add(_lblTitle);
        }

        private async void BtnContinue_Click(object? sender, EventArgs e)
        {
            var email = _txtEmail.Text.Trim();
            if (string.IsNullOrWhiteSpace(email))
            {
                MessageBox.Show("Please enter your email.", "Validation Error",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            ToggleInputs(false);
            try
            {
                var status = await _service.IsUserRegistered(email);
                switch (status)
                {
                    case 1:
                        using (var login = new LoginForm(_service))
                        {
                            login.PrefillEmail(email);
                            this.Hide();
                            login.ShowDialog();
                            this.Close();
                        }
                        break;
                    case 0:
                        using (var register = new RegisterForm(_service))
                        {
                            register.PrefillEmail(email);
                            this.Hide();
                            register.ShowDialog();
                            this.Close();
                        }
                        break;
                    case -1:
                        MessageBox.Show("This account is deleted. Contact support.", "Account Inactive",
                            MessageBoxButtons.OK, MessageBoxIcon.Warning);
                        break;
                    case -2:
                        MessageBox.Show("Invalid email format.", "Validation Error",
                            MessageBoxButtons.OK, MessageBoxIcon.Warning);
                        break;
                    default:
                        MessageBox.Show("Unable to determine account status.", "Error",
                            MessageBoxButtons.OK, MessageBoxIcon.Error);
                        break;
                }
            }
            catch (NotImplementedException)
            {
                MessageBox.Show("Selected backend has not implemented lookup yet.", "Not Implemented",
                    MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Lookup failed: {ex.Message}", "Error",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            finally
            {
                ToggleInputs(true);
            }
        }

        private void ToggleInputs(bool enabled)
        {
            _txtEmail.Enabled = enabled;
            _btnContinue.Enabled = enabled;
            _btnBack.Enabled = enabled;
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
    }
}
