using System.Windows.Forms;
using NUnit.Framework;
using WinformsVibes.GUI;

namespace WinformsVibes.Tests;

[TestFixture]
public class DatabaseSetupDialogTests
{
    private DatabaseSetupDialog? _dialog;

    [SetUp]
    public void SetUp()
    {
        _dialog = new DatabaseSetupDialog();
    }

    [TearDown]
    public void TearDown()
    {
        _dialog?.Dispose();
    }

    [Test]
    public void Constructor_SetsDialogTitle()
    {
        Assert.That(_dialog!.Text, Is.EqualTo("Database Setup"));
    }

    [Test]
    public void Constructor_HasProviderSelector()
    {
        var combo = FindControl<ComboBox>(_dialog!);
        Assert.That(combo, Is.Not.Null);
    }

    [Test]
    public void ProviderSelector_HasThreeOptions()
    {
        var combo = FindControl<ComboBox>(_dialog!);
        Assert.That(combo!.Items.Count, Is.EqualTo(3));
    }

    [Test]
    public void ProviderSelector_ContainsSqlServer()
    {
        var combo = FindControl<ComboBox>(_dialog!);
        Assert.That(combo!.Items.Cast<object>().Contains("SQL Server"));
    }

    [Test]
    public void ProviderSelector_ContainsPostgreSQL()
    {
        var combo = FindControl<ComboBox>(_dialog!);
        Assert.That(combo!.Items.Cast<object>().Contains("PostgreSQL"));
    }

    [Test]
    public void ProviderSelector_ContainsMySql()
    {
        var combo = FindControl<ComboBox>(_dialog!);
        Assert.That(combo!.Items.Cast<object>().Contains("MySQL"));
    }

    [Test]
    public void ProviderSelector_DefaultsToSqlServer()
    {
        var combo = FindControl<ComboBox>(_dialog!);
        Assert.That(combo!.SelectedIndex, Is.EqualTo(0));
    }

    [Test]
    public void Server_DefaultsToLocalhost()
    {
        var textBoxes = _dialog!.Controls.OfType<Panel>().FirstOrDefault()!.Controls.OfType<TextBox>().ToList();
        // Server is the first TextBox
        Assert.That(textBoxes[0].Text, Is.EqualTo("localhost"));
    }

    [Test]
    public void Username_DefaultsToSa()
    {
        var textBoxes = _dialog!.Controls.OfType<Panel>().FirstOrDefault()!.Controls.OfType<TextBox>().ToList();
        // Username is the third TextBox (index 2)
        Assert.That(textBoxes[2].Text, Is.EqualTo("sa"));
    }

    [Test]
    public void ProviderSelector_SelectingPostgreSQL_ChangesUsernameToPostgres()
    {
        var combo = FindControl<ComboBox>(_dialog!);
        var textBoxes = _dialog!.Controls.OfType<Panel>().FirstOrDefault()!.Controls.OfType<TextBox>().ToList();

        combo!.SelectedIndex = 1; // PostgreSQL
        Assert.That(textBoxes[2].Text, Is.EqualTo("postgres"));
    }

    [Test]
    public void ProviderSelector_SelectingMySql_ChangesUsernameToRoot()
    {
        var combo = FindControl<ComboBox>(_dialog!);
        var textBoxes = _dialog!.Controls.OfType<Panel>().FirstOrDefault()!.Controls.OfType<TextBox>().ToList();

        combo!.SelectedIndex = 2; // MySQL
        Assert.That(textBoxes[2].Text, Is.EqualTo("root"));
    }

    [Test]
    public void ProviderSelector_SelectingSqlServer_ChangesUsernameToSa()
    {
        var combo = FindControl<ComboBox>(_dialog!);
        var textBoxes = _dialog!.Controls.OfType<Panel>().FirstOrDefault()!.Controls.OfType<TextBox>().ToList();

        combo!.SelectedIndex = 0; // SQL Server
        Assert.That(textBoxes[2].Text, Is.EqualTo("sa"));
    }

    private static T? FindControl<T>(Control form) where T : Control
    {
        var panel = form.Controls.OfType<Panel>().FirstOrDefault();
        return panel?.Controls.OfType<T>().FirstOrDefault();
    }
}
