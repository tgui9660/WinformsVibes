using System.Text.Json;

namespace WinformsVibes;

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
        System.IO.Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "dbconfig.json");

    public static DbSettings Load()
    {
        if (System.IO.File.Exists(ConfigPath))
        {
            var json = System.IO.File.ReadAllText(ConfigPath);
            return JsonSerializer.Deserialize<DbSettings>(json) ?? new DbSettings();
        }
        return new DbSettings();
    }

    public static void Save(DbSettings settings)
    {
        var json = JsonSerializer.Serialize(settings, new JsonSerializerOptions { WriteIndented = true });
        System.IO.File.WriteAllText(ConfigPath, json);
    }
}
