namespace TokenGenerator.Models
{
    public class CalendarDayCell
    {
        public DateTime Date { get; set; }
        public int Day { get; set; }
        public bool HasEvents { get; set; }
        public Color CellColor { get; set; } = Colors.Transparent;
        public Color TextColor { get; set; } = Colors.Black;
        public FontAttributes FontAttr { get; set; } = FontAttributes.None;
    }
}
