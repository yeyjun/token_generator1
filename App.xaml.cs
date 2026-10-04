using TokenGenerator.Services;

namespace TokenGenerator
{
    public partial class App : Application
    {
        public App()
        {
            InitializeComponent();

            ThemeService.ApplySavedTheme();

            var shell = new AppShell();
            MainPage = shell;
            NetworkBannerService.Initialize(shell);
        }
    }
}
