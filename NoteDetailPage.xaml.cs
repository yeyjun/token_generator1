using CommunityToolkit.Maui.Views;
using TokenGenerator.Models;

namespace TokenGenerator
{
    public partial class NoteDetailPage : ContentPage
    {
        const string ContentHtmlTemplate = """
        <!DOCTYPE html>
        <html>
        <head>
        <meta name="viewport" content="width=device-width, initial-scale=1.0, user-scalable=no" />
        <style>
          html, body { margin:0; padding:0; }
          body { font-family: -apple-system, Segoe UI, Roboto, sans-serif; font-size: 16px; color: #1F2937; }
          u { text-decoration: underline; }
          hr { border: none; border-top: 1px solid #E2E8F0; margin: 12px 0; }
          h2 { font-size: 1.3em; margin: 0.4em 0; }
          ul { margin: 0.4em 0; padding-left: 1.4em; }
        </style>
        </head>
        <body>__CONTENT__</body>
        </html>
        """;

        readonly Note _note;
        readonly Action _onEdited;
        readonly Action _onDeleted;

        public NoteDetailPage(Note note, Action onEdited, Action onDeleted)
        {
            InitializeComponent();
            _note = note;
            _onEdited = onEdited;
            _onDeleted = onDeleted;

            RefreshLabels();
        }

        async void OnContentWebViewNavigated(object sender, WebNavigatedEventArgs e)
        {
            var result = await ContentWebView.EvaluateJavaScriptAsync("document.body.scrollHeight");
            if (double.TryParse(result, out var height) && height > 0)
                ContentWebView.HeightRequest = height;
        }

        void RefreshLabels()
        {
            TitleLabel.Text = _note.Title;
            DateLabel.Text = _note.CreatedAt.ToString("g");
            ContentWebView.Source = new HtmlWebViewSource { Html = ContentHtmlTemplate.Replace("__CONTENT__", _note.Content) };

            var hasColor = !string.IsNullOrWhiteSpace(_note.ColorTag);
            ColorSwatch.Color = hasColor ? Color.FromArgb(_note.ColorTag!) : Colors.Transparent;
            BackgroundColor = hasColor ? Color.FromArgb(_note.ColorTag!).WithAlpha(0.10f) : Colors.Transparent;

            PinnedLabel.IsVisible = _note.IsPinned;

            ReminderLabel.IsVisible = _note.ReminderAt is not null;
            ReminderLabel.Text = _note.ReminderAt is DateTime reminder
                ? $"Reminder: {reminder:g}"
                : string.Empty;
        }

        async void OnEditClicked(object sender, EventArgs e)
        {
            await Navigation.PushAsync(new NoteEditPage(_note, draft =>
            {
                _note.Title = draft.Title;
                _note.Content = draft.Content;
                _note.ColorTag = draft.ColorTag;
                _note.IsPinned = draft.IsPinned;
                _note.ReminderAt = draft.ReminderAt;
                _note.SoundName = draft.SoundName;
                RefreshLabels();
                _onEdited();
            }));
        }

        async void OnDeleteClicked(object sender, EventArgs e)
        {
            var confirmed = await DisplayAlert("Delete note", $"Delete \"{_note.Title}\"?", "Delete", "Cancel");
            if (!confirmed)
                return;

            _onDeleted();
            await Navigation.PopAsync();
        }
    }
}
