﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Threading.Tasks;
using System.Net;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI;
using Windows.UI.Core;
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
        private CoreDispatcher dispatcher;
        private List<Skype4Sharp.Chat> recent;
        private List<Skype4Sharp.User> contacts;
        public MainPage()
        {
            this.InitializeComponent();
            this.NavigationCacheMode = NavigationCacheMode.Enabled;
            dispatcher = CoreWindow.GetForCurrentThread().Dispatcher;
        }

        private async void Page_Loaded(object sender, RoutedEventArgs e)
        {
            if (App.mainSkype == null)
            {
                CookieContainer cookieContainer = await CookieManager.ReadCookiesFromDisk(App.cookieFilename);
                App.mainSkype = new Skype4Sharp.Skype4Sharp(cookieContainer);
                if (await App.mainSkype.Login())
                {
                    CookieManager.WriteCookiesToDisk(App.cookieFilename, App.mainSkype.mainCookies);
                    App.mainSkype.messageReceived += messageReceived;
                    App.mainSkype.StartPoll();
                }
                else
                {
                    Frame.Navigate(typeof(LoginPage));
                    return;
                }
            }
            if (!App.mainSkype.isPolling)
            {
                App.mainSkype.messageReceived += messageReceived;
                App.mainSkype.StartPoll();
            }
            selfAvatarImage.Source = new BitmapImage(App.mainSkype.selfProfile.AvatarUri);
            recent = await App.mainSkype.GetRecent();
            recentListView.ItemsSource = recent;
            contacts = await App.mainSkype.GetContacts();
            peopleListView.ItemsSource = contacts;
            progressBar.Visibility = Visibility.Collapsed;
        }

        private void profileButton_Click(object sender, RoutedEventArgs e)
        {
            Frame.Navigate(typeof(ProfilePage));
        }

        private void recentListView_ItemClick(object sender, ItemClickEventArgs e)
        {
            Frame.Navigate(typeof(ChatPage));
        }

        private void peopleListView_ItemClick(object sender, ItemClickEventArgs e)
        {
            Frame.Navigate(typeof(ChatPage));
        }

        private async void messageReceived(Skype4Sharp.ChatMessage pMessage)
        {
            recent = await App.mainSkype.GetRecent();
            foreach (Skype4Sharp.Chat chat in recent)
            {
                if (chat.ID == pMessage.Chat.ID)
                {
                    chat.Unread = true;
                    break;
                }
            }
            await dispatcher.RunAsync(CoreDispatcherPriority.Normal, () => {
                recentListView.ItemsSource = recent;
            });
        }
    }

    class UnreadToColorConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, string language)
        {
            var val = (bool)value;
            return val ? new SolidColorBrush(Colors.Orange) : new SolidColorBrush(Colors.Black);
        }

        // No need to implement converting back on a one-way binding 
        public object ConvertBack(object value, Type targetType,
            object parameter, string language)
        {
            throw new NotImplementedException();
        }
    }
}
