using System;
using System.Drawing;
using System.Threading.Tasks;
using System.Windows.Forms;
using Microsoft.Web.WebView2.WinForms;
using ReaLTaiizor.Forms;

namespace WinformsVibes.GUI
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

            // Chat
            _menuStrip.Items.Add(new ToolStripMenuItem("&Chat") { DropDownItems = {
                new ToolStripMenuItem("AI Chat", null, (_, _) => ChatWindow.GetInstance().Show()),
            }});

            // Help
            var helpMenu = new ToolStripMenuItem("&Help");
            var helpAboutItem = new ToolStripMenuItem("&About");
            helpAboutItem.Click += (_, _) => ShowAboutDialog();
            helpMenu.DropDownItems.AddRange(new ToolStripItem[] {
                new ToolStripMenuItem("&Contents", null, (_, _) => new HelpWindow().Show()),
                new ToolStripMenuItem("AI &Help", null, (_, _) => AIHelpWindow.GetInstance().Show()),
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

            var coordPanel = new Panel
            {
                Dock = DockStyle.Bottom,
                Height = 36,
                Padding = new Padding(8, 4, 8, 4),
            };

            var latLabel = new Label
            {
                Text = "Lat",
                ForeColor = Color.Black,
                Location = new Point(8, 6),
                AutoSize = true,
            };

            var latInput = new TextBox
            {
                Text = "0",
                Location = new Point(36, 4),
                Size = new Size(80, 24),
            };

            var longLabel = new Label
            {
                Text = "Long",
                ForeColor = Color.Black,
                Location = new Point(124, 6),
                AutoSize = true,
            };

            var longInput = new TextBox
            {
                Text = "0",
                Location = new Point(160, 4),
                Size = new Size(80, 24),
            };

            var tellMeMoreButton = new Button
            {
                Text = "Tell Me More!",
                Location = new Point(248, 3),
                Size = new Size(100, 24),
                FlatStyle = FlatStyle.Flat,
                Enabled = false,
            };

            coordPanel.Controls.AddRange(new Control[] { latLabel, latInput, longLabel, longInput, tellMeMoreButton });

            var webView = new WebView2 { Dock = DockStyle.Fill };
            mapTab.Controls.Add(coordPanel);
            mapTab.Controls.Add(webView);
            InitializeMapAsync(webView, latInput, longInput, tellMeMoreButton);

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

        private async void InitializeMapAsync(WebView2 webView, TextBox latInput, TextBox longInput, Button tellMeMoreButton)
        {
            await webView.EnsureCoreWebView2Async();
            webView.CoreWebView2.Navigate("https://www.google.com/maps/@0,0,2z");

            var Navigate = () =>
            {
                if (double.TryParse(latInput.Text, out var lat) && double.TryParse(longInput.Text, out var lon))
                {
                    webView.CoreWebView2.Navigate($"https://www.google.com/maps/@{lat},{lon},12z");
                }
            };

            latInput.KeyDown += (_, e) => { if (e.KeyCode == Keys.Enter) Navigate(); };
            longInput.KeyDown += (_, e) => { if (e.KeyCode == Keys.Enter) Navigate(); };

            var UpdateButton = () =>
            {
                tellMeMoreButton.Enabled =
                    double.TryParse(latInput.Text, out var lat) &&
                    double.TryParse(longInput.Text, out var lon) &&
                    (lat != 0 || lon != 0);
            };
            latInput.KeyUp += (_, _) => UpdateButton();
            longInput.KeyUp += (_, _) => UpdateButton();

            tellMeMoreButton.Click += (_, _) =>
            {
                if (double.TryParse(latInput.Text, out var lat) && double.TryParse(longInput.Text, out var lon))
                {
                    _ = AIMapWindow.GetInstance().AskAsync($"Tell me more about the first city found within a 5 mile radius of coordinates {lat}, {lon}.");
                }
            };

            webView.CoreWebView2.SourceChanged += (_, _) =>
            {
                var src = webView.Source.ToString();
                var atIdx = src.IndexOf("@");
                if (atIdx < 0) return;

                var after = src.Substring(atIdx + 1);
                var stopIdx = after.IndexOfAny(new[] { '!', '&', '#', '?' });
                if (stopIdx >= 0) after = after.Substring(0, stopIdx);

                var parts = after.Split(',');
                if (parts.Length < 2) return;

                if (double.TryParse(parts[0], out var lat) && double.TryParse(parts[1], out var lon))
                {
                    latInput.Text = lat.ToString("F5");
                    longInput.Text = lon.ToString("F5");
                    UpdateButton();
                }
            };
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
