using System.Drawing;
using System.Windows.Forms;

namespace WinformsVibes;

public class ChatWindow : Form
{
    private const string ApiKey = "apikey";
    private const string Model = "Qwen3.6-27B-MTP-Q4_K_M";

    private static ChatWindow? _instance;

    private readonly RichTextBox _chatLog;
    private readonly TextBox _inputBox;
    private readonly Button _sendButton;
    private readonly OpenAIChatClient _client;

    public static ChatWindow GetInstance()
    {
        _instance ??= new ChatWindow();
        _instance.BringToFront();
        return _instance;
    }

    private ChatWindow()
    {
        StartPosition = FormStartPosition.CenterScreen;
        Text = "AI Chat";
        Size = new Size(700, 550);
        MinimumSize = new Size(500, 400);
        BackColor = Color.FromArgb(30, 30, 46);

        _client = new OpenAIChatClient(ApiKey, Model);

        var panel = new Panel { Dock = DockStyle.Fill, BackColor = Color.FromArgb(30, 30, 46) };

        // --- Chat log ---
        _chatLog = new RichTextBox
        {
            Dock = DockStyle.Fill,
            BackColor = Color.FromArgb(35, 35, 50),
            ForeColor = Color.LightGray,
            BorderStyle = BorderStyle.None,
            ReadOnly = true,
            Font = new Font("Consolas", 10f),
        };

        // --- Input bar ---
        _inputBox = new TextBox
        {
            Dock = DockStyle.Bottom,
            BackColor = Color.FromArgb(45, 45, 60),
            ForeColor = Color.WhiteSmoke,
            Font = new Font("Segoe UI", 11f),
            PlaceholderText = "Type a message...",
        };
        _inputBox.KeyDown += OnInputKeyDown;

        _sendButton = new Button
        {
            Dock = DockStyle.Bottom,
            Height = 37,
            Text = "Send",
            FlatStyle = FlatStyle.Flat,
            BackColor = Color.FromArgb(55, 55, 80),
            ForeColor = Color.WhiteSmoke,
            Font = new Font("Segoe UI", 10f),
        };
        _sendButton.Click += OnSend;

        // Wrap input and button in a bottom panel
        var inputPanel = new Panel
        {
            Dock = DockStyle.Bottom,
            Height = 45,
            BackColor = Color.FromArgb(30, 30, 46),
        };
        inputPanel.Controls.Add(_sendButton);
        inputPanel.Controls.Add(_inputBox);
        _sendButton.Dock = DockStyle.Right;
        _sendButton.Width = 90;
        _inputBox.Dock = DockStyle.Fill;

        panel.Controls.Add(_chatLog);
        panel.Controls.Add(inputPanel);
        Controls.Add(panel);

        AppendSystem($"Connected to {Model} at http://192.168.2.15:8888/v1");
        _inputBox.Focus();
    }

    private async void OnSend(object? sender, EventArgs e)
    {
        var message = _inputBox.Text.Trim();
        if (string.IsNullOrEmpty(message)) return;

        _inputBox.Clear();
        AppendUser(message);
        AppendSystem("Thinking...");

        try
        {
            var reply = await _client.ChatAsync(message);
            RemoveThinking();
            AppendAssistant(reply);
        }
        catch (Exception ex)
        {
            RemoveThinking();
            AppendSystem($"Error: {ex.Message}");
        }
    }

    private void RemoveThinking()
    {
        var suffix = "Thinking...\n";
        var text = _chatLog.Text;
        if (text.Length >= suffix.Length && text.EndsWith(suffix))
            _chatLog.Text = text[..^suffix.Length];
    }

    private void OnInputKeyDown(object? sender, KeyEventArgs e)
    {
        if (e.KeyCode == Keys.Enter && (e.Modifiers & Keys.Shift) == 0)
        {
            e.SuppressKeyPress = true;
            OnSend(sender, e);
        }
    }

    private void AppendUser(string text)
    {
        _chatLog.SelectionColor = Color.FromArgb(100, 181, 246);
        _chatLog.AppendText($"You: {text}\n\n");
        _chatLog.ScrollToCaret();
    }

    private void AppendAssistant(string text)
    {
        _chatLog.SelectionColor = Color.FromArgb(129, 199, 132);
        _chatLog.AppendText($"Assistant: {text}\n\n");
        _chatLog.ScrollToCaret();
    }

    private void AppendSystem(string text)
    {
        _chatLog.SelectionColor = Color.Gray;
        _chatLog.AppendText($"{text}\n");
        _chatLog.ScrollToCaret();
    }

    protected override void OnFormClosing(FormClosingEventArgs e)
    {
        if (e.CloseReason == CloseReason.UserClosing)
        {
            e.Cancel = true;
            Hide();
        }
        else
        {
            _client.Dispose();
            base.OnFormClosing(e);
        }
    }
}
