namespace TokenGenerator.Services
{
    public static class FavoritesStore
    {
        const string PreferenceKey = "dictionary_favorites";
        const char Separator = '';

        public static HashSet<string> Load()
        {
            var raw = Preferences.Default.Get(PreferenceKey, string.Empty);
            return string.IsNullOrEmpty(raw)
                ? new HashSet<string>()
                : raw.Split(Separator).ToHashSet();
        }

        public static void Save(IEnumerable<string> ids)
            => Preferences.Default.Set(PreferenceKey, string.Join(Separator, ids));
    }
}
