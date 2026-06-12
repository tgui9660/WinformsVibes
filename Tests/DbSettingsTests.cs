using NUnit.Framework;
using WinformsVibes.Database;

namespace WinformsVibes.Tests;

[TestFixture]
public class DbSettingsTests
{
    [Test]
    public void NewSettings_HasNoHardcodedCredentials()
    {
        var settings = new DbSettings();

        Assert.That(settings.Server, Is.Empty);
        Assert.That(settings.DatabaseName, Is.Empty);
        Assert.That(settings.UserId, Is.Empty);
        Assert.That(settings.Password, Is.Empty);
    }

    [Test]
    public void DatabaseProvider_ContainsSqlServer()
    {
        var values = Enum.GetValues<DatabaseProvider>();
        Assert.That(values, Does.Contain(DatabaseProvider.SqlServer));
    }

    [Test]
    public void DatabaseProvider_ContainsPostgreSQL()
    {
        var values = Enum.GetValues<DatabaseProvider>();
        Assert.That(values, Does.Contain(DatabaseProvider.PostgreSQL));
    }

    [Test]
    public void DatabaseProvider_ContainsMySql()
    {
        var values = Enum.GetValues<DatabaseProvider>();
        Assert.That(values, Does.Contain(DatabaseProvider.MySql));
    }

    [Test]
    public void DatabaseProvider_HasThreeValues()
    {
        var values = Enum.GetValues<DatabaseProvider>();
        Assert.That(values.Length, Is.EqualTo(3));
    }

    [Test]
    public void DbSettings_CanSetProviderToMySql()
    {
        var settings = new DbSettings { Provider = DatabaseProvider.MySql };
        Assert.That(settings.Provider, Is.EqualTo(DatabaseProvider.MySql));
    }

    [Test]
    public void DbSettings_CanSetProviderToPostgreSQL()
    {
        var settings = new DbSettings { Provider = DatabaseProvider.PostgreSQL };
        Assert.That(settings.Provider, Is.EqualTo(DatabaseProvider.PostgreSQL));
    }

    [Test]
    public void DbSettings_CanSetProviderToSqlServer()
    {
        var settings = new DbSettings { Provider = DatabaseProvider.SqlServer };
        Assert.That(settings.Provider, Is.EqualTo(DatabaseProvider.SqlServer));
    }
}
