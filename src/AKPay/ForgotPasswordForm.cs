using System;
using System.Drawing;
using System.Windows.Forms;

namespace AKPay
{
    public partial class ForgotPasswordForm : Form
    {
        private TextBox txtEmail;
        private Button btnSendLink;
        private Button btnCancel;

        public ForgotPasswordForm()
        {
            InitializeComponent();
        }

        private void InitializeComponent()
        {
            UITheme.Apply(this);

            Text = "Forgot Password";

            var headerPanel = new Panel
            {
                BackColor = Color.FromArgb(0, 200, 150),
                Dock = DockStyle.Top,
                Height = 120
            };

            var lblTitle = new Label
            {
                Text = "Reset Password",
                Font = new Font("Segoe UI", 26, FontStyle.Bold),
                ForeColor = Color.White,
                AutoSize = false,
                TextAlign = ContentAlignment.MiddleCenter,
                Dock = DockStyle.Fill
            };
            headerPanel.Controls.Add(lblTitle);

            var contentPanel = UITheme.CreateContentPanel(this, 260);
            contentPanel.Top = 200;

            var lblEmail = new Label
            {
                Text = "Enter your email",
                Location = new Point(0, 0),
                Size = new Size(contentPanel.Width, 24),
                Font = new Font("Segoe UI", 11, FontStyle.Bold),
                ForeColor = Color.FromArgb(220, 220, 220)
            };

            txtEmail = new TextBox
            {
                Location = new Point(0, 28),
                Size = new Size(contentPanel.Width, 38),
                Font = new Font("Segoe UI", 12),
                BackColor = UITheme.SurfaceColor,
                ForeColor = Color.White,
                BorderStyle = BorderStyle.FixedSingle
            };

            btnSendLink = new Button
            {
                Text = "Send Reset Link",
                Location = new Point(0, 90),
                Size = new Size(contentPanel.Width, 48),
                BackColor = Color.FromArgb(0, 200, 150),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Font = new Font("Segoe UI", 12, FontStyle.Bold),
                Cursor = Cursors.Hand
            };
            btnSendLink.FlatAppearance.BorderSize = 0;
            btnSendLink.Click += BtnSendLink_Click;

            btnCancel = new Button
            {
                Text = "Cancel",
                Location = new Point(0, 154),
                Size = new Size(contentPanel.Width, 48),
                BackColor = Color.FromArgb(70, 70, 75),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Font = new Font("Segoe UI", 12, FontStyle.Bold),
                Cursor = Cursors.Hand
            };
            btnCancel.FlatAppearance.BorderSize = 0;
            btnCancel.Click += (_, _) => this.Close();

            contentPanel.Controls.Add(lblEmail);
            contentPanel.Controls.Add(txtEmail);
            contentPanel.Controls.Add(btnSendLink);
            contentPanel.Controls.Add(btnCancel);

            Controls.Add(contentPanel);
            Controls.Add(headerPanel);
        }

        private void BtnSendLink_Click(object? sender, EventArgs e)
        {
            if (string.IsNullOrWhiteSpace(txtEmail.Text))
            {
                MessageBox.Show("Please enter your email.", "Validation Error",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            MessageBox.Show("A reset link has been sent to your email.", "Success",
                MessageBoxButtons.OK, MessageBoxIcon.Information);
            this.Close();
        }
    }
}
