using System.Windows.Forms;

namespace WinformsVibes;

internal static class Program
{
    [STAThread]
    static void Main()
    {
        Application.EnableVisualStyles();
        Application.SetCompatibleTextRenderingDefault(false);

        // Show splash screen — waits for user to click it to close
        var info = DbConfig.GetApplicationInfo() ?? new();
        var splash = new SplashScreen(info);
        Application.Run(splash);

        // Start main form after splash is closed
        Application.Run(new MainForm());
    }
}
