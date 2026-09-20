using CommunityToolkit.Maui;
using Microsoft.Extensions.Logging;
using Microsoft.Maui.LifecycleEvents;
using ZXing.Net.Maui.Controls;

#if ANDROID
    using Entrelinhas.Platforms.Android;
#endif

namespace Entrelinhas
{
    public static class MauiProgram
    {
        public static MauiApp CreateMauiApp()
        {
            var builder = MauiApp.CreateBuilder();
            builder
                .UseMauiApp<App>()
                .UseMauiCommunityToolkit()
                .UseBarcodeReader()

                .ConfigureFonts(fonts =>
                {
                    fonts.AddFont("OpenSans-Regular.ttf", "OpenSansRegular");
                    fonts.AddFont("OpenSans-Semibold.ttf", "OpenSansSemibold");
                    fonts.AddFont("Fraunces.ttf", "Fraunces");
                    fonts.AddFont("Karla.ttf", "Karla");
                })

                .ConfigureMauiHandlers(handlers =>
                {
                    #if ANDROID
                        handlers.AddHandler<Shell, CustomShellHandler>();
                    #endif
                });

            #if DEBUG
                builder.Logging.AddDebug();
            #endif

            return builder.Build();
        }
    }
}