using TokenGenerator.Models;
using TokenGenerator.Services;

namespace TokenGenerator
{
    public partial class RosaryPage : ContentPage
    {
        static readonly string[] MysterySets = { "Joyful Mysteries", "Sorrowful Mysteries", "Glorious Mysteries", "Luminous Mysteries" };

        List<RosaryStep> _steps = new();
        int _index;

        public RosaryPage()
        {
            InitializeComponent();

            MysteryPicker.ItemsSource = MysterySets;
            MysteryPicker.SelectedItem = RosaryService.GetMysterySetForDay(DateTime.Now.DayOfWeek);
        }

        void OnMysteryChanged(object sender, EventArgs e)
        {
            if (MysteryPicker.SelectedItem is not string mysterySet)
                return;

            _steps = RosaryService.BuildSteps(mysterySet);
            _index = 0;
            RenderStep();
        }

        void OnBackClicked(object sender, EventArgs e)
        {
            if (_index == 0)
                return;

            _index--;
            RenderStep();
        }

        void OnNextClicked(object sender, EventArgs e)
        {
            _index = _index >= _steps.Count - 1 ? 0 : _index + 1;
            RenderStep();
        }

        void OnRestartClicked(object sender, EventArgs e)
        {
            _index = 0;
            RenderStep();
        }

        void RenderStep()
        {
            if (_steps.Count == 0)
                return;

            var step = _steps[_index];

            SectionLabel.Text = step.SectionLabel;
            StepTitleLabel.Text = step.Title;
            StepTextLabel.Text = step.Text;
            ProgressLabel.Text = $"Step {_index + 1} of {_steps.Count}";
            StepProgressBar.Progress = (double)(_index + 1) / _steps.Count;

            BackButton.IsEnabled = _index > 0;
            NextButton.Text = _index >= _steps.Count - 1 ? "Done" : "Next";
        }
    }
}
