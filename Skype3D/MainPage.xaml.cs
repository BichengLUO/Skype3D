using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Threading.Tasks;
using System.Net;
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
using Skype3D.Utility;

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
            this.NavigationCacheMode = NavigationCacheMode.Enabled;
        }

        private async void Page_Loaded(object sender, RoutedEventArgs e)
        {
            if (App.mainSkype == null)
            {
                CookieContainer cookieContainer = await CookieManager.ReadCookiesFromDisk(App.cookieFilename);
                App.mainSkype = new Skype4Sharp.Skype4Sharp(cookieContainer);
                if (await Task.Run(() => App.mainSkype.Login()))
                    CookieManager.WriteCookiesToDisk(App.cookieFilename, App.mainSkype.mainCookies);
                else
                {
                    Frame.Navigate(typeof(LoginPage));
                    return;
                }
            }
            selfAvatarImage.Source = new BitmapImage(App.mainSkype.selfProfile.AvatarUri);
            List<Skype4Sharp.Chat> recent = await Task.Run(() => App.mainSkype.GetRecent());
            recentListView.ItemsSource = recent;
            List<Skype4Sharp.User> contacts = await Task.Run(() => App.mainSkype.GetContacts());
            peopleListView.ItemsSource = contacts;
            progressBar.Visibility = Visibility.Collapsed;
        }

        private void profileButton_Click(object sender, RoutedEventArgs e)
        {
            Frame.Navigate(typeof(ProfilePage));
        }
    }
}
