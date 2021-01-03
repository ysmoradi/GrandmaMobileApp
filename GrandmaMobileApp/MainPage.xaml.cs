using System;
using System.Collections.Generic;
using Xamarin.Essentials;
using Xamarin.Forms;

namespace GrandmaMobileApp
{
    public partial class MainPage
    {
        public List<ContactPerson> ContactPeople { get; set; } = new List<ContactPerson>();

        public Command<ContactNumber> CallNumberCommand { get; set; }

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
        }

        protected async override void OnAppearing()
        {
            base.OnAppearing();

            await Permissions.RequestAsync<Permissions.ContactsRead>();

            ContactPeople = SimpleInjections.GetContactPeople();
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
    }
}