using System;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using AKPay.Models;
using AKPay.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace AKPay
{
    /// <summary>
    /// Main dashboard form showing user balance and menu options
    /// </summary>
    public partial class DashboardForm : Form
    {
        private readonly User _currentUser;
        private readonly IAKPayService _service;
        private Label lblWelcome;
        private Label lblBalance;
        private Label lblBalanceValue;
        private Button btnUserToUser;
        private Button btnRegularTransaction;
        private Button btnTopUp;
        private Button btnTransactionHistory;
        private Button btnInsights;
        private Button btnProfile;
        private Button btnLogout;
        private Panel headerPanel;
        private Panel balancePanel;
        private Panel menuPanel;

        public DashboardForm(User user, IAKPayService service)
        {
            _currentUser = user;
            _service = service;
            InitializeComponent();
            LoadUserData();
        }

        private void InitializeComponent()
        {
            UITheme.Apply(this);

            this.Text = "AKPay - Dashboard";

            headerPanel = new Panel
            {
                BackColor = UITheme.SurfaceColor,
                Dock = DockStyle.Top,
                Height = 140
            };

            lblWelcome = new Label
            {
                Text = "Welcome to AKPay",
                Font = new Font("Segoe UI", 26, FontStyle.Bold),
                ForeColor = Color.FromArgb(0, 200, 150),
                AutoSize = false,
                TextAlign = ContentAlignment.MiddleLeft,
                Location = new Point(20, 0),
                Size = new Size(500, 140)
            };

            headerPanel.Controls.Add(lblWelcome);

            balancePanel = UITheme.CreateContentPanel(this, 120);
            balancePanel.Top = 180;
            balancePanel.Height = 120;
            balancePanel.BackColor = UITheme.SurfaceColor;
            balancePanel.BorderStyle = BorderStyle.None;

            lblBalance = new Label
            {
                Text = "Current Balance",
                Font = new Font("Segoe UI", 14),
                ForeColor = Color.FromArgb(180, 180, 180),
                Location = new Point(20, 20),
                AutoSize = true
            };

            lblBalanceValue = new Label
            {
                Text = "PKR 0.00",
                Font = new Font("Segoe UI", 30, FontStyle.Bold),
                ForeColor = Color.FromArgb(0, 200, 150),
                Location = new Point(20, 45),
                AutoSize = true
            };

            balancePanel.Controls.Add(lblBalance);
            balancePanel.Controls.Add(lblBalanceValue);

            menuPanel = UITheme.CreateContentPanel(this, 520);
            menuPanel.Top = 340;
            menuPanel.BackColor = Color.Transparent;

            btnUserToUser = new Button
            {
                Text = "User to User",
                Location = new Point(0, 20),
                Size = new Size(menuPanel.Width, 60),
                BackColor = Color.FromArgb(0, 200, 150),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Font = new Font("Segoe UI", 13, FontStyle.Bold),
                Cursor = Cursors.Hand
            };
            btnUserToUser.FlatAppearance.BorderSize = 0;
            btnUserToUser.Click += BtnUserToUser_Click;

            btnRegularTransaction = new Button
            {
                Text = "Regular Transaction",
                Location = new Point(0, 90),
                Size = new Size(menuPanel.Width, 60),
                BackColor = Color.FromArgb(0, 200, 150),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Font = new Font("Segoe UI", 13, FontStyle.Bold),
                Cursor = Cursors.Hand
            };
            btnRegularTransaction.FlatAppearance.BorderSize = 0;
            btnRegularTransaction.Click += BtnRegularTransaction_Click;

            btnTopUp = new Button
            {
                Text = "Top Up Account",
                Location = new Point(0, 160),
                Size = new Size(menuPanel.Width, 60),
                BackColor = Color.FromArgb(155, 89, 182),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Font = new Font("Segoe UI", 13, FontStyle.Bold),
                Cursor = Cursors.Hand
            };
            btnTopUp.FlatAppearance.BorderSize = 0;
            btnTopUp.Click += BtnTopUp_Click;

            btnTransactionHistory = new Button
            {
                Text = "Transaction History",
                Location = new Point(0, 230),
                Size = new Size(menuPanel.Width, 60),
                BackColor = Color.FromArgb(60, 130, 200),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Font = new Font("Segoe UI", 13, FontStyle.Bold),
                Cursor = Cursors.Hand
            };
            btnTransactionHistory.FlatAppearance.BorderSize = 0;
            btnTransactionHistory.Click += BtnTransactionHistory_Click;

            btnInsights = new Button
            {
                Text = "Insights",
                Location = new Point(0, 300),
                Size = new Size(menuPanel.Width, 60),
                BackColor = Color.FromArgb(140, 100, 200),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Font = new Font("Segoe UI", 13, FontStyle.Bold),
                Cursor = Cursors.Hand
            };
            btnInsights.FlatAppearance.BorderSize = 0;
            btnInsights.Click += BtnInsights_Click;

            btnProfile = new Button
            {
                Text = "Profile / Settings",
                Location = new Point(0, 370),
                Size = new Size((menuPanel.Width / 2) - 5, 60),
                BackColor = Color.FromArgb(70, 70, 75),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Font = new Font("Segoe UI", 13, FontStyle.Bold),
                Cursor = Cursors.Hand
            };
            btnProfile.FlatAppearance.BorderSize = 0;
            btnProfile.Click += BtnProfile_Click;

            btnLogout = new Button
            {
                Text = "Logout",
                Location = new Point((menuPanel.Width / 2) + 5, 370),
                Size = new Size((menuPanel.Width / 2) - 5, 60),
                BackColor = Color.FromArgb(200, 50, 50),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Font = new Font("Segoe UI", 13, FontStyle.Bold),
                Cursor = Cursors.Hand
            };
            btnLogout.FlatAppearance.BorderSize = 0;
            btnLogout.Click += BtnLogout_Click;

            menuPanel.Controls.Add(btnUserToUser);
            menuPanel.Controls.Add(btnRegularTransaction);
            menuPanel.Controls.Add(btnTopUp);
            menuPanel.Controls.Add(btnTransactionHistory);
            menuPanel.Controls.Add(btnInsights);
            menuPanel.Controls.Add(btnProfile);
            menuPanel.Controls.Add(btnLogout);

            this.Controls.Add(headerPanel);
            this.Controls.Add(balancePanel);
            this.Controls.Add(menuPanel);
        }

        private void LoadUserData()
        {
            if (_currentUser != null)
            {
                try
                {
                    using var scope = HostAccessor.Current?.Services.CreateScope();
                    if (scope != null)
                    {
                        var db = scope.ServiceProvider.GetRequiredService<AkpayDbContext>();
                        var refreshed = db.UserAccounts.AsNoTracking()
                            .FirstOrDefault(a => a.UserId == _currentUser.UserId);
                        if (refreshed != null)
                        {
                            _currentUser.UserAccount = refreshed;
                        }
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Could not refresh dashboard balance: {ex.Message}");
                }

                lblWelcome.Text = $"Welcome, {_currentUser.FullName}";
                var balance = _currentUser.UserAccount?.UserBalance ?? 0m;
                lblBalanceValue.Text = $"PKR {balance:N2}";
            }
        }

        private void BtnUserToUser_Click(object? sender, EventArgs e)
        {
            using var userToUserMenu = new UserToUserMenuForm(_currentUser, _service);
            userToUserMenu.ShowDialog();
            LoadUserData();
        }

        private void BtnRegularTransaction_Click(object? sender, EventArgs e)
        {
            using var regularTransaction = new RegularTransactionForm(_currentUser, _service);
            regularTransaction.ShowDialog();
            LoadUserData();
        }

        private void BtnTopUp_Click(object? sender, EventArgs e)
        {
            using var topUp = new TopUpForm(_currentUser, _service);
            topUp.ShowDialog();
            LoadUserData();
        }

        private void BtnTransactionHistory_Click(object? sender, EventArgs e)
        {
            using var history = new TransactionHistoryForm(_currentUser, _service);
            history.ShowDialog();
        }

        private void BtnInsights_Click(object? sender, EventArgs e)
        {
            using var insights = new InsightsForm(_currentUser, _service);
            insights.ShowDialog();
        }

        private void BtnProfile_Click(object? sender, EventArgs e)
        {
            using var profile = new ProfileSettingsForm(_currentUser, _service);
            profile.ShowDialog();
        }

        private void BtnLogout_Click(object? sender, EventArgs e)
        {
            this.Hide();
            using var preAuth = new PreAuthForm(_service);
            preAuth.ShowDialog();
            this.Close();
        }
    }
}
