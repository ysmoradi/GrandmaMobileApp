using Android.Content;
using Android.Database;
using Android.Media;
using Android.Provider;
using Java.Net;
using static Android.Provider.ContactsContract.CommonDataKinds;

namespace GrandmaMobileApp;

public partial class MainPage
{
    public List<ContactPerson> ContactPeople { get; set; } = [];

    public Command<ContactNumber> CallNumberCommand { get; set; }

    public Command<ContactPerson> PlayDisplayNameCommand { get; set; }

    public string ConnectivityStatus { get; set; } = "؟";

    public MainPage()
    {
        InitializeComponent();

        CallNumberCommand = new Command<ContactNumber>(async contactNumber =>
        {
            if (contactNumber.Type == "📷")
            {
                Intent intent = new(Intent.ActionView);
                var url = "https://api.whatsapp.com/send?phone=" + contactNumber.Number + "&text=" + URLEncoder.Encode("سلام!", "UTF-8");
                intent.SetPackage("com.whatsapp");
                intent.SetData(Android.Net.Uri.Parse(url));
                if (intent.ResolveActivity(MauiApplication.Current.PackageManager) != null)
                {
                    MauiApplication.Current.StartActivity(intent);
                }
            }
            else
            {
                await Permissions.RequestAsync<Permissions.Phone>();

                Intent intent = new(Intent.ActionCall, Android.Net.Uri.Parse($"tel:{contactNumber.Number}"));
                MauiApplication.Current.StartActivity(intent);
            }
        });

        PlayDisplayNameCommand = new Command<ContactPerson>(contact =>
        {
            MediaPlayer player = MediaPlayer.Create(MauiApplication.Current, Android.Net.Uri.Parse($"http://api.farsireader.com/ArianaCloudService/ReadTextGET?APIKey=$JPK7Q2IRVPJHWKQ5A&Text={contact.DisplayName}&Speaker=Female1&Format=mp3"));
            player.Start();
        });
    }

    protected async override void OnAppearing()
    {
        base.OnAppearing();

        await Permissions.RequestAsync<Permissions.ContactsRead>();

        List<ContactPerson> result = [];

        using ICursor contactDetailCursor = MauiApplication.Current.ContentResolver.Query(
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

        int count = 0;

        if (contactDetailCursor.MoveToFirst())
        {
            do
            {
                if (contactDetailCursor.GetShort(2) != 1)
                    continue;

                ContactPerson contact = new()
                {
                    Id = contactDetailCursor.GetInt(0),
                    DisplayName = contactDetailCursor.GetString(1)
                };

                string imageUri = contactDetailCursor.GetString(3);


                if (imageUri is not null)
                {
                    contact.ImageUri = ImageSource.FromStream(() =>
                    {
                        try
                        {
                            return MauiApplication.Current.ContentResolver.OpenInputStream(Android.Net.Uri.Parse(imageUri));
                        }
                        catch (Java.IO.FileNotFoundException)
                        {
                            return null;
                        }
                    });
                }

                using ICursor numbers = MauiApplication.Current.ContentResolver.Query(Phone.ContentUri, new string[] { Phone.Number, Phone.InterfaceConsts.Type }, $"{Phone.InterfaceConsts.ContactId} = {contact.Id}", null, null);

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

                count++;
#if DEBUG
                if (count > 19)
                    break;
#endif

            } while (contactDetailCursor.MoveToNext());
        };

        ContactPeople = result;

        try
        {
            using HttpClient client = new();

            using HttpResponseMessage response = await client.GetAsync("https://google.com/");

            response.EnsureSuccessStatusCode();

            ConnectivityStatus = "😊";
        }
        catch
        {
            ConnectivityStatus = "😕";
        }
    }
}

public class ContactPerson
{
    public int Id { get; set; }

    public string DisplayName { get; set; }

    public List<ContactNumber> Numbers { get; set; } = [];

    public ImageSource ImageUri { get; set; }
}

public class ContactNumber
{
    public int ContactId { get; set; }

    public string Number { get; set; }

    public string Type { get; set; }
}
