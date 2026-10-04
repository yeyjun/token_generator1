using CommunityToolkit.Maui;
using Microsoft.Extensions.Logging;
using Microsoft.Maui.Handlers;
using Plugin.LocalNotification;
using TokenGenerator.Data;

#if WINDOWS
using Microsoft.UI.Xaml.Media;
#endif

namespace TokenGenerator
{
    public static class MauiProgram
    {
        public static MauiApp CreateMauiApp()
        {
            var builder = MauiApp.CreateBuilder();
            builder
                .UseMauiApp<App>()
                .UseMauiCommunityToolkit()
                .UseLocalNotification()
                .ConfigureFonts(fonts =>
                {
                    fonts.AddFont("OpenSans-Regular.ttf", "OpenSansRegular");
                    fonts.AddFont("OpenSans-Semibold.ttf", "OpenSansSemibold");
                });

#if ANDROID
            EntryHandler.Mapper.AppendToMapping("NoBackground", (handler, view) =>
            {
                handler.PlatformView.Background = null;
            });
            EditorHandler.Mapper.AppendToMapping("NoBackground", (handler, view) =>
            {
                handler.PlatformView.Background = null;
            });
#endif
#if WINDOWS
            EntryHandler.Mapper.AppendToMapping("NoBackground", (handler, view) =>
            {
                var transparent = new Microsoft.UI.Xaml.Media.SolidColorBrush(Microsoft.UI.Colors.Transparent);
                handler.PlatformView.Background = transparent;
                handler.PlatformView.Resources["TextControlBorderBrushFocused"] = transparent;
                handler.PlatformView.Resources["TextControlBorderBrushPointerOver"] = transparent;
                handler.PlatformView.Resources["TextControlBackground"] = transparent;
                handler.PlatformView.Resources["TextControlBackgroundPointerOver"] = transparent;
                handler.PlatformView.Resources["TextControlBackgroundFocused"] = transparent;
                handler.PlatformView.Resources["TextControlBackgroundDisabled"] = transparent;
            });
            EditorHandler.Mapper.AppendToMapping("NoBackground", (handler, view) =>
            {
                var transparent = new Microsoft.UI.Xaml.Media.SolidColorBrush(Microsoft.UI.Colors.Transparent);
                handler.PlatformView.Background = transparent;
                handler.PlatformView.Resources["TextControlBorderBrushFocused"] = transparent;
                handler.PlatformView.Resources["TextControlBorderBrushPointerOver"] = transparent;
                handler.PlatformView.Resources["TextControlBackground"] = transparent;
                handler.PlatformView.Resources["TextControlBackgroundPointerOver"] = transparent;
                handler.PlatformView.Resources["TextControlBackgroundFocused"] = transparent;
                handler.PlatformView.Resources["TextControlBackgroundDisabled"] = transparent;
            });
#endif

            builder.Services.AddSingleton<DatabaseService>();
            builder.Services.AddTransient<CaesarCipherPage>();
            builder.Services.AddTransient<TokenGeneratorPage>();
            builder.Services.AddTransient<CipherGamePage>();
            builder.Services.AddTransient<DealOrNoDealPage>();
            builder.Services.AddTransient<NotesPage>();
            builder.Services.AddTransient<CalePage>();
            builder.Services.AddTransient<ToolsPage>();
            builder.Services.AddTransient<CalculatorPage>();

#if DEBUG
    		builder.Logging.AddDebug();
#endif

            return builder.Build();
        }
    }
}
