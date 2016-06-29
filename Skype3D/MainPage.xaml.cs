using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Threading.Tasks;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;
using Windows.UI.Xaml.Media.Imaging;

// The Blank Page item template is documented at http://go.microsoft.com/fwlink/?LinkId=234238

namespace Skype3D
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class MainPage : Page
    {
        public MainPage()
        {
            this.InitializeComponent();
            selfAvatarImage.Source = new BitmapImage(App.mainSkype.selfProfile.AvatarUri);
        }

        private async void Page_Loaded(object sender, RoutedEventArgs e)
        {
            List<Skype4Sharp.Chat> recent = await Task.Run(() => App.mainSkype.GetRecent());
            recentListView.ItemsSource = recent;
            List<Skype4Sharp.User> contacts = await Task.Run(() => App.mainSkype.GetContacts());
            peopleListView.ItemsSource = contacts;
        }
    }
}
