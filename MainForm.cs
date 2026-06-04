using System;
using System.Drawing;
using System.Threading.Tasks;
using System.Windows.Forms;
using Microsoft.Web.WebView2.WinForms;

namespace WinformsVibes
{
    class MainForm : Form
    {
        private MenuStrip _menuStrip;
        private ToolStripStatusLabel _statusLabel;
        private StatusStrip _statusStrip;

        public MainForm()
        {
            Text = "Winforms Vibes";
            WindowState = FormWindowState.Maximized;

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
            var fullscreenItem = new ToolStripMenuItem("T&oggle Fullscreen")
            {
                ShortcutKeys = Keys.F11,
            };
            fullscreenItem.Click += (_, _) =>
            {
                WindowState = WindowState == FormWindowState.Maximized ? FormWindowState.Normal : FormWindowState.Maximized;
            };
            var viewAboutItem = new ToolStripMenuItem("&About")
            {
                ShortcutKeys = Keys.F1,
            };
            viewAboutItem.Click += (_, _) => ShowAboutDialog();
            viewMenu.DropDownItems.AddRange(new ToolStripItem[] { fullscreenItem, viewAboutItem });
            _menuStrip.Items.Add(viewMenu);

            // Settings
            _menuStrip.Items.Add(new ToolStripMenuItem("&Settings") { DropDownItems = {
                new ToolStripMenuItem("&Preferences...", null, (_, _) => MessageBox.Show("Preferences not implemented.", Text, MessageBoxButtons.OK, MessageBoxIcon.Information)),
            }});

            // Help
            var helpMenu = new ToolStripMenuItem("&Help");
            var helpAboutItem = new ToolStripMenuItem("&About")
            {
                ShortcutKeys = Keys.F1,
            };
            helpAboutItem.Click += (_, _) => ShowAboutDialog();
            helpMenu.DropDownItems.AddRange(new ToolStripItem[] {
                new ToolStripMenuItem("&Contents", null, (_, _) => MessageBox.Show("Help not implemented.", Text, MessageBoxButtons.OK, MessageBoxIcon.Information)),
                new ToolStripSeparator(),
                helpAboutItem,
            });
            _menuStrip.Items.Add(helpMenu);

            // Status bar
            _statusStrip = new StatusStrip();
            _statusLabel = new ToolStripStatusLabel("Ready");
            _statusStrip.Items.Add(_statusLabel);

            // Tab content
            var tab = new TabControl { Dock = DockStyle.Fill };

            // Tab 1: Current Time
            var timeTab = new TabPage("Clock");
            var timeLabel = new Label
            {
                Dock = DockStyle.Fill,
                TextAlign = ContentAlignment.MiddleCenter,
                Font = new Font(Font.FontFamily, 48, FontStyle.Regular),
            };
            timeTab.Controls.Add(timeLabel);

            var timer = new System.Windows.Forms.Timer { Interval = 1000 };
            timer.Tick += (_, _) => timeLabel.Text = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
            timer.Start();
            timeLabel.Text = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");

            // Tab 2: Google Maps World View
            var mapTab = new TabPage("World Map");
            var webView = new WebView2 { Dock = DockStyle.Fill };
            mapTab.Controls.Add(webView);
            InitializeMapAsync(webView);

            tab.TabPages.AddRange(new TabPage[] { timeTab, mapTab });

            // Layout: menu top, status bottom, tabs fill the rest
            Controls.Add(tab);
            Controls.Add(_statusStrip);
            Controls.Add(_menuStrip);
            MainMenuStrip = _menuStrip;
        }

        private void ShowAboutDialog()
        {
            MessageBox.Show(
                "Winforms Vibes\nVersion 1.0\n\nA simple Windows Forms demo application.",
                "About Winforms Vibes",
                MessageBoxButtons.OK,
                MessageBoxIcon.Information);
        }

        private async void InitializeMapAsync(WebView2 webView)
        {
            await webView.EnsureCoreWebView2Async();
            webView.CoreWebView2.Navigate("https://www.google.com/maps/@0,0,2z");
        }
    }
}
