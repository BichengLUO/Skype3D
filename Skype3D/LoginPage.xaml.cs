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
using Skype4Sharp.Auth;
using Skype3D.Utility;

// The Blank Page item template is documented at http://go.microsoft.com/fwlink/?LinkId=402352&clcid=0x409

namespace Skype3D
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class LoginPage : Page
    {
        public LoginPage()
        {
            this.InitializeComponent();
            Frame.BackStack.Clear();
        }

        private async void signInButton_Click(object sender, RoutedEventArgs e)
        {
            string authUser = skypeNameBox.Text;
            string authPass = passwordBox.Password;
            App.mainSkype = new Skype4Sharp.Skype4Sharp(new SkypeCredentials(authUser, authPass));
            progressBar.Visibility = Visibility.Visible;
            if (await App.mainSkype.Login())
            {
                signInInfoBlock.Text = "Signed in!";
                CookieManager.WriteCookiesToDisk(App.cookieFilename, App.mainSkype.mainCookies);
                Frame.Navigate(typeof(MainPage));
            }
            else
                signInInfoBlock.Text = "Oops, please check your details";
            progressBar.Visibility = Visibility.Collapsed;
        }
    }
}
