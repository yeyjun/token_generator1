using CommunityToolkit.Maui.Views;
using Plugin.LocalNotification;
using TokenGenerator.Models;
using TokenGenerator.Services;

namespace TokenGenerator
{
    public partial class CalePage : ContentPage
    {
        static readonly Color TodayColor = Color.FromArgb("#EDE4FF");
        static readonly Color SelectedColor = Color.FromArgb("#7C3AED");
        static readonly Color CurrentMonthText = Color.FromArgb("#1F2937");
        static readonly Color OtherMonthText = Color.FromArgb("#C4C4C4");

        readonly CalendarStore _store = new();
        List<CalendarEvent> _events = new();
        DateTime _displayedMonth = new(DateTime.Today.Year, DateTime.Today.Month, 1);
        DateTime _selectedDate = DateTime.Today;
        bool _notificationsSynced;

        public CalePage()
        {
            InitializeComponent();
            _events = _store.Load();
            RenderMonth();
        }

        protected override async void OnAppearing()
        {
            base.OnAppearing();

            if (_notificationsSynced)
                return;

            _notificationsSynced = true;
            await LocalNotificationCenter.Current.RequestNotificationPermission();
            foreach (var calendarEvent in _events)
                await CalendarNotificationService.SyncAsync(calendarEvent);
        }

        void OnPrevMonthClicked(object sender, EventArgs e)
        {
            _displayedMonth = _displayedMonth.AddMonths(-1);
            RenderMonth();
        }

        void OnNextMonthClicked(object sender, EventArgs e)
        {
            _displayedMonth = _displayedMonth.AddMonths(1);
            RenderMonth();
        }

        void OnDayTapped(object sender, TappedEventArgs e)
        {
            if (sender is not BindableObject { BindingContext: CalendarDayCell cell })
                return;

            _selectedDate = cell.Date;

            if (cell.Date.Year != _displayedMonth.Year || cell.Date.Month != _displayedMonth.Month)
                _displayedMonth = new DateTime(cell.Date.Year, cell.Date.Month, 1);

            RenderMonth();
        }

        async void OnAddEventClicked(object sender, EventArgs e)
        {
            var popup = new CalEventPopup(null, _selectedDate);
            var result = await this.ShowPopupAsync(popup, CancellationToken.None);

            if (result is not CalEventResult draft)
                return;

            var calendarEvent = new CalendarEvent
            {
                Title = draft.Title,
                Date = draft.Date,
                StartTime = draft.StartTime,
                Notes = draft.Notes,
                ReminderEnabled = draft.ReminderEnabled
            };

            _events.Add(calendarEvent);
            _selectedDate = calendarEvent.Date;
            _displayedMonth = new DateTime(calendarEvent.Date.Year, calendarEvent.Date.Month, 1);
            _store.Save(_events);
            RenderMonth();
            await CalendarNotificationService.SyncAsync(calendarEvent);
        }

        async void OnEditEventClicked(object sender, EventArgs e)
        {
            if (sender is not Button { BindingContext: CalendarEvent calendarEvent })
                return;

            var popup = new CalEventPopup(calendarEvent, calendarEvent.Date);
            var result = await this.ShowPopupAsync(popup, CancellationToken.None);

            if (result is not CalEventResult draft)
                return;

            calendarEvent.Title = draft.Title;
            calendarEvent.Date = draft.Date;
            calendarEvent.StartTime = draft.StartTime;
            calendarEvent.Notes = draft.Notes;
            calendarEvent.ReminderEnabled = draft.ReminderEnabled;

            _selectedDate = calendarEvent.Date;
            _displayedMonth = new DateTime(calendarEvent.Date.Year, calendarEvent.Date.Month, 1);
            _store.Save(_events);
            RenderMonth();
            await CalendarNotificationService.SyncAsync(calendarEvent);
        }

        void OnDeleteEventSwiped(object sender, EventArgs e)
        {
            if (sender is not SwipeItem { BindingContext: CalendarEvent calendarEvent })
                return;

            _events.Remove(calendarEvent);
            _store.Save(_events);
            CalendarNotificationService.Cancel(calendarEvent);
            RenderMonth();
        }

        void RenderMonth()
        {
            MonthLabel.Text = _displayedMonth.ToString("MMMM yyyy");

            var firstOfMonth = _displayedMonth;
            var gridStart = firstOfMonth.AddDays(-(int)firstOfMonth.DayOfWeek);
            var eventDates = _events.Select(ev => ev.Date).ToHashSet();

            var cells = new List<CalendarDayCell>();
            for (var i = 0; i < 42; i++)
            {
                var date = gridStart.AddDays(i);
                var isCurrentMonth = date.Month == firstOfMonth.Month && date.Year == firstOfMonth.Year;
                var isToday = date == DateTime.Today;
                var isSelected = date == _selectedDate;

                cells.Add(new CalendarDayCell
                {
                    Date = date,
                    Day = date.Day,
                    HasEvents = eventDates.Contains(date),
                    CellColor = isSelected ? SelectedColor : isToday ? TodayColor : Colors.Transparent,
                    TextColor = isSelected ? Colors.White : isCurrentMonth ? CurrentMonthText : OtherMonthText,
                    FontAttr = isToday || isSelected ? FontAttributes.Bold : FontAttributes.None
                });
            }

            DaysCollection.ItemsSource = cells;
            RenderSelectedDayEvents();
        }

        void RenderSelectedDayEvents()
        {
            SelectedDayLabel.Text = _selectedDate.ToString("dddd, MMMM d");
            EventsList.ItemsSource = _events
                .Where(ev => ev.Date == _selectedDate)
                .OrderBy(ev => ev.StartTime)
                .ToList();
        }
    }
}
