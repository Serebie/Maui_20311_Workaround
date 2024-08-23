using Microsoft.Extensions.Logging;

namespace Maui_20311_Workaround;

public static class MauiProgram
{
    public static MauiApp CreateMauiApp()
    {
        var builder = MauiApp.CreateBuilder();
        builder
            .UseMauiApp<App>()
            .ConfigureFonts(fonts =>
            {
                fonts.AddFont("OpenSans-Regular.ttf", "OpenSansRegular");
                fonts.AddFont("OpenSans-Semibold.ttf", "OpenSansSemibold");
            });

#if DEBUG
        builder.Logging.AddDebug();
#endif

#if IOS
        builder.Services.AddSingleton<IImagePickerService, ImagePickerService>();
        builder.Services.AddTransient<MainPage>();
#endif
        
        return builder.Build();
    }
}