using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json.Serialization;
using TokenGenerator.Models;

namespace TokenGenerator.Services
{
    public record CurrentlyPlaying(SpotifyTrack Track, bool IsPlaying);

    public static class SpotifyService
    {
        static readonly HttpClient _httpClient = new() { BaseAddress = new Uri("https://api.spotify.com/v1/") };

        public static async Task<List<SpotifyTrack>> SearchTracksAsync(string query)
        {
            var request = await AuthorizedRequestAsync(HttpMethod.Get, $"search?type=track&limit=20&q={Uri.EscapeDataString(query)}");
            var response = await _httpClient.SendAsync(request);
            response.EnsureSuccessStatusCode();

            var result = await response.Content.ReadFromJsonAsync<SearchResponse>();
            return result?.Tracks?.Items?.Select(MapTrack).ToList() ?? new List<SpotifyTrack>();
        }

        public static async Task PlayAsync(string trackUri)
        {
            var request = await AuthorizedRequestAsync(HttpMethod.Put, "me/player/play");
            request.Content = JsonContent.Create(new { uris = new[] { trackUri } });
            var response = await _httpClient.SendAsync(request);
            EnsurePlaybackSuccess(response);
        }

        public static async Task PauseAsync()
        {
            var request = await AuthorizedRequestAsync(HttpMethod.Put, "me/player/pause");
            var response = await _httpClient.SendAsync(request);
            EnsurePlaybackSuccess(response);
        }

        public static async Task NextAsync()
        {
            var request = await AuthorizedRequestAsync(HttpMethod.Post, "me/player/next");
            var response = await _httpClient.SendAsync(request);
            EnsurePlaybackSuccess(response);
        }

        public static async Task PreviousAsync()
        {
            var request = await AuthorizedRequestAsync(HttpMethod.Post, "me/player/previous");
            var response = await _httpClient.SendAsync(request);
            EnsurePlaybackSuccess(response);
        }

        public static async Task<CurrentlyPlaying?> GetCurrentlyPlayingAsync()
        {
            var request = await AuthorizedRequestAsync(HttpMethod.Get, "me/player/currently-playing");
            var response = await _httpClient.SendAsync(request);

            if (response.StatusCode == HttpStatusCode.NoContent || !response.IsSuccessStatusCode)
                return null;

            var payload = await response.Content.ReadFromJsonAsync<CurrentlyPlayingResponse>();
            if (payload?.Item is null)
                return null;

            return new CurrentlyPlaying(MapTrack(payload.Item), payload.IsPlaying);
        }

        static async Task<HttpRequestMessage> AuthorizedRequestAsync(HttpMethod method, string url)
        {
            var token = await SpotifyAuthService.GetAccessTokenAsync();
            var request = new HttpRequestMessage(method, url);
            if (token is not null)
                request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);
            return request;
        }

        static void EnsurePlaybackSuccess(HttpResponseMessage response)
        {
            if (response.IsSuccessStatusCode)
                return;

            if (response.StatusCode == HttpStatusCode.NotFound)
                throw new InvalidOperationException("No active Spotify device found. Open Spotify on a device and start playing something first.");

            if (response.StatusCode == HttpStatusCode.Forbidden)
                throw new InvalidOperationException("Playback control requires a Spotify Premium account.");

            throw new InvalidOperationException($"Spotify request failed: {(int)response.StatusCode}");
        }

        static SpotifyTrack MapTrack(TrackDto track) => new()
        {
            Id = track.Id,
            Uri = track.Uri,
            Name = track.Name,
            Artists = string.Join(", ", track.Artists?.Select(a => a.Name) ?? Enumerable.Empty<string>()),
            AlbumImageUrl = track.Album?.Images?.FirstOrDefault()?.Url,
        };

        class SearchResponse
        {
            [JsonPropertyName("tracks")] public TracksContainer? Tracks { get; set; }
        }

        class TracksContainer
        {
            [JsonPropertyName("items")] public List<TrackDto>? Items { get; set; }
        }

        class TrackDto
        {
            [JsonPropertyName("id")] public string Id { get; set; } = string.Empty;
            [JsonPropertyName("uri")] public string Uri { get; set; } = string.Empty;
            [JsonPropertyName("name")] public string Name { get; set; } = string.Empty;
            [JsonPropertyName("artists")] public List<ArtistDto>? Artists { get; set; }
            [JsonPropertyName("album")] public AlbumDto? Album { get; set; }
        }

        class ArtistDto
        {
            [JsonPropertyName("name")] public string Name { get; set; } = string.Empty;
        }

        class AlbumDto
        {
            [JsonPropertyName("images")] public List<ImageDto>? Images { get; set; }
        }

        class ImageDto
        {
            [JsonPropertyName("url")] public string Url { get; set; } = string.Empty;
        }

        class CurrentlyPlayingResponse
        {
            [JsonPropertyName("is_playing")] public bool IsPlaying { get; set; }
            [JsonPropertyName("item")] public TrackDto? Item { get; set; }
        }
    }
}
