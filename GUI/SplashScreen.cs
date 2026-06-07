using System.Drawing;
using System.Windows.Forms;
using WinformsVibes.Models;

namespace WinformsVibes.GUI;

public class SplashScreen : Form
{
    public SplashScreen(ApplicationInfo info)
    {
        this.StartPosition = FormStartPosition.CenterScreen;
        this.FormBorderStyle = FormBorderStyle.FixedDialog;
        this.MaximizeBox = false;
        this.MinimizeBox = false;
        this.BackColor = Color.FromArgb(30, 30, 46);
        this.Size = new Size(420, 390);
        this.DoubleBuffered = true;
        this.Icon = CreateBearIcon();

        var mainPanel = new Panel
        {
            Dock = DockStyle.Fill,
            BackColor = Color.FromArgb(30, 30, 46),
        };

        var title = new Label
        {
            Text = info.ApplicationName,
            Font = new Font("Segoe UI", 22f, FontStyle.Bold),
            ForeColor = Color.WhiteSmoke,
            Location = new Point(20, 30),
            Size = new Size(380, 40),
            AutoSize = false,
        };

        var version = new Label
        {
            Text = $"Version {info.Version}",
            Font = new Font("Segoe UI", 12f, FontStyle.Regular),
            ForeColor = Color.Gray,
            Location = new Point(20, 68),
            Size = new Size(380, 20),
            AutoSize = false,
        };

        var author = new Label
        {
            Text = $"Author: {info.Author}",
            Font = new Font("Segoe UI", 10f),
            ForeColor = Color.LightGray,
            Location = new Point(20, 105),
            Size = new Size(380, 20),
            AutoSize = false,
        };

        var framework = new Label
        {
            Text = $"Framework: {info.Framework}",
            Font = new Font("Segoe UI", 10f),
            ForeColor = Color.LightGray,
            Location = new Point(20, 130),
            Size = new Size(380, 20),
            AutoSize = false,
        };

        var database = new Label
        {
            Text = $"Database: {info.DatabaseName}",
            Font = new Font("Segoe UI", 10f),
            ForeColor = Color.LightGray,
            Location = new Point(20, 155),
            Size = new Size(380, 20),
            AutoSize = false,
        };

        var server = new Label
        {
            Text = $"Server: {info.Server}",
            Font = new Font("Segoe UI", 10f),
            ForeColor = Color.LightGray,
            Location = new Point(20, 180),
            Size = new Size(380, 20),
            AutoSize = false,
        };

        var username = new Label
        {
            Text = $"User: {info.UserId}",
            Font = new Font("Segoe UI", 10f),
            ForeColor = Color.LightGray,
            Location = new Point(20, 205),
            Size = new Size(380, 20),
            AutoSize = false,
        };

        var separator = new Label
        {
            Text = new string('─', 58),
            Font = new Font("Segoe UI", 8f),
            ForeColor = Color.FromArgb(70, 70, 90),
            Location = new Point(20, 230),
            Size = new Size(380, 15),
            AutoSize = false,
        };

        mainPanel.Controls.AddRange(new Control[] { title, version, author, framework, database, server, username, separator });

        // --- Bottom toolbar ---
        var toolbar = new Panel
        {
            Dock = DockStyle.Bottom,
            Height = 50,
            BackColor = Color.FromArgb(35, 35, 50),
        };

        var continueBtn = new Button
        {
            Text = "Continue",
            Font = new Font("Segoe UI", 10f),
            Location = new Point(300, 10),
            Size = new Size(80, 30),
            BackColor = Color.FromArgb(70, 70, 90),
            ForeColor = Color.WhiteSmoke,
            FlatStyle = FlatStyle.Flat,
        };
        continueBtn.FlatAppearance.BorderSize = 0;
        continueBtn.Click += (_, _) => this.Close();

        toolbar.Controls.Add(continueBtn);
        this.Controls.Add(mainPanel);
        this.Controls.Add(toolbar);

        this.Text = info.ApplicationName;
    }

    static Icon CreateBearIcon()
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
