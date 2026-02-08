using System;
using System.Drawing;
using System.Windows.Forms;
using AKPay.Models;
using AKPay.Services;

namespace AKPay
{
    public partial class InsightsForm : Form
    {
        private readonly User _currentUser;
        private readonly IAKPayService _service;
        private Panel headerPanel;
        private Label lblTitle;
        private Label lblSummary;
        private ProgressBar progressSavings;
        private ProgressBar progressSpending;
        private Label lblSavings;
        private Label lblSpending;
        private Button btnClose;

        public InsightsForm(User user, IAKPayService service)
        {
            _currentUser = user;
            _service = service;
            InitializeComponent();
        }

        private void InitializeComponent()
        {
            this.Text = "Insights";
            this.Size = new Size(500, 450);
            this.StartPosition = FormStartPosition.CenterScreen;
            this.FormBorderStyle = FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            this.BackColor = Color.FromArgb(30, 30, 30);

            headerPanel = new Panel
            {
                Dock = DockStyle.Top,
                Height = 80,
                BackColor = Color.FromArgb(45, 45, 48)
            };

            lblTitle = new Label
            {
                Text = "Insights",
                Font = new Font("Segoe UI", 20, FontStyle.Bold),
                ForeColor = Color.FromArgb(0, 200, 150),
                Dock = DockStyle.Fill,
                TextAlign = ContentAlignment.MiddleCenter
            };

            headerPanel.Controls.Add(lblTitle);

            lblSummary = new Label
            {
                Text = "This month you saved 65% of your budget and spent 35%.",
                Location = new Point(30, 110),
                Size = new Size(440, 40),
                Font = new Font("Segoe UI", 11, FontStyle.Bold),
                ForeColor = Color.FromArgb(220, 220, 220)
            };

            lblSavings = new Label
            {
                Text = "Savings",
                Location = new Point(30, 170),
                Size = new Size(200, 25),
                Font = new Font("Segoe UI", 11, FontStyle.Bold),
                ForeColor = Color.FromArgb(220, 220, 220)
            };

            progressSavings = new ProgressBar
            {
                Location = new Point(30, 200),
                Size = new Size(420, 25),
                Value = 65,
                ForeColor = Color.FromArgb(0, 200, 150)
            };

            lblSpending = new Label
            {
                Text = "Spending",
                Location = new Point(30, 240),
                Size = new Size(200, 25),
                Font = new Font("Segoe UI", 11, FontStyle.Bold),
                ForeColor = Color.FromArgb(220, 220, 220)
            };

            progressSpending = new ProgressBar
            {
                Location = new Point(30, 270),
                Size = new Size(420, 25),
                Value = 35,
                ForeColor = Color.FromArgb(220, 80, 80)
            };

            btnClose = new Button
            {
                Text = "Close",
                Location = new Point(190, 320),
                Size = new Size(120, 35),
                BackColor = Color.FromArgb(70, 70, 75),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Font = new Font("Segoe UI", 11, FontStyle.Bold),
                Cursor = Cursors.Hand
            };
            btnClose.FlatAppearance.BorderSize = 0;
            btnClose.Click += (_, _) => this.Close();

            this.Controls.Add(headerPanel);
            this.Controls.Add(lblSummary);
            this.Controls.Add(lblSavings);
            this.Controls.Add(progressSavings);
            this.Controls.Add(lblSpending);
            this.Controls.Add(progressSpending);
            this.Controls.Add(btnClose);
        }
    }
}
