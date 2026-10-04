using CommunityToolkit.Maui.Views;
using Microsoft.Maui.Controls.Shapes;
using TokenGenerator.Models;

namespace TokenGenerator
{
    enum ReminderMode { None, In30Min, In1Hour, Tomorrow9Am, Custom }

    public record NoteSettingsResult(string? ColorTag, DateTime? ReminderAt, string? SoundName);

    public class NoteSettingsPopup : Popup
    {
        static readonly Color Accent = Color.FromArgb("#7C3AED");
        static readonly Color Heading = Color.FromArgb("#0F172A");
        static readonly Color Caption = Color.FromArgb("#64748B");
        static readonly Color BorderColor = Color.FromArgb("#E2E8F0");
        static readonly Color FieldText = Color.FromArgb("#334155");

        static readonly string?[] ColorPalette = { null, "#F87171", "#FBBF24", "#34D399", "#60A5FA", "#A78BFA" };

        static readonly (string Label, ReminderMode Mode)[] ReminderChips =
        {
            ("None", ReminderMode.None),
            ("30 min", ReminderMode.In30Min),
            ("1 hour", ReminderMode.In1Hour),
            ("Tomorrow 9 AM", ReminderMode.Tomorrow9Am),
            ("Custom", ReminderMode.Custom),
        };

        readonly List<Border> _colorSwatches = new();
        readonly List<Border> _reminderChipViews = new();
        readonly List<Border> _soundChipViews = new();
        readonly HorizontalStackLayout _colorSwatchRow = new() { Spacing = 10 };
        readonly HorizontalStackLayout _reminderChipRow = new() { Spacing = 8 };
        readonly HorizontalStackLayout _soundChipRow = new() { Spacing = 8 };
        readonly HorizontalStackLayout _customReminderRow;
        readonly DatePicker _reminderDatePicker = new() { TextColor = FieldText, FontSize = 13 };
        readonly TimePicker _reminderTimePicker = new() { TextColor = FieldText, FontSize = 13 };
        readonly Label _pastReminderWarning = new()
        {
            Text = "This time is in the past — the reminder won't fire.",
            FontSize = 11,
            TextColor = Color.FromArgb("#DC2626"),
            IsVisible = false
        };

        string? _selectedColor;
        ReminderMode _reminderMode = ReminderMode.None;
        string? _selectedSoundKey;

        public NoteSettingsPopup(string? selectedColor, DateTime? reminderAt, string? selectedSound)
        {
            Color = Colors.Transparent;

            _customReminderRow = new HorizontalStackLayout
            {
                Spacing = 10,
                IsVisible = false,
                Children =
                {
                    FieldFrame(_reminderDatePicker),
                    FieldFrame(_reminderTimePicker)
                }
            };

            _reminderDatePicker.DateSelected += (_, _) => UpdatePastReminderWarning();
            _reminderTimePicker.PropertyChanged += (_, e) =>
            {
                if (e.PropertyName == nameof(TimePicker.Time))
                    UpdatePastReminderWarning();
            };

            var cancelButton = new Button
            {
                Text = "Cancel",
                BackgroundColor = Colors.Transparent,
                TextColor = Caption,
                FontAttributes = FontAttributes.Bold,
                FontSize = 14
            };
            cancelButton.Clicked += async (_, _) => await CloseAsync(null, CancellationToken.None);

            var doneButton = new Button
            {
                Text = "Done",
                BackgroundColor = Accent,
                TextColor = Colors.White,
                FontAttributes = FontAttributes.Bold,
                FontSize = 14,
                CornerRadius = 10,
                Padding = new Thickness(22, 10)
            };
            doneButton.Clicked += async (_, _) => await CloseAsync(new NoteSettingsResult(_selectedColor, ResolveReminder(), _selectedSoundKey), CancellationToken.None);

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
                        new VerticalStackLayout
                        {
                            Spacing = 2,
                            Children =
                            {
                                new Label { Text = "Note settings", FontSize = 18, FontAttributes = FontAttributes.Bold, TextColor = Heading },
                                new Label { Text = "Choose a theme and set a reminder.", FontSize = 12, TextColor = Caption }
                            }
                        },

                        Section("THEME", _colorSwatchRow),
                        Section("REMINDER", new VerticalStackLayout { Spacing = 10, Children = { _reminderChipRow, _customReminderRow, _pastReminderWarning } }),
                        Section("REMINDER SOUND", _soundChipRow),

                        new BoxView { HeightRequest = 1, Color = BorderColor },

                        new HorizontalStackLayout
                        {
                            Spacing = 12,
                            HorizontalOptions = LayoutOptions.End,
                            Children = { cancelButton, doneButton }
                        }
                    }
                }
            };

            BuildColorSwatches(selectedColor);
            BuildReminderChips(reminderAt);
            BuildSoundChips(selectedSound);
        }

        static VerticalStackLayout Section(string title, View content) => new()
        {
            Spacing = 8,
            Children =
            {
                new Label
                {
                    Text = title,
                    FontSize = 11,
                    FontAttributes = FontAttributes.Bold,
                    CharacterSpacing = 1.2,
                    TextColor = Caption
                },
                content
            }
        };

        static Border FieldFrame(View field) => new()
        {
            Padding = new Thickness(10, 4),
            StrokeShape = new RoundRectangle { CornerRadius = 8 },
            Stroke = BorderColor,
            StrokeThickness = 1,
            BackgroundColor = Color.FromArgb("#F8FAFC"),
            Content = field
        };

        void BuildColorSwatches(string? selected)
        {
            _selectedColor = selected;

            foreach (var hex in ColorPalette)
            {
                var swatch = new Border
                {
                    WidthRequest = 28,
                    HeightRequest = 28,
                    StrokeShape = new RoundRectangle { CornerRadius = 14 },
                    BackgroundColor = hex is null ? Colors.White : Color.FromArgb(hex),
                    Stroke = BorderColor,
                    StrokeThickness = 1,
                    ClassId = hex ?? string.Empty,
                    Shadow = new Shadow { Brush = Colors.Black, Opacity = 0.10f, Radius = 4, Offset = new Point(0, 2) }
                };
                swatch.GestureRecognizers.Add(new TapGestureRecognizer
                {
                    Command = new Command(() => SelectColor(hex))
                });

                _colorSwatches.Add(swatch);
                _colorSwatchRow.Children.Add(swatch);
            }

            HighlightSelectedSwatch();
        }

        void SelectColor(string? hex)
        {
            _selectedColor = hex;
            HighlightSelectedSwatch();
        }

        void HighlightSelectedSwatch()
        {
            foreach (var swatch in _colorSwatches)
            {
                var isSelected = swatch.ClassId == (_selectedColor ?? string.Empty);
                swatch.Stroke = isSelected ? Accent : BorderColor;
                swatch.StrokeThickness = isSelected ? 3 : 1;
            }
        }

        void BuildReminderChips(DateTime? existingReminder)
        {
            foreach (var (label, mode) in ReminderChips)
            {
                var chip = new Border
                {
                    Padding = new Thickness(12, 7),
                    StrokeShape = new RoundRectangle { CornerRadius = 16 },
                    Stroke = BorderColor,
                    StrokeThickness = 1,
                    BackgroundColor = Colors.White,
                    ClassId = mode.ToString(),
                    Content = new Label { Text = label, FontSize = 12, FontAttributes = FontAttributes.Bold, TextColor = FieldText }
                };
                chip.GestureRecognizers.Add(new TapGestureRecognizer
                {
                    Command = new Command(() => SelectReminderMode(mode))
                });

                _reminderChipViews.Add(chip);
                _reminderChipRow.Children.Add(chip);
            }

            _reminderMode = existingReminder is null ? ReminderMode.None : ReminderMode.Custom;
            var initial = existingReminder ?? DateTime.Now.AddMinutes(30);
            _reminderDatePicker.Date = initial.Date;
            _reminderTimePicker.Time = initial.TimeOfDay;

            UpdateReminderChipHighlight();
            _customReminderRow.IsVisible = _reminderMode == ReminderMode.Custom;
            UpdatePastReminderWarning();
        }

        void SelectReminderMode(ReminderMode mode)
        {
            _reminderMode = mode;
            UpdateReminderChipHighlight();
            _customReminderRow.IsVisible = mode == ReminderMode.Custom;
            UpdatePastReminderWarning();
        }

        void UpdatePastReminderWarning()
        {
            _pastReminderWarning.IsVisible = _reminderMode == ReminderMode.Custom
                && _reminderDatePicker.Date + _reminderTimePicker.Time <= DateTime.Now;
        }

        void UpdateReminderChipHighlight()
        {
            foreach (var chip in _reminderChipViews)
            {
                var isSelected = chip.ClassId == _reminderMode.ToString();
                chip.BackgroundColor = isSelected ? Accent : Colors.White;
                chip.Stroke = isSelected ? Accent : BorderColor;
                if (chip.Content is Label label)
                    label.TextColor = isSelected ? Colors.White : FieldText;
            }
        }

        void BuildSoundChips(string? selectedSound)
        {
            _selectedSoundKey = ReminderSounds.Find(selectedSound).Key;

            foreach (var sound in ReminderSounds.All)
            {
                var chip = new Border
                {
                    Padding = new Thickness(12, 7),
                    StrokeShape = new RoundRectangle { CornerRadius = 16 },
                    Stroke = BorderColor,
                    StrokeThickness = 1,
                    BackgroundColor = Colors.White,
                    ClassId = sound.Key ?? string.Empty,
                    Content = new Label { Text = sound.Label, FontSize = 12, FontAttributes = FontAttributes.Bold, TextColor = FieldText }
                };
                chip.GestureRecognizers.Add(new TapGestureRecognizer
                {
                    Command = new Command(() => SelectSound(sound.Key))
                });

                _soundChipViews.Add(chip);
                _soundChipRow.Children.Add(chip);
            }

            UpdateSoundChipHighlight();
        }

        void SelectSound(string? key)
        {
            _selectedSoundKey = key;
            UpdateSoundChipHighlight();
        }

        void UpdateSoundChipHighlight()
        {
            foreach (var chip in _soundChipViews)
            {
                var isSelected = chip.ClassId == (_selectedSoundKey ?? string.Empty);
                chip.BackgroundColor = isSelected ? Accent : Colors.White;
                chip.Stroke = isSelected ? Accent : BorderColor;
                if (chip.Content is Label label)
                    label.TextColor = isSelected ? Colors.White : FieldText;
            }
        }

        DateTime? ResolveReminder() => _reminderMode switch
        {
            ReminderMode.None => null,
            ReminderMode.In30Min => DateTime.Now.AddMinutes(30),
            ReminderMode.In1Hour => DateTime.Now.AddHours(1),
            ReminderMode.Tomorrow9Am => DateTime.Today.AddDays(1).AddHours(9),
            ReminderMode.Custom => _reminderDatePicker.Date + _reminderTimePicker.Time,
            _ => null
        };
    }
}
