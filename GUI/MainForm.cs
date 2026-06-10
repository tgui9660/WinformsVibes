using System;
using System.Drawing;
using System.Windows.Forms;
using ReaLTaiizor.Forms;
using ReaLTaiizor.Controls;

namespace WinformsVibes.GUI
{
    class MainForm : TitleBarTooltipMaterialForm
    {
        private CrownMenuStrip _menuStrip;
        private CrownStatusStrip _statusStrip;
        private ToolStripStatusLabel _statusLabel;

        public MainForm()
        {
            Text = "Winforms Vibes";
            Size = new Size(1280, 800);
            WindowState = FormWindowState.Maximized;
            Icon = CreateBearIcon();

            // Menu bar
            _menuStrip = new CrownMenuStrip
            {
                Dock = DockStyle.Top,
                Renderer = new DarkMenuRenderer(),
            };

            // File
            var fileMenu = new ToolStripMenuItem("&File");
            var newItem = new ToolStripMenuItem("&New");
            newItem.Click += (_, _) => MessageBox.Show("New not implemented.", Text, MessageBoxButtons.OK, MessageBoxIcon.Information);
            var openItem = new ToolStripMenuItem("&Open...");
            openItem.Click += (_, _) => MessageBox.Show("Open not implemented.", Text, MessageBoxButtons.OK, MessageBoxIcon.Information);
            var saveItem = new ToolStripMenuItem("&Save");
            saveItem.Click += (_, _) => MessageBox.Show("Save not implemented.", Text, MessageBoxButtons.OK, MessageBoxIcon.Information);
            var exitItem = new ToolStripMenuItem("E&xit");
            exitItem.Click += (_, _) => Close();
            fileMenu.DropDownItems.AddRange(new ToolStripItem[] { newItem, openItem, saveItem, new ToolStripSeparator(), exitItem });
            _menuStrip.Items.Add(fileMenu);

            // Edit
            var editMenu = new ToolStripMenuItem("&Edit");
            var copyItem = new ToolStripMenuItem("&Copy");
            copyItem.Click += (_, _) => MessageBox.Show("Copy not implemented.", Text, MessageBoxButtons.OK, MessageBoxIcon.Information);
            var pasteItem = new ToolStripMenuItem("&Paste");
            pasteItem.Click += (_, _) => MessageBox.Show("Paste not implemented.", Text, MessageBoxButtons.OK, MessageBoxIcon.Information);
            editMenu.DropDownItems.AddRange(new ToolStripItem[] { copyItem, pasteItem });
            _menuStrip.Items.Add(editMenu);

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
            var settingsMenu = new ToolStripMenuItem("&Settings");
            var prefsItem = new ToolStripMenuItem("&Preferences...");
            prefsItem.Click += (_, _) => MessageBox.Show("Preferences not implemented.", Text, MessageBoxButtons.OK, MessageBoxIcon.Information);
            settingsMenu.DropDownItems.Add(prefsItem);
            _menuStrip.Items.Add(settingsMenu);

            // Chat
            var chatMenu = new ToolStripMenuItem("&Chat");
            var aiChatItem = new ToolStripMenuItem("AI Chat");
            aiChatItem.Click += (_, _) => ChatWindow.GetInstance().Show();
            chatMenu.DropDownItems.Add(aiChatItem);
            _menuStrip.Items.Add(chatMenu);

            // Help
            var helpMenu = new ToolStripMenuItem("&Help");
            var contentsItem = new ToolStripMenuItem("&Contents");
            contentsItem.Click += (_, _) => new HelpWindow().Show();
            var aiHelpItem = new ToolStripMenuItem("AI &Help");
            aiHelpItem.Click += (_, _) => AIHelpWindow.GetInstance().Show();
            var helpAboutItem = new ToolStripMenuItem("&About");
            helpAboutItem.Click += (_, _) => ShowAboutDialog();
            helpMenu.DropDownItems.AddRange(new ToolStripItem[] { contentsItem, aiHelpItem, new ToolStripSeparator(), helpAboutItem });
            _menuStrip.Items.Add(helpMenu);

            // Status bar
            _statusStrip = new CrownStatusStrip
            {
                Dock = DockStyle.Bottom,
                Height = 40,
            };
            _statusLabel = new ToolStripStatusLabel { Text = "Ready" };
            var spacer = new ToolStripStatusLabel { Spring = true };
            var timeLabel = new ToolStripStatusLabel
            {
                Text = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"),
            };
            _statusStrip.Items.AddRange(new ToolStripItem[] { _statusLabel, spacer, timeLabel });

            // Tab content
            var tab = new TabControl { Dock = DockStyle.Fill };
            var mapTab = new System.Windows.Forms.TabPage("World Map") { Controls = { new WorldMapTab { Dock = DockStyle.Fill } } };
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

       class DarkMenuRenderer : ToolStripProfessionalRenderer
        {
            static readonly Color BackColor = Color.FromArgb(66, 66, 66);
            static readonly Color ForeColor = Color.White;
            static readonly Color HoverBack = Color.FromArgb(85, 85, 85);

            protected override void OnRenderMenuItemBackground(ToolStripItemRenderEventArgs e)
            {
                var item = e.Item as ToolStripMenuItem;
                using var brush = new SolidBrush((item?.Selected == true) ? HoverBack : BackColor);
                e.Graphics.FillRectangle(brush, e.Item.ContentRectangle);
            }

            protected override void OnRenderItemText(ToolStripItemTextRenderEventArgs e)
            {
                e.Item.ForeColor = ForeColor;
                base.OnRenderItemText(e);
            }

            protected override void OnRenderImageMargin(ToolStripRenderEventArgs e)
            {
                using var brush = new SolidBrush(BackColor);
                e.Graphics.FillRectangle(brush, e.AffectedBounds);
            }

            protected override void OnRenderToolStripBackground(ToolStripRenderEventArgs e)
            {
                e.ToolStrip.BackColor = BackColor;
                base.OnRenderToolStripBackground(e);
            }

            protected override void OnRenderSeparator(ToolStripSeparatorRenderEventArgs e)
            {
                e.Item.ForeColor = Color.FromArgb(150, 150, 150);
                base.OnRenderSeparator(e);
            }
        }
    }
}
