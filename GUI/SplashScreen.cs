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
}
