using Microsoft.Extensions.Logging;
using CalorieCounter.ViewModels;
using CalorieCounter.Views;
namespace CalorieCounter
{
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
            builder.Services.AddTransient<UploadPage>();
            builder.Services.AddTransient<UploadViewModel>();
            return builder.Build();
        }
    }
}
