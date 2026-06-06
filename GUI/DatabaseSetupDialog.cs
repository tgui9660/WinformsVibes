using System;
using System.Drawing;
using System.Windows.Forms;

namespace WinformsVibes.GUI;

public class DatabaseSetupDialog : Form
{
    public string? DatabaseName { get; private set; }

    public DatabaseSetupDialog()
    {
        this.StartPosition = FormStartPosition.CenterScreen;
        this.FormBorderStyle = FormBorderStyle.FixedDialog;
        this.MaximizeBox = false;
        this.MinimizeBox = false;
        this.Size = new Size(420, 220);
        this.BackColor = Color.FromArgb(30, 30, 46);

        var panel = new Panel
        {
            Dock = DockStyle.Fill,
            BackColor = Color.FromArgb(30, 30, 46),
        };

        var title = new Label
        {
            Text = "No Database Found",
            Font = new Font("Segoe UI", 16f, FontStyle.Bold),
            ForeColor = Color.WhiteSmoke,
            Location = new Point(20, 20),
            Size = new Size(380, 35),
            AutoSize = false,
        };

        var description = new Label
        {
            Text = "The application database could not be reached.\nPlease enter a database name to create and populate.",
            Font = new Font("Segoe UI", 10f),
            ForeColor = Color.LightGray,
            Location = new Point(20, 60),
            Size = new Size(380, 45),
            AutoSize = false,
        };

        var nameLabel = new Label
        {
            Text = "Database Name:",
            Font = new Font("Segoe UI", 10f),
            ForeColor = Color.Gray,
            Location = new Point(20, 115),
            Size = new Size(380, 20),
            AutoSize = false,
        };

        var nameInput = new TextBox
        {
            Font = new Font("Segoe UI", 11f),
            ForeColor = Color.WhiteSmoke,
            BackColor = Color.FromArgb(45, 45, 60),
            Location = new Point(20, 138),
            Size = new Size(180, 30),
        };
        nameInput.Enter += (_, _) => nameInput.SelectAll();

        var createBtn = new Button
        {
            Text = "Create",
            Font = new Font("Segoe UI", 10f),
            Location = new Point(210, 135),
            Size = new Size(80, 35),
            BackColor = Color.FromArgb(40, 40, 55),
            ForeColor = Color.WhiteSmoke,
            FlatStyle = FlatStyle.Flat,
        };
        createBtn.FlatAppearance.BorderSize = 0;

        var cancelBtn = new Button
        {
            Text = "Cancel",
            Font = new Font("Segoe UI", 10f),
            Location = new Point(300, 135),
            Size = new Size(80, 35),
            BackColor = Color.FromArgb(40, 40, 55),
            ForeColor = Color.Gray,
            FlatStyle = FlatStyle.Flat,
        };
        cancelBtn.FlatAppearance.BorderSize = 0;

        createBtn.Click += (_, _) =>
        {
            var name = nameInput.Text.Trim();
            if (!string.IsNullOrEmpty(name))
            {
                DatabaseName = name;
                this.DialogResult = DialogResult.OK;
                this.Close();
            }
        };

        cancelBtn.Click += (_, _) =>
        {
            this.DialogResult = DialogResult.Cancel;
            this.Close();
        };

        nameInput.KeyDown += (_, e) =>
        {
            if (e.KeyCode == Keys.Enter)
                createBtn.PerformClick();
            if (e.KeyCode == Keys.Escape)
                cancelBtn.PerformClick();
        };

        panel.Controls.AddRange(new Control[] { title, description, nameLabel, nameInput, createBtn, cancelBtn });
        this.Controls.Add(panel);

        this.Text = "Database Setup";
    }
}
