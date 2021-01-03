using Xamarin.Forms;

namespace GrandmaMobileApp
{
    public partial class App
    {
        public App()
        {
            InitializeComponent();

            MainPage = new NavigationPage(new MainPage());
        }
    }
}
