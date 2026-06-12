using System.Reflection;
using System.Drawing;
using System.Windows.Forms;
using NUnit.Framework;
using WinformsVibes.GUI;

namespace WinformsVibes.Tests;

[TestFixture]
public class AIHelpWindowTests
{
    private AIHelpWindow? _window;

    [SetUp]
    public void SetUp()
    {
        _window = AIHelpWindow.GetInstance();
    }

    [TearDown]
    public void TearDown()
    {
        if (_window is not null)
        {
            _window.Dispose();
            ResetSingleton();
        }
    }

    [Test]
    public void GetInstance_ReturnsSameInstanceOnRepeatedCalls()
    {
        var first = AIHelpWindow.GetInstance();
        var second = AIHelpWindow.GetInstance();

        Assert.That(second, Is.SameAs(first));
    }

    [Test]
    public void GetInstance_ReturnsNonNull()
    {
        Assert.That(_window, Is.Not.Null);
    }

    [Test]
    public void Constructor_SetsWindowTitle()
    {
        Assert.That(_window!.Text, Is.EqualTo("Fella - AI Helper"));
    }

    [Test]
    public void Constructor_SetsQuestionMarkIcon()
    {
        Assert.That(_window!.Icon, Is.Not.Null);
    }

    [Test]
    public void Constructor_SetsStartPosition()
    {
        Assert.That(_window!.StartPosition, Is.EqualTo(FormStartPosition.CenterScreen));
    }

    [Test]
    public void Constructor_SetsWindowSize()
    {
        Assert.That(_window!.Size, Is.EqualTo(new Size(700, 550)));
    }

    [Test]
    public void Constructor_SetsMinimumSize()
    {
        Assert.That(_window!.MinimumSize, Is.EqualTo(new Size(500, 400)));
    }

    [Test]
    public void ApiKey_IsHardcoded()
    {
        var field = typeof(AIHelpWindow).GetField("ApiKey", BindingFlags.NonPublic | BindingFlags.Static);

        Assert.That(field, Is.Not.Null);
        Assert.That(field!.GetValue(null), Is.EqualTo("apikey"));
    }

    [Test]
    public void Model_IsHardcoded()
    {
        var field = typeof(AIHelpWindow).GetField("Model", BindingFlags.NonPublic | BindingFlags.Static);

        Assert.That(field, Is.Not.Null);
        Assert.That(field!.GetValue(null), Is.EqualTo("Qwen3.6-27B-MTP-Q4_K_M"));
    }

    [Test]
    public void Constructor_AppendsConnectionMessage()
    {
        var chatLog = GetChatLog(_window!);

        Assert.That(chatLog.Text, Does.Contain("Connected to"));
        Assert.That(chatLog.Text, Does.Contain("Qwen3.6-27B-MTP-Q4_K_M"));
    }

    [Test]
    public void Constructor_AppendsWelcomeMessage()
    {
        var chatLog = GetChatLog(_window!);

        Assert.That(chatLog.Text, Does.Contain("Welcome to Fella!"));
    }

    [Test]
    public void OnFormClosing_UserClosing_HidesForm()
    {
        var args = new FormClosingEventArgs(CloseReason.UserClosing, false);

        var method = typeof(Form).GetMethod("OnFormClosing",
            BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public);
        method!.Invoke(_window!, new object?[] { args });

        Assert.That(args.Cancel, Is.True);
    }

    [Test]
    public void OnFormClosing_ApplicationExit_DoesNotCancel()
    {
        var args = new FormClosingEventArgs(CloseReason.ApplicationExitCall, false);

        var method = typeof(Form).GetMethod("OnFormClosing",
            BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public);
        method!.Invoke(_window!, new object?[] { args });

        Assert.That(args.Cancel, Is.False);
    }

    private static RichTextBox GetChatLog(AIHelpWindow window)
    {
        var field = typeof(AIHelpWindow).GetField("_chatLog", BindingFlags.NonPublic | BindingFlags.Instance);
        return (RichTextBox)field!.GetValue(window)!;
    }

    private static void ResetSingleton()
    {
        var field = typeof(AIHelpWindow).GetField("_instance", BindingFlags.NonPublic | BindingFlags.Static);
        if (field is not null)
            field.SetValue(null, null);
    }
}
