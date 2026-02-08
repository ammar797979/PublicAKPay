using System.Drawing;
using System.Windows.Forms;

namespace AKPay;

public static class UITheme
{
    public static readonly Size FormSize = new Size(540, 960);
    public static readonly int ContentWidth = 420;
    public static readonly Color BackgroundColor = Color.FromArgb(30, 30, 30);
    public static readonly Color SurfaceColor = Color.FromArgb(45, 45, 48);

    public static void Apply(Form form)
    {
        form.SuspendLayout();
        form.AutoScaleMode = AutoScaleMode.None;
        form.Size = FormSize;
        form.StartPosition = FormStartPosition.CenterScreen;
        form.FormBorderStyle = FormBorderStyle.FixedDialog;
        form.MaximizeBox = false;
        form.BackColor = BackgroundColor;
        form.ResumeLayout();
    }

    public static Panel CreateContentPanel(Control parent, int height)
    {
        var panel = new Panel
        {
            Size = new Size(ContentWidth, height),
            BackColor = Color.Transparent
        };

        CenterHorizontally(parent, panel);
        return panel;
    }

    public static void CenterHorizontally(Control parent, Control child)
    {
        child.Left = (parent.ClientSize.Width - child.Width) / 2;
    }
}
