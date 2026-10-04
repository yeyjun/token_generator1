using TokenGenerator.Services;

namespace TokenGenerator
{
    public partial class WeatherPage : ContentPage
    {
        const double DefaultLatitude = 14.5995;
        const double DefaultLongitude = 120.9842;

        public WeatherPage()
        {
            InitializeComponent();
        }

        protected override async void OnAppearing()
        {
            base.OnAppearing();
            await RefreshAsync();
        }

        async void OnMyLocationClicked(object sender, EventArgs e) => await RefreshAsync();

        async Task RefreshAsync()
        {
            LocationLabel.Text = "Locating...";
            ConditionLabel.Text = string.Empty;
            WeatherIconLabel.Text = "⏳";
            TemperatureLabel.Text = string.Empty;

            var location = await LocationService.GetCurrentLocationAsync();
            var latitude = location?.Latitude ?? DefaultLatitude;
            var longitude = location?.Longitude ?? DefaultLongitude;

            LocationLabel.Text = location is not null
                ? await LocationService.DescribePlaceAsync(location)
                : "Manila, Philippines (default)";

            LoadWindyMap(latitude, longitude);

            var weather = await WeatherService.FetchAsync(latitude, longitude);
            if (weather is null)
            {
                WeatherIconLabel.Text = "⚠️";
                ConditionLabel.Text = "Weather unavailable";
                return;
            }

            WeatherIconLabel.Text = WeatherService.DescribeIcon(weather.WeatherCode, weather.IsDay);
            ConditionLabel.Text = WeatherService.DescribeCode(weather.WeatherCode);
            TemperatureLabel.Text = $"{weather.Temperature:F1}°C";
        }

        void LoadWindyMap(double latitude, double longitude)
        {
            var url =
                "https://embed.windy.com/embed2.html" +
                $"?lat={latitude:F4}&lon={longitude:F4}" +
                $"&detailLat={latitude:F4}&detailLon={longitude:F4}" +
                "&zoom=8&level=surface&overlay=wind&product=ecmwf" +
                "&menu=&message=true&marker=true&calendar=now&pressure=&type=map" +
                "&location=coordinates&detail=&metricWind=default&metricTemp=default&radarRange=-1";

            WindyWebView.Source = new UrlWebViewSource { Url = url };
        }
    }
}
