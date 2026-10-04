using System.Diagnostics;

namespace TokenGenerator.Services
{
    public record SpeedTestResult(double PingMs, double DownloadMbps, double UploadMbps);

    public static class SpeedTestService
    {
        static readonly HttpClient _httpClient = new() { Timeout = TimeSpan.FromSeconds(30) };

        const string PingUrl = "https://speed.cloudflare.com/__down?bytes=0";
        const string DownloadUrl = "https://speed.cloudflare.com/__down?bytes=25000000";
        const string UploadUrl = "https://speed.cloudflare.com/__up";
        const int UploadBytes = 5_000_000;

        public static async Task<double> MeasurePingAsync(CancellationToken token)
        {
            var samples = new List<double>();

            for (int i = 0; i < 4; i++)
            {
                var sw = Stopwatch.StartNew();
                using var response = await _httpClient.GetAsync(PingUrl, HttpCompletionOption.ResponseHeadersRead, token);
                sw.Stop();

                if (response.IsSuccessStatusCode)
                    samples.Add(sw.Elapsed.TotalMilliseconds);
            }

            if (samples.Count == 0)
                throw new IOException("Ping test failed: no successful responses.");

            return samples.Min();
        }

        public static async Task<double> MeasureDownloadAsync(IProgress<double>? progress, CancellationToken token)
        {
            var sw = Stopwatch.StartNew();
            using var response = await _httpClient.GetAsync(DownloadUrl, HttpCompletionOption.ResponseHeadersRead, token);
            response.EnsureSuccessStatusCode();

            var total = response.Content.Headers.ContentLength ?? 25_000_000;
            var buffer = new byte[81920];
            long bytesRead = 0;

            using var stream = await response.Content.ReadAsStreamAsync(token);
            int read;
            while ((read = await stream.ReadAsync(buffer, token)) > 0)
            {
                bytesRead += read;
                progress?.Report(Math.Min(1.0, (double)bytesRead / total));
            }

            sw.Stop();
            var seconds = sw.Elapsed.TotalSeconds;
            return seconds <= 0 ? 0 : bytesRead * 8.0 / seconds / 1_000_000.0;
        }

        public static async Task<double> MeasureUploadAsync(IProgress<double>? progress, CancellationToken token)
        {
            var payload = new byte[UploadBytes];
            Random.Shared.NextBytes(payload);

            using var content = new ProgressableContent(payload, progress);
            var sw = Stopwatch.StartNew();
            using var response = await _httpClient.PostAsync(UploadUrl, content, token);
            sw.Stop();
            response.EnsureSuccessStatusCode();

            var seconds = sw.Elapsed.TotalSeconds;
            return seconds <= 0 ? 0 : payload.Length * 8.0 / seconds / 1_000_000.0;
        }

        class ProgressableContent : ByteArrayContent
        {
            readonly byte[] _buffer;
            readonly IProgress<double>? _progress;

            public ProgressableContent(byte[] content, IProgress<double>? progress) : base(content)
            {
                _buffer = content;
                _progress = progress;
            }

            protected override async Task SerializeToStreamAsync(Stream stream, System.Net.TransportContext? context, CancellationToken cancellationToken)
            {
                const int chunkSize = 65536;
                int sent = 0;

                while (sent < _buffer.Length)
                {
                    var count = Math.Min(chunkSize, _buffer.Length - sent);
                    await stream.WriteAsync(_buffer.AsMemory(sent, count), cancellationToken);
                    sent += count;
                    _progress?.Report(Math.Min(1.0, (double)sent / _buffer.Length));
                }
            }
        }
    }
}
