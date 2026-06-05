using System;
using System.Drawing;
using System.Threading.Tasks;
using System.Windows.Forms;
using Microsoft.Web.WebView2.WinForms;
using ReaLTaiizor.Forms;

namespace WinformsVibes
{
    class MainForm : MaterialForm
    {
        private MenuStrip _menuStrip;
        private StatusStrip _statusStrip;
        private ToolStripStatusLabel _statusLabel;

        public MainForm()
        {
            Text = "Winforms Vibes";
            WindowState = FormWindowState.Maximized;
            Icon = CreateBearIcon();

            // Menu bar
            _menuStrip = new MenuStrip { Dock = DockStyle.Top };

            // File
            var fileMenu = new ToolStripMenuItem("&File");
            fileMenu.DropDownItems.AddRange(new ToolStripItem[] {
                new ToolStripMenuItem("&New", null, (_, _) => MessageBox.Show("New not implemented.", Text, MessageBoxButtons.OK, MessageBoxIcon.Information)),
                new ToolStripMenuItem("&Open...", null, (_, _) => MessageBox.Show("Open not implemented.", Text, MessageBoxButtons.OK, MessageBoxIcon.Information)),
                new ToolStripMenuItem("&Save", null, (_, _) => MessageBox.Show("Save not implemented.", Text, MessageBoxButtons.OK, MessageBoxIcon.Information)),
                new ToolStripSeparator(),
                new ToolStripMenuItem("E&xit", null, (_, _) => Close()),
            });
            _menuStrip.Items.Add(fileMenu);

            // Edit
            _menuStrip.Items.Add(new ToolStripMenuItem("&Edit") { DropDownItems = {
                new ToolStripMenuItem("&Copy", null, (_, _) => MessageBox.Show("Copy not implemented.", Text, MessageBoxButtons.OK, MessageBoxIcon.Information)),
                new ToolStripMenuItem("&Paste", null, (_, _) => MessageBox.Show("Paste not implemented.", Text, MessageBoxButtons.OK, MessageBoxIcon.Information)),
            }});

            // View
            var viewMenu = new ToolStripMenuItem("&View");
            var fullscreenItem = new ToolStripMenuItem("T&oggle Fullscreen");
            fullscreenItem.Click += (_, _) =>
            {
                WindowState = WindowState == FormWindowState.Maximized ? FormWindowState.Normal : FormWindowState.Maximized;
            };
            var viewAboutItem = new ToolStripMenuItem("&About");
            viewAboutItem.Click += (_, _) => ShowAboutDialog();
            viewMenu.DropDownItems.AddRange(new ToolStripItem[] { fullscreenItem, viewAboutItem });
            _menuStrip.Items.Add(viewMenu);

            // Settings
            _menuStrip.Items.Add(new ToolStripMenuItem("&Settings") { DropDownItems = {
                new ToolStripMenuItem("&Preferences...", null, (_, _) => MessageBox.Show("Preferences not implemented.", Text, MessageBoxButtons.OK, MessageBoxIcon.Information)),
            }});

            // Help
            var helpMenu = new ToolStripMenuItem("&Help");
            var helpAboutItem = new ToolStripMenuItem("&About");
            helpAboutItem.Click += (_, _) => ShowAboutDialog();
            helpMenu.DropDownItems.AddRange(new ToolStripItem[] {
                new ToolStripMenuItem("&Contents", null, (_, _) => new HelpWindow().Show()),
                new ToolStripMenuItem("C&hat", null, (_, _) => ChatWindow.GetInstance().Show()),
                new ToolStripSeparator(),
                helpAboutItem,
            });
            _menuStrip.Items.Add(helpMenu);

            // Status bar
            _statusStrip = new StatusStrip();
            _statusLabel = new ToolStripStatusLabel("Ready");
            var spacer = new ToolStripStatusLabel { Spring = true };
            var timeLabel = new ToolStripStatusLabel
            {
                Text = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"),
            };
            _statusStrip.Items.AddRange(new ToolStripItem[] { _statusLabel, spacer, timeLabel });

            // Tab content
            var tab = new TabControl { Dock = DockStyle.Fill };

            // Google Maps World View
            var mapTab = new System.Windows.Forms.TabPage("World Map");
            var webView = new WebView2 { Dock = DockStyle.Fill };
            mapTab.Controls.Add(webView);
            InitializeMapAsync(webView);

            tab.TabPages.Add(mapTab);

            // Timer to update the clock in the status bar
            var timer = new System.Windows.Forms.Timer { Interval = 1000 };
            timer.Tick += (_, _) => timeLabel.Text = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
            timer.Start();

            // Layout: menu top, status bottom, tabs fill the rest
            Controls.Add(tab);
            Controls.Add(_statusStrip);
            Controls.Add(_menuStrip);
            MainMenuStrip = _menuStrip;
        }

        private void ShowAboutDialog()
        {
            MessageBox.Show(
                "Winforms Vibes\nVersion 1.0\n\nAuthor: Eric\n\nA simple Windows Forms demo application.",
                "About Winforms Vibes",
                MessageBoxButtons.OK,
                MessageBoxIcon.Information);
        }

        private async void InitializeMapAsync(WebView2 webView)
        {
            await webView.EnsureCoreWebView2Async();
            webView.CoreWebView2.Navigate("https://www.google.com/maps/@0,0,2z");
        }

        private static Icon CreateBearIcon()
        {
            using var bmp = new Bitmap(32, 32);
            using var g = Graphics.FromImage(bmp);
            g.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.AntiAlias;

            var brown = Color.FromArgb(139, 90, 43);
            var darkBrown = Color.FromArgb(80, 50, 20);
            var lightBrown = Color.FromArgb(180, 130, 80);

            g.FillEllipse(new SolidBrush(brown), 2, 4, 10, 10);
            g.FillEllipse(new SolidBrush(brown), 20, 4, 10, 10);
            g.FillEllipse(new SolidBrush(lightBrown), 4, 6, 6, 6);
            g.FillEllipse(new SolidBrush(lightBrown), 22, 6, 6, 6);
            g.FillEllipse(new SolidBrush(brown), 4, 8, 24, 20);
            g.FillEllipse(new SolidBrush(lightBrown), 10, 16, 12, 10);
            g.FillEllipse(new SolidBrush(darkBrown), 14, 16, 4, 3);

            using var pen = new Pen(darkBrown, 1);
            var mouthPath = new System.Drawing.Drawing2D.GraphicsPath();
            mouthPath.AddArc(13, 19, 6, 4, 0, 180);
            g.DrawPath(pen, mouthPath);

            g.FillEllipse(new SolidBrush(darkBrown), 9, 12, 3, 3);
            g.FillEllipse(new SolidBrush(darkBrown), 20, 12, 3, 3);
            g.FillEllipse(new SolidBrush(Color.White), 10, 12, 1, 1);
            g.FillEllipse(new SolidBrush(Color.White), 21, 12, 1, 1);

            return Icon.FromHandle(bmp.GetHicon());
        }
    }
}
