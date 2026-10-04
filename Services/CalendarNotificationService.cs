using Plugin.LocalNotification;
using TokenGenerator.Models;

namespace TokenGenerator.Services
{
    public static class CalendarNotificationService
    {
        const int IdOffset = 100_000_000;

        public static async Task SyncAsync(CalendarEvent calendarEvent)
        {
            var notificationId = IdOffset + (calendarEvent.Id % IdOffset);
            LocalNotificationCenter.Current.Cancel(notificationId);

            if (!calendarEvent.ReminderEnabled || calendarEvent.StartDateTime <= DateTime.Now)
                return;

            await LocalNotificationCenter.Current.Show(new NotificationRequest
            {
                NotificationId = notificationId,
                Title = "Event reminder",
                Description = calendarEvent.Title,
                Schedule = new NotificationRequestSchedule { NotifyTime = calendarEvent.StartDateTime }
            });
        }

        public static void Cancel(CalendarEvent calendarEvent)
            => LocalNotificationCenter.Current.Cancel(IdOffset + (calendarEvent.Id % IdOffset));
    }
}
