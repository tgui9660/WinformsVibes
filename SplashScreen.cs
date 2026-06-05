using System.Drawing;
using System.Windows.Forms;
using WinformsVibes.Models;

namespace WinformsVibes;

public class SplashScreen : Form
{
    public SplashScreen(ApplicationInfo info)
    {
        this.StartPosition = FormStartPosition.CenterScreen;
        this.FormBorderStyle = FormBorderStyle.FixedDialog;
        this.MaximizeBox = false;
        this.MinimizeBox = false;
        this.BackColor = Color.FromArgb(30, 30, 46);
        this.Size = new Size(420, 350);
        this.DoubleBuffered = true;

        var panel = new Panel
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

        var separator = new Label
        {
            Text = new string('─', 58),
            Font = new Font("Segoe UI", 8f),
            ForeColor = Color.FromArgb(70, 70, 90),
            Location = new Point(20, 180),
            Size = new Size(380, 15),
            AutoSize = false,
        };

        var depsLabel = new Label
        {
            Text = "Dependencies:",
            Font = new Font("Segoe UI", 9f, FontStyle.Bold),
            ForeColor = Color.Gray,
            Location = new Point(20, 205),
            Size = new Size(380, 20),
            AutoSize = false,
        };

        var deps = new TextBox
        {
            Text = info.Dependencies ?? string.Empty,
            Font = new Font("Consolas", 9f),
            ForeColor = Color.FromArgb(140, 140, 160),
            BackColor = Color.FromArgb(30, 30, 46),
            Location = new Point(20, 225),
            Size = new Size(380, 50),
            Multiline = true,
            ReadOnly = true,
            BorderStyle = BorderStyle.None,
        };

        var loading = new Label
        {
            Text = "Loading...",
            Font = new Font("Segoe UI", 10f, FontStyle.Italic),
            ForeColor = Color.Gray,
            Location = new Point(20, this.Height - 40),
            Size = new Size(380, 20),
            AutoSize = false,
        };

        panel.Controls.AddRange(new Control[] { title, version, author, framework, database, separator, depsLabel, deps, loading });
        this.Controls.Add(panel);

        this.Text = info.ApplicationName;
    }
}
