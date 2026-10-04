using TokenGenerator.Models;
using TokenGenerator.Services;

namespace TokenGenerator
{
    public partial class SpotifyPage : ContentPage
    {
        SpotifyTrack? _currentTrack;

        public SpotifyPage()
        {
            InitializeComponent();
        }

        protected override async void OnAppearing()
        {
            base.OnAppearing();
            await RefreshAuthStateAsync();
        }

        async Task RefreshAuthStateAsync()
        {
            if (!SpotifyAuthService.IsConfigured)
            {
                ConnectLayout.IsVisible = true;
                MainLayout.IsVisible = false;
                ConnectButton.IsEnabled = false;
                ConnectStatusLabel.Text = "Spotify Client ID is not configured yet. Set SpotifyAuthService.ClientId first.";
                ConnectStatusLabel.IsVisible = true;
                return;
            }

            var loggedIn = await SpotifyAuthService.IsLoggedInAsync();
            ConnectLayout.IsVisible = !loggedIn;
            MainLayout.IsVisible = loggedIn;

            if (loggedIn)
                await RefreshNowPlayingAsync();
        }

        async void OnConnectClicked(object sender, EventArgs e)
        {
            ConnectButton.IsEnabled = false;
            ConnectStatusLabel.IsVisible = false;

            try
            {
                var success = await SpotifyAuthService.LoginAsync();
                if (!success)
                {
                    ConnectStatusLabel.Text = "Login was cancelled or failed. Please try again.";
                    ConnectStatusLabel.IsVisible = true;
                    return;
                }

                await RefreshAuthStateAsync();
            }
            catch (Exception ex)
            {
                ConnectStatusLabel.Text = $"Login failed: {ex.Message}";
                ConnectStatusLabel.IsVisible = true;
            }
            finally
            {
                ConnectButton.IsEnabled = true;
            }
        }

        async void OnSearchPressed(object sender, EventArgs e)
        {
            var query = SearchBox.Text?.Trim();
            if (string.IsNullOrEmpty(query))
                return;

            try
            {
                ResultsList.ItemsSource = await SpotifyService.SearchTracksAsync(query);
            }
            catch (Exception ex)
            {
                await DisplayAlert("Search failed", ex.Message, "OK");
            }
        }

        async void OnPlayTrackClicked(object sender, EventArgs e)
        {
            if ((sender as Button)?.BindingContext is not SpotifyTrack track)
                return;

            try
            {
                await SpotifyService.PlayAsync(track.Uri);
                _currentTrack = track;
                ShowNowPlaying(track, isPlaying: true);
            }
            catch (Exception ex)
            {
                await DisplayAlert("Playback failed", ex.Message, "OK");
            }
        }

        async void OnPlayPauseClicked(object sender, EventArgs e)
        {
            try
            {
                if (PlayPauseButton.Text == "⏸")
                {
                    await SpotifyService.PauseAsync();
                    PlayPauseButton.Text = "▶";
                }
                else
                {
                    if (_currentTrack is not null)
                        await SpotifyService.PlayAsync(_currentTrack.Uri);
                    PlayPauseButton.Text = "⏸";
                }
            }
            catch (Exception ex)
            {
                await DisplayAlert("Playback failed", ex.Message, "OK");
            }
        }

        async void OnNextClicked(object sender, EventArgs e)
        {
            try
            {
                await SpotifyService.NextAsync();
                await RefreshNowPlayingAsync();
            }
            catch (Exception ex)
            {
                await DisplayAlert("Playback failed", ex.Message, "OK");
            }
        }

        async void OnPreviousClicked(object sender, EventArgs e)
        {
            try
            {
                await SpotifyService.PreviousAsync();
                await RefreshNowPlayingAsync();
            }
            catch (Exception ex)
            {
                await DisplayAlert("Playback failed", ex.Message, "OK");
            }
        }

        async Task RefreshNowPlayingAsync()
        {
            try
            {
                var current = await SpotifyService.GetCurrentlyPlayingAsync();
                if (current is null)
                {
                    NowPlayingBar.IsVisible = false;
                    return;
                }

                _currentTrack = current.Track;
                ShowNowPlaying(current.Track, current.IsPlaying);
            }
            catch
            {
                NowPlayingBar.IsVisible = false;
            }
        }

        void ShowNowPlaying(SpotifyTrack track, bool isPlaying)
        {
            NowPlayingTitle.Text = track.Name;
            NowPlayingArtist.Text = track.Artists;
            PlayPauseButton.Text = isPlaying ? "⏸" : "▶";
            NowPlayingBar.IsVisible = true;
        }
    }
}
