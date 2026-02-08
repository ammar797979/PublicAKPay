using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;
using AKPay.Models;
using AKPay.Services;

namespace AKPay
{
    public partial class UserToUserMenuForm : Form
    {
        private readonly User _currentUser;
        private readonly IAKPayService _service;
        private TextBox txtSearch = null!;
        private Button btnAddBeneficiary = null!;
        private Button btnPayNow = null!;
        private ListView lvBeneficiaries = null!;
        private Label lblStatus = null!;
        private Label lblHint = null!;
        private List<BeneficiaryDisplayDto> _beneficiaries = new();

        public UserToUserMenuForm(User user, IAKPayService service)
        {
            _currentUser = user;
            _service = service;
            InitializeComponent();
            this.Load += async (_, _) => await LoadBeneficiariesAsync();
        }

        private void InitializeComponent()
        {
            UITheme.Apply(this);

            Text = "User to User Menu";

            var headerPanel = new Panel
            {
                BackColor = Color.FromArgb(0, 200, 150),
                Dock = DockStyle.Top,
                Height = 160
            };

            var lblTitle = new Label
            {
                Text = "User to User Services",
                Font = new Font("Segoe UI", 30, FontStyle.Bold),
                ForeColor = Color.White,
                AutoSize = false,
                TextAlign = ContentAlignment.MiddleCenter,
                Dock = DockStyle.Fill
            };
            headerPanel.Controls.Add(lblTitle);

            var contentPanel = UITheme.CreateContentPanel(this, 560);
            contentPanel.Top = 220;

            txtSearch = new TextBox
            {
                Location = new Point(0, 0),
                Size = new Size(contentPanel.Width, 40),
                Font = new Font("Segoe UI", 12),
                BackColor = UITheme.SurfaceColor,
                ForeColor = Color.White,
                BorderStyle = BorderStyle.FixedSingle,
                PlaceholderText = "Search beneficiaries..."
            };
            txtSearch.TextChanged += (_, _) => ApplyFilter(txtSearch.Text);

            btnAddBeneficiary = new Button
            {
                Text = "Add Beneficiary",
                Location = new Point(0, 58),
                Size = new Size(contentPanel.Width, 52),
                BackColor = Color.FromArgb(0, 200, 150),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Font = new Font("Segoe UI", 13, FontStyle.Bold),
                Cursor = Cursors.Hand
            };
            btnAddBeneficiary.FlatAppearance.BorderSize = 0;
            btnAddBeneficiary.Click += OnAddBeneficiaryClick;

            btnPayNow = new Button
            {
                Text = "Pay Now",
                Location = new Point(0, 126),
                Size = new Size(contentPanel.Width, 52),
                BackColor = Color.FromArgb(0, 122, 204),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Font = new Font("Segoe UI", 13, FontStyle.Bold),
                Cursor = Cursors.Hand
            };
            btnPayNow.FlatAppearance.BorderSize = 0;
            btnPayNow.Click += OnPayNowClick;

            lvBeneficiaries = new ListView
            {
                Location = new Point(0, 194),
                Size = new Size(contentPanel.Width, 280),
                View = View.Details,
                FullRowSelect = true,
                GridLines = false,
                BackColor = UITheme.SurfaceColor,
                ForeColor = Color.White,
                Font = new Font("Segoe UI", 10)
            };
            lvBeneficiaries.Columns.Add("Nickname", 150);
            lvBeneficiaries.Columns.Add("Full Name", 170);
            lvBeneficiaries.Columns.Add("Username", 100);
            lvBeneficiaries.DoubleClick += OnBeneficiaryActivated;

            lblHint = new Label
            {
                Text = string.Empty,
                Location = new Point(0, 484),
                Size = new Size(contentPanel.Width, 20),
                ForeColor = Color.FromArgb(140, 140, 140),
                Font = new Font("Segoe UI", 9),
                TextAlign = ContentAlignment.MiddleCenter,
                Visible = false
            };

            lblStatus = new Label
            {
                Text = string.Empty,
                Location = new Point(0, 508),
                Size = new Size(contentPanel.Width, 40),
                ForeColor = Color.FromArgb(180, 180, 180),
                Font = new Font("Segoe UI", 9),
                AutoEllipsis = true,
                TextAlign = ContentAlignment.MiddleCenter
            };

            contentPanel.Controls.Add(txtSearch);
            contentPanel.Controls.Add(btnAddBeneficiary);
            contentPanel.Controls.Add(btnPayNow);
            contentPanel.Controls.Add(lvBeneficiaries);
            contentPanel.Controls.Add(lblHint);
            contentPanel.Controls.Add(lblStatus);

            Controls.Add(contentPanel);
            Controls.Add(headerPanel);
        }

        private async Task LoadBeneficiariesAsync()
        {
            try
            {
                var list = await _service.FetchRemittersBeneficiaries(_currentUser.UserId);
                _beneficiaries = list ?? new List<BeneficiaryDisplayDto>();
                ApplyFilter(txtSearch.Text);
            }
            catch (NotImplementedException)
            {
                lblStatus.Text = "Beneficiary list not implemented for this backend.";
            }
            catch (Exception ex)
            {
                lblStatus.Text = $"Could not load beneficiaries: {ex.Message}";
            }
        }

        private void ApplyFilter(string term)
        {
            lvBeneficiaries.Items.Clear();
            var needle = term?.Trim().ToLowerInvariant() ?? string.Empty;

            IEnumerable<BeneficiaryDisplayDto> source = _beneficiaries;
            if (!string.IsNullOrEmpty(needle))
            {
                source = source.Where(b =>
                    (b.NickName ?? string.Empty).ToLowerInvariant().Contains(needle) ||
                    (b.BeneficiaryFullName ?? string.Empty).ToLowerInvariant().Contains(needle) ||
                    (b.UserName ?? string.Empty).ToLowerInvariant().Contains(needle));
            }

            foreach (var b in source)
            {
                var item = new ListViewItem(new[] { b.NickName, b.BeneficiaryFullName, b.UserName })
                {
                    Tag = b
                };
                lvBeneficiaries.Items.Add(item);
            }

            lblHint.Visible = lvBeneficiaries.Items.Count > 0;
            if (lblHint.Visible)
            {
                lblHint.Text = "Tip: double-click a beneficiary to pay.";
            }
        }

        private async void OnAddBeneficiaryClick(object? sender, EventArgs e)
        {
            using var form = new AddBeneficiaryForm(_service, _currentUser.UserId);
            var result = form.ShowDialog();
            if (result == DialogResult.OK)
            {
                if (form.AddedBeneficiary != null)
                {
                    _beneficiaries.Add(form.AddedBeneficiary);
                    ApplyFilter(txtSearch.Text);
                    lblStatus.Text = $"Added {form.AddedBeneficiary.BeneficiaryFullName} ({form.AddedBeneficiary.UserName}) as '{form.AddedBeneficiary.NickName}'.";
                }
                else
                {
                    await LoadBeneficiariesAsync();
                }
            }
        }

        private void OnPayNowClick(object? sender, EventArgs e)
        {
            using var form = new SendMoneyForm(_currentUser, _service, null);
            form.ShowDialog();
        }

        private void OnBeneficiaryActivated(object? sender, EventArgs e)
        {
            if (lvBeneficiaries.SelectedItems.Count == 0) return;
            var tag = lvBeneficiaries.SelectedItems[0].Tag as BeneficiaryDisplayDto;
            using var form = new SendMoneyForm(_currentUser, _service, tag);
            form.ShowDialog();
        }
    }
}
