namespace TokenGenerator.Models
{
    public class SpotifyTrack
    {
        public string Id { get; set; } = string.Empty;
        public string Uri { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public string Artists { get; set; } = string.Empty;
        public string? AlbumImageUrl { get; set; }
    }
}
