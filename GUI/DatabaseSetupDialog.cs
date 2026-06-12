using System.Drawing;
using System.Windows.Forms;
using WinformsVibes.Database;
using WinformsVibes.GUI;

namespace WinformsVibes;

public class DatabaseSetupDialog : TitleBarTooltipForm
{
    public DatabaseProvider? Provider { get; private set; }
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
        this.Size = new Size(460, 400);
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
            Text = "The application database could not be reached.\nPlease select a database type and enter connection details.",
            Font = new Font("Segoe UI", 10f),
            ForeColor = Color.LightGray,
            Location = new Point(20, 55),
            Size = new Size(420, 40),
            AutoSize = false,
        };

        // --- Provider ---
        var providerLabel = new Label
        {
            Text = "Provider:",
            Font = new Font("Segoe UI", 10f),
            ForeColor = Color.Gray,
            Location = new Point(20, 110),
            Size = new Size(80, 20),
            AutoSize = false,
        };

        var providerCombo = new ComboBox
        {
            Font = new Font("Segoe UI", 11f),
            ForeColor = Color.WhiteSmoke,
            BackColor = Color.FromArgb(45, 45, 60),
            Location = new Point(100, 107),
            Size = new Size(340, 30),
            DropDownStyle = ComboBoxStyle.DropDownList,
        };
        providerCombo.Items.Add("SQL Server");
        providerCombo.Items.Add("PostgreSQL");
        providerCombo.Items.Add("MySQL");
        providerCombo.SelectedIndex = 0;

        // --- Server ---
        var serverLabel = new Label
        {
            Text = "Server:",
            Font = new Font("Segoe UI", 10f),
            ForeColor = Color.Gray,
            Location = new Point(20, 150),
            Size = new Size(80, 20),
            AutoSize = false,
        };

        var serverInput = new TextBox
        {
            Text = "localhost",
            Font = new Font("Segoe UI", 11f),
            ForeColor = Color.WhiteSmoke,
            BackColor = Color.FromArgb(45, 45, 60),
            Location = new Point(100, 147),
            Size = new Size(340, 30),
        };
        serverInput.Enter += (_, _) => serverInput.SelectAll();

        // --- Database Name ---
        var nameLabel = new Label
        {
            Text = "Database:",
            Font = new Font("Segoe UI", 10f),
            ForeColor = Color.Gray,
            Location = new Point(20, 190),
            Size = new Size(80, 20),
            AutoSize = false,
        };

        var nameInput = new TextBox
        {
            Font = new Font("Segoe UI", 11f),
            ForeColor = Color.WhiteSmoke,
            BackColor = Color.FromArgb(45, 45, 60),
            Location = new Point(100, 187),
            Size = new Size(340, 30),
        };
        nameInput.Enter += (_, _) => nameInput.SelectAll();

        // --- Username ---
        var userLabel = new Label
        {
            Text = "Username:",
            Font = new Font("Segoe UI", 10f),
            ForeColor = Color.Gray,
            Location = new Point(20, 230),
            Size = new Size(80, 20),
            AutoSize = false,
        };

        var userInput = new TextBox
        {
            Text = "sa",
            Font = new Font("Segoe UI", 11f),
            ForeColor = Color.WhiteSmoke,
            BackColor = Color.FromArgb(45, 45, 60),
            Location = new Point(100, 227),
            Size = new Size(340, 30),
        };
        userInput.Enter += (_, _) => userInput.SelectAll();

        // Update defaults when provider changes
        providerCombo.SelectedIndexChanged += (_, _) =>
        {
            userInput.Text = providerCombo.SelectedIndex switch
            {
                1 => "postgres",
                2 => "root",
                _ => "sa"
            };
        };

        // --- Password ---
        var passLabel = new Label
        {
            Text = "Password:",
            Font = new Font("Segoe UI", 10f),
            ForeColor = Color.Gray,
            Location = new Point(20, 270),
            Size = new Size(80, 20),
            AutoSize = false,
        };

        var passInput = new TextBox
        {
            Font = new Font("Segoe UI", 11f),
            ForeColor = Color.WhiteSmoke,
            BackColor = Color.FromArgb(45, 45, 60),
            Location = new Point(100, 267),
            Size = new Size(340, 30),
            UseSystemPasswordChar = true,
        };
        passInput.Enter += (_, _) => passInput.SelectAll();

        // --- Buttons ---
        var createBtn = new Button
        {
            Text = "Create",
            Font = new Font("Segoe UI", 10f),
            Location = new Point(250, 320),
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
            Location = new Point(340, 320),
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
                Provider = providerCombo.SelectedIndex switch
                {
                    1 => DatabaseProvider.PostgreSQL,
                    2 => DatabaseProvider.MySql,
                    _ => DatabaseProvider.SqlServer
                };
                Server = serverInput.Text.Trim();
                DatabaseName = name;
                UserId = userInput.Text.Trim();
                Password = passInput.Text;
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

        panel.Controls.AddRange(new Control[] { title, description, providerLabel, providerCombo, serverLabel, serverInput, nameLabel, nameInput, userLabel, userInput, passLabel, passInput, createBtn, cancelBtn });
        this.Controls.Add(panel);

        this.Text = "Database Setup";

        nameInput.Focus();
    }
}
