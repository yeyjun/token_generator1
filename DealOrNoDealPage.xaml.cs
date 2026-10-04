namespace TokenGenerator
{
    public partial class DealOrNoDealPage : ContentPage
    {
        static readonly decimal[] CaseValues =
        {
            0.01m, 1m, 5m, 10m, 25m, 50m, 75m, 100m, 200m, 300m,
            400m, 500m, 750m, 1000m, 5000m, 10000m, 25000m, 50000m, 75000m, 100000m,
            200000m, 300000m, 400000m, 500000m, 750000m, 1000000m
        };

        static readonly int[] RoundSchedule = { 6, 5, 4, 3, 2, 1, 1, 1, 1, 1 };

        readonly Random _random = new();
        readonly List<CaseInfo> _cases = new();
        readonly Dictionary<int, Button> _caseButtons = new();

        int? _playerCaseNumber;
        int _roundIndex;
        int _casesToOpenThisRound;

        public DealOrNoDealPage()
        {
            InitializeComponent();
            NewGame();
        }

        class CaseInfo
        {
            public int Number;
            public decimal Value;
            public bool IsOpened;
        }

        void NewGame()
        {
            _cases.Clear();
            _caseButtons.Clear();
            CasesLayout.Children.Clear();

            var shuffled = CaseValues.OrderBy(_ => _random.Next()).ToList();
            for (var i = 0; i < shuffled.Count; i++)
            {
                _cases.Add(new CaseInfo { Number = i + 1, Value = shuffled[i] });
            }

            _playerCaseNumber = null;
            _roundIndex = 0;
            OfferPanel.IsVisible = false;
            ResultLabel.Text = string.Empty;
            YourCaseLabel.Text = string.Empty;
            InstructionLabel.Text = "Pick your case.";

            foreach (var caseInfo in _cases)
            {
                var number = caseInfo.Number;
                var button = new Button
                {
                    Text = number.ToString(),
                    WidthRequest = 56,
                    HeightRequest = 56,
                    Margin = new Thickness(3),
                    Padding = 0,
                    FontSize = 12,
                    FontAttributes = FontAttributes.Bold,
                    BackgroundColor = Color.FromArgb("#7C3AED"),
                    TextColor = Colors.White,
                };
                button.Clicked += (_, _) => OnCaseClicked(number);

                _caseButtons[number] = button;
                CasesLayout.Children.Add(button);
            }
        }

        void OnCaseClicked(int number)
        {
            var caseInfo = _cases.First(c => c.Number == number);
            var button = _caseButtons[number];

            if (caseInfo.IsOpened || number == _playerCaseNumber)
            {
                return;
            }

            if (_playerCaseNumber is null)
            {
                _playerCaseNumber = number;
                button.Text = "YOU";
                button.BackgroundColor = Color.FromArgb("#F59E0B");
                YourCaseLabel.Text = $"Your case: #{number}";
                StartRound();
                return;
            }

            if (_casesToOpenThisRound <= 0)
            {
                return;
            }

            OpenCase(caseInfo, button);
        }

        void StartRound()
        {
            _casesToOpenThisRound = _roundIndex < RoundSchedule.Length ? RoundSchedule[_roundIndex] : 1;
            InstructionLabel.Text = $"Round {_roundIndex + 1}: open {_casesToOpenThisRound} case(s).";
        }

        void OpenCase(CaseInfo caseInfo, Button button)
        {
            caseInfo.IsOpened = true;
            button.Text = FormatMoney(caseInfo.Value);
            button.BackgroundColor = Color.FromArgb("#94A3B8");
            button.IsEnabled = false;
            _casesToOpenThisRound--;

            if (RemainingUnopenedExcludingPlayer().Count == 1)
            {
                EndGameWithOwnCase();
                return;
            }

            if (_casesToOpenThisRound == 0)
            {
                _roundIndex++;
                ShowOffer();
            }
            else
            {
                InstructionLabel.Text = $"Round {_roundIndex + 1}: open {_casesToOpenThisRound} more case(s).";
            }
        }

        List<CaseInfo> RemainingUnopenedExcludingPlayer() =>
            _cases.Where(c => !c.IsOpened && c.Number != _playerCaseNumber).ToList();

        void ShowOffer()
        {
            var remainingValues = _cases.Where(c => !c.IsOpened).Select(c => c.Value).ToList();
            var average = remainingValues.Average();
            var factor = Math.Min(0.9m, 0.3m + _roundIndex * 0.1m);
            var offer = Math.Round(average * factor, 2);

            OfferLabel.Text = FormatFullMoney(offer);
            OfferPanel.IsVisible = true;
            InstructionLabel.Text = "The banker is calling. Deal or No Deal?";
        }

        void OnDealClicked(object sender, EventArgs e)
        {
            var offerText = OfferLabel.Text;
            OfferPanel.IsVisible = false;
            ResultLabel.Text = $"Deal! You walk away with {offerText}.";
            InstructionLabel.Text = "Game over.";
            RevealAllCases();
        }

        void OnNoDealClicked(object sender, EventArgs e)
        {
            OfferPanel.IsVisible = false;
            StartRound();
        }

        void EndGameWithOwnCase()
        {
            var playerCase = _cases.First(c => c.Number == _playerCaseNumber);
            var button = _caseButtons[playerCase.Number];
            button.Text = FormatMoney(playerCase.Value);
            button.BackgroundColor = Color.FromArgb("#16A34A");

            OfferPanel.IsVisible = false;
            ResultLabel.Text = $"No more deals! You opened your own case: {FormatFullMoney(playerCase.Value)}.";
            InstructionLabel.Text = "Game over.";
            RevealAllCases();
        }

        void RevealAllCases()
        {
            foreach (var caseInfo in _cases.Where(c => !c.IsOpened))
            {
                var button = _caseButtons[caseInfo.Number];
                button.Text = FormatMoney(caseInfo.Value);
                button.BackgroundColor = caseInfo.Number == _playerCaseNumber
                    ? Color.FromArgb("#16A34A")
                    : Color.FromArgb("#94A3B8");
                caseInfo.IsOpened = true;
            }

            foreach (var button in _caseButtons.Values)
            {
                button.IsEnabled = false;
            }
        }

        static string FormatMoney(decimal value)
        {
            if (value >= 1_000_000m)
            {
                return $"${value / 1_000_000m:0.#}M";
            }

            if (value >= 1_000m)
            {
                return $"${value / 1_000m:0.#}K";
            }

            return value == Math.Floor(value) ? $"${value:0}" : $"${value:0.00}";
        }

        static string FormatFullMoney(decimal value) => value.ToString("C2", System.Globalization.CultureInfo.GetCultureInfo("en-US"));

        void OnNewGameClicked(object sender, EventArgs e) => NewGame();
    }
}
