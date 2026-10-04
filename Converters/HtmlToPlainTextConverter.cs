using System.Globalization;
using System.Net;
using System.Text.RegularExpressions;

namespace TokenGenerator.Converters
{
    public class HtmlToPlainTextConverter : IValueConverter
    {
        public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
        {
            if (value is not string html || string.IsNullOrWhiteSpace(html))
                return string.Empty;

            var text = Regex.Replace(html, "<[^>]*>", " ");
            text = WebUtility.HtmlDecode(text);
            return Regex.Replace(text, @"\s+", " ").Trim();
        }

        public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
            => throw new NotSupportedException();
    }
}
