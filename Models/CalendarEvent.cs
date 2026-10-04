using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace TokenGenerator.Models
{
    public class CalendarEvent : INotifyPropertyChanged
    {
        public int Id { get; set; } = Random.Shared.Next(1, int.MaxValue);

        DateTime _date = DateTime.Today;
        TimeSpan _startTime = new(9, 0, 0);
        string _title = string.Empty;
        string _notes = string.Empty;
        bool _reminderEnabled;

        public DateTime Date
        {
            get => _date;
            set => SetField(ref _date, value.Date);
        }

        public TimeSpan StartTime
        {
            get => _startTime;
            set => SetField(ref _startTime, value);
        }

        public string Title
        {
            get => _title;
            set => SetField(ref _title, value);
        }

        public string Notes
        {
            get => _notes;
            set => SetField(ref _notes, value);
        }

        public bool ReminderEnabled
        {
            get => _reminderEnabled;
            set => SetField(ref _reminderEnabled, value);
        }

        public DateTime StartDateTime => Date + StartTime;

        public string TimeLabel => DateTime.Today.Add(StartTime).ToString("h:mm tt");

        public event PropertyChangedEventHandler? PropertyChanged;

        void SetField<T>(ref T field, T value, [CallerMemberName] string? propertyName = null)
        {
            if (EqualityComparer<T>.Default.Equals(field, value))
                return;

            field = value;
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
