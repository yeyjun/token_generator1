using System.Xml.Serialization;
using TokenGenerator.Models;

namespace TokenGenerator.Services
{
    public class CipherOfflineStore
    {
        static readonly string FilePath = Path.Combine(FileSystem.AppDataDirectory, "cipher_pending.xml");
        static readonly XmlSerializer Serializer = new(typeof(List<PendingCipherEntry>));

        public List<PendingCipherEntry> Load()
        {
            if (!File.Exists(FilePath))
                return new List<PendingCipherEntry>();

            using var stream = File.OpenRead(FilePath);
            return (List<PendingCipherEntry>?)Serializer.Deserialize(stream) ?? new List<PendingCipherEntry>();
        }

        public void Save(List<PendingCipherEntry> entries)
        {
            using var stream = File.Create(FilePath);
            Serializer.Serialize(stream, entries);
        }

        public void Add(PendingCipherEntry entry)
        {
            var entries = Load();
            entries.Add(entry);
            Save(entries);
        }

        public bool Contains(string content) => Load().Any(e => e.Content == content);
    }
}
