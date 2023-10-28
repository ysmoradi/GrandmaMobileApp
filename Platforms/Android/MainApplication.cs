using Android.App;
using Android.Runtime;

[assembly: UsesPermission(Android.Manifest.Permission.Internet)]
[assembly: UsesPermission(Android.Manifest.Permission.WriteExternalStorage)]
[assembly: UsesPermission(Android.Manifest.Permission.ReadContacts)]
[assembly: UsesPermission(Android.Manifest.Permission.CallPhone)]
[assembly: UsesPermission(Android.Manifest.Permission.ReadPhoneState)]

namespace GrandmaMobileApp;

[Application]
public class MainApplication(IntPtr handle, JniHandleOwnership ownership)
    : MauiApplication(handle, ownership)
{
    protected override MauiApp CreateMauiApp() => MauiProgram.CreateMauiApp();
}
