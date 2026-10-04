namespace TokenGenerator.Models
{
    public class PendingCipherEntry
    {
        public string Content { get; set; } = string.Empty;
        public string CipherText { get; set; } = string.Empty;
        public int CipherShift { get; set; }
        public string Source { get; set; } = "manual";
        public DateTime CreatedAt { get; set; } = DateTime.Now;
    }
}
