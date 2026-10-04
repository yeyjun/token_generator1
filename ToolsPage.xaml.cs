namespace TokenGenerator
{
    public partial class ToolsPage : ContentPage
    {
        CancellationTokenSource? _cts;

        public ToolsPage()
        {
            InitializeComponent();
        }

        protected override void OnDisappearing()
        {
            base.OnDisappearing();
            StopTracking();
        }

        async void OnToggleClicked(object sender, EventArgs e)
        {
            if (_cts is not null)
            {
                StopTracking();
                return;
            }

            var status = await Permissions.RequestAsync<Permissions.LocationWhenInUse>();
            if (status != PermissionStatus.Granted)
            {
                StatusLabel.Text = "Location permission denied.";
                return;
            }

            _cts = new CancellationTokenSource();
            ToggleButton.Text = "Stop";
            StatusLabel.Text = "Tracking speed...";
            _ = TrackSpeedAsync(_cts.Token);
        }

        void StopTracking()
        {
            _cts?.Cancel();
            _cts = null;
            ToggleButton.Text = "Start";
            StatusLabel.Text = "Tap Start to track your current speed via GPS.";
            SpeedLabel.Text = "—";
        }

        async Task TrackSpeedAsync(CancellationToken token)
        {
            while (!token.IsCancellationRequested)
            {
                try
                {
                    var location = await Geolocation.Default.GetLocationAsync(
                        new GeolocationRequest(GeolocationAccuracy.Best, TimeSpan.FromSeconds(5)), token);

                    if (token.IsCancellationRequested)
                        return;

                    if (location?.Speed is double metersPerSecond)
                    {
                        var kmh = Math.Max(0, metersPerSecond) * 3.6;
                        SpeedLabel.Text = kmh.ToString("F0");
                        StatusLabel.Text = $"Updated {DateTime.Now:T}";
                    }
                    else
                    {
                        SpeedLabel.Text = "—";
                        StatusLabel.Text = "Waiting for GPS speed data...";
                    }
                }
                catch (Exception ex) when (ex is not OperationCanceledException)
                {
                    StatusLabel.Text = $"Unavailable: {ex.Message}";
                }

                try
                {
                    await Task.Delay(TimeSpan.FromSeconds(2), token);
                }
                catch (TaskCanceledException)
                {
                    return;
                }
            }
        }
    }
}
