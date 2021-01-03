using Android.App;
using Android.Content.PM;
using Android.Runtime;
using Android.OS;
using Xamarin.Forms.Platform.Android;
using Xamarin.Forms;
using Android.Provider;
using Android.Database;
using System.Collections.Generic;
using static Android.Provider.ContactsContract.CommonDataKinds;
using System.Linq;
using Android.Content;
using Android.Net;
using Java.Net;

namespace GrandmaMobileApp.Droid
{
    [Activity(Label = "تماس‌های ملکه", Icon = "@mipmap/icon", Theme = "@style/MainTheme", MainLauncher = true, ConfigurationChanges = ConfigChanges.ScreenSize | ConfigChanges.Orientation | ConfigChanges.UiMode | ConfigChanges.ScreenLayout | ConfigChanges.SmallestScreenSize)]
    public class MainActivity : FormsAppCompatActivity
    {
        protected override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);

            SimpleInjections.CallNumber = number =>
            {
                Intent intent = new Intent(Intent.ActionCall, Uri.Parse($"tel:{number}"));
                StartActivity(intent);
            };

            SimpleInjections.MakeWhatsAppVideoCall = number =>
            {
                Intent intent = new Intent(Intent.ActionView);
                var url = "https://api.whatsapp.com/send?phone=" + number + "&text=" + URLEncoder.Encode("سلام!", "UTF-8");
                intent.SetPackage("com.whatsapp");
                intent.SetData(Uri.Parse(url));
                if (intent.ResolveActivity(PackageManager) != null)
                {
                    StartActivity(intent);
                }
            };

            SimpleInjections.GetContactPeople = () =>
            {
                List<ContactPerson> result = new List<ContactPerson>();

                using ICursor contactDetailCursor = ContentResolver.Query(
                    ContactsContract.Contacts.ContentUri,
                    new[]
                    {
                        ContactsContract.Contacts.InterfaceConsts.Id,
                        ContactsContract.Contacts.InterfaceConsts.DisplayName,
                        ContactsContract.Contacts.InterfaceConsts.HasPhoneNumber,
                        ContactsContract.Contacts.InterfaceConsts.PhotoUri
                    },
                    null,
                    null,
                    ContactsContract.Contacts.InterfaceConsts.DisplayName
                );

                if (contactDetailCursor.MoveToFirst())
                {
                    do
                    {
                        if (contactDetailCursor.GetShort(2) != 1)
                            continue;

                        ContactPerson contact = new ContactPerson
                        {
                            Id = contactDetailCursor.GetInt(0),
                            DisplayName = contactDetailCursor.GetString(1)
                        };

                        string imageUri = contactDetailCursor.GetString(3);

                        if (imageUri != null)
                        {
                            contact.ImageUri = ImageSource.FromStream(() =>
                            {
                                try
                                {
                                    return ContentResolver.OpenInputStream(Uri.Parse(imageUri));
                                }
                                catch (Java.IO.FileNotFoundException)
                                {
                                    return null;
                                }
                            });
                        }

                        using ICursor numbers = ContentResolver.Query(Phone.ContentUri, new string[] { Phone.Number, Phone.InterfaceConsts.Type }, $"{Phone.InterfaceConsts.ContactId} = {contact.Id}", null, null);

                        while (numbers.MoveToNext())
                        {
                            string number = numbers.GetString(0);
                            int type = numbers.GetInt(1);
                            bool isMobile = type == 2;

                            contact.Numbers.Add(new ContactNumber
                            {
                                ContactId = contact.Id,
                                Number = number,
                                Type = isMobile ? "📱" : "🏡"
                            });

                            if (isMobile)
                            {
                                contact.Numbers.Add(new ContactNumber
                                {
                                    ContactId = contact.Id,
                                    Number = number,
                                    Type = "📷"
                                });
                            }
                        }

                        result.Add(contact);

                    } while (contactDetailCursor.MoveToNext());
                };

                return result
                    .ToList();
            };

            Xamarin.Essentials.Platform.Init(this, savedInstanceState);
            Forms.Init(this, savedInstanceState);

            LoadApplication(new App());
        }

        public override void OnRequestPermissionsResult(int requestCode, string[] permissions, [GeneratedEnum] Permission[] grantResults)
        {
            Xamarin.Essentials.Platform.OnRequestPermissionsResult(requestCode, permissions, grantResults);

            base.OnRequestPermissionsResult(requestCode, permissions, grantResults);
        }
    }
}