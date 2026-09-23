using CommunityToolkit.Maui;
using Microsoft.Extensions.Logging;
using Microsoft.Maui.LifecycleEvents;
using ZXing.Net.Maui.Controls;
using Entrelinhas.Services;
using Entrelinhas.Views;

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

                // Custom Handler para Android
                .ConfigureMauiHandlers(handlers =>
                {
#if ANDROID
                    handlers.AddHandler<Shell, CustomShellHandler>();

                    Microsoft.Maui.Handlers.EntryHandler.Mapper.AppendToMapping("RemoveUnderline", (handler, view) =>
                        {
                            handler.PlatformView.BackgroundTintList =
                            Android.Content.Res.ColorStateList.ValueOf(Android.Graphics.Color.Transparent);
                        });
#endif
                })

                // Serviços
                .Services
                    .AddSingleton<DatabaseService>()
                    .AddSingleton<ApiService>()
                    .AddTransient<AddLivroPage>()
                    .AddTransient<EstantePage>();


            #if DEBUG
                builder.Logging.AddDebug();
            #endif

            return builder.Build();
        }
    }
}