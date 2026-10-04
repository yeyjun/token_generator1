using System.Xml.Serialization;
using TokenGenerator.Models;

namespace TokenGenerator.Services
{
    public class CalendarStore
    {
        static readonly string FilePath = Path.Combine(FileSystem.AppDataDirectory, "calendar.xml");
        static readonly XmlSerializer Serializer = new(typeof(List<CalendarEvent>));

        public List<CalendarEvent> Load()
        {
            if (!File.Exists(FilePath))
                return new List<CalendarEvent>();

            using var stream = File.OpenRead(FilePath);
            return (List<CalendarEvent>?)Serializer.Deserialize(stream) ?? new List<CalendarEvent>();
        }

        public void Save(List<CalendarEvent> events)
        {
            using var stream = File.Create(FilePath);
            Serializer.Serialize(stream, events);
        }
    }
}
