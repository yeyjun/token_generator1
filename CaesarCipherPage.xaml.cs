using TokenGenerator.Data;
using TokenGenerator.Models;
using TokenGenerator.Services;

namespace TokenGenerator
{
    public partial class CaesarCipherPage : ContentPage
    {
        readonly DatabaseService _databaseService;
        readonly CipherOfflineStore _offlineStore = new();
        bool _isSyncing;

        public CaesarCipherPage(DatabaseService databaseService)
        {
            InitializeComponent();
            _databaseService = databaseService;
        }

        protected override async void OnAppearing()
        {
            base.OnAppearing();

            Connectivity.Current.ConnectivityChanged += OnConnectivityChanged;

            try
            {
                await _databaseService.EnsureSchemaAsync();
            }
            catch (Exception ex)
            {
                await DisplayAlert("Warning", $"DB error: {ex.Message}", "OK");
            }

            await TrySyncPendingAsync();
        }

        protected override void OnDisappearing()
        {
            base.OnDisappearing();
            Connectivity.Current.ConnectivityChanged -= OnConnectivityChanged;
        }

        async void OnConnectivityChanged(object? sender, ConnectivityChangedEventArgs e)
        {
            if (e.NetworkAccess == NetworkAccess.Internet)
            {
                await TrySyncPendingAsync();
            }
        }

        const int DefaultShift = 25;
        const string SaveIcon = "💾";
        const string SavedIcon = "✔";
        const string AlreadySavedMessage = "This entry is already saved.";

        async void OnSaveClicked(object sender, EventArgs e)
        {
            var shift = DefaultShift;
            var plainText = NotesEditor.Text ?? string.Empty;

            if (_offlineStore.Contains(plainText))
            {
                StatusLabel.Text = AlreadySavedMessage;
                return;
            }

            var cipherText = CaesarCipher.Encrypt(plainText, shift);

            if (Connectivity.Current.NetworkAccess != NetworkAccess.Internet)
            {
                _offlineStore.Add(new PendingCipherEntry
                {
                    Content = plainText,
                    CipherText = cipherText,
                    CipherShift = shift,
                    Source = "manual"
                });
                NotesEditor.Text = string.Empty;
                StatusLabel.Text = "Saved offline. Will sync when online.";
                await ShowSavedIconAsync();
                return;
            }

            try
            {
                if (await _databaseService.ContentExistsAsync(plainText))
                {
                    StatusLabel.Text = AlreadySavedMessage;
                    return;
                }

                await _databaseService.SaveTextAsync(plainText, cipherText, shift, "manual");
                NotesEditor.Text = string.Empty;
                StatusLabel.Text = "Saved.";
                await ShowSavedIconAsync();
            }
            catch
            {
                _offlineStore.Add(new PendingCipherEntry
                {
                    Content = plainText,
                    CipherText = cipherText,
                    CipherShift = shift,
                    Source = "manual"
                });
                NotesEditor.Text = string.Empty;
                StatusLabel.Text = "Saved offline. Will sync when online.";
                await ShowSavedIconAsync();
            }
        }

        async Task ShowSavedIconAsync()
        {
            SaveButton.Text = SavedIcon;
            await Task.Delay(1200);
            SaveButton.Text = SaveIcon;
        }

        async Task TrySyncPendingAsync()
        {
            if (_isSyncing || Connectivity.Current.NetworkAccess != NetworkAccess.Internet)
            {
                return;
            }

            var pending = _offlineStore.Load();
            if (pending.Count == 0)
            {
                return;
            }

            _isSyncing = true;

            try
            {
                var remaining = new List<PendingCipherEntry>(pending);

                foreach (var entry in pending)
                {
                    try
                    {
                        await _databaseService.SaveTextAsync(entry.Content, entry.CipherText, entry.CipherShift, entry.Source);
                        remaining.Remove(entry);
                    }
                    catch
                    {
                        break;
                    }
                }

                _offlineStore.Save(remaining);

                var syncedCount = pending.Count - remaining.Count;
                if (syncedCount > 0)
                {
                    StatusLabel.Text = remaining.Count == 0
                        ? $"Synced {syncedCount} offline item(s)."
                        : $"Synced {syncedCount} item(s), {remaining.Count} pending.";
                }
            }
            finally
            {
                _isSyncing = false;
            }
        }
    }
}
