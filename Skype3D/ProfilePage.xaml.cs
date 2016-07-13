using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Threading.Tasks;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI.Xaml;
using Windows.UI.Core;
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
    public sealed partial class ProfilePage : Page
    {
        private Skype4Sharp.User user;
        public ProfilePage()
        {
            this.InitializeComponent();
        }

        protected override async void OnNavigatedTo(NavigationEventArgs e)
        {
            showBackButton();
            user = (Skype4Sharp.User)e.Parameter;
            nameBlock.Text = user.DisplayName;
            if (user.Username != App.mainSkype.selfProfile.Username)
                signOutButton.Visibility = Visibility.Collapsed;

            int charID = await CharacterUtil.CharacterManager.GetCharacter(user);
            if (charID == CharacterUtil.CharacterManager.NotFound)
                charID = 0;
            Image characterImg = (Image)charactersSelectionPanel.Children[charID];
            updateSelectionForImg(characterImg);
        }

        private void showBackButton()
        {
            Frame rootFrame = Window.Current.Content as Frame;
            if (rootFrame.CanGoBack)
            {
                // Show UI in title bar if opted-in and in-app backstack is not empty.
                SystemNavigationManager.GetForCurrentView().AppViewBackButtonVisibility =
                    AppViewBackButtonVisibility.Visible;
            }
            else
            {
                // Remove the UI from the title bar if in-app back stack is empty.
                SystemNavigationManager.GetForCurrentView().AppViewBackButtonVisibility =
                    AppViewBackButtonVisibility.Collapsed;
            }
        }

        private void exitButton_Click(object sender, RoutedEventArgs e)
        {
            Frame.GoBack();
        }

        private async void signOutButton_Click(object sender, RoutedEventArgs e)
        {
            await App.mainSkype.Logout();
            CookieManager.RemoveFile(App.cookieFilename);
            Frame.Navigate(typeof(LoginPage));
        }

        private void charactersSelectionViewer_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            charactersSelectionPanel.Height = charactersSelectionViewer.ViewportHeight;
        }

        private async void characterImg_PointerPressed(object sender, PointerRoutedEventArgs e)
        {
            Image characterImg = (Image)sender;
            updateSelectionForImg(characterImg);
            int charID = charactersSelectionPanel.Children.IndexOf(characterImg);
            await CharacterUtil.CharacterManager.SetCharacter(charID);
        }

        private void updateSelectionForImg(Image characterImg)
        {
            var ttf = characterImg.TransformToVisual(charactersSelectionPanel);
            Point pos = ttf.TransformPoint(new Point(characterImg.ActualWidth / 2.0, 0));
            selection.Margin = new Thickness(pos.X + charactersSelectionPanel.Margin.Left - selection.ActualWidth / 2.0, 0, 0, 0);
            selectionPop.Begin();
        }
    }
}
