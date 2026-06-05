using System.Drawing;
using System.Windows.Forms;

namespace WinformsVibes;

public class HelpWindow : Form
{
    private TextBox _searchBox;
    private ListView _listView;
    private RichTextBox _contentBox;
    private Label _titleLabel;
    private Label _noResultsLabel;
    private List<HelpTopic> _allTopics = new();

    public HelpWindow()
    {
        this.StartPosition = FormStartPosition.CenterScreen;
        this.Text = "Help Contents";
        this.Size = new Size(640, 500);
        this.MinimumSize = new Size(480, 400);
        this.BackColor = Color.FromArgb(30, 30, 46);

        var panel = new Panel { Dock = DockStyle.Fill, BackColor = Color.FromArgb(30, 30, 46) };

        _titleLabel = new Label
        {
            Text = "Help Topics",
            Font = new Font("Segoe UI", 16f, FontStyle.Bold),
            ForeColor = Color.WhiteSmoke,
            Location = new Point(20, 15),
            Size = new Size(580, 35),
            AutoSize = false,
        };

        _searchBox = new TextBox
        {
            Font = new Font("Segoe UI", 11f),
            ForeColor = Color.WhiteSmoke,
            BackColor = Color.FromArgb(45, 45, 60),
            Location = new Point(20, 55),
            Size = new Size(580, 35),
            PlaceholderText = "Search topics...",
        };
        _searchBox.TextChanged += (_, _) => FilterResults();
        _searchBox.Enter += (_, _) => _searchBox.SelectAll();

        _listView = new ListView
        {
            Location = new Point(20, 100),
            Size = new Size(260, 340),
            BackColor = Color.FromArgb(35, 35, 50),
            ForeColor = Color.LightGray,
            BorderStyle = BorderStyle.None,
            View = View.List,
            FullRowSelect = true,
            HideSelection = false,
        };
        _listView.Font = new Font("Segoe UI", 10f);
        _listView.SelectedIndexChanged += (_, _) => ShowSelectedTopic();

        _contentBox = new RichTextBox
        {
            Location = new Point(290, 100),
            Size = new Size(310, 340),
            BackColor = Color.FromArgb(35, 35, 50),
            ForeColor = Color.LightGray,
            BorderStyle = BorderStyle.None,
            ReadOnly = true,
            Font = new Font("Segoe UI", 10f),
        };

        _noResultsLabel = new Label
        {
            Text = "No matching topics found.",
            Font = new Font("Segoe UI", 10f, FontStyle.Italic),
            ForeColor = Color.Gray,
            Location = new Point(290, 260),
            Size = new Size(310, 20),
            AutoSize = false,
            Visible = false,
        };

        panel.Controls.AddRange(new Control[] { _searchBox, _titleLabel, _contentBox, _listView, _noResultsLabel });
        this.Controls.Add(panel);

        LoadTopics();
    }

    private void LoadTopics()
    {
        _allTopics = DbConfig.GetHelpTopics();
        PopulateList(_allTopics);
    }

    private void PopulateList(IEnumerable<HelpTopic> topics)
    {
        _listView.Items.Clear();
        foreach (var topic in topics)
        {
            var item = _listView.Items.Add($"{topic.Category} — {topic.Topic}");
            item.Tag = topic;
        }
    }

    private void FilterResults()
    {
        var query = _searchBox.Text.Trim().ToLower();
        var filtered = _allTopics;

        if (!string.IsNullOrEmpty(query))
            filtered = _allTopics.Where(t =>
                t.Category.ToLower().Contains(query) ||
                t.Topic.ToLower().Contains(query) ||
                t.Content.ToLower().Contains(query)).ToList();

        PopulateList(filtered);
        _noResultsLabel.Visible = filtered.Count() == 0 && !string.IsNullOrEmpty(query);
    }

    private void ShowSelectedTopic()
    {
        if (_listView.SelectedItems.Count == 0) return;

        var topic = (HelpTopic)_listView.SelectedItems[0].Tag!;
        _contentBox.Text = $"Category: {topic.Category}\nTopic: {topic.Topic}\n\n{topic.Content}";
        _noResultsLabel.Visible = false;
    }
}

public record HelpTopic(string Category, string Topic, string Content);
