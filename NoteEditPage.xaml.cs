using System.Text.Json;
using System.Text.RegularExpressions;
using CommunityToolkit.Maui.Views;
using TokenGenerator.Models;

namespace TokenGenerator
{
    public partial class NoteEditPage : ContentPage
    {
        const string EditorHtmlTemplate = """
        <!DOCTYPE html>
        <html>
        <head>
        <meta name="viewport" content="width=device-width, initial-scale=1.0, user-scalable=no" />
        <style>
          html, body { margin:0; padding:0; height:100%; }
          body { font-family: -apple-system, Segoe UI, Roboto, sans-serif; }
          #editor { min-height: 100%; padding: 8px; font-size: 16px; color: #1F2937; outline: none; box-sizing: border-box; }
          #editor:empty:before { content: attr(data-placeholder); color: #9CA3AF; }
          #editor u { text-decoration: underline; }
          #editor hr { border: none; border-top: 1px solid #E2E8F0; margin: 12px 0; }
          #editor h2 { font-size: 1.3em; margin: 0.4em 0; }
          #editor ul { margin: 0.4em 0; padding-left: 1.4em; }
        </style>
        </head>
        <body>
        <div id="editor" contenteditable="true" data-placeholder="Write your note...">__CONTENT__</div>
        <script>
          var editor = document.getElementById('editor');
          var lastRange = null;
          document.addEventListener('selectionchange', function () {
            var sel = window.getSelection();
            if (sel.rangeCount > 0 && editor.contains(sel.anchorNode)) {
              lastRange = sel.getRangeAt(0).cloneRange();
            }
          });
          function restoreSelection() {
            editor.focus();
            var sel = window.getSelection();
            sel.removeAllRanges();
            if (lastRange) {
              sel.addRange(lastRange);
            } else {
              var range = document.createRange();
              range.selectNodeContents(editor);
              range.collapse(false);
              sel.addRange(range);
            }
          }
        </script>
        </body>
        </html>
        """;

        readonly Action<NoteDraft> _onSave;
        string? _selectedColor;
        DateTime? _reminderAt;
        string? _soundName;
        bool _isPinned;
        bool _ribbonExpanded;

        public NoteEditPage(Note? existing, Action<NoteDraft> onSave)
        {
            InitializeComponent();
            _onSave = onSave;

            Title = existing is null ? "New note" : "Edit note";
            TitleEntry.Text = existing?.Title;
            ContentWebView.Source = new HtmlWebViewSource { Html = EditorHtmlTemplate.Replace("__CONTENT__", existing?.Content ?? string.Empty) };
            _selectedColor = existing?.ColorTag;
            _reminderAt = existing?.ReminderAt;
            _soundName = existing?.SoundName;
            _isPinned = existing?.IsPinned ?? false;
            UpdatePinIcon();
        }

        void OnPinClicked(object sender, EventArgs e)
        {
            _isPinned = !_isPinned;
            UpdatePinIcon();
        }

        void UpdatePinIcon() => PinToolbarItem.Text = _isPinned ? "★" : "☆";

        void OnRibbonToggleTapped(object sender, TappedEventArgs e)
        {
            _ribbonExpanded = !_ribbonExpanded;
            RibbonToolsScroll.IsVisible = _ribbonExpanded;
            RibbonToggleIcon.Text = _ribbonExpanded ? "▲" : "▾";
        }

        async void OnBoldClicked(object sender, EventArgs e) => await RunEditorCommandAsync("document.execCommand('bold');");

        async void OnItalicClicked(object sender, EventArgs e) => await RunEditorCommandAsync("document.execCommand('italic');");

        async void OnUnderlineClicked(object sender, EventArgs e) => await RunEditorCommandAsync("document.execCommand('underline');");

        async void OnBulletClicked(object sender, EventArgs e) => await RunEditorCommandAsync("document.execCommand('insertUnorderedList');");

        async void OnChecklistClicked(object sender, EventArgs e) => await RunEditorCommandAsync("document.execCommand('insertHTML', false, '<div>\\u2610&nbsp;</div>');");

        async void OnHeadingClicked(object sender, EventArgs e) => await RunEditorCommandAsync("document.execCommand('formatBlock', false, 'H2');");

        async void OnDividerClicked(object sender, EventArgs e) => await RunEditorCommandAsync("document.execCommand('insertHTML', false, '<hr>');");

        async void OnTimestampClicked(object sender, EventArgs e)
        {
            var timestamp = JsonSerializer.Serialize(DateTime.Now.ToString("g"));
            await RunEditorCommandAsync($"document.execCommand('insertText', false, {timestamp});");
        }

        async Task RunEditorCommandAsync(string script)
            => await ContentWebView.EvaluateJavaScriptAsync($"restoreSelection(); {script}");

        async Task<string> GetEditorContentAsync()
        {
            var encoded = await ContentWebView.EvaluateJavaScriptAsync(
                "encodeURIComponent(document.getElementById('editor').innerHTML)");
            return string.IsNullOrEmpty(encoded) ? string.Empty : Uri.UnescapeDataString(encoded);
        }

        static bool IsEffectivelyEmpty(string html)
            => string.IsNullOrWhiteSpace(Regex.Replace(html, "<[^>]*>", string.Empty));

        async void OnSettingsClicked(object sender, EventArgs e)
        {
            var popup = new NoteSettingsPopup(_selectedColor, _reminderAt, _soundName);
            var result = await this.ShowPopupAsync(popup, CancellationToken.None);

            if (result is not NoteSettingsResult settings)
                return;

            _selectedColor = settings.ColorTag;
            _reminderAt = settings.ReminderAt;
            _soundName = settings.SoundName;
        }

        async void OnSaveClicked(object sender, EventArgs e)
        {
            var title = TitleEntry.Text?.Trim();
            var content = await GetEditorContentAsync();

            if (!string.IsNullOrEmpty(title) || !IsEffectivelyEmpty(content))
            {
                _onSave(new NoteDraft(
                    string.IsNullOrEmpty(title) ? "Untitled" : title,
                    content,
                    _selectedColor,
                    _isPinned,
                    _reminderAt,
                    _soundName));
            }

            await Navigation.PopAsync();
        }
    }

    public record NoteDraft(string Title, string Content, string? ColorTag, bool IsPinned, DateTime? ReminderAt, string? SoundName);
}
