namespace TokenGenerator.Models
{
    public record ReminderSound(string? Key, string Label, string AndroidFileName, string IosFileName);

    public static class ReminderSounds
    {
        public static readonly ReminderSound Default = new(null, "Default", string.Empty, string.Empty);
        public static readonly ReminderSound Alarm = new("alarm", "Alarm", "alarm", "alarm.wav");

        public static readonly IReadOnlyList<ReminderSound> All = new[] { Default, Alarm };

        public static ReminderSound Find(string? key) => All.FirstOrDefault(s => s.Key == key) ?? Default;
    }
}
