using Microsoft.Extensions.Logging;
using Microsoft.Maui.Controls.Compatibility.Hosting;
using Sharpnado.CollectionView;

namespace Test_MauiApp1;
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
            }).UseSharpnadoCollectionView(loggerEnable: false)
            .UseMauiCompatibility(); ;

#if DEBUG
		builder.Logging.AddDebug();
#endif

        return builder.Build();
    }
}
