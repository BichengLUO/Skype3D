﻿using System;
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

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            showBackButton();
            user = (Skype4Sharp.User)e.Parameter;
            nameBlock.Text = user.DisplayName;
            if (user.Username != App.mainSkype.selfProfile.Username)
                signOutButton.Visibility = Visibility.Collapsed;
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

        private void updateSelectionForImg(int ind)
        {
            selection.Margin = new Thickness((ind + 0.5) * 150 + charactersSelectionPanel.Margin.Left - selection.ActualWidth / 2.0, 0, 0, 0);
            selectionPop.Begin();
        }

        private async void characterImg_Tapped(object sender, TappedRoutedEventArgs e)
        {
            Image characterImg = (Image)sender;
            int ind = charactersSelectionPanel.Children.IndexOf(characterImg);
            updateSelectionForImg(ind);
            int charID = charactersSelectionPanel.Children.IndexOf(characterImg);
            await CharacterUtil.CharacterManager.SetCharacter(charID);
        }

        private async void Page_Loaded(object sender, RoutedEventArgs e)
        {
            int ind = await CharacterUtil.CharacterManager.GetCharIDForUser(user);
            updateSelectionForImg(ind);

            double offset = selection.Margin.Left - (charactersSelectionViewer.ActualWidth - selection.ActualWidth) / 2.0;
            charactersSelectionViewer.ChangeView(offset, null, null, false);
        }
    }
}
