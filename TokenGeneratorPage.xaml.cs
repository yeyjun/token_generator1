using System.Collections.ObjectModel;
using System.Security.Cryptography;
using TokenGenerator.Data;

namespace TokenGenerator
{
    public partial class TokenGeneratorPage : ContentPage
    {
        readonly DatabaseService _databaseService;
        readonly ObservableCollection<string> _businesses = new();

        const string TokenChars = "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789";
        const int TokenLength = 32;

        bool _isActive;

        public TokenGeneratorPage(DatabaseService databaseService)
        {
            InitializeComponent();
            _databaseService = databaseService;
            BusinessPicker.ItemsSource = _businesses;
        }

        protected override async void OnAppearing()
        {
            base.OnAppearing();

            try
            {
                await _databaseService.EnsureSchemaAsync();
                var businesses = await _databaseService.GetBusinessesAsync();

                _businesses.Clear();
                foreach (var business in businesses)
                {
                    _businesses.Add(business);
                }
            }
            catch (Exception ex)
            {
                await DisplayAlert("Warning", $"DB error: {ex.Message}", "OK");
            }
        }

        async void OnAddBusinessClicked(object sender, EventArgs e)
        {
            var name = await DisplayPromptAsync("Add Business", "Enter the business name:");
            name = name?.Trim();

            if (string.IsNullOrEmpty(name))
            {
                return;
            }

            if (_businesses.Contains(name))
            {
                BusinessPicker.SelectedItem = name;
                return;
            }

            try
            {
                await _databaseService.AddBusinessAsync(name);
                _businesses.Add(name);
                BusinessPicker.SelectedItem = name;
            }
            catch (Exception ex)
            {
                await DisplayAlert("Warning", $"Add business failed: {ex.Message}", "OK");
            }
        }

        async void OnGenerateTokenClicked(object sender, EventArgs e)
        {
            var token = RandomNumberGenerator.GetString(TokenChars, TokenLength);
            TokenLabel.Text = token;

            try
            {
                var business = BusinessPicker.SelectedItem as string;
                await _databaseService.SaveTokenAsync(token, business);
                _isActive = true;
                UpdateActiveUi();
            }
            catch (Exception ex)
            {
                await DisplayAlert("Warning", $"Token save failed: {ex.Message}", "OK");
            }
        }

        async void OnBusinessSelectedIndexChanged(object sender, EventArgs e)
        {
            var business = BusinessPicker.SelectedItem as string;

            try
            {
                var record = await _databaseService.GetTokenAsync(business);
                if (record is null)
                {
                    TokenLabel.Text = string.Empty;
                    ToggleActiveButton.IsEnabled = false;
                    StatusLabel.Text = string.Empty;
                    return;
                }

                TokenLabel.Text = record.Value.Token;
                _isActive = record.Value.IsActive;
                ToggleActiveButton.IsEnabled = true;
                UpdateActiveUi();
            }
            catch (Exception ex)
            {
                await DisplayAlert("Warning", $"Token lookup failed: {ex.Message}", "OK");
            }
        }

        async void OnToggleActiveClicked(object sender, EventArgs e)
        {
            var business = BusinessPicker.SelectedItem as string;
            var newState = !_isActive;

            try
            {
                await _databaseService.SetTokenActiveAsync(business, newState);
                _isActive = newState;
                UpdateActiveUi();
            }
            catch (Exception ex)
            {
                await DisplayAlert("Warning", $"Token update failed: {ex.Message}", "OK");
            }
        }

        void UpdateActiveUi()
        {
            ToggleActiveButton.IsEnabled = true;
            ToggleActiveButton.Text = _isActive ? "Deactivate" : "Activate";
            StatusLabel.Text = _isActive ? "Active" : "Inactive";
        }
    }
}
