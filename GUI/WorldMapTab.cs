using System;
using System.Drawing;
using System.Windows.Forms;
using Microsoft.Web.WebView2.WinForms;
using ReaLTaiizor.Controls;

namespace WinformsVibes.GUI
{
    class WorldMapTab : UserControl
    {
        private readonly WebView2 _webView = new WebView2 { Dock = DockStyle.Fill };
        private readonly MaterialTextBoxEdit _latInput;
        private readonly MaterialTextBoxEdit _longInput;
        private readonly MaterialButton _tellMeMoreButton;

        public WorldMapTab()
        {
            _latInput = new MaterialTextBoxEdit
            {
                Text = "0",
                Location = new Point(76, 7),
                Size = new Size(160, 62),
                Font = new Font(Font.FontFamily, 16f),
            };
            _latInput.Region = CreateRoundedRegion(_latInput.Size, 16);

            _longInput = new MaterialTextBoxEdit
            {
                Text = "0",
                Location = new Point(324, 7),
                Size = new Size(160, 62),
                Font = new Font(Font.FontFamily, 16f),
            };
            _longInput.Region = CreateRoundedRegion(_longInput.Size, 16);

            _tellMeMoreButton = new MaterialButton
            {
                Text = "Tell Me More!",
                Location = new Point(532, 10),
                Size = new Size(200, 62),
                Enabled = false,
                Font = new Font(Font.FontFamily, 16f),
            };

            var coordPanel = new System.Windows.Forms.Panel
            {
                Dock = DockStyle.Bottom,
                Height = 60,
                Padding = new Padding(16, 6, 16, 6),
                Controls =
                {
                    new MaterialLabel { Text = "Lat",  Location = new Point(36, 23),  Size = new Size(36, 30), Font = new Font(Font.FontFamily, 16f) },
                    _latInput,
                    new MaterialLabel { Text = "Long", Location = new Point(284, 23), Size = new Size(36, 30), Font = new Font(Font.FontFamily, 16f) },
                    _longInput,
                    _tellMeMoreButton,
                },
            };

            Controls.Add(_webView);
            Controls.Add(coordPanel);

            InitializeMapAsync();
        }

        private async void InitializeMapAsync()
        {
            await _webView.EnsureCoreWebView2Async();
            _webView.CoreWebView2.Navigate("https://www.google.com/maps/@0,0,2z");

            var navigate = () =>
            {
                if (double.TryParse(_latInput.Text, out var lat) && double.TryParse(_longInput.Text, out var lon))
                {
                    _webView.CoreWebView2.Navigate($"https://www.google.com/maps/@{lat},{lon},12z");
                }
            };

            _latInput.KeyDown += (_, e) => { if (e.KeyCode == Keys.Enter) navigate(); };
            _longInput.KeyDown += (_, e) => { if (e.KeyCode == Keys.Enter) navigate(); };

            var updateButton = () =>
            {
                _tellMeMoreButton.Enabled =
                    double.TryParse(_latInput.Text, out var lat) &&
                    double.TryParse(_longInput.Text, out var lon) &&
                    (lat != 0 || lon != 0);
            };
            _latInput.KeyUp += (_, _) => updateButton();
            _longInput.KeyUp += (_, _) => updateButton();

            _tellMeMoreButton.Click += (_, _) =>
            {
                if (double.TryParse(_latInput.Text, out var lat) && double.TryParse(_longInput.Text, out var lon))
                {
                    _ = AIMapWindow.GetInstance().AskAsync($"Tell me more about the first city found within a 5 mile radius of coordinates {lat}, {lon}.");
                }
            };

            _webView.CoreWebView2.SourceChanged += (_, _) =>
            {
                var src = _webView.Source.ToString();
                var atIdx = src.IndexOf("@");
                if (atIdx < 0) return;

                var after = src.Substring(atIdx + 1);
                var stopIdx = after.IndexOfAny(new[] { '!', '&', '#', '?' });
                if (stopIdx >= 0) after = after.Substring(0, stopIdx);

                var parts = after.Split(',');
                if (parts.Length < 2) return;

                if (double.TryParse(parts[0], out var lat) && double.TryParse(parts[1], out var lon))
                {
                    _latInput.Text = lat.ToString("F5");
                    _longInput.Text = lon.ToString("F5");
                    updateButton();
                }
            };
        }

        private static Region CreateRoundedRegion(Size size, int radius)
        {
            var path = new System.Drawing.Drawing2D.GraphicsPath();
            var d = radius * 2;
            path.AddArc(0, 0, d, d, 180, 90);
            path.AddArc(size.Width - d, 0, d, d, 270, 90);
            path.AddArc(size.Width - d, size.Height - d, d, d, 0, 90);
            path.AddArc(0, size.Height - d, d, d, 90, 90);
            path.CloseFigure();
            return new Region(path);
        }
    }
}
