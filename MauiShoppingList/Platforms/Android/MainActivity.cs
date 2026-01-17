using Android.App;
using Android.Content;
using Android.Content.PM;
using Android.OS;

namespace Test_MauiApp1;
[Activity(Theme = "@style/Maui.SplashTheme", MainLauncher = true, ConfigurationChanges = ConfigChanges.ScreenSize | ConfigChanges.Orientation | ConfigChanges.UiMode | ConfigChanges.ScreenLayout | ConfigChanges.SmallestScreenSize | ConfigChanges.Density)]
public class MainActivity : MauiAppCompatActivity
{
}


[Activity(NoHistory = true, LaunchMode = LaunchMode.SingleTop, Exported = true)]
[IntentFilter(
    new[] { Intent.ActionView },
    Categories = new[] { Intent.CategoryDefault, Intent.CategoryBrowsable },
    DataScheme = "fb259675572518658",   // Twój schemat URI
    DataHost = "*"   // Twój host URI
)]
public class WebAuthenticatorCallbackActivityForFacebook : WebAuthenticatorCallbackActivity
{

}