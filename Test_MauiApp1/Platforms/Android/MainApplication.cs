using Android.App;
using Android.Runtime;
using Test_MauiApp1.Platforms.Android;
using Test_MauiApp1.Services;

namespace Test_MauiApp1;
[Application]
public class MainApplication : MauiApplication
{
    public MainApplication(IntPtr handle, JniHandleOwnership ownership)
        : base(handle, ownership)
    {
        DependencyService.Register<IClearCookies, ClearCookies>();
    }

    protected override MauiApp CreateMauiApp() => MauiProgram.CreateMauiApp();
}
