using System.Reflection;
using System.Drawing;
using System.Windows.Forms;
using NUnit.Framework;
using WinformsVibes.GUI;

namespace WinformsVibes.Tests;

[TestFixture]
public class AIMapWindowTests
{
    private AIMapWindow? _window;

    [SetUp]
    public void SetUp()
    {
        _window = AIMapWindow.GetInstance();
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
        var first = AIMapWindow.GetInstance();
        var second = AIMapWindow.GetInstance();

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
        Assert.That(_window!.Text, Is.EqualTo("AI Map Chat"));
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
        var field = typeof(AIMapWindow).GetField("ApiKey", BindingFlags.NonPublic | BindingFlags.Static);

        Assert.That(field, Is.Not.Null);
        Assert.That(field!.GetValue(null), Is.EqualTo("apikey"));
    }

    [Test]
    public void Model_IsHardcoded()
    {
        var field = typeof(AIMapWindow).GetField("Model", BindingFlags.NonPublic | BindingFlags.Static);

        Assert.That(field, Is.Not.Null);
        Assert.That(field!.GetValue(null), Is.EqualTo("Qwen3.6-27B-MTP-Q4_K_M"));
    }

    [Test]
    public void Constructor_AppendsConnectionMessage()
    {
        var chatLog = GetChatLog(_window!);

        Assert.That(chatLog.Text, Does.Contain("Connected to"));
        Assert.That(chatLog.Text, Does.Contain("Qwen3.6-27B-MTP-Q4_K_M"));
        Assert.That(chatLog.Text, Does.Contain("192.168.2.15:8888/v1"));
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

    // --- Append Methods (via reflection) ---

    [Test]
    public void AppendUser_AddsBlueTextWithYouPrefix()
    {
        var chatLog = GetChatLog(_window!);

        CallAppend(_window!, "AppendUser", "Hello");

        Assert.That(chatLog.Text, Does.Contain("You: Hello"));
    }

    [Test]
    public void AppendAssistant_AddsGreenTextWithAssistantPrefix()
    {
        var chatLog = GetChatLog(_window!);

        CallAppend(_window!, "AppendAssistant", "Reply text");

        Assert.That(chatLog.Text, Does.Contain("Assistant: Reply text"));
    }

    private static RichTextBox GetChatLog(AIMapWindow window)
    {
        var field = typeof(AIMapWindow).GetField("_chatLog", BindingFlags.NonPublic | BindingFlags.Instance);
        return (RichTextBox)field!.GetValue(window)!;
    }

    private static void CallAppend(AIMapWindow window, string methodName, string text)
    {
        var method = typeof(AIMapWindow).GetMethod(methodName, BindingFlags.NonPublic | BindingFlags.Instance);
        method!.Invoke(window, new object[] { text });
    }

    private static void ResetSingleton()
    {
        var field = typeof(AIMapWindow).GetField("_instance", BindingFlags.NonPublic | BindingFlags.Static);
        if (field is not null)
            field.SetValue(null, null);
    }
}
