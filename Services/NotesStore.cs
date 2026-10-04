using System.Xml.Serialization;
using TokenGenerator.Models;

namespace TokenGenerator.Services
{
    public class NotesStore
    {
        static readonly string FilePath = Path.Combine(FileSystem.AppDataDirectory, "notes.xml");
        static readonly XmlSerializer Serializer = new(typeof(List<Note>));

        public List<Note> Load()
        {
            if (!File.Exists(FilePath))
                return new List<Note>();

            using var stream = File.OpenRead(FilePath);
            return (List<Note>?)Serializer.Deserialize(stream) ?? new List<Note>();
        }

        public void Save(List<Note> notes)
        {
            using var stream = File.Create(FilePath);
            Serializer.Serialize(stream, notes);
        }
    }
}
