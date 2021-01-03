using System.Collections.ObjectModel;
using Xamarin.Essentials;

namespace GrandmaMobileApp
{
    public partial class MainPage 
    {
        public ObservableCollection<Contact> Contacts { get; set; } = new ObservableCollection<Contact>();

        public MainPage()
        {
            InitializeComponent();            
        }

        protected async override void OnAppearing()
        {
            base.OnAppearing();

            await Permissions.RequestAsync<Permissions.ContactsRead>();

            Contacts = new ObservableCollection<Contact>(await Xamarin.Essentials.Contacts.GetAllAsync(default));
        }
    }
}