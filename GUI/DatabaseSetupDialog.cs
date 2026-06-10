using System.Drawing;
using System.Windows.Forms;

namespace WinformsVibes.GUI;

public class DatabaseSetupDialog : TitleBarTooltipForm
{
    public string? Server { get; private set; }
    public string? DatabaseName { get; private set; }
    public string? UserId { get; private set; }
    public string? Password { get; private set; }

    public DatabaseSetupDialog()
    {
        this.StartPosition = FormStartPosition.CenterScreen;
        this.FormBorderStyle = FormBorderStyle.FixedDialog;
        this.MaximizeBox = false;
        this.MinimizeBox = false;
        this.Size = new Size(460, 360);
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
            Location = new Point(20, 15),
            Size = new Size(420, 35),
            AutoSize = false,
        };

        var description = new Label
        {
            Text = "The application database could not be reached.\nPlease enter connection details to create and populate a database.",
            Font = new Font("Segoe UI", 10f),
            ForeColor = Color.LightGray,
            Location = new Point(20, 55),
            Size = new Size(420, 40),
            AutoSize = false,
        };

        // --- Server ---
        var serverLabel = new Label
        {
            Text = "Server:",
            Font = new Font("Segoe UI", 10f),
            ForeColor = Color.Gray,
            Location = new Point(20, 110),
            Size = new Size(80, 20),
            AutoSize = false,
        };

        var serverInput = new TextBox
        {
            Text = "localhost",
            Font = new Font("Segoe UI", 11f),
            ForeColor = Color.WhiteSmoke,
            BackColor = Color.FromArgb(45, 45, 60),
            Location = new Point(100, 107),
            Size = new Size(340, 30),
        };
        serverInput.Enter += (_, _) => serverInput.SelectAll();

        // --- Database Name ---
        var nameLabel = new Label
        {
            Text = "Database:",
            Font = new Font("Segoe UI", 10f),
            ForeColor = Color.Gray,
            Location = new Point(20, 150),
            Size = new Size(80, 20),
            AutoSize = false,
        };

        var nameInput = new TextBox
        {
            Font = new Font("Segoe UI", 11f),
            ForeColor = Color.WhiteSmoke,
            BackColor = Color.FromArgb(45, 45, 60),
            Location = new Point(100, 147),
            Size = new Size(340, 30),
        };
        nameInput.Enter += (_, _) => nameInput.SelectAll();

        // --- Username ---
        var userLabel = new Label
        {
            Text = "Username:",
            Font = new Font("Segoe UI", 10f),
            ForeColor = Color.Gray,
            Location = new Point(20, 190),
            Size = new Size(80, 20),
            AutoSize = false,
        };

        var userInput = new TextBox
        {
            Text = "sa",
            Font = new Font("Segoe UI", 11f),
            ForeColor = Color.WhiteSmoke,
            BackColor = Color.FromArgb(45, 45, 60),
            Location = new Point(100, 187),
            Size = new Size(340, 30),
        };
        userInput.Enter += (_, _) => userInput.SelectAll();

        // --- Password ---
        var passLabel = new Label
        {
            Text = "Password:",
            Font = new Font("Segoe UI", 10f),
            ForeColor = Color.Gray,
            Location = new Point(20, 230),
            Size = new Size(80, 20),
            AutoSize = false,
        };

        var passInput = new TextBox
        {
            Font = new Font("Segoe UI", 11f),
            ForeColor = Color.WhiteSmoke,
            BackColor = Color.FromArgb(45, 45, 60),
            Location = new Point(100, 227),
            Size = new Size(340, 30),
            UseSystemPasswordChar = true,
        };
        passInput.Enter += (_, _) => passInput.SelectAll();

        // --- Buttons ---
        var createBtn = new Button
        {
            Text = "Create",
            Font = new Font("Segoe UI", 10f),
            Location = new Point(250, 280),
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
            Location = new Point(340, 280),
            Size = new Size(80, 35),
            BackColor = Color.FromArgb(40, 40, 55),
            ForeColor = Color.Gray,
            FlatStyle = FlatStyle.Flat,
        };
        cancelBtn.FlatAppearance.BorderSize = 0;

        createBtn.Click += (_, _) =>
        {
            var server = serverInput.Text.Trim();
            var name = nameInput.Text.Trim();
            var user = userInput.Text.Trim();
            var pass = passInput.Text;

            if (!string.IsNullOrEmpty(name))
            {
                Server = server;
                DatabaseName = name;
                UserId = user;
                Password = pass;
                this.DialogResult = DialogResult.OK;
                this.Close();
            }
        };

        cancelBtn.Click += (_, _) =>
        {
            this.DialogResult = DialogResult.Cancel;
            this.Close();
        };

        var inputs = new[] { serverInput, nameInput, userInput, passInput };
        foreach (var input in inputs)
        {
            input.KeyDown += (_, e) =>
            {
                if (e.KeyCode == Keys.Enter)
                    createBtn.PerformClick();
                if (e.KeyCode == Keys.Escape)
                    cancelBtn.PerformClick();
            };
        }

        panel.Controls.AddRange(new Control[] { title, description, serverLabel, serverInput, nameLabel, nameInput, userLabel, userInput, passLabel, passInput, createBtn, cancelBtn });
        this.Controls.Add(panel);

        this.Text = "Database Setup";

        nameInput.Focus();
    }
}
