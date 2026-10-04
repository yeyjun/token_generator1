using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace TokenGenerator.Models
{
    public class DictionaryEntry : INotifyPropertyChanged
    {
        public required string Category { get; init; }
        public required string Term { get; init; }
        public string? Pronunciation { get; init; }
        public required string Definition { get; init; }
        public required string Example { get; init; }

        public string Id => $"{Category}:{Term}";

        public bool HasPronunciation => !string.IsNullOrEmpty(Pronunciation);

        public Color CategoryColor => Category switch
        {
            "Latin" => Color.FromArgb("#7C3AED"),
            "Legal" => Color.FromArgb("#DC2626"),
            "Spanish" => Color.FromArgb("#16A34A"),
            _ => Colors.Gray,
        };

        bool _isFavorite;
        public bool IsFavorite
        {
            get => _isFavorite;
            set
            {
                if (_isFavorite == value)
                    return;

                _isFavorite = value;
                OnPropertyChanged();
                OnPropertyChanged(nameof(FavoriteGlyph));
            }
        }

        public string FavoriteGlyph => IsFavorite ? "★" : "☆";

        public event PropertyChangedEventHandler? PropertyChanged;

        void OnPropertyChanged([CallerMemberName] string? name = null)
            => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
    }
}
