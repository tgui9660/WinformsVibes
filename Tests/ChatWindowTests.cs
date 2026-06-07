using System.Reflection;
using System.Drawing;
using System.Windows.Forms;
using NUnit.Framework;
using WinformsVibes.GUI;

namespace WinformsVibes.Tests;

[TestFixture]
public class ChatWindowTests
{
    private ChatWindow? _window;

    [SetUp]
    public void SetUp()
    {
        _window = ChatWindow.GetInstance();
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

    // --- Singleton Pattern ---

    [Test]
    public void GetInstance_ReturnsSameInstanceOnRepeatedCalls()
    {
        var first = ChatWindow.GetInstance();
        var second = ChatWindow.GetInstance();

        Assert.That(second, Is.SameAs(first));
    }

    [Test]
    public void GetInstance_ReturnsNonNull()
    {
        Assert.That(_window, Is.Not.Null);
    }

    // --- Form Properties ---

    [Test]
    public void Constructor_SetsWindowTitle()
    {
        Assert.That(_window!.Text, Is.EqualTo("AI Chat"));
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
    public void Constructor_CreatesSmileyIcon()
    {
        Assert.That(_window!.Icon, Is.Not.Null);
    }

    // --- API Constants (via reflection) ---

    [Test]
    public void ApiKey_IsHardcoded()
    {
        var field = typeof(ChatWindow).GetField("ApiKey", BindingFlags.NonPublic | BindingFlags.Static);

        Assert.That(field, Is.Not.Null);
        Assert.That(field!.GetValue(null), Is.EqualTo("apikey"));
    }

    [Test]
    public void Model_IsHardcoded()
    {
        var field = typeof(ChatWindow).GetField("Model", BindingFlags.NonPublic | BindingFlags.Static);

        Assert.That(field, Is.Not.Null);
        Assert.That(field!.GetValue(null), Is.EqualTo("Qwen3.6-27B-MTP-Q4_K_M"));
    }

    // --- Chat Log Content ---

    [Test]
    public void Constructor_AppendsConnectionMessage()
    {
        var chatLog = GetChatLog(_window!);

        Assert.That(chatLog.Text, Does.Contain("Connected to"));
        Assert.That(chatLog.Text, Does.Contain("Qwen3.6-27B-MTP-Q4_K_M"));
        Assert.That(chatLog.Text, Does.Contain("192.168.2.15:8888/v1"));
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

    [Test]
    public void AppendSystem_AddsGrayText()
    {
        var chatLog = GetChatLog(_window!);

        CallAppend(_window!, "AppendSystem", "System note");

        Assert.That(chatLog.Text, Does.Contain("System note"));
    }

    [Test]
    public void AppendUser_AddsNewlinesAfterMessage()
    {
        var chatLog = GetChatLog(_window!);

        CallAppend(_window!, "AppendUser", "test");

        Assert.That(chatLog.Text, Does.Contain("You: test\n\n"));
    }

    [Test]
    public void AppendAssistant_AddsNewlinesAfterMessage()
    {
        var chatLog = GetChatLog(_window!);

        CallAppend(_window!, "AppendAssistant", "test");

        Assert.That(chatLog.Text, Does.Contain("test\n\n"));
    }

    // --- Form Closing Behavior ---

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

    // --- Chat with Mock Client ---

    [Test]
    public void OnSend_WithMockClient_AppendsAssistantReply()
    {
        var mockClient = new MockChatClient("Hello from mock!");
        var clientField = typeof(ChatWindow).GetField("_client", BindingFlags.NonPublic | BindingFlags.Instance);
        clientField!.SetValue(_window, mockClient);

        var chatLog = GetChatLog(_window!);
        var inputBox = GetInputBox(_window!);
        var sendButton = GetSendButton(_window!);

        inputBox.Text = "Hi there";
        // Fire the Click event via OnClick (protected method)
        var onClickMethod = typeof(Button).GetMethod("OnClick", BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public);
        onClickMethod!.Invoke(sendButton, new object?[] { EventArgs.Empty });

        // Pump messages to let async void OnSend complete
        for (var i = 0; i < 30 && !chatLog.Text.Contains("Assistant: Hello from mock!"); i++)
        {
            Application.DoEvents();
            Thread.Sleep(100);
        }

        Assert.That(chatLog.Text, Does.Contain("You: Hi there"));
        Assert.That(chatLog.Text, Does.Contain("Assistant: Hello from mock!"));
    }

    [Test]
    public void OnSend_WhenClientThrows_AppendsErrorMessage()
    {
        var mockClient = new MockChatClient(null, new InvalidOperationException("Test error"));
        var clientField = typeof(ChatWindow).GetField("_client", BindingFlags.NonPublic | BindingFlags.Instance);
        clientField!.SetValue(_window, mockClient);

        var chatLog = GetChatLog(_window!);
        var inputBox = GetInputBox(_window!);
        var sendButton = GetSendButton(_window!);

        inputBox.Text = "fail me";
        var onClickMethod = typeof(Button).GetMethod("OnClick", BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public);
        onClickMethod!.Invoke(sendButton, new object?[] { EventArgs.Empty });

        for (var i = 0; i < 30 && !chatLog.Text.Contains("Error: Test error"); i++)
        {
            Application.DoEvents();
            Thread.Sleep(100);
        }

        Assert.That(chatLog.Text, Does.Contain("Error: Test error"));
    }

    [Test]
    public void OnSend_EmptyMessage_DoesNothing()
    {
        var chatLog = GetChatLog(_window!);
        var initialLength = chatLog.TextLength;
        var inputBox = GetInputBox(_window!);
        var sendButton = GetSendButton(_window!);

        inputBox.Text = "   ";
        sendButton.PerformClick();

        Assert.That(chatLog.TextLength, Is.EqualTo(initialLength));
    }

    // --- Helpers ---

    private static RichTextBox GetChatLog(ChatWindow window)
    {
        var field = typeof(ChatWindow).GetField("_chatLog", BindingFlags.NonPublic | BindingFlags.Instance);
        return (RichTextBox)field!.GetValue(window)!;
    }

    private static TextBox GetInputBox(ChatWindow window)
    {
        var field = typeof(ChatWindow).GetField("_inputBox", BindingFlags.NonPublic | BindingFlags.Instance);
        return (TextBox)field!.GetValue(window)!;
    }

    private static Button GetSendButton(ChatWindow window)
    {
        var field = typeof(ChatWindow).GetField("_sendButton", BindingFlags.NonPublic | BindingFlags.Instance);
        return (Button)field!.GetValue(window)!;
    }

    private static void CallAppend(ChatWindow window, string methodName, string text)
    {
        var method = typeof(ChatWindow).GetMethod(methodName, BindingFlags.NonPublic | BindingFlags.Instance);
        method!.Invoke(window, new object[] { text });
    }

    private static void ResetSingleton()
    {
        var field = typeof(ChatWindow).GetField("_instance", BindingFlags.NonPublic | BindingFlags.Static);
        if (field is not null)
            field.SetValue(null, null);
    }

    // --- Mock Chat Client ---

    private sealed class MockChatClient : WinformsVibes.AI.OpenAIChatClient
    {
        private readonly string? _reply;
        private readonly Exception? _throwException;

        public MockChatClient(string reply)
            : base("mock-key", "mock-model", "http://mock")
        {
            _reply = reply;
        }

        public MockChatClient(string? reply, Exception throwException)
            : base("mock-key", "mock-model", "http://mock")
        {
            _reply = reply;
            _throwException = throwException;
        }

        public override async Task<string> ChatAsync(string message, string systemPrompt = "You are a helpful assistant.")
        {
            if (_throwException is not null)
                throw _throwException;

            return _reply ?? string.Empty;
        }
    }
}
