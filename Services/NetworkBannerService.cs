using System.Runtime.CompilerServices;
using Microsoft.Maui.Controls.Shapes;

namespace TokenGenerator.Services
{
    public static class NetworkBannerService
    {
        static readonly Label MessageLabel = new()
        {
            Text = "You're offline. Changes will be saved locally.",
            TextColor = Colors.White,
            FontAttributes = FontAttributes.Bold,
            FontSize = 13,
            HorizontalTextAlignment = TextAlignment.Center,
        };

        static readonly Border Banner = new()
        {
            BackgroundColor = Color.FromArgb("#B3261E"),
            Padding = new Thickness(16, 10),
            HorizontalOptions = LayoutOptions.Fill,
            VerticalOptions = LayoutOptions.Start,
            StrokeShape = new RoundRectangle { CornerRadius = new CornerRadius(0, 0, 14, 14) },
            StrokeThickness = 0,
            ZIndex = 999,
            IsVisible = false,
            TranslationY = -80,
            Content = MessageLabel,
        };

        static readonly ConditionalWeakTable<ContentPage, Grid> Hosts = new();

        static Shell? _shell;
        static Grid? _bannerParent;
        static bool _isOffline;
        static bool _isAnimating;

        public static void Initialize(Shell shell)
        {
            _shell = shell;
            _isOffline = Connectivity.Current.NetworkAccess != NetworkAccess.Internet;

            Connectivity.Current.ConnectivityChanged += OnConnectivityChanged;
            shell.Navigated += (_, _) => AttachToCurrentPage();

            AttachToCurrentPage();
        }

        static void AttachToCurrentPage()
        {
            if (_shell?.CurrentPage is not ContentPage page || page.Content is null)
            {
                return;
            }

            if (!Hosts.TryGetValue(page, out var host))
            {
                var content = page.Content;
                host = new Grid();
                host.Add(content);
                page.Content = host;
                Hosts.Add(page, host);
            }

            if (ReferenceEquals(_bannerParent, host))
            {
                return;
            }

            _bannerParent?.Remove(Banner);
            host.Add(Banner);
            _bannerParent = host;

            Banner.TranslationY = _isOffline ? 0 : -80;
            Banner.IsVisible = _isOffline;
        }

        static async void OnConnectivityChanged(object? sender, ConnectivityChangedEventArgs e)
        {
            var offline = e.NetworkAccess != NetworkAccess.Internet;
            if (offline == _isOffline || _isAnimating)
            {
                return;
            }

            _isOffline = offline;
            _isAnimating = true;

            try
            {
                await MainThread.InvokeOnMainThreadAsync(async () =>
                {
                    if (offline)
                    {
                        Banner.IsVisible = true;
                        await Banner.TranslateTo(0, 0, 250, Easing.CubicOut);
                    }
                    else
                    {
                        await Banner.TranslateTo(0, -80, 250, Easing.CubicIn);
                        Banner.IsVisible = false;
                    }
                });
            }
            finally
            {
                _isAnimating = false;
            }
        }
    }
}
