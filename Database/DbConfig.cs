using System.Data.SqlClient;
using System.Xml.Linq;
using FluentNHibernate.Cfg;
using FluentNHibernate.Cfg.Db;
using MySqlConnector;
using Npgsql;
using NHibernate;
using NHibernate.Tool.hbm2ddl;
using WinformsVibes.GUI;
using WinformsVibes.Maps;
using WinformsVibes.Models;

namespace WinformsVibes.Database;

public static class DbConfig
{
    private static DbSettings _settings = DbSettingsManager.Load();
    private static ISessionFactory? _sessionFactory;

    public static string CurrentDatabaseName => _settings.DatabaseName;
    public static string CurrentServer => _settings.Server;
    public static string CurrentUserId => _settings.UserId;
    public static DatabaseProvider Provider => _settings.Provider;

    // SQL type for long-string columns, used by the Fluent maps
    public static string LongStringSqlType => _settings.Provider switch
    {
        DatabaseProvider.SqlServer => "nvarchar(max)",
        DatabaseProvider.MySql => "LONGTEXT",
        _ => "text"
    };

    private static string ConnectionString => _settings.Provider switch
    {
        DatabaseProvider.SqlServer => $"Server={_settings.Server};Database={_settings.DatabaseName};User Id={_settings.UserId};Password={_settings.Password};",
        DatabaseProvider.PostgreSQL => new NpgsqlConnectionStringBuilder
        {
            Host = _settings.Server,
            Database = _settings.DatabaseName,
            Username = _settings.UserId,
            Password = _settings.Password,
        }.ToString(),
        DatabaseProvider.MySql => new MySqlConnectionStringBuilder
        {
            Server = _settings.Server,
            Database = _settings.DatabaseName,
            UserID = _settings.UserId,
            Password = _settings.Password,
        }.ToString(),
        _ => throw new NotSupportedException($"Provider {_settings.Provider} is not supported."),
    };

    private static FluentConfiguration BuildFluentConfig()
    {
        var fluentCfg = Fluently.Configure();

        if (_settings.Provider == DatabaseProvider.SqlServer)
        {
            fluentCfg.Database(MsSqlConfiguration.MsSql2012.ConnectionString(ConnectionString));
        }
        else if (_settings.Provider == DatabaseProvider.MySql)
        {
            fluentCfg.Database(MySQLConfiguration.Standard
                .ConnectionString(ConnectionString)
                .Dialect<NHibernate.Dialect.MySQLDialect>()
                .Driver<NHibernate.Driver.MySqlDataDriver>());
        }
        else
        {
            fluentCfg.Database(PostgreSQLConfiguration.PostgreSQL82
                .ConnectionString(ConnectionString));
        }

        fluentCfg.Mappings(m =>
        {
            m.FluentMappings.Add<ApplicationInfoMap>();
            m.FluentMappings.Add<HelpInfoMap>();
        });

        fluentCfg.ExposeConfiguration(cfg =>
        {
            cfg.SetProperty("use_proxy_validator", "false");
            cfg.SetProperty("default_lazy", "false");
        });

        return fluentCfg;
    }

    static void SetExtraProps(NHibernate.Cfg.Configuration cfg)
    {
        cfg.SetProperty("use_proxy_validator", "false");
        cfg.SetProperty("default_lazy", "false");
    }

    public static ISessionFactory SessionFactory =>
        _sessionFactory ??= BuildFluentConfig().BuildSessionFactory();

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

    public static List<HelpTopic> GetHelpTopics()
    {
        using var session = SessionFactory.OpenSession();
        return session.CreateCriteria<Models.HelpInfo>()
            .List<Models.HelpInfo>()
            .Select(h => new HelpTopic(h.Category, h.Topic, h.Content))
            .ToList();
    }

    public static void SyncHelpTopics()
    {
        var xmlPath = System.IO.Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "HelpTopics.xml");
        if (!System.IO.File.Exists(xmlPath)) return;

        var doc = XDocument.Load(xmlPath);

        if (_settings.Provider == DatabaseProvider.SqlServer)
        {
            using var conn = new SqlConnection(ConnectionString);
            conn.Open();
            using (var cmd = conn.CreateCommand())
            {
                cmd.CommandText = "TRUNCATE TABLE HelpInfo";
                cmd.ExecuteNonQuery();
            }
            foreach (var topic in doc.Descendants("Topic"))
            {
                using var cmd = conn.CreateCommand();
                cmd.CommandText = "INSERT INTO HelpInfo (Category, Topic, Content) VALUES (@cat, @topic, @content)";
                cmd.Parameters.AddWithValue("@cat", topic.Attribute("Category")!.Value);
                cmd.Parameters.AddWithValue("@topic", topic.Attribute("Name")!.Value);
                cmd.Parameters.AddWithValue("@content", topic.Value.Trim());
                cmd.ExecuteNonQuery();
            }
        }
        else if (_settings.Provider == DatabaseProvider.MySql)
        {
            using var conn = new MySqlConnection(ConnectionString);
            conn.Open();
            using (var cmd = conn.CreateCommand())
            {
                cmd.CommandText = "TRUNCATE TABLE `HelpInfo`";
                cmd.ExecuteNonQuery();
            }
            foreach (var topic in doc.Descendants("Topic"))
            {
                using var cmd = conn.CreateCommand();
                cmd.CommandText = "INSERT INTO `HelpInfo` (`Category`, `Topic`, `Content`) VALUES (@cat, @topic, @content)";
                cmd.Parameters.AddWithValue("@cat", topic.Attribute("Category")!.Value);
                cmd.Parameters.AddWithValue("@topic", topic.Attribute("Name")!.Value);
                cmd.Parameters.AddWithValue("@content", topic.Value.Trim());
                cmd.ExecuteNonQuery();
            }
        }
        else
        {
            using var conn = new NpgsqlConnection(ConnectionString);
            conn.Open();
            using (var cmd = conn.CreateCommand())
            {
                cmd.CommandText = "TRUNCATE TABLE \"HelpInfo\" RESTART IDENTITY";
                cmd.ExecuteNonQuery();
            }
            foreach (var topic in doc.Descendants("Topic"))
            {
                using var cmd = conn.CreateCommand();
                cmd.CommandText = "INSERT INTO \"HelpInfo\" (\"Category\", \"Topic\", \"Content\") VALUES (@cat, @topic, @content)";
                cmd.Parameters.Add(new NpgsqlParameter("@cat", topic.Attribute("Category")!.Value));
                cmd.Parameters.Add(new NpgsqlParameter("@topic", topic.Attribute("Name")!.Value));
                cmd.Parameters.Add(new NpgsqlParameter("@content", topic.Value.Trim()));
                cmd.ExecuteNonQuery();
            }
        }
    }

    public static bool CreateAndSeedDatabase(DatabaseProvider provider, string server, string databaseName, string userId, string password, out string errorMessage)
    {
        try
        {
            _settings.Provider = provider;
            _settings.Server = server;
            _settings.DatabaseName = databaseName;
            _settings.UserId = userId;
            _settings.Password = password;

            if (provider == DatabaseProvider.SqlServer)
            {
                CreateSqlServerDatabase(server, databaseName, userId, password);
            }
            else if (provider == DatabaseProvider.MySql)
            {
                CreateMySqlDatabase(server, databaseName, userId, password);
            }
            else
            {
                CreatePostgreSQLDatabase(server, databaseName, userId, password);
            }

            // Build Fluent NHibernate config and create schema
            var fluentCfg = BuildFluentConfig();
            var nhConfig = fluentCfg.BuildConfiguration();
            using var schemaConn = CreateDbConnection();
            schemaConn.Open();
            // Drop then recreate to ensure fresh schema with correct column types
            try
            {
                new SchemaExport(nhConfig)
                    .Drop(false, true, schemaConn);
            }
            catch { /* tables may not exist on fresh db */ }
            new SchemaExport(nhConfig)
                .Create(true, true, schemaConn);

            // Seed data
            SeedData(provider);

            // Persist settings to disk
            DbSettingsManager.Save(_settings);

            // Reset session factory so it picks up the new connection string
            _sessionFactory = null;

            errorMessage = string.Empty;
            return true;
        }
        catch (Exception ex)
        {
            var inner = ex.InnerException;
            errorMessage = inner != null ? $"{ex.Message} -> {inner.Message}" : ex.Message;
            return false;
        }
    }

    static void CreateSqlServerDatabase(string server, string databaseName, string userId, string password)
    {
        var masterConnStr = $"Server={server};User Id={userId};Password={password};";
        using var conn = new SqlConnection(masterConnStr);
        conn.Open();
        using var cmd = conn.CreateCommand();
        cmd.CommandText = $"IF NOT EXISTS (SELECT 1 FROM sys.databases WHERE name = N'{databaseName}') CREATE DATABASE [{databaseName}]";
        cmd.ExecuteNonQuery();
    }

    static void CreateMySqlDatabase(string server, string databaseName, string userId, string password)
    {
        var masterConnStr = new MySqlConnectionStringBuilder
        {
            Server = server,
            UserID = userId,
            Password = password,
        }.ToString();

        using var conn = new MySqlConnection(masterConnStr);
        conn.Open();
        using var cmd = conn.CreateCommand();
        cmd.CommandText = $"SELECT SCHEMA_NAME FROM information_schema.SCHEMATA WHERE SCHEMA_NAME = '{databaseName}'";
        var exists = cmd.ExecuteScalar() != null;
        if (!exists)
        {
            cmd.CommandText = $"CREATE DATABASE `{databaseName}`";
            cmd.ExecuteNonQuery();
        }
    }

    static void CreatePostgreSQLDatabase(string server, string databaseName, string userId, string password)
    {
        var masterConnStr = new NpgsqlConnectionStringBuilder
        {
            Host = server,
            Database = "postgres",
            Username = userId,
            Password = password,
        }.ToString();

        using var conn = new NpgsqlConnection(masterConnStr);
        conn.Open();
        using var tx = conn.BeginTransaction();
        using (var cmd = conn.CreateCommand())
        {
            cmd.CommandText = $"SELECT 1 FROM pg_database WHERE datname = '{databaseName}'";
            var exists = (int?)cmd.ExecuteScalar() == 1;
            if (!exists)
            {
                cmd.CommandText = $"CREATE DATABASE \"{databaseName}\"";
                cmd.ExecuteNonQuery();
            }
        }
        tx.Commit();
    }

     static void SeedData(DatabaseProvider provider)
    {
        var xmlPath = System.IO.Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "HelpTopics.xml");

        if (provider == DatabaseProvider.SqlServer)
        {
            using var conn = new SqlConnection(ConnectionString);
            conn.Open();

            using (var cmd = conn.CreateCommand())
            {
                cmd.CommandText = @"
                IF NOT EXISTS (SELECT 1 FROM ApplicationInfo)
                    INSERT INTO ApplicationInfo (ApplicationName, Author, Version, Description, Framework, Dependencies)
                    VALUES (N'Winforms Vibes', N'Eric', N'1.0', N'A simple Windows Forms demo application.', N'net10.0-windows', N'Microsoft.Web.WebView2 1.0.3967.48, ReaLTaiizor 3.8.1.8');";
                cmd.ExecuteNonQuery();
            }

            using (var countCmd = conn.CreateCommand())
            {
                countCmd.CommandText = "SELECT COUNT(1) FROM HelpInfo";
                var count = (int)countCmd.ExecuteScalar();
                if (count == 0 && System.IO.File.Exists(xmlPath))
                {
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
        else if (provider == DatabaseProvider.MySql)
        {
            using var conn = new MySqlConnection(ConnectionString);
            conn.Open();

            using (var cmd = conn.CreateCommand())
            {
                cmd.CommandText = @"
                INSERT INTO `ApplicationInfo` (`ApplicationName`, `Author`, `Version`, `Description`, `Framework`, `Dependencies`)
                SELECT 'Winforms Vibes', 'Eric', '1.0', 'A simple Windows Forms demo application.', 'net10.0-windows', 'Microsoft.Web.WebView2 1.0.3967.48, ReaLTaiizor 3.8.1.8'
                FROM DUAL
                WHERE NOT EXISTS (SELECT 1 FROM `ApplicationInfo`)";
                cmd.ExecuteNonQuery();
            }

            using (var countCmd = conn.CreateCommand())
            {
                countCmd.CommandText = "SELECT COUNT(1) FROM `HelpInfo`";
                var count = (int)(long?)(countCmd.ExecuteScalar() ?? 0);
                if (count == 0 && System.IO.File.Exists(xmlPath))
                {
                    var doc = XDocument.Load(xmlPath);
                    foreach (var topic in doc.Descendants("Topic"))
                    {
                        using var insertCmd = conn.CreateCommand();
                        insertCmd.CommandText = "INSERT INTO `HelpInfo` (`Category`, `Topic`, `Content`) VALUES (@cat, @topic, @content)";
                        insertCmd.Parameters.AddWithValue("@cat", topic.Attribute("Category")!.Value);
                        insertCmd.Parameters.AddWithValue("@topic", topic.Attribute("Name")!.Value);
                        insertCmd.Parameters.AddWithValue("@content", topic.Value.Trim());
                        insertCmd.ExecuteNonQuery();
                    }
                }
            }
        }
        else
        {
            using var conn = new NpgsqlConnection(ConnectionString);
            conn.Open();

            using (var cmd = conn.CreateCommand())
            {
                cmd.CommandText = @"
                INSERT INTO ""ApplicationInfo"" (""ApplicationName"", ""Author"", ""Version"", ""Description"", ""Framework"", ""Dependencies"")
                SELECT 'Winforms Vibes', 'Eric', '1.0', 'A simple Windows Forms demo application.', 'net10.0-windows', 'Microsoft.Web.WebView2 1.0.3967.48, ReaLTaiizor 3.8.1.8'
                WHERE NOT EXISTS (SELECT 1 FROM ""ApplicationInfo"")";
                cmd.ExecuteNonQuery();
            }

            using (var countCmd = conn.CreateCommand())
            {
                countCmd.CommandText = "SELECT COUNT(1) FROM \"HelpInfo\"";
                var count = (int)(long?)(countCmd.ExecuteScalar() ?? 0);
                if (count == 0 && System.IO.File.Exists(xmlPath))
                {
                    var doc = XDocument.Load(xmlPath);
                    foreach (var topic in doc.Descendants("Topic"))
                    {
                        using var insertCmd = conn.CreateCommand();
                        insertCmd.CommandText = "INSERT INTO \"HelpInfo\" (\"Category\", \"Topic\", \"Content\") VALUES (@cat, @topic, @content)";
                        insertCmd.Parameters.Add(new NpgsqlParameter("@cat", topic.Attribute("Category")!.Value));
                        insertCmd.Parameters.Add(new NpgsqlParameter("@topic", topic.Attribute("Name")!.Value));
                        insertCmd.Parameters.Add(new NpgsqlParameter("@content", topic.Value.Trim()));
                        insertCmd.ExecuteNonQuery();
                    }
                }
            }
        }
    }

    private static System.Data.Common.DbConnection CreateDbConnection() => _settings.Provider switch
    {
        DatabaseProvider.SqlServer => new SqlConnection(ConnectionString),
        DatabaseProvider.PostgreSQL => new NpgsqlConnection(ConnectionString),
        DatabaseProvider.MySql => new MySqlConnection(ConnectionString),
        _ => throw new NotSupportedException($"Provider {_settings.Provider} is not supported."),
    };
}
