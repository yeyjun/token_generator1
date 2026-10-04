using CommunityToolkit.Maui.Views;
using Microsoft.Maui.Controls.Shapes;
using TokenGenerator.Models;

namespace TokenGenerator
{
    public record CalEventResult(string Title, DateTime Date, TimeSpan StartTime, string Notes, bool ReminderEnabled);

    public class CalEventPopup : Popup
    {
        static readonly Color Accent = Color.FromArgb("#7C3AED");
        static readonly Color Heading = Color.FromArgb("#0F172A");
        static readonly Color Caption = Color.FromArgb("#64748B");
        static readonly Color BorderColor = Color.FromArgb("#E2E8F0");
        static readonly Color FieldText = Color.FromArgb("#334155");

        readonly Entry _titleEntry = new() { Placeholder = "Event title", TextColor = FieldText, FontSize = 14 };
        readonly Editor _notesEditor = new() { Placeholder = "Notes (optional)", TextColor = FieldText, FontSize = 13, HeightRequest = 70, AutoSize = EditorAutoSizeOption.Disabled };
        readonly DatePicker _datePicker = new() { TextColor = FieldText, FontSize = 13 };
        readonly TimePicker _timePicker = new() { TextColor = FieldText, FontSize = 13 };
        readonly Switch _reminderSwitch = new() { OnColor = Accent };

        public CalEventPopup(CalendarEvent? existing, DateTime defaultDate)
        {
            Color = Colors.Transparent;

            _titleEntry.Text = existing?.Title;
            _notesEditor.Text = existing?.Notes;
            _datePicker.Date = existing?.Date ?? defaultDate;
            _timePicker.Time = existing?.StartTime ?? new TimeSpan(9, 0, 0);
            _reminderSwitch.IsToggled = existing?.ReminderEnabled ?? false;

            var cancelButton = new Button
            {
                Text = "Cancel",
                BackgroundColor = Colors.Transparent,
                TextColor = Caption,
                FontAttributes = FontAttributes.Bold,
                FontSize = 14
            };
            cancelButton.Clicked += async (_, _) => await CloseAsync(null, CancellationToken.None);

            var saveButton = new Button
            {
                Text = existing is null ? "Add" : "Save",
                BackgroundColor = Accent,
                TextColor = Colors.White,
                FontAttributes = FontAttributes.Bold,
                FontSize = 14,
                CornerRadius = 10,
                Padding = new Thickness(22, 10)
            };
            saveButton.Clicked += async (_, _) =>
            {
                var title = _titleEntry.Text?.Trim();
                if (string.IsNullOrEmpty(title))
                    return;

                await CloseAsync(
                    new CalEventResult(title, _datePicker.Date, _timePicker.Time, _notesEditor.Text?.Trim() ?? string.Empty, _reminderSwitch.IsToggled),
                    CancellationToken.None);
            };

            Content = new Border
            {
                Padding = new Thickness(22),
                StrokeShape = new RoundRectangle { CornerRadius = 20 },
                Stroke = Colors.Transparent,
                BackgroundColor = Colors.White,
                Shadow = new Shadow { Brush = Colors.Black, Opacity = 0.18f, Radius = 24, Offset = new Point(0, 10) },
                Content = new VerticalStackLayout
                {
                    Spacing = 16,
                    WidthRequest = 290,
                    Children =
                    {
                        new Label
                        {
                            Text = existing is null ? "New event" : "Edit event",
                            FontSize = 18,
                            FontAttributes = FontAttributes.Bold,
                            TextColor = Heading
                        },

                        FieldFrame(_titleEntry),
                        FieldFrame(_notesEditor),

                        new HorizontalStackLayout
                        {
                            Spacing = 10,
                            Children = { FieldFrame(_datePicker), FieldFrame(_timePicker) }
                        },

                        new HorizontalStackLayout
                        {
                            Spacing = 10,
                            Children =
                            {
                                _reminderSwitch,
                                new Label { Text = "Remind me", FontSize = 13, TextColor = FieldText, VerticalOptions = LayoutOptions.Center }
                            }
                        },

                        new BoxView { HeightRequest = 1, Color = BorderColor },

                        new HorizontalStackLayout
                        {
                            Spacing = 12,
                            HorizontalOptions = LayoutOptions.End,
                            Children = { cancelButton, saveButton }
                        }
                    }
                }
            };
        }

        static Border FieldFrame(View field) => new()
        {
            Padding = new Thickness(10, 4),
            StrokeShape = new RoundRectangle { CornerRadius = 8 },
            Stroke = BorderColor,
            StrokeThickness = 1,
            BackgroundColor = Color.FromArgb("#F8FAFC"),
            Content = field
        };
    }
}
