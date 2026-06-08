using System.Data.SqlClient;
using FluentNHibernate.Cfg;
using FluentNHibernate.Cfg.Db;
using NHibernate.Tool.hbm2ddl;
using NUnit.Framework;
using WinformsVibes.Maps;

namespace WinformsVibes.Tests;

[TestFixture]
public class DatabaseTests
{
    private const string Server = "localhost";
    private const string UserId = "sa";
    private const string Password = "password";
    private const string TestDatabaseName = "testdb_schema";

    [OneTimeSetUp]
    public void DropTestDatabase()
    {
        var masterConnStr = $"Server={Server};User Id={UserId};Password={Password};";
        using var conn = new SqlConnection(masterConnStr);
        conn.Open();
        using var cmd = conn.CreateCommand();
        cmd.CommandText = $"IF EXISTS (SELECT 1 FROM sys.databases WHERE name = N'{TestDatabaseName}') BEGIN ALTER DATABASE [{TestDatabaseName}] SET SINGLE_USER WITH ROLLBACK IMMEDIATE; DROP DATABASE [{TestDatabaseName}]; END";
        cmd.ExecuteNonQuery();
    }

    [Test]
    public void CreateDatabase_ConnectionSucceeds()
    {
        var masterConnStr = $"Server={Server};User Id={UserId};Password={Password};";

        using var conn = new SqlConnection(masterConnStr);
        conn.Open();
        using var cmd = conn.CreateCommand();
        cmd.CommandText = $"IF NOT EXISTS (SELECT 1 FROM sys.databases WHERE name = N'{TestDatabaseName}') CREATE DATABASE [{TestDatabaseName}]";
        cmd.ExecuteNonQuery();

        Assert.Pass("Database created (or already existed).");
    }

    [Test]
    public void CreateDatabase_SchemaExportCreatesTables()
    {
        var masterConnStr = $"Server={Server};User Id={UserId};Password={Password};";
        var connStr = $"Server={Server};Database={TestDatabaseName};User Id={UserId};Password={Password};";

        // Ensure the database exists
        using (var conn = new SqlConnection(masterConnStr))
        {
            conn.Open();
            using var cmd = conn.CreateCommand();
            cmd.CommandText = $"IF NOT EXISTS (SELECT 1 FROM sys.databases WHERE name = N'{TestDatabaseName}') CREATE DATABASE [{TestDatabaseName}]";
            cmd.ExecuteNonQuery();
        }

        // Build Fluent NHibernate config
        var fluentCfg = Fluently.Configure()
            .Database(MsSqlConfiguration.MsSql2012.ConnectionString(connStr))
            .Mappings(m =>
            {
                m.FluentMappings.Add<ApplicationInfoMap>();
                m.FluentMappings.Add<HelpInfoMap>();
            })
            .ExposeConfiguration(cfg =>
            {
                cfg.SetProperty("use_proxy_validator", "false");
                cfg.SetProperty("default_lazy", "false");
            });

        var nhConfig = fluentCfg.BuildConfiguration();

        // Run SchemaExport
        using var schemaConn = new SqlConnection(connStr);
        schemaConn.Open();
        new SchemaExport(nhConfig)
            .Create(true, true, schemaConn);

        // Verify tables exist
        using (var conn = new SqlConnection(connStr))
        {
            conn.Open();
            using var cmd = conn.CreateCommand();
            cmd.CommandText = @"
                SELECT COUNT(1) FROM sys.tables
                WHERE name IN ('ApplicationInfo', 'HelpInfo')";
            var count = (int)cmd.ExecuteScalar();
            Assert.That(count, Is.EqualTo(2), $"Expected 2 tables, found {count}");
        }
    }

    [Test]
    public void CreateDatabase_FullFlowWithSeeding()
    {
        var masterConnStr = $"Server={Server};User Id={UserId};Password={Password};";
        var connStr = $"Server={Server};Database={TestDatabaseName};User Id={UserId};Password={Password};";

        // Ensure the database exists
        using (var conn = new SqlConnection(masterConnStr))
        {
            conn.Open();
            using var cmd = conn.CreateCommand();
            cmd.CommandText = $"IF NOT EXISTS (SELECT 1 FROM sys.databases WHERE name = N'{TestDatabaseName}') CREATE DATABASE [{TestDatabaseName}]";
            cmd.ExecuteNonQuery();
        }

        // Build Fluent NHibernate config
        var fluentCfg = Fluently.Configure()
            .Database(MsSqlConfiguration.MsSql2012.ConnectionString(connStr))
            .Mappings(m =>
            {
                m.FluentMappings.Add<ApplicationInfoMap>();
                m.FluentMappings.Add<HelpInfoMap>();
            })
            .ExposeConfiguration(cfg =>
            {
                cfg.SetProperty("use_proxy_validator", "false");
                cfg.SetProperty("default_lazy", "false");
            });

        var nhConfig = fluentCfg.BuildConfiguration();

        // Run SchemaExport
        using (var schemaConn = new SqlConnection(connStr))
        {
            schemaConn.Open();
            new SchemaExport(nhConfig)
                .Create(true, true, schemaConn);
        }

        // Seed ApplicationInfo
        using (var conn = new SqlConnection(connStr))
        {
            conn.Open();
            using var cmd = conn.CreateCommand();
            cmd.CommandText = @"
                IF NOT EXISTS (SELECT 1 FROM ApplicationInfo)
                    INSERT INTO ApplicationInfo (ApplicationName, Author, Version, Description, Framework, Dependencies)
                    VALUES (N'Test App', N'Test', N'1.0', N'Test description', N'net10.0', N'Test deps');";
            cmd.ExecuteNonQuery();
        }

        // Verify seed data exists
        using (var conn = new SqlConnection(connStr))
        {
            conn.Open();
            using var cmd = conn.CreateCommand();
            cmd.CommandText = "SELECT COUNT(1) FROM ApplicationInfo";
            var count = (int)cmd.ExecuteScalar();
            Assert.That(count, Is.GreaterThanOrEqualTo(1), "ApplicationInfo should have at least 1 row");
        }
    }
}
