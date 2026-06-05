using System.Data.SqlClient;
using System.Xml.Linq;
using FluentNHibernate.Cfg;
using FluentNHibernate.Cfg.Db;
using NHibernate;
using WinformsVibes.Models;

namespace WinformsVibes;

public static class DbConfig
{
    private static DbSettings _settings = DbSettingsManager.Load();
    private static ISessionFactory? _sessionFactory;

    public static string CurrentDatabaseName => _settings.DatabaseName;

    private static string ConnectionString =>
        $"Server={_settings.Server};Database={_settings.DatabaseName};User Id={_settings.UserId};Password={_settings.Password};";

    private static string MasterConnectionString =>
        $"Server={_settings.Server};User Id={_settings.UserId};Password={_settings.Password};";

    public static ISessionFactory SessionFactory =>
        _sessionFactory ??= Fluently.Configure()
            .Database(MsSqlConfiguration.MsSql2012
                .ConnectionString(ConnectionString))
            .Mappings(m => m.FluentMappings.AddFromAssemblyOf<ApplicationInfo>())
            .ExposeConfiguration(cfg =>
            {
                cfg.SetProperty("use_proxy_validator", "false");
                cfg.SetProperty("default_lazy", "false");
            })
            .BuildSessionFactory();

    public static bool CheckConnection()
    {
        try
        {
            using var session = SessionFactory.OpenSession();
            using var cmd = session.Connection.CreateCommand();
            cmd.CommandText = "SELECT 1";
            cmd.ExecuteNonQuery();
            return true;
        }
        catch
        {
            return false;
        }
    }

    public static ApplicationInfo? GetApplicationInfo()
    {
        using var session = SessionFactory.OpenSession();
        return session.CreateCriteria<ApplicationInfo>().UniqueResult<ApplicationInfo?>();
    }

    public static bool CreateAndSeedDatabase(string databaseName, out string errorMessage)
    {
        try
        {
            // Create the database
            using (var conn = new SqlConnection(MasterConnectionString))
            {
                conn.Open();
                using var cmd = conn.CreateCommand();
                cmd.CommandText = $"IF NOT EXISTS (SELECT 1 FROM sys.databases WHERE name = N'{databaseName}') CREATE DATABASE [{databaseName}]";
                cmd.ExecuteNonQuery();
            }

            // Update settings with the new database name
            _settings.DatabaseName = databaseName;
            DbSettingsManager.Save(_settings);

            // Create the table and seed data
            using (var conn = new SqlConnection(ConnectionString))
            {
                conn.Open();
                using (var cmd = conn.CreateCommand())
                {
                    cmd.CommandText = @"
                    IF OBJECT_ID('ApplicationInfo', 'U') IS NULL
                    BEGIN
                        CREATE TABLE ApplicationInfo (
                            Id              INT IDENTITY(1,1) PRIMARY KEY,
                            ApplicationName NVARCHAR(100) NOT NULL,
                            Author          NVARCHAR(100) NOT NULL,
                            Version         NVARCHAR(50)  NOT NULL,
                            Description     NVARCHAR(500),
                            Framework       NVARCHAR(50),
                            Dependencies    NVARCHAR(MAX),
                            CreatedAt       DATETIME2     DEFAULT SYSUTCDATETIME() NOT NULL,
                            UpdatedAt       DATETIME2     DEFAULT SYSUTCDATETIME() NOT NULL
                        );
                    END";
                    cmd.ExecuteNonQuery();
                }

                using (var cmd = conn.CreateCommand())
                {
                    cmd.CommandText = @"
                    IF NOT EXISTS (SELECT 1 FROM ApplicationInfo)
                        INSERT INTO ApplicationInfo (ApplicationName, Author, Version, Description, Framework, Dependencies)
                        VALUES (N'Winforms Vibes', N'Eric', N'1.0', N'A simple Windows Forms demo application.', N'net10.0-windows', N'Microsoft.Web.WebView2 1.0.3967.48, ReaLTaiizor 3.8.1.8');";
                    cmd.ExecuteNonQuery();
                }

                // Create the HelpInfo table
                using (var cmd = conn.CreateCommand())
                {
                    cmd.CommandText = @"
                    IF OBJECT_ID('HelpInfo', 'U') IS NULL
                    BEGIN
                        CREATE TABLE HelpInfo (
                            Id      INT IDENTITY(1,1) PRIMARY KEY,
                            Category NVARCHAR(100) NOT NULL,
                            Topic    NVARCHAR(200) NOT NULL,
                            Content  NVARCHAR(MAX)  NOT NULL
                        );
                    END";
                    cmd.ExecuteNonQuery();
                }

                // Seed HelpInfo rows from XML
                using (var countCmd = conn.CreateCommand())
                {
                    countCmd.CommandText = "SELECT COUNT(1) FROM HelpInfo";
                    var count = (int)countCmd.ExecuteScalar();

                    if (count == 0)
                    {
                        var xmlPath = System.IO.Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "HelpTopics.xml");
                        var doc = XDocument.Load(xmlPath);
                        foreach (var topic in doc.Descendants("Topic"))
                        {
                            using var insertCmd = conn.CreateCommand();
                            insertCmd.CommandText = "INSERT INTO HelpInfo (Category, Topic, Content) VALUES (@cat, @topic, @content)";
                            insertCmd.Parameters.AddWithValue("@cat", topic.Attribute("Category")!.Value);
                            insertCmd.Parameters.AddWithValue("@topic", topic.Attribute("Name")!.Value);
                            insertCmd.Parameters.AddWithValue("@content", topic.Value.Trim());
                            insertCmd.ExecuteNonQuery();
                        }
                    }
                }
            }

            // Reset session factory so it picks up the new connection string
            _sessionFactory = null;

            errorMessage = string.Empty;
            return true;
        }
        catch (Exception ex)
        {
            errorMessage = ex.Message;
            return false;
        }
    }
}
