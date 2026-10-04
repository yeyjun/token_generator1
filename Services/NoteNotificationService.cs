using Plugin.LocalNotification;
using Plugin.LocalNotification.AndroidOption;
using TokenGenerator.Models;

namespace TokenGenerator.Services
{
    public static class NoteNotificationService
    {
        static bool _channelsReady;

        public static async Task SyncAsync(Note note)
        {
            EnsureChannels();

            LocalNotificationCenter.Current.Cancel(note.Id);

            if (note.ReminderAt is not DateTime reminderAt || reminderAt <= DateTime.Now)
                return;

            var sound = ReminderSounds.Find(note.SoundName);

            var request = new NotificationRequest
            {
                NotificationId = note.Id,
                Title = "Note reminder",
                Description = note.Title,
                Schedule = new NotificationRequestSchedule { NotifyTime = reminderAt }
            };

            if (sound.Key is not null)
            {
                request.Sound = sound.IosFileName;
                request.Android.ChannelId = ChannelId(sound);
            }

            await LocalNotificationCenter.Current.Show(request);
        }

        public static void Cancel(Note note) => LocalNotificationCenter.Current.Cancel(note.Id);

        static void EnsureChannels()
        {
            if (_channelsReady)
                return;

            _channelsReady = true;

#if ANDROID
            var channels = ReminderSounds.All
                .Where(sound => sound.Key is not null)
                .Select(sound => new NotificationChannelRequest
                {
                    Id = ChannelId(sound),
                    Name = $"Note reminders ({sound.Label})",
                    Sound = sound.AndroidFileName,
                    EnableSound = true
                })
                .ToList();

            LocalNotificationCenter.CreateNotificationChannels(channels);
#endif
        }

        static string ChannelId(ReminderSound sound) => $"note_reminder_{sound.Key}";
    }
}
