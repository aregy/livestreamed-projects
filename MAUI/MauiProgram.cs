using CommunityToolkit.Maui;
using DevExpress.Drawing.Internal;
using DevExpress.Maui;
using SignPDF.ViewModels;
using SkiaSharp.Views.Maui.Controls.Hosting;

namespace SignPDF;

public static class MauiProgram {
    public readonly static int PORT = 5184;
    //public readonly static int PORT = 7290;

    public static MauiApp CreateMauiApp() {
        // DXDrawingEngine.ForceSkia();

        var builder = MauiApp.CreateBuilder();
        builder
            .UseMauiApp<App>()
            .UseDevExpress()
            .ConfigureFonts(fonts => {
                fonts.AddFont("OpenSans-Regular.ttf", "OpenSansRegular");
                fonts.AddFont("OpenSans-Semibold.ttf", "OpenSansSemibold");
            })
            .UseSkiaSharp()
            .UseMauiCommunityToolkit();
        builder.Services.AddSingleton<NavigationViewModel>();
        builder.Services.AddSingleton<DocumentViewModel>();
        builder.Services.AddSingleton<PdfViewModel>();
        return builder.Build();
    }
}
