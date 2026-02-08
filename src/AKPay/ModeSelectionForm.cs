using System;
using System.Drawing;
using System.Windows.Forms;
using AKPay.Services;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace AKPay
{
    public class ModeSelectionForm : Form
    {
        private readonly IHost _host;
        private Button _btnLinq = null!;
        private Button _btnSproc = null!;
        private Label _lblTitle = null!;
        private Label _lblSubtitle = null!;

        public ModeSelectionForm(IHost host)
        {
            _host = host;
            InitializeComponent();
        }

        private void InitializeComponent()
        {
            UITheme.Apply(this);

            Text = "Select Implementation";

            var headerPanel = new Panel
            {
                BackColor = Color.FromArgb(0, 200, 150),
                Dock = DockStyle.Top,
                Height = 180
            };

            _lblTitle = new Label
            {
                Text = "Choose Backend",
                Font = new Font("Segoe UI", 32, FontStyle.Bold),
                ForeColor = Color.White,
                AutoSize = false,
                TextAlign = ContentAlignment.MiddleCenter,
                Dock = DockStyle.Fill
            };
            headerPanel.Controls.Add(_lblTitle);

            var contentPanel = UITheme.CreateContentPanel(this, 320);
            contentPanel.Top = 220;

            _lblSubtitle = new Label
            {
                Text = "Pick LINQ (EF) or stored procedures for this session.",
                Location = new Point(0, 0),
                Size = new Size(contentPanel.Width, 60),
                Font = new Font("Segoe UI", 11),
                ForeColor = Color.FromArgb(200, 200, 200),
                TextAlign = ContentAlignment.MiddleCenter
            };

            _btnLinq = new Button
            {
                Text = "LINQ",
                Location = new Point(0, 80),
                Size = new Size(contentPanel.Width, 58),
                BackColor = Color.FromArgb(0, 200, 150),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Font = new Font("Segoe UI", 14, FontStyle.Bold),
                Cursor = Cursors.Hand
            };
            _btnLinq.FlatAppearance.BorderSize = 0;
            _btnLinq.Click += (_, _) => Launch(ServiceFlavor.Linq);

            _btnSproc = new Button
            {
                Text = "S-Proc",
                Location = new Point(0, 152),
                Size = new Size(contentPanel.Width, 58),
                BackColor = Color.FromArgb(70, 70, 75),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Font = new Font("Segoe UI", 14, FontStyle.Bold),
                Cursor = Cursors.Hand
            };
            _btnSproc.FlatAppearance.BorderSize = 0;
            _btnSproc.Click += (_, _) => Launch(ServiceFlavor.Sproc);

            contentPanel.Controls.Add(_lblSubtitle);
            contentPanel.Controls.Add(_btnLinq);
            contentPanel.Controls.Add(_btnSproc);

            Controls.Add(contentPanel);
            Controls.Add(headerPanel);
        }

        private void Launch(ServiceFlavor flavor)
        {
            ServiceMode.Current = flavor;

            using var scope = _host.Services.CreateScope();
            var preAuth = scope.ServiceProvider.GetRequiredService<PreAuthForm>();

            this.Hide();
            preAuth.ShowDialog();
            this.Close();
        }
    }
}
