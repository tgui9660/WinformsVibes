using System.Drawing;
using System.Runtime.InteropServices;
using System.Windows.Forms;
using ReaLTaiizor.Forms;

namespace WinformsVibes.GUI;

/// <summary>
/// MaterialForm equivalent of TitleBarTooltipForm for MainForm.
/// </summary>
public class TitleBarTooltipMaterialForm : MaterialForm
{
    private readonly ToolTip _tooltip = new();
    private int _prevNcHit = 0;

    protected override void WndProc(ref Message m)
    {
        base.WndProc(ref m);

        if (m.Msg == WM_MOUSEMOVE && _tooltip != null)
        {
            int ncHit = SendMessage(Handle, WM_NCHITTEST, IntPtr.Zero, m.LParam);

            if (ncHit == HTMINBUTTON || ncHit == HTMAXBUTTON || ncHit == HTCLOSE)
            {
                string? text = ncHit switch
                {
                    HTMINBUTTON => "Minimize",
                    HTMAXBUTTON => WindowState == FormWindowState.Maximized ? "Restore Down" : "Maximize",
                    HTCLOSE => "Close",
                    _ => null,
                };
                if (text != null)
                {
                    _tooltip.SetToolTip(this, text);
                    _tooltip.Show(text, this, Cursor.Position.X, Cursor.Position.Y + 12);
                }
            }
            else if (ncHit != 0)
            {
                _tooltip.Hide(this);
            }

            _prevNcHit = ncHit;
        }
    }

    protected override void OnMouseMove(MouseEventArgs e)
    {
        base.OnMouseMove(e);
        if (_tooltip != null && _prevNcHit != 0)
        {
            _tooltip.Hide(this);
            _prevNcHit = 0;
        }
    }

    protected override void OnHandleDestroyed(EventArgs e)
    {
        _tooltip.Dispose();
        base.OnHandleDestroyed(e);
    }

    [DllImport("user32.dll")]
    private static extern int SendMessage(IntPtr hWnd, int msg, IntPtr wParam, IntPtr lParam);

    private const int WM_MOUSEMOVE = 0x0002;
    private const int WM_NCHITTEST = 0x0084;
    private const int HTMINBUTTON = 8;
    private const int HTMAXBUTTON = 9;
    private const int HTCLOSE = 20;
}
