using System.Text.Json;

namespace WinformsVibes.Database;

public class DbSettings
{
    public string Server { get; set; } = "localhost";
    public string DatabaseName { get; set; } = "winformsvibes";
    public string UserId { get; set; } = "sa";
    public string Password { get; set; } = "password";
}

public static class DbSettingsManager
{
    private static string ConfigPath =>
        System.IO.Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
            "WinformsVibes", "dbconfig.json");

    public static DbSettings Load()
    {
        // Migrate from old location (app directory) to user AppData if needed
        var oldPath = System.IO.Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "dbconfig.json");
        if (!System.IO.File.Exists(ConfigPath) && System.IO.File.Exists(oldPath))
        {
            var dir = System.IO.Path.GetDirectoryName(ConfigPath)!;
            if (!System.IO.Directory.Exists(dir))
                System.IO.Directory.CreateDirectory(dir);
            System.IO.File.Copy(oldPath, ConfigPath, overwrite: true);
        }

        if (System.IO.File.Exists(ConfigPath))
        {
            var json = System.IO.File.ReadAllText(ConfigPath);
            return JsonSerializer.Deserialize<DbSettings>(json) ?? new DbSettings();
        }
        return new DbSettings();
    }

    public static void Save(DbSettings settings)
    {
        var dir = System.IO.Path.GetDirectoryName(ConfigPath)!;
        if (!System.IO.Directory.Exists(dir))
            System.IO.Directory.CreateDirectory(dir);
        var json = JsonSerializer.Serialize(settings, new JsonSerializerOptions { WriteIndented = true });
        System.IO.File.WriteAllText(ConfigPath, json);
    }
}
