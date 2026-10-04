using CommunityToolkit.Maui.Views;
using TokenGenerator.Services;

namespace TokenGenerator
{
    public partial class HomePage : ContentPage
    {
        const string PenguinSvgTemplate = """
        <!DOCTYPE html>
        <html>
        <head>
        <meta name="viewport" content="width=device-width, initial-scale=1.0, user-scalable=no" />
        <style>
          html, body { margin:0; padding:0; height:100%; background:transparent; }
        </style>
        </head>
        <body>
        <svg xmlns="http://www.w3.org/2000/svg" viewBox="0 0 200 240">
          <ellipse cx="78" cy="222" rx="22" ry="10" fill="#F4A400" />
          <ellipse cx="122" cy="222" rx="22" ry="10" fill="#F4A400" />
          <path d="M100 12 C 60 12 44 56 44 100 C 44 140 40 176 34 200 C 30 214 40 226 56 226 L 144 226 C 160 226 170 214 166 200 C 160 176 156 140 156 100 C 156 56 140 12 100 12 Z" fill="__COLOR__" />
          <path d="M100 46 C 76 46 66 76 66 108 C 66 142 63 172 58 196 C 55 208 62 216 74 216 L 126 216 C 138 216 145 208 142 196 C 137 172 134 142 134 108 C 134 76 124 46 100 46 Z" fill="#FFFFFF" />
          <circle cx="82" cy="80" r="11" fill="#FFFFFF" />
          <circle cx="118" cy="80" r="11" fill="#FFFFFF" />
          <circle cx="84" cy="82" r="4.5" fill="__COLOR__" />
          <circle cx="116" cy="82" r="4.5" fill="__COLOR__" />
          <path d="M90 96 L110 96 L100 112 Z" fill="#F4A400" />
          <path d="M44 90 C 24 100 18 140 30 168 C 40 158 48 130 50 100 Z" fill="__COLOR__" />
          <path d="M156 90 C 176 100 182 140 170 168 C 160 158 152 130 150 100 Z" fill="__COLOR__" />
        </svg>
        </body>
        </html>
        """;

        const string ClockSvgTemplate = """
        <!DOCTYPE html>
        <html>
        <head>
        <meta name="viewport" content="width=device-width, initial-scale=1.0, user-scalable=no" />
        <style>
          html, body { margin:0; padding:0; height:100%; background:transparent; }
        </style>
        </head>
        <body>
        <svg xmlns="http://www.w3.org/2000/svg" viewBox="0 0 200 200">
          <circle cx="100" cy="100" r="92" fill="#FFFFFF" stroke="__COLOR__" stroke-width="6" />
          <g stroke="__COLOR__" stroke-width="4" stroke-linecap="round">
            <line x1="100" y1="14" x2="100" y2="28" />
            <line x1="100" y1="172" x2="100" y2="186" />
            <line x1="14" y1="100" x2="28" y2="100" />
            <line x1="172" y1="100" x2="186" y2="100" />
          </g>
          <line x1="100" y1="100" x2="100" y2="56" stroke="__COLOR__" stroke-width="7" stroke-linecap="round" transform="rotate(__HOUR__ 100 100)" />
          <line x1="100" y1="100" x2="100" y2="34" stroke="__COLOR__" stroke-width="5" stroke-linecap="round" transform="rotate(__MINUTE__ 100 100)" />
          <line x1="100" y1="100" x2="100" y2="26" stroke="#F4A400" stroke-width="2" stroke-linecap="round" transform="rotate(__SECOND__ 100 100)" />
          <circle cx="100" cy="100" r="6" fill="__COLOR__" />
        </svg>
        </body>
        </html>
        """;

        string _penguinColor = "#0B0B0B";
        bool _isClockMode;
        IDispatcherTimer? _clockTimer;

        public HomePage()
        {
            InitializeComponent();
            ApplyPenguinColor(_penguinColor);
            DateTimeLabel.GestureRecognizers.Add(new TapGestureRecognizer { Command = new Command(ToggleClockMode) });
        }

        protected override async void OnAppearing()
        {
            base.OnAppearing();

            DateTimeLabel.Text = DateTime.Now.ToString("dddd, MMMM d, yyyy – h:mm tt");
            ShowMysteryOfTheDay();
            await LoadLocationAndWeatherAsync();
        }

        void ShowMysteryOfTheDay()
        {
            var mysterySet = RosaryService.GetMysterySetForDay(DateTime.Now.DayOfWeek);
            var mysteries = RosaryService.GetMysteries(mysterySet);

            MysterySetLabel.Text = mysterySet;
            MysteryListLabel.Text = string.Join("\n", mysteries.Select((m, i) => $"{i + 1}. {m}"));
        }

        async void OnMysteryCardTapped(object sender, EventArgs e)
        {
            await Shell.Current.GoToAsync("//RosaryPage");
        }

        protected override void OnDisappearing()
        {
            base.OnDisappearing();
            StopClockTimer();
        }

        void ToggleClockMode()
        {
            _isClockMode = !_isClockMode;

            if (_isClockMode)
            {
                ApplyAnalogClock(DateTime.Now);
                _clockTimer ??= Dispatcher.CreateTimer();
                _clockTimer.Interval = TimeSpan.FromSeconds(1);
                _clockTimer.Tick += OnClockTimerTick;
                _clockTimer.Start();
            }
            else
            {
                StopClockTimer();
                ApplyPenguinColor(_penguinColor);
            }
        }

        void OnClockTimerTick(object? sender, EventArgs e) => ApplyAnalogClock(DateTime.Now);

        void StopClockTimer()
        {
            if (_clockTimer is null)
                return;

            _clockTimer.Stop();
            _clockTimer.Tick -= OnClockTimerTick;
        }

        void ApplyAnalogClock(DateTime now)
        {
            var seconds = now.Second + now.Millisecond / 1000.0;
            var minutes = now.Minute + seconds / 60.0;
            var hours = now.Hour % 12 + minutes / 60.0;

            var html = ClockSvgTemplate
                .Replace("__COLOR__", _penguinColor)
                .Replace("__HOUR__", (hours * 30).ToString("F1"))
                .Replace("__MINUTE__", (minutes * 6).ToString("F1"))
                .Replace("__SECOND__", (seconds * 6).ToString("F1"));

            PenguinWebView.Source = new HtmlWebViewSource { Html = html };
        }

        void ApplyPenguinColor(string hex)
        {
            _penguinColor = hex;

            if (_isClockMode)
                ApplyAnalogClock(DateTime.Now);
            else
                PenguinWebView.Source = new HtmlWebViewSource { Html = PenguinSvgTemplate.Replace("__COLOR__", hex) };
        }

        async Task LoadLocationAndWeatherAsync()
        {
            LocationLabel.Text = "Location: locating...";
            WeatherIconLabel.Text = "⏳";
            WeatherTempLabel.Text = string.Empty;

            try
            {
                var location = await LocationService.GetCurrentLocationAsync();
                if (location is null)
                {
                    LocationLabel.Text = "Location: unavailable";
                    WeatherIconLabel.Text = "⚠️";
                    return;
                }

                LocationLabel.Text = $"Location: {await LocationService.DescribePlaceAsync(location)}";

                var weather = await WeatherService.FetchAsync(location.Latitude, location.Longitude);
                if (weather is null)
                {
                    WeatherIconLabel.Text = "⚠️";
                    return;
                }

                WeatherIconLabel.Text = WeatherService.DescribeIcon(weather.WeatherCode, weather.IsDay);
                SemanticProperties.SetDescription(WeatherIconLabel, WeatherService.DescribeCode(weather.WeatherCode));
                WeatherTempLabel.Text = $"{weather.Temperature:F1}°C";
                ApplyWeatherTheme(weather.WeatherCode, weather.IsDay);
            }
            catch (Exception ex)
            {
                LocationLabel.Text = "Location: unavailable";
                WeatherIconLabel.Text = "⚠️";
                WeatherTempLabel.Text = ex.Message;
            }
        }

        void ApplyWeatherTheme(int weatherCode, bool isDay)
        {
            var (start, end, isDark, penguinColor) = weatherCode switch
            {
                0 => isDay ? ("#56CCF2", "#2F80ED", false, "#1F6FEB") : ("#0F2027", "#203A43", true, "#0B1F3A"),
                1 or 2 or 3 => isDay ? ("#E6E9F0", "#A8B4C4", false, "#5B6670") : ("#232526", "#414345", true, "#3B4650"),
                45 or 48 => isDay ? ("#D7D2CC", "#93A5B1", false, "#8D99A6") : ("#232526", "#1C1C1C", true, "#5C6670"),
                51 or 53 or 55 or 56 or 57 => isDay ? ("#89A9C2", "#4B79A1", false, "#2C3E50") : ("#1C1C1C", "#283E51", true, "#1B2A38"),
                61 or 63 or 65 or 66 or 67 or 80 or 81 or 82 => ("#283E51", "#1C1C1C", true, "#1B2A38"),
                71 or 73 or 75 or 77 or 85 or 86 => isDay ? ("#83A4D4", "#B6FBFF", false, "#3A6EA5") : ("#2C3E50", "#4CA1AF", true, "#2E5A87"),
                95 or 96 or 99 => ("#141E30", "#243B55", true, "#4B0082"),
                _ => isDay ? ("#FFFFFF", "#E9ECEF", false, "#0B0B0B") : ("#232526", "#0D0D0D", true, "#0B0B0B")
            };

            Background = new LinearGradientBrush
            {
                StartPoint = new Point(0, 0),
                EndPoint = new Point(0, 1),
                GradientStops =
                {
                    new GradientStop(Color.FromArgb(start), 0),
                    new GradientStop(Color.FromArgb(end), 1)
                }
            };

            var foreground = isDark ? Colors.White : Color.FromArgb("#3F2E22");
            TitleLabel.TextColor = foreground;
            DateTimeLabel.TextColor = foreground;
            LocationLabel.TextColor = foreground.WithAlpha(0.75f);
            WeatherTempLabel.TextColor = foreground.WithAlpha(0.75f);
            MysterySetLabel.TextColor = foreground;
            MysteryListLabel.TextColor = foreground.WithAlpha(0.75f);

            ApplyPenguinColor(penguinColor);
        }

    }
}
