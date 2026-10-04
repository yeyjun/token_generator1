using TokenGenerator.Models;
using TokenGenerator.Services;

namespace TokenGenerator
{
    public partial class LETReviewerPage : ContentPage
    {
        static readonly Color CorrectColor = Color.FromArgb("#16A34A");
        static readonly Color WrongColor = Color.FromArgb("#DC2626");

        List<LETQuestion> _quizQuestions = new();
        int _currentIndex;
        int _score;
        bool _answered;

        public LETReviewerPage()
        {
            InitializeComponent();

            CategoryPicker.ItemsSource = LETReviewerService.Categories.Prepend("All Categories").ToList();
            CategoryPicker.SelectedIndex = 0;
        }

        void OnStartClicked(object sender, EventArgs e)
        {
            var category = CategoryPicker.SelectedItem as string ?? "All Categories";

            _quizQuestions = (category == "All Categories"
                    ? LETReviewerService.All
                    : LETReviewerService.All.Where(q => q.Category == category))
                .OrderBy(_ => Random.Shared.Next())
                .ToList();

            if (_quizQuestions.Count == 0)
                return;

            _currentIndex = 0;
            _score = 0;

            SetupLayout.IsVisible = false;
            ResultLayout.IsVisible = false;
            QuizLayout.IsVisible = true;

            ShowQuestion();
        }

        void ShowQuestion()
        {
            _answered = false;
            var question = _quizQuestions[_currentIndex];

            ProgressLabel.Text = $"Question {_currentIndex + 1} of {_quizQuestions.Count}  •  {question.Category}";
            QuestionLabel.Text = question.Question;

            FeedbackLabel.IsVisible = false;
            NextButton.IsVisible = false;

            ChoicesLayout.Children.Clear();
            for (int i = 0; i < question.Choices.Length; i++)
            {
                var index = i;
                var button = new Button
                {
                    Text = question.Choices[i],
                    HorizontalOptions = LayoutOptions.Fill,
                    LineBreakMode = LineBreakMode.WordWrap,
                };
                button.Clicked += (_, _) => OnChoiceSelected(button, index);
                ChoicesLayout.Children.Add(button);
            }
        }

        void OnChoiceSelected(Button selected, int choiceIndex)
        {
            if (_answered)
                return;

            _answered = true;
            var question = _quizQuestions[_currentIndex];
            var isCorrect = choiceIndex == question.CorrectIndex;

            if (isCorrect)
                _score++;

            foreach (var child in ChoicesLayout.Children)
            {
                if (child is not Button button)
                    continue;

                var index = ChoicesLayout.Children.IndexOf(child);
                if (index == question.CorrectIndex)
                    button.BackgroundColor = CorrectColor;
                else if (button == selected)
                    button.BackgroundColor = WrongColor;

                button.IsEnabled = false;
            }

            FeedbackLabel.Text = isCorrect
                ? $"Correct! {question.Explanation}"
                : $"Incorrect. {question.Explanation}";
            FeedbackLabel.TextColor = isCorrect ? CorrectColor : WrongColor;
            FeedbackLabel.IsVisible = true;

            NextButton.Text = _currentIndex == _quizQuestions.Count - 1 ? "See Results" : "Next";
            NextButton.IsVisible = true;
        }

        void OnNextClicked(object sender, EventArgs e)
        {
            if (_currentIndex < _quizQuestions.Count - 1)
            {
                _currentIndex++;
                ShowQuestion();
                return;
            }

            QuizLayout.IsVisible = false;
            ScoreLabel.Text = $"You scored {_score} out of {_quizQuestions.Count}.";
            ResultLayout.IsVisible = true;
        }

        void OnRetryClicked(object sender, EventArgs e)
        {
            ResultLayout.IsVisible = false;
            SetupLayout.IsVisible = true;
        }
    }
}
