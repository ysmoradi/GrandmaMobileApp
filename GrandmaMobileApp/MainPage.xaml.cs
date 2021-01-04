using System;
using System.Collections.Generic;
using System.Net.Http;
using Xamarin.Essentials;
using Xamarin.Forms;

namespace GrandmaMobileApp
{
    public partial class MainPage
    {
        public List<ContactPerson> ContactPeople { get; set; } = new List<ContactPerson>();

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
                    SimpleInjections.MakeWhatsAppVideoCall(contactNumber.Number);
                }
                else
                {
                    await Permissions.RequestAsync<Permissions.Phone>();

                    SimpleInjections.CallNumber(contactNumber.Number);
                }
            });

            PlayDisplayNameCommand = new Command<ContactPerson>(async contact =>
            {
                SimpleInjections.PlayVoice($"http://api.farsireader.com/ArianaCloudService/ReadTextGET?APIKey=$JPK7Q2IRVPJHWKQ5A&Text={contact.DisplayName}&Speaker=Female1&Format=mp3");
            });
        }

        protected async override void OnAppearing()
        {
            base.OnAppearing();

            await Permissions.RequestAsync<Permissions.ContactsRead>();

            ContactPeople = SimpleInjections.GetContactPeople();

            try
            {
                HttpClient client = new HttpClient();

                HttpResponseMessage response = await client.GetAsync("https://google.com/");

                if (response.IsSuccessStatusCode)
                {
                    await response.Content.ReadAsStreamAsync();

                    ConnectivityStatus = "😊";
                }
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

        public List<ContactNumber> Numbers { get; set; } = new List<ContactNumber> { };

        public ImageSource ImageUri { get; set; }
    }

    public class ContactNumber
    {
        public int ContactId { get; set; }

        public string Number { get; set; }

        public string Type { get; set; }
    }

    public static class SimpleInjections
    {
        public static Func<List<ContactPerson>> GetContactPeople;

        public static Action<string> CallNumber;

        public static Action<string> MakeWhatsAppVideoCall;

        public static Action<string> PlayVoice;
    }
}