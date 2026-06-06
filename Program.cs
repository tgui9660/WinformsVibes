using System.Windows.Forms;
using WinformsVibes.Database;
using WinformsVibes.GUI;

namespace WinformsVibes;

internal static class Program
{
    [STAThread]
    static void Main()
    {
        Application.EnableVisualStyles();
        Application.SetCompatibleTextRenderingDefault(false);

        // Try connecting to the database; if it fails, offer to set one up
        if (!DbConfig.CheckConnection())
        {
            var setup = new DatabaseSetupDialog();
            if (setup.ShowDialog() != DialogResult.OK || string.IsNullOrEmpty(setup.DatabaseName))
            {
                MessageBox.Show("A database connection is required to run this application.",
                    "Database Required",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Warning);
                return;
            }

            if (!DbConfig.CreateAndSeedDatabase(setup.Server ?? "localhost", setup.DatabaseName, setup.UserId ?? "sa", setup.Password ?? "", out var err))
            {
                MessageBox.Show($"Failed to create the database: {err}",
                    "Database Error",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error);
                return;
            }
        }

        // Sync HelpInfo table with HelpTopics.xml if counts differ
        DbConfig.SyncHelpTopics();

        // Show splash screen — waits for user to click it to close
        var info = DbConfig.GetApplicationInfo() ?? new();
        info.DatabaseName = DbConfig.CurrentDatabaseName;
        info.Server = DbConfig.CurrentServer;
        info.UserId = DbConfig.CurrentUserId;
        var splash = new SplashScreen(info);
        Application.Run(splash);

        // Start main form after splash is closed
        Application.Run(new MainForm());
    }
}
