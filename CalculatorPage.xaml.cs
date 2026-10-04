using System.Globalization;
using System.Net.Http.Json;
using System.Text.Json.Serialization;

namespace TokenGenerator
{
    public partial class CalculatorPage : ContentPage
    {
        static readonly HttpClient _httpClient = new();

        static readonly string[] CurrencyCodes =
        {
            "AUD", "BGN", "BRL", "CAD", "CHF", "CNY", "CZK", "DKK", "EUR", "GBP",
            "HKD", "HUF", "IDR", "ILS", "INR", "ISK", "JPY", "KRW", "MXN", "MYR",
            "NOK", "NZD", "PHP", "PLN", "RON", "SEK", "SGD", "THB", "TRY", "USD", "ZAR",
        };

        static readonly (string Unit, double ToMeters)[] LengthUnits =
        {
            ("Millimeters", 0.001),
            ("Centimeters", 0.01),
            ("Meters", 1),
            ("Kilometers", 1000),
            ("Inches", 0.0254),
            ("Feet", 0.3048),
            ("Yards", 0.9144),
            ("Miles", 1609.344),
        };

        static readonly (string Unit, double ToKilograms)[] WeightUnits =
        {
            ("Milligrams", 0.000001),
            ("Grams", 0.001),
            ("Kilograms", 1),
            ("Ounces", 0.0283495),
            ("Pounds", 0.453592),
        };

        static readonly string[] TemperatureUnits = { "Celsius", "Fahrenheit", "Kelvin" };

        public CalculatorPage()
        {
            InitializeComponent();

            ModePicker.SelectedIndex = 0;
            PercentageModePicker.SelectedIndex = 0;
            ConverterCategoryPicker.SelectedIndex = 0;

            var now = DateTime.Now;
            StartDatePicker.Date = now.Date;
            StartTimePicker.Time = now.TimeOfDay;
            EndDatePicker.Date = now.Date;
            EndTimePicker.Time = now.TimeOfDay;

            CurrencyFromPicker.ItemsSource = CurrencyCodes;
            CurrencyToPicker.ItemsSource = CurrencyCodes;
            CurrencyFromPicker.SelectedItem = "USD";
            CurrencyToPicker.SelectedItem = "PHP";
        }

        void OnModeChanged(object sender, EventArgs e)
        {
            PercentageSection.IsVisible = ModePicker.SelectedIndex == 0;
            AverageSection.IsVisible = ModePicker.SelectedIndex == 1;
            TimeDiffSection.IsVisible = ModePicker.SelectedIndex == 2;
            ConverterSection.IsVisible = ModePicker.SelectedIndex == 3;
            CurrencySection.IsVisible = ModePicker.SelectedIndex == 4;
        }

        void OnPercentageModeChanged(object sender, EventArgs e)
        {
            PercentageResultLabel.Text = string.Empty;

            switch (PercentageModePicker.SelectedIndex)
            {
                case 0:
                    PercentXEntry.Placeholder = "Percent (%)";
                    PercentYEntry.Placeholder = "Of value";
                    break;
                case 1:
                    PercentXEntry.Placeholder = "Value";
                    PercentYEntry.Placeholder = "Total";
                    break;
                case 2:
                    PercentXEntry.Placeholder = "From value";
                    PercentYEntry.Placeholder = "To value";
                    break;
            }
        }

        void OnCalculatePercentageClicked(object sender, EventArgs e)
        {
            if (!TryParse(PercentXEntry.Text, out var x) || !TryParse(PercentYEntry.Text, out var y))
            {
                PercentageResultLabel.Text = "Enter valid numbers for both fields.";
                return;
            }

            switch (PercentageModePicker.SelectedIndex)
            {
                case 0:
                    PercentageResultLabel.Text = $"{x}% of {y} = {x / 100 * y:0.####}";
                    break;
                case 1:
                    if (y == 0)
                    {
                        PercentageResultLabel.Text = "Total cannot be zero.";
                        return;
                    }
                    PercentageResultLabel.Text = $"{x} is {x / y * 100:0.####}% of {y}";
                    break;
                case 2:
                    if (x == 0)
                    {
                        PercentageResultLabel.Text = "From value cannot be zero.";
                        return;
                    }
                    var change = (y - x) / x * 100;
                    var direction = change >= 0 ? "increase" : "decrease";
                    PercentageResultLabel.Text = $"{Math.Abs(change):0.####}% {direction}";
                    break;
            }
        }

        void OnCalculateAverageClicked(object sender, EventArgs e)
        {
            var tokens = (AverageNumbersEditor.Text ?? string.Empty)
                .Split(new[] { ',', ' ', '\n', '\r', '\t' }, StringSplitOptions.RemoveEmptyEntries);

            var numbers = new List<double>();
            foreach (var token in tokens)
            {
                if (TryParse(token, out var value))
                {
                    numbers.Add(value);
                }
            }

            if (numbers.Count == 0)
            {
                AverageResultLabel.Text = "Enter at least one valid number.";
                return;
            }

            AverageResultLabel.Text =
                $"Count: {numbers.Count}\n" +
                $"Sum: {numbers.Sum():0.####}\n" +
                $"Average: {numbers.Average():0.####}\n" +
                $"Min: {numbers.Min():0.####}\n" +
                $"Max: {numbers.Max():0.####}";
        }

        void OnCalculateTimeDiffClicked(object sender, EventArgs e)
        {
            var start = StartDatePicker.Date + StartTimePicker.Time;
            var end = EndDatePicker.Date + EndTimePicker.Time;
            var diff = end - start;

            if (diff < TimeSpan.Zero)
            {
                TimeDiffResultLabel.Text = $"End is before start by {FormatTimeSpan(-diff)}.";
                return;
            }

            TimeDiffResultLabel.Text = FormatTimeSpan(diff);
        }

        static string FormatTimeSpan(TimeSpan span) =>
            $"{(int)span.TotalDays} days, {span.Hours} hours, {span.Minutes} minutes\n" +
            $"({span.TotalHours:0.##} hours total, {span.TotalMinutes:0} minutes total)";

        void OnConverterCategoryChanged(object sender, EventArgs e)
        {
            ConverterResultLabel.Text = string.Empty;

            var units = ConverterCategoryPicker.SelectedIndex switch
            {
                0 => LengthUnits.Select(u => u.Unit).ToList(),
                1 => WeightUnits.Select(u => u.Unit).ToList(),
                _ => TemperatureUnits.ToList(),
            };

            ConverterFromPicker.ItemsSource = units;
            ConverterToPicker.ItemsSource = units;
            ConverterFromPicker.SelectedIndex = 0;
            ConverterToPicker.SelectedIndex = units.Count > 1 ? 1 : 0;

            ComputeConversion();
        }

        void OnConverterValueChanged(object sender, TextChangedEventArgs e) => ComputeConversion();

        void OnConverterUnitChanged(object sender, EventArgs e) => ComputeConversion();

        void ComputeConversion()
        {
            if (string.IsNullOrWhiteSpace(ConverterValueEntry.Text))
            {
                ConverterResultLabel.Text = string.Empty;
                return;
            }

            if (!TryParse(ConverterValueEntry.Text, out var value))
            {
                ConverterResultLabel.Text = "Enter a valid number.";
                return;
            }

            if (ConverterFromPicker.SelectedItem is not string fromUnit || ConverterToPicker.SelectedItem is not string toUnit)
            {
                return;
            }

            double result;
            switch (ConverterCategoryPicker.SelectedIndex)
            {
                case 0:
                    result = ConvertLinear(value, fromUnit, toUnit, LengthUnits.ToDictionary(u => u.Unit, u => u.ToMeters));
                    break;
                case 1:
                    result = ConvertLinear(value, fromUnit, toUnit, WeightUnits.ToDictionary(u => u.Unit, u => u.ToKilograms));
                    break;
                default:
                    result = ConvertTemperature(value, fromUnit, toUnit);
                    break;
            }

            ConverterResultLabel.Text = $"{value:0.####} {fromUnit} = {result:0.####} {toUnit}";
        }

        static double ConvertLinear(double value, string fromUnit, string toUnit, Dictionary<string, double> toBaseFactors)
        {
            var baseValue = value * toBaseFactors[fromUnit];
            return baseValue / toBaseFactors[toUnit];
        }

        static double ConvertTemperature(double value, string fromUnit, string toUnit)
        {
            var celsius = fromUnit switch
            {
                "Celsius" => value,
                "Fahrenheit" => (value - 32) * 5 / 9,
                "Kelvin" => value - 273.15,
                _ => value,
            };

            return toUnit switch
            {
                "Celsius" => celsius,
                "Fahrenheit" => celsius * 9 / 5 + 32,
                "Kelvin" => celsius + 273.15,
                _ => celsius,
            };
        }

        static bool TryParse(string? text, out double value) =>
            double.TryParse(text, NumberStyles.Float, CultureInfo.InvariantCulture, out value);

        CancellationTokenSource? _currencyDebounceCts;

        void OnCurrencyAmountChanged(object sender, TextChangedEventArgs e) => DebounceCurrencyConvert();

        void OnCurrencyUnitChanged(object sender, EventArgs e) => DebounceCurrencyConvert();

        async void DebounceCurrencyConvert()
        {
            _currencyDebounceCts?.Cancel();
            var cts = new CancellationTokenSource();
            _currencyDebounceCts = cts;

            if (string.IsNullOrWhiteSpace(CurrencyAmountEntry.Text))
            {
                CurrencyResultLabel.Text = string.Empty;
                CurrencyStatusLabel.Text = string.Empty;
                return;
            }

            if (!TryParse(CurrencyAmountEntry.Text, out var amount))
            {
                CurrencyResultLabel.Text = "Enter a valid amount.";
                CurrencyStatusLabel.Text = string.Empty;
                return;
            }

            if (CurrencyFromPicker.SelectedItem is not string fromCode || CurrencyToPicker.SelectedItem is not string toCode)
            {
                return;
            }

            if (fromCode == toCode)
            {
                CurrencyResultLabel.Text = $"{amount:0.####} {fromCode} = {amount:0.####} {toCode}";
                CurrencyStatusLabel.Text = string.Empty;
                return;
            }

            CurrencyStatusLabel.Text = "Fetching latest rate...";

            try
            {
                // Debounce: wait for a pause in typing before hitting the network.
                await Task.Delay(400, cts.Token);

                var url = $"https://api.frankfurter.app/latest?amount={amount:0.####}&from={fromCode}&to={toCode}";
                var response = await _httpClient.GetFromJsonAsync<FrankfurterResponse>(url, cts.Token);

                if (cts.Token.IsCancellationRequested)
                {
                    return;
                }

                if (response?.Rates is null || !response.Rates.TryGetValue(toCode, out var converted))
                {
                    CurrencyResultLabel.Text = "Conversion unavailable.";
                    CurrencyStatusLabel.Text = string.Empty;
                    return;
                }

                CurrencyResultLabel.Text = $"{amount:0.####} {fromCode} = {converted:0.####} {toCode}";
                CurrencyStatusLabel.Text = $"Rate as of {response.Date}";
            }
            catch (OperationCanceledException)
            {
                // Superseded by a newer edit; the latest call will update the UI.
            }
            catch (Exception ex)
            {
                CurrencyResultLabel.Text = "Conversion failed.";
                CurrencyStatusLabel.Text = ex.Message;
            }
        }

        class FrankfurterResponse
        {
            [JsonPropertyName("amount")]
            public double Amount { get; set; }

            [JsonPropertyName("base")]
            public string Base { get; set; } = string.Empty;

            [JsonPropertyName("date")]
            public string Date { get; set; } = string.Empty;

            [JsonPropertyName("rates")]
            public Dictionary<string, double>? Rates { get; set; }
        }
    }
}
