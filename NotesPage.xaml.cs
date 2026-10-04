using System.Collections.ObjectModel;
using Plugin.LocalNotification;
using TokenGenerator.Models;
using TokenGenerator.Services;

namespace TokenGenerator
{
    public partial class NotesPage : ContentPage
    {
        readonly NotesStore _store = new();
        readonly ObservableCollection<Note> _notes = new();
        bool _notificationsSynced;

        public NotesPage()
        {
            InitializeComponent();
            NotesList.ItemsSource = _notes;

            foreach (var note in _store.Load())
                _notes.Add(note);

            Resort();
        }

        protected override async void OnAppearing()
        {
            base.OnAppearing();

            if (_notificationsSynced)
                return;

            _notificationsSynced = true;
            await LocalNotificationCenter.Current.RequestNotificationPermission();
            foreach (var note in _notes)
                await NoteNotificationService.SyncAsync(note);
        }

        void Resort()
        {
            var sorted = _notes.OrderByDescending(n => n.IsPinned).ThenByDescending(n => n.CreatedAt).ToList();
            _notes.Clear();
            foreach (var note in sorted)
                _notes.Add(note);
        }

        async void OnNewClicked(object sender, EventArgs e)
        {
            await Navigation.PushAsync(new NoteEditPage(null, async draft =>
            {
                var note = new Note
                {
                    Title = draft.Title,
                    Content = draft.Content,
                    ColorTag = draft.ColorTag,
                    IsPinned = draft.IsPinned,
                    ReminderAt = draft.ReminderAt,
                    SoundName = draft.SoundName,
                    CreatedAt = DateTime.Now
                };
                _notes.Insert(0, note);
                Resort();
                _store.Save(_notes.ToList());
                await NoteNotificationService.SyncAsync(note);
            }));
        }

        async void OnEditClicked(object sender, EventArgs e)
        {
            if (sender is not Button { BindingContext: Note note })
                return;

            await Navigation.PushAsync(new NoteEditPage(note, async draft =>
            {
                note.Title = draft.Title;
                note.Content = draft.Content;
                note.ColorTag = draft.ColorTag;
                note.IsPinned = draft.IsPinned;
                note.ReminderAt = draft.ReminderAt;
                note.SoundName = draft.SoundName;
                Resort();
                _store.Save(_notes.ToList());
                await NoteNotificationService.SyncAsync(note);
            }));
        }

        async void OnNoteTapped(object sender, TappedEventArgs e)
        {
            if (sender is not BindableObject { BindingContext: Note note })
                return;

            await Navigation.PushAsync(new NoteDetailPage(
                note,
                onEdited: async () =>
                {
                    Resort();
                    _store.Save(_notes.ToList());
                    await NoteNotificationService.SyncAsync(note);
                },
                onDeleted: () =>
                {
                    _notes.Remove(note);
                    _store.Save(_notes.ToList());
                    NoteNotificationService.Cancel(note);
                }));
        }

        void OnDeleteSwiped(object sender, EventArgs e)
        {
            if (sender is not SwipeItem { BindingContext: Note note })
                return;

            _notes.Remove(note);
            _store.Save(_notes.ToList());
            NoteNotificationService.Cancel(note);
        }
    }
}
