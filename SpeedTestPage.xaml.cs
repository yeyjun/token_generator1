using TokenGenerator.Services;

namespace TokenGenerator
{
    public partial class SpeedTestPage : ContentPage
    {
        CancellationTokenSource? _cts;

        public SpeedTestPage()
        {
            InitializeComponent();
        }

        protected override void OnDisappearing()
        {
            base.OnDisappearing();
            _cts?.Cancel();
            _cts = null;
        }

        async void OnStartClicked(object sender, EventArgs e)
        {
            if (_cts is not null)
                return;

            _cts = new CancellationTokenSource();
            var token = _cts.Token;

            StartButton.IsEnabled = false;
            PingLabel.Text = "—";
            DownloadLabel.Text = "—";
            UploadLabel.Text = "—";
            TestProgressBar.Progress = 0;

            try
            {
                StatusLabel.Text = "Measuring ping...";
                var ping = await SpeedTestService.MeasurePingAsync(token);
                PingLabel.Text = ping.ToString("F0");

                StatusLabel.Text = "Testing download speed...";
                var downloadProgress = new Progress<double>(p => TestProgressBar.Progress = p);
                var download = await SpeedTestService.MeasureDownloadAsync(downloadProgress, token);
                DownloadLabel.Text = download.ToString("F1");

                TestProgressBar.Progress = 0;
                StatusLabel.Text = "Testing upload speed...";
                var uploadProgress = new Progress<double>(p => TestProgressBar.Progress = p);
                var upload = await SpeedTestService.MeasureUploadAsync(uploadProgress, token);
                UploadLabel.Text = upload.ToString("F1");

                TestProgressBar.Progress = 1;
                StatusLabel.Text = $"Done. {DateTime.Now:T}";
            }
            catch (OperationCanceledException)
            {
                StatusLabel.Text = "Test cancelled.";
            }
            catch (Exception ex)
            {
                StatusLabel.Text = $"Test failed: {ex.Message}";
            }
            finally
            {
                StartButton.IsEnabled = true;
                _cts = null;
            }
        }
    }
}
