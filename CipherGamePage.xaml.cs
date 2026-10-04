namespace TokenGenerator
{
    public partial class CipherGamePage : ContentPage
    {
        const int MaxWrongGuesses = 6;

        static readonly string[] WordBank =
        {
            "apple", "banana", "cipher", "dotnet", "keyboard",
            "mountain", "notebook", "sunlight", "security", "wizard"
        };

        readonly Random _random = new();
        readonly HashSet<char> _guessedLetters = new();
        string _currentWord = string.Empty;
        int _wrongGuesses;
        int _score;
        bool _roundOver;

        public CipherGamePage()
        {
            InitializeComponent();
            NewRound();
        }

        void NewRound()
        {
            _currentWord = WordBank[_random.Next(WordBank.Length)];
            _guessedLetters.Clear();
            _wrongGuesses = 0;
            _roundOver = false;

            UpdateWordDisplay();
            AttemptsLabel.Text = $"Attempts left: {MaxWrongGuesses}";
            GuessedLettersLabel.Text = "Guessed: ";
            FeedbackLabel.Text = string.Empty;
            LetterEntry.Text = string.Empty;
        }

        void UpdateWordDisplay()
        {
            var display = string.Join(' ', _currentWord.Select(c => _guessedLetters.Contains(c) ? char.ToUpper(c).ToString() : "_"));
            WordDisplayLabel.Text = display;
        }

        void OnGuessClicked(object sender, EventArgs e)
        {
            if (_roundOver)
            {
                return;
            }

            var text = LetterEntry.Text?.Trim().ToLowerInvariant();
            LetterEntry.Text = string.Empty;

            if (string.IsNullOrEmpty(text) || text.Length != 1 || !char.IsLetter(text[0]))
            {
                FeedbackLabel.TextColor = Colors.OrangeRed;
                FeedbackLabel.Text = "Enter a single letter.";
                return;
            }

            var letter = text[0];

            if (_guessedLetters.Contains(letter))
            {
                FeedbackLabel.TextColor = Colors.OrangeRed;
                FeedbackLabel.Text = $"Already guessed '{letter}'.";
                return;
            }

            _guessedLetters.Add(letter);
            GuessedLettersLabel.Text = "Guessed: " + string.Join(", ", _guessedLetters.OrderBy(c => c));

            if (_currentWord.Contains(letter))
            {
                UpdateWordDisplay();

                if (_currentWord.All(c => _guessedLetters.Contains(c)))
                {
                    _score++;
                    ScoreLabel.Text = $"Score: {_score}";
                    FeedbackLabel.TextColor = Colors.Green;
                    FeedbackLabel.Text = $"Correct! The word was \"{_currentWord}\".";
                    _roundOver = true;
                }
                else
                {
                    FeedbackLabel.TextColor = Colors.Green;
                    FeedbackLabel.Text = "Good guess!";
                }
            }
            else
            {
                _wrongGuesses++;
                AttemptsLabel.Text = $"Attempts left: {MaxWrongGuesses - _wrongGuesses}";

                if (_wrongGuesses >= MaxWrongGuesses)
                {
                    WordDisplayLabel.Text = _currentWord.ToUpperInvariant();
                    FeedbackLabel.TextColor = Colors.OrangeRed;
                    FeedbackLabel.Text = $"Out of attempts! The word was \"{_currentWord}\".";
                    _roundOver = true;
                }
                else
                {
                    FeedbackLabel.TextColor = Colors.OrangeRed;
                    FeedbackLabel.Text = $"'{letter}' is not in the word.";
                }
            }
        }

        void OnNewWordClicked(object sender, EventArgs e) => NewRound();
    }
}
