using System.Drawing;
using System.Threading.Tasks;
using System.Windows.Forms;
using WinformsVibes.AI;

namespace WinformsVibes.GUI;

public class AIMapWindow : TitleBarTooltipForm
{
    private const string ApiKey = "apikey";
    private const string Model = "Qwen3.6-27B-MTP-Q4_K_M";

    private static AIMapWindow? _instance;

    private readonly RichTextBox _chatLog;
    private readonly TextBox _inputBox;
    private readonly Button _sendButton;
    private readonly OpenAIChatClient _client;
    private readonly System.Windows.Forms.Timer _thinkingTimer;
    private int _thinkingStartIndex;
    private int _thinkingDotCount;

    public static AIMapWindow GetInstance()
    {
        _instance ??= new AIMapWindow();
        _instance.BringToFront();
        return _instance;
    }

    private AIMapWindow()
    {
        StartPosition = FormStartPosition.CenterScreen;
        Text = "AI Map Chat";
        Size = new Size(700, 550);
        MinimumSize = new Size(500, 400);
        BackColor = Color.FromArgb(30, 30, 46);

        _client = new OpenAIChatClient(ApiKey, Model);

        _thinkingTimer = new System.Windows.Forms.Timer { Interval = 400 };
        _thinkingTimer.Tick += (_, _) =>
        {
            _thinkingDotCount = (_thinkingDotCount + 1) % 4;
            var dots = new string('.', _thinkingDotCount);
            var text = $"Thinking{dots}";
            _chatLog.Select(_thinkingStartIndex, 12);
            _chatLog.SelectedText = text;
            _chatLog.ScrollToCaret();
        };

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
        _inputBox.Focus();
    }

    public async Task AskAsync(string message)
    {
        Show();
        Activate();
        AppendUser(message);
        var rtfBeforePlaceholder = _chatLog.Rtf;
        _thinkingStartIndex = _chatLog.TextLength;
        AppendSystem("Thinking...");
        _thinkingDotCount = 0;
        _thinkingTimer.Start();

        try
        {
            var reply = await _client.ChatAsync(message);
            _thinkingTimer.Stop();
            _chatLog.Rtf = rtfBeforePlaceholder;
            AppendAssistant(reply);
        }
        catch (Exception ex)
        {
            _thinkingTimer.Stop();
            _chatLog.Rtf = rtfBeforePlaceholder;
            AppendSystem($"Error: {ex.Message}");
        }
    }

    private async void OnSend(object? sender, EventArgs e)
    {
        var message = _inputBox.Text.Trim();
        if (string.IsNullOrEmpty(message)) return;

        _inputBox.Clear();
        AppendUser(message);
        var rtfBeforePlaceholder = _chatLog.Rtf;
        _thinkingStartIndex = _chatLog.TextLength;
        AppendSystem("Thinking...");
        _thinkingDotCount = 0;
        _thinkingTimer.Start();

        try
        {
            var reply = await _client.ChatAsync(message);
            _thinkingTimer.Stop();
            _chatLog.Rtf = rtfBeforePlaceholder;
            AppendAssistant(reply);
        }
        catch (Exception ex)
        {
            _thinkingTimer.Stop();
            _chatLog.Rtf = rtfBeforePlaceholder;
            AppendSystem($"Error: {ex.Message}");
        }
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
