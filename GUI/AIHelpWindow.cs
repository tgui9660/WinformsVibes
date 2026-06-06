using System.Drawing;
using System.Windows.Forms;
using WinformsVibes.AI;

namespace WinformsVibes.GUI;

public class AIHelpWindow : Form
{
    private const string ApiKey = "apikey";
    private const string Model = "Qwen3.6-27B-MTP-Q4_K_M";

    private static AIHelpWindow? _instance;

    private readonly List<HelpTopic> _helpTopics;
    private readonly RichTextBox _chatLog;
    private readonly TextBox _inputBox;
    private readonly Button _sendButton;
    private readonly OpenAIChatClient _client;

    public static AIHelpWindow GetInstance()
    {
        _instance ??= new AIHelpWindow();
        _instance.BringToFront();
        return _instance;
    }

    private AIHelpWindow()
    {
        StartPosition = FormStartPosition.CenterScreen;
        Text = "Fella - AI Helper";
        Icon = SystemIcons.Question;
        Size = new Size(700, 550);
        MinimumSize = new Size(500, 400);
        BackColor = Color.FromArgb(30, 30, 46);

        _client = new OpenAIChatClient(ApiKey, Model);
        _helpTopics = DbConfig.GetHelpTopics();

        var panel = new Panel { Dock = DockStyle.Fill, BackColor = Color.FromArgb(30, 30, 46) };

        // --- Chat log ---
        _chatLog = new RichTextBox
        {
            Dock = DockStyle.Fill,
            BackColor = Color.FromArgb(35, 35, 50),
            ForeColor = Color.LightGray,
            BorderStyle = BorderStyle.None,
            ReadOnly = true,
            Font = new Font("Consolas", 15f),
        };

        // --- Input bar ---
        _inputBox = new TextBox
        {
            BackColor = Color.FromArgb(45, 45, 60),
            ForeColor = Color.WhiteSmoke,
            Font = new Font("Segoe UI", 16.5f),
            PlaceholderText = "Type a message...",
            Height = 37,
        };
        _inputBox.KeyDown += OnInputKeyDown;

        _sendButton = new Button
        {
            Text = "Send",
            FlatStyle = FlatStyle.Flat,
            BackColor = Color.FromArgb(55, 55, 80),
            ForeColor = Color.WhiteSmoke,
            Font = new Font("Segoe UI", 16.5f),
            Height = 37,
        };
        _sendButton.Click += OnSend;

        // Wrap input and button in a TableLayoutPanel for proper spacing
        var inputPanel = new TableLayoutPanel
        {
            Dock = DockStyle.Bottom,
            Height = 68,
            BackColor = Color.FromArgb(30, 30, 46),
            ColumnCount = 3,
            RowCount = 1,
            ColumnStyles =
            {
                new ColumnStyle(SizeType.Percent, 100),
                new ColumnStyle(SizeType.Absolute, 16), // spacer
                new ColumnStyle(SizeType.Absolute, 90),
            },
            Padding = new Padding(10, 6, 10, 6),
        };
        inputPanel.Controls.Add(_inputBox, 0, 0);
        inputPanel.Controls.Add(_sendButton, 2, 0);
        _inputBox.Anchor = AnchorStyles.Left | AnchorStyles.Right;
        _sendButton.Anchor = AnchorStyles.None;

        panel.Controls.Add(_chatLog);
        panel.Controls.Add(inputPanel);
        Controls.Add(panel);

        AppendSystem($"Connected to {Model} at http://192.168.2.15:8888/v1");
        _chatLog.SelectionColor = Color.Red;
        _chatLog.AppendText("Welcome to Fella! Your helpful AI dude.\n");
        _chatLog.ScrollToCaret();
        _inputBox.Focus();
    }

    private async void OnSend(object? sender, EventArgs e)
    {
        var message = _inputBox.Text.Trim();
        if (string.IsNullOrEmpty(message)) return;

        _inputBox.Clear();
        AppendUser(message);
        var rtfBeforePlaceholder = _chatLog.Rtf;
        AppendSystem("Thinking...");

        try
        {
            var reply = await _client.ChatAsync(message, BuildSystemPrompt());
            _chatLog.Rtf = rtfBeforePlaceholder;
            AppendAssistant(reply);
        }
        catch (Exception ex)
        {
            _chatLog.Rtf = rtfBeforePlaceholder;
            AppendSystem($"Error: {ex.Message}");
        }
    }

    private string BuildSystemPrompt()
    {
        var sb = new System.Text.StringBuilder();
        sb.Append("You are a helpful assistant for the Winforms Vibes application. ");
        sb.Append("Use the following help topics to answer user questions. ");
        sb.Append("If a question isn't covered by these topics, do your best to help.\n\n");
        foreach (var topic in _helpTopics)
        {
            sb.Append($"[{topic.Category}] {topic.Topic}:\n{topic.Content}\n\n");
        }
        return sb.ToString();
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
        _chatLog.SelectionColor = Color.Yellow;
        _chatLog.AppendText("Assistant: ");
        _chatLog.SelectionColor = Color.FromArgb(129, 199, 132);
        _chatLog.AppendText($"{text}\n\n");
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
