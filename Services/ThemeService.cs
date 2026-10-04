namespace TokenGenerator.Services
{
    public static class ThemeService
    {
        const string PreferenceKey = "AppTheme";

        public static AppTheme Current => Application.Current?.UserAppTheme ?? AppTheme.Unspecified;

        public static void ApplySavedTheme()
        {
            var saved = Preferences.Get(PreferenceKey, AppTheme.Unspecified.ToString());

            if (Application.Current is not null && Enum.TryParse<AppTheme>(saved, out var theme))
                Application.Current.UserAppTheme = theme;
        }

        public static void SetTheme(AppTheme theme)
        {
            Preferences.Set(PreferenceKey, theme.ToString());

            if (Application.Current is not null)
                Application.Current.UserAppTheme = theme;
        }
    }
}
