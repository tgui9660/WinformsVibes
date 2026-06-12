using System.Reflection;
using System.Text.Json;

namespace WinformsVibes.Database;

public enum DatabaseProvider
{
    SqlServer,
    PostgreSQL,
    MySql
}

public class DbSettings
{
    public DatabaseProvider Provider { get; set; }
    public string Server { get; set; } = string.Empty;
    public string DatabaseName { get; set; } = string.Empty;
    public string UserId { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;
}

public static class DbSettingsManager
{
    private static readonly string _configName;
    private static readonly string _configDir;
    private static readonly bool _isRelease;

    static DbSettingsManager()
    {
        var assembly = System.Reflection.Assembly.GetExecutingAssembly();
        var configAttr = assembly.GetCustomAttribute<System.Reflection.AssemblyConfigurationAttribute>();
        var buildConfig = configAttr?.Configuration ?? "Debug";
        _configName = $"dbconfig.{buildConfig.ToLower()}.json";
        _isRelease = string.Equals(buildConfig, "Release", StringComparison.OrdinalIgnoreCase);

        // Release builds use a separate AppData directory that dev machines never write to
        var baseDir = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);
        _configDir = _isRelease
            ? System.IO.Path.Combine(baseDir, "WinformsVibes-Release")
            : System.IO.Path.Combine(baseDir, "WinformsVibes");
    }

    private static string ConfigPath => System.IO.Path.Combine(_configDir, _configName);

    public static DbSettings Load()
    {
        if (_isRelease)
        {
            // Release: never migrate from dev directories — only read the release-specific path
            var releaseDir = System.IO.Path.GetDirectoryName(ConfigPath)!;
            if (!System.IO.Directory.Exists(releaseDir))
                System.IO.Directory.CreateDirectory(releaseDir);

            if (System.IO.File.Exists(ConfigPath))
            {
                var json = System.IO.File.ReadAllText(ConfigPath);
                return JsonSerializer.Deserialize<DbSettings>(json) ?? new DbSettings();
            }
            return new DbSettings();
        }

        // Debug builds: migrate from legacy location (app directory) to user AppData if needed
        var legacyPath = System.IO.Path.Combine(AppDomain.CurrentDomain.BaseDirectory, _configName);
        var dir = System.IO.Path.GetDirectoryName(ConfigPath)!;
        if (!System.IO.Directory.Exists(dir))
            System.IO.Directory.CreateDirectory(dir);

        if (!System.IO.File.Exists(ConfigPath) && System.IO.File.Exists(legacyPath))
        {
            System.IO.File.Copy(legacyPath, ConfigPath, overwrite: true);
        }

        // Migrate from old unscoped config name for Debug builds
        var oldName = System.IO.Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
            "WinformsVibes", "dbconfig.json");
        if (!System.IO.File.Exists(ConfigPath) && System.IO.File.Exists(oldName))
        {
            System.IO.File.Copy(oldName, ConfigPath, overwrite: true);
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

    public static void Clear()
    {
        if (System.IO.File.Exists(ConfigPath))
            System.IO.File.Delete(ConfigPath);
    }
}
