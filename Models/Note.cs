using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace TokenGenerator.Models
{
    public class Note : INotifyPropertyChanged
    {
        public int Id { get; set; } = Random.Shared.Next(1, int.MaxValue);

        string _title = string.Empty;
        string _content = string.Empty;
        string? _colorTag;
        bool _isPinned;
        DateTime? _reminderAt;
        string? _soundName;

        public string Title
        {
            get => _title;
            set => SetField(ref _title, value);
        }

        public string Content
        {
            get => _content;
            set => SetField(ref _content, value);
        }

        public string? ColorTag
        {
            get => _colorTag;
            set => SetField(ref _colorTag, value);
        }

        public bool IsPinned
        {
            get => _isPinned;
            set => SetField(ref _isPinned, value);
        }

        public DateTime? ReminderAt
        {
            get => _reminderAt;
            set => SetField(ref _reminderAt, value);
        }

        public string? SoundName
        {
            get => _soundName;
            set => SetField(ref _soundName, value);
        }

        public DateTime CreatedAt { get; set; } = DateTime.Now;

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
