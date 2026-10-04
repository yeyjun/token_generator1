using CommunityToolkit.Maui.Behaviors;
using Microsoft.Maui.Controls.Shapes;
using TokenGenerator.Services;

namespace TokenGenerator
{
    public partial class AppShell : Shell
    {
        static readonly Color Accent = Color.FromArgb("#8B6A52");
        static readonly Color BorderColor = Color.FromArgb("#EDE4DA");
        static readonly Color TextColor = Color.FromArgb("#3F2E22");
        static readonly Color SelectedBackground = Color.FromArgb("#F3E7DA");
        static readonly Color SelectedStroke = Color.FromArgb("#E3C9AC");

        static readonly (string Label, AppTheme Theme)[] ThemeOptions =
        {
            ("System", AppTheme.Unspecified),
            ("Light", AppTheme.Light),
            ("Dark", AppTheme.Dark),
        };

        record MenuEntry(string Title, string Icon, string? Route, bool IsGroup = false, List<MenuEntry>? Children = null);

        readonly List<MenuEntry> _menu = new()
        {
            new("Home", "icon_linux.svg", "HomePage"),
            new("Weather", "icon_weather.svg", "WeatherPage"),
            new("Speed Test", "icon_speedtest.svg", "SpeedTestPage"),
            new("Spotify", "icon_spotify.svg", "SpotifyPage"),
            new("Cipher", "icon_cipher.svg", "CaesarCipherPage"),
            new("Token", "icon_token.svg", "TokenGeneratorPage"),
            new("Game", "icon_game.svg", null, IsGroup: true, Children: new List<MenuEntry>
            {
                new("Word Puzzle", "icon_game.svg", "CipherGamePage"),
                new("Deal or No Deal", "icon_game.svg", "DealOrNoDealPage"),
                new("Hangaroo", "icon_game.svg", "HangarooPage"),
            }),
            new("Note", "icon_note.svg", "NotesPage"),
            new("Cal-e", "icon_calendar.svg", "CalePage"),
            new("Holy Rosary", "icon_rosary.svg", "RosaryPage"),
            new("Dictionary", "icon_dictionary.svg", "DictionaryPage"),
            new("LET Reviewer", "icon_reviewer.svg", "LETReviewerPage"),
            new("Tools", "icon_tools.svg", "ToolsPage"),
            new("Calculator", "icon_tools.svg", "CalculatorPage"),
        };

        readonly List<Border> _themeChips = new();
        string _selectedRoute = "HomePage";
        bool _gameExpanded = true;

        public AppShell()
        {
            InitializeComponent();
            BuildThemeChips();
            BuildFlyoutMenu();
        }

        void BuildFlyoutMenu()
        {
            MenuItemsLayout.Children.Clear();

            foreach (var entry in _menu)
            {
                MenuItemsLayout.Children.Add(BuildMenuRow(entry, isChild: false));

                if (entry.IsGroup && _gameExpanded)
                {
                    foreach (var child in entry.Children!)
                        MenuItemsLayout.Children.Add(BuildMenuRow(child, isChild: true));
                }
            }
        }

        Border BuildMenuRow(MenuEntry entry, bool isChild)
        {
            var isSelected = entry.Route is not null && entry.Route == _selectedRoute;

            var icon = new Image
            {
                Source = ImageSource.FromFile(entry.Icon),
                HeightRequest = 18,
                WidthRequest = 18,
                VerticalOptions = LayoutOptions.Center,
            };
            icon.Behaviors.Add(new IconTintColorBehavior { TintColor = Accent });

            var label = new Label
            {
                Text = entry.Title,
                FontSize = isChild ? 13 : 14,
                TextColor = TextColor,
                VerticalOptions = LayoutOptions.Center,
            };

            var grid = new Grid
            {
                ColumnDefinitions =
                {
                    new ColumnDefinition(GridLength.Auto),
                    new ColumnDefinition(GridLength.Star),
                    new ColumnDefinition(GridLength.Auto),
                },
                ColumnSpacing = 12,
            };
            grid.Add(icon, 0);
            grid.Add(label, 1);

            if (entry.IsGroup)
            {
                var chevron = new Label
                {
                    Text = _gameExpanded ? "▾" : "▸",
                    FontSize = 12,
                    TextColor = Accent,
                    VerticalOptions = LayoutOptions.Center,
                };
                grid.Add(chevron, 2);
            }

            var border = new Border
            {
                Margin = isChild ? new Thickness(30, 2, 10, 2) : new Thickness(10, 3, 10, 3),
                Padding = new Thickness(14, isChild ? 8 : 10),
                StrokeShape = new RoundRectangle { CornerRadius = 10 },
                StrokeThickness = 1,
                Stroke = isSelected ? SelectedStroke : Colors.Transparent,
                BackgroundColor = isSelected ? SelectedBackground : Colors.Transparent,
                Content = grid,
            };
            border.GestureRecognizers.Add(new TapGestureRecognizer
            {
                Command = new Command(() => OnMenuRowTapped(entry)),
            });

            return border;
        }

        async void OnMenuRowTapped(MenuEntry entry)
        {
            if (entry.IsGroup)
            {
                _gameExpanded = !_gameExpanded;
                BuildFlyoutMenu();
                return;
            }

            if (entry.Route is null)
                return;

            _selectedRoute = entry.Route;
            BuildFlyoutMenu();
            FlyoutIsPresented = false;
            await Shell.Current.GoToAsync($"//{entry.Route}");
        }

        void BuildThemeChips()
        {
            foreach (var (label, theme) in ThemeOptions)
            {
                var chip = new Border
                {
                    Padding = new Thickness(10, 5),
                    StrokeShape = new RoundRectangle { CornerRadius = 12 },
                    Stroke = BorderColor,
                    StrokeThickness = 1,
                    BackgroundColor = Colors.Transparent,
                    ClassId = theme.ToString(),
                    Content = new Label { Text = label, FontSize = 11, FontAttributes = FontAttributes.Bold, TextColor = TextColor }
                };
                chip.GestureRecognizers.Add(new TapGestureRecognizer
                {
                    Command = new Command(() => SelectTheme(theme))
                });

                _themeChips.Add(chip);
                ThemeOptionRow.Children.Add(chip);
            }

            HighlightSelectedChip();
        }

        void SelectTheme(AppTheme theme)
        {
            ThemeService.SetTheme(theme);
            HighlightSelectedChip();
        }

        void HighlightSelectedChip()
        {
            foreach (var chip in _themeChips)
            {
                var isSelected = chip.ClassId == ThemeService.Current.ToString();
                chip.BackgroundColor = isSelected ? Accent : Colors.Transparent;
                chip.Stroke = isSelected ? Accent : BorderColor;
                if (chip.Content is Label label)
                    label.TextColor = isSelected ? Colors.White : TextColor;
            }
        }
    }
}
