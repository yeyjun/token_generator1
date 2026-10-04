namespace TokenGenerator
{
    public partial class HangarooPage : ContentPage
    {
        const int MaxWrongGuesses = 6;

        static readonly (string Category, string Phrase)[] PhraseBank =
        {
            ("ANIMAL", "KANGAROO"),
            ("COUNTRY", "AUSTRALIA"),
            ("SPORT", "CRICKET"),
            ("FOOD", "PANCAKE"),
            ("MOVIE", "FROZEN"),
            ("TECH", "DOT NET MAUI"),
            ("PLACE", "GREAT BARRIER REEF"),
            ("ANIMAL", "PLATYPUS"),
        };

        readonly Random _random = new();
        readonly HashSet<char> _guessedLetters = new();
        readonly Dictionary<char, Button> _letterButtons = new();
        string _currentPhrase = string.Empty;
        int _wrongGuesses;
        int _score;
        bool _roundOver;

        public HangarooPage()
        {
            InitializeComponent();
            BuildLetterButtons();
            NewRound();
        }

        void BuildLetterButtons()
        {
            foreach (var letter in "ABCDEFGHIJKLMNOPQRSTUVWXYZ")
            {
                var button = new Button
                {
                    Text = letter.ToString(),
                    WidthRequest = 40,
                    HeightRequest = 40,
                    Padding = 0,
                    Margin = 2,
                };
                button.Clicked += OnLetterClicked;

                _letterButtons[letter] = button;
                LettersLayout.Children.Add(button);
            }
        }

        void NewRound()
        {
            var entry = PhraseBank[_random.Next(PhraseBank.Length)];
            _currentPhrase = entry.Phrase;
            _guessedLetters.Clear();
            _wrongGuesses = 0;
            _roundOver = false;

            CategoryLabel.Text = $"Category: {entry.Category}";
            LivesLabel.Text = $"Lives left: {MaxWrongGuesses}";
            FeedbackLabel.Text = string.Empty;

            foreach (var button in _letterButtons.Values)
            {
                button.IsEnabled = true;
                button.BackgroundColor = null;
            }

            UpdatePhraseDisplay();
        }

        void UpdatePhraseDisplay()
        {
            var display = string.Join(' ', _currentPhrase.Select(c =>
                c == ' ' ? "  " : _guessedLetters.Contains(c) ? c.ToString() : "_"));
            PhraseDisplayLabel.Text = display;
        }

        void OnLetterClicked(object? sender, EventArgs e)
        {
            if (_roundOver || sender is not Button button || button.Text.Length != 1)
                return;

            var letter = button.Text[0];
            button.IsEnabled = false;
            _guessedLetters.Add(letter);

            if (_currentPhrase.Contains(letter))
            {
                button.BackgroundColor = Color.FromArgb("#16A34A");
                UpdatePhraseDisplay();

                if (_currentPhrase.Where(c => c != ' ').All(c => _guessedLetters.Contains(c)))
                {
                    _score++;
                    ScoreLabel.Text = $"Score: {_score}";
                    FeedbackLabel.TextColor = Colors.Green;
                    FeedbackLabel.Text = $"Correct! The phrase was \"{_currentPhrase}\".";
                    _roundOver = true;
                    DisableAllLetters();
                }
                else
                {
                    FeedbackLabel.TextColor = Colors.Green;
                    FeedbackLabel.Text = "Good guess!";
                }
            }
            else
            {
                button.BackgroundColor = Color.FromArgb("#DC3545");
                _wrongGuesses++;
                LivesLabel.Text = $"Lives left: {MaxWrongGuesses - _wrongGuesses}";

                if (_wrongGuesses >= MaxWrongGuesses)
                {
                    PhraseDisplayLabel.Text = _currentPhrase;
                    FeedbackLabel.TextColor = Colors.OrangeRed;
                    FeedbackLabel.Text = $"Out of hops! The phrase was \"{_currentPhrase}\".";
                    _roundOver = true;
                    DisableAllLetters();
                }
                else
                {
                    FeedbackLabel.TextColor = Colors.OrangeRed;
                    FeedbackLabel.Text = $"'{letter}' is not in the phrase.";
                }
            }
        }

        void DisableAllLetters()
        {
            foreach (var button in _letterButtons.Values)
                button.IsEnabled = false;
        }

        void OnNewGameClicked(object sender, EventArgs e) => NewRound();
    }
}
