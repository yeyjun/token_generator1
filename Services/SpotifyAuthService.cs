using System.Net.Http.Json;
using System.Security.Cryptography;
using System.Text;

namespace TokenGenerator.Services
{
    public static class SpotifyAuthService
    {
        // Create an app at https://developer.spotify.com/dashboard, then paste its Client ID here.
        // Register this exact redirect URI on that app: tokengenerator://callback
        public const string ClientId = "1d6f30d574884b3e899f1192cc630ee9";

        const string RedirectUri = "tokengenerator://callback";
        const string Scopes = "user-read-playback-state user-modify-playback-state user-read-currently-playing";

        const string AccessTokenKey = "spotify_access_token";
        const string RefreshTokenKey = "spotify_refresh_token";
        const string ExpiresAtKey = "spotify_expires_at";

        static readonly HttpClient _httpClient = new();

        public static bool IsConfigured => !string.IsNullOrWhiteSpace(ClientId) && ClientId != "YOUR_SPOTIFY_CLIENT_ID";

        public static async Task<bool> LoginAsync()
        {
            var verifier = GenerateCodeVerifier();
            var challenge = GenerateCodeChallenge(verifier);

            var authUrl = "https://accounts.spotify.com/authorize" +
                $"?client_id={Uri.EscapeDataString(ClientId)}" +
                "&response_type=code" +
                $"&redirect_uri={Uri.EscapeDataString(RedirectUri)}" +
                "&code_challenge_method=S256" +
                $"&code_challenge={challenge}" +
                $"&scope={Uri.EscapeDataString(Scopes)}";

            WebAuthenticatorResult authResult;
            try
            {
                authResult = await WebAuthenticator.AuthenticateAsync(new WebAuthenticatorOptions
                {
                    Url = new Uri(authUrl),
                    CallbackUrl = new Uri(RedirectUri),
                });
            }
            catch (TaskCanceledException)
            {
                return false;
            }

            if (!authResult.Properties.TryGetValue("code", out var code) || string.IsNullOrEmpty(code))
                return false;

            return await ExchangeCodeAsync(code, verifier);
        }

        public static async Task<bool> IsLoggedInAsync() => await GetAccessTokenAsync() is not null;

        public static void Logout()
        {
            SecureStorage.Default.Remove(AccessTokenKey);
            SecureStorage.Default.Remove(RefreshTokenKey);
            SecureStorage.Default.Remove(ExpiresAtKey);
        }

        public static async Task<string?> GetAccessTokenAsync()
        {
            var accessToken = await SecureStorage.Default.GetAsync(AccessTokenKey);
            if (string.IsNullOrEmpty(accessToken))
                return null;

            var expiresAtRaw = await SecureStorage.Default.GetAsync(ExpiresAtKey);
            if (DateTimeOffset.TryParse(expiresAtRaw, out var expiresAt) && expiresAt > DateTimeOffset.UtcNow.AddSeconds(30))
                return accessToken;

            return await RefreshAsync();
        }

        static async Task<bool> ExchangeCodeAsync(string code, string verifier)
        {
            var request = new FormUrlEncodedContent(new Dictionary<string, string>
            {
                ["grant_type"] = "authorization_code",
                ["code"] = code,
                ["redirect_uri"] = RedirectUri,
                ["client_id"] = ClientId,
                ["code_verifier"] = verifier,
            });

            var response = await _httpClient.PostAsync("https://accounts.spotify.com/api/token", request);
            if (!response.IsSuccessStatusCode)
                return false;

            var payload = await response.Content.ReadFromJsonAsync<TokenResponse>();
            if (payload is null)
                return false;

            await StoreTokensAsync(payload);
            return true;
        }

        static async Task<string?> RefreshAsync()
        {
            var refreshToken = await SecureStorage.Default.GetAsync(RefreshTokenKey);
            if (string.IsNullOrEmpty(refreshToken))
                return null;

            var request = new FormUrlEncodedContent(new Dictionary<string, string>
            {
                ["grant_type"] = "refresh_token",
                ["refresh_token"] = refreshToken,
                ["client_id"] = ClientId,
            });

            var response = await _httpClient.PostAsync("https://accounts.spotify.com/api/token", request);
            if (!response.IsSuccessStatusCode)
                return null;

            var payload = await response.Content.ReadFromJsonAsync<TokenResponse>();
            if (payload is null)
                return null;

            payload.refresh_token ??= refreshToken;
            await StoreTokensAsync(payload);
            return payload.access_token;
        }

        static async Task StoreTokensAsync(TokenResponse payload)
        {
            await SecureStorage.Default.SetAsync(AccessTokenKey, payload.access_token);

            if (!string.IsNullOrEmpty(payload.refresh_token))
                await SecureStorage.Default.SetAsync(RefreshTokenKey, payload.refresh_token);

            var expiresAt = DateTimeOffset.UtcNow.AddSeconds(payload.expires_in);
            await SecureStorage.Default.SetAsync(ExpiresAtKey, expiresAt.ToString("O"));
        }

        static string GenerateCodeVerifier() => Base64UrlEncode(RandomNumberGenerator.GetBytes(64));

        static string GenerateCodeChallenge(string verifier) =>
            Base64UrlEncode(SHA256.HashData(Encoding.ASCII.GetBytes(verifier)));

        static string Base64UrlEncode(byte[] bytes) =>
            Convert.ToBase64String(bytes).Replace('+', '-').Replace('/', '_').TrimEnd('=');

        class TokenResponse
        {
            public string access_token { get; set; } = string.Empty;
            public string? refresh_token { get; set; }
            public int expires_in { get; set; }
        }
    }
}
