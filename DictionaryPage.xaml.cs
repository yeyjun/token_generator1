using TokenGenerator.Services;

namespace TokenGenerator
{
    public partial class DictionaryPage : ContentPage
    {
        public DictionaryPage()
        {
            InitializeComponent();

            CategoryPicker.ItemsSource = DictionaryService.Categories;
            CategoryPicker.SelectedIndex = 0;
        }

        void OnSearchChanged(object sender, TextChangedEventArgs e) => ApplyFilter();

        void OnFilterChanged(object sender, EventArgs e) => ApplyFilter();

        void ApplyFilter()
        {
            var query = SearchBox.Text?.Trim() ?? string.Empty;
            var category = CategoryPicker.SelectedItem as string ?? "All";

            var results = DictionaryService.All
                .Where(entry => category == "All" || entry.Category == category)
                .Where(entry =>
                    string.IsNullOrEmpty(query) ||
                    entry.Term.Contains(query, StringComparison.OrdinalIgnoreCase) ||
                    entry.Definition.Contains(query, StringComparison.OrdinalIgnoreCase) ||
                    entry.Example.Contains(query, StringComparison.OrdinalIgnoreCase))
                .OrderBy(entry => entry.Term)
                .ToList();

            ResultCountLabel.Text = $"{results.Count} term{(results.Count == 1 ? "" : "s")}";
            EntriesList.ItemsSource = results;
        }
    }
}
