using System;
using System.Drawing;
using System.Windows.Forms;
using AKPay.Models;
using AKPay.Services;

namespace AKPay
{
    public partial class AddBeneficiaryForm : Form
    {
        private readonly IAKPayService _service;
        private readonly int _remitterId;
        private TextBox txtUsername;
        private TextBox txtNickname;
        private Button btnAdd;
        private Button btnCancel;

        public BeneficiaryDisplayDto? AddedBeneficiary { get; private set; }

        public AddBeneficiaryForm(IAKPayService service, int remitterId)
        {
            _service = service;
            _remitterId = remitterId;
            InitializeComponent();
        }

        private void InitializeComponent()
        {
            UITheme.Apply(this);

            Text = "Add Beneficiary";

            var headerPanel = new Panel
            {
                BackColor = Color.FromArgb(0, 200, 150),
                Dock = DockStyle.Top,
                Height = 140
            };

            var lblTitle = new Label
            {
                Text = "Add Beneficiary",
                Font = new Font("Segoe UI", 30, FontStyle.Bold),
                ForeColor = Color.White,
                AutoSize = false,
                TextAlign = ContentAlignment.MiddleCenter,
                Dock = DockStyle.Fill
            };
            headerPanel.Controls.Add(lblTitle);

            var contentPanel = UITheme.CreateContentPanel(this, 360);
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

            var lblNickname = new Label
            {
                Text = "Nickname (optional)",
                Location = new Point(0, 88),
                Size = new Size(contentPanel.Width, 24),
                Font = new Font("Segoe UI", 11, FontStyle.Bold),
                ForeColor = Color.FromArgb(220, 220, 220)
            };

            txtNickname = new TextBox
            {
                Location = new Point(0, 116),
                Size = new Size(contentPanel.Width, 40),
                Font = new Font("Segoe UI", 12),
                BackColor = UITheme.SurfaceColor,
                ForeColor = Color.White,
                BorderStyle = BorderStyle.FixedSingle
            };

            btnAdd = new Button
            {
                Text = "Add Beneficiary",
                Location = new Point(0, 188),
                Size = new Size(contentPanel.Width, 52),
                BackColor = Color.FromArgb(0, 200, 150),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Font = new Font("Segoe UI", 13, FontStyle.Bold),
                Cursor = Cursors.Hand
            };
            btnAdd.FlatAppearance.BorderSize = 0;
            btnAdd.Click += BtnAdd_Click;

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
            contentPanel.Controls.Add(lblNickname);
            contentPanel.Controls.Add(txtNickname);
            contentPanel.Controls.Add(btnAdd);
            contentPanel.Controls.Add(btnCancel);

            Controls.Add(contentPanel);
            Controls.Add(headerPanel);
        }

        private async void BtnAdd_Click(object? sender, EventArgs e)
        {
            if (string.IsNullOrWhiteSpace(txtUsername.Text))
            {
                MessageBox.Show("Please enter a beneficiary username.", "Validation Error",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            btnAdd.Enabled = false;
            try
            {
                var nickname = string.IsNullOrWhiteSpace(txtNickname.Text) ? null : txtNickname.Text.Trim();
                var created = await _service.CreateBeneficiary(_remitterId, txtUsername.Text.Trim(), nickname);
                if (!created)
                {
                    MessageBox.Show("Could not add beneficiary. Please check details and try again.", "Failed",
                        MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return;
                }

                // Fetch details to display back
                var details = await _service.FetchBenfDetails(txtUsername.Text.Trim());
                if (details != null)
                {
                    AddedBeneficiary = new BeneficiaryDisplayDto
                    {
                        BeneficiaryFullName = details.FullName,
                        UserName = details.UserName ?? details.Email,
                        NickName = nickname ?? txtUsername.Text.Trim()
                    };
                }

                MessageBox.Show("Beneficiary added successfully!", "Success",
                    MessageBoxButtons.OK, MessageBoxIcon.Information);
                this.DialogResult = DialogResult.OK;
                this.Close();
            }
            catch (NotImplementedException)
            {
                MessageBox.Show("Selected backend has not implemented add beneficiary yet.", "Not Implemented",
                    MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Failed to add beneficiary: {ex.Message}", "Error",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            finally
            {
                btnAdd.Enabled = true;
            }
        }
    }
}
