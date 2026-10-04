using System.Globalization;

namespace TokenGenerator.Converters
{
    public class ColorHexToTintConverter : IValueConverter
    {
        public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
        {
            if (value is string hex && !string.IsNullOrWhiteSpace(hex))
                return Color.FromArgb(hex).WithAlpha(0.14f);

            return Colors.Transparent;
        }

        public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
            => throw new NotSupportedException();
    }
}
