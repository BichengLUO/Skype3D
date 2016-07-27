using System;
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

            SystemNavigationManager.GetForCurrentView().BackRequested += App_BackRequested;
        }

        private async void Page_Loaded(object sender, RoutedEventArgs e)
        {
            if (App.mainSkype == null)
            {
                Skype4Sharp.Auth.Tokens tokens = await CookieManager.ReadTokenFromDisk(App.tokensFilename);
                App.mainSkype = new Skype4Sharp.Skype4Sharp(tokens);
                if (await App.mainSkype.Login())
                {
                    CookieManager.WriteTokenToDisk(App.tokensFilename, App.mainSkype.authTokens);
                    App.mainSkype.messageReceived += messageReceived;
                    App.mainSkype.StartPoll();
                }
                else
                {
                    Frame.Navigate(typeof(LoginPage));
                    return;
                }
                recent = null;
                contacts = null;
            }
            Frame.BackStack.Clear();
            if (!App.mainSkype.isPolling)
            {
                App.mainSkype.messageReceived += messageReceived;
                App.mainSkype.StartPoll();
            }
            selfAvatarImage.Source = new BitmapImage(await CharacterUtil.CharacterManager.GetCharAvatarForUser(App.mainSkype.selfProfile));
            if (recent == null || App.recentNeedUpdate)
            {
                recent = await App.mainSkype.GetRecent();
                await CharacterUtil.CharacterManager.GetCharAvatarUrlsForChats(recent);
                recentListView.ItemsSource = recent;
                App.recentNeedUpdate = false;
                refreshUnreadCount();
            }
            if (contacts == null)
            {
                contacts = await App.mainSkype.GetContacts();
                await CharacterUtil.CharacterManager.GetCharAvatarUrlsForUsers(contacts);
                peopleListView.ItemsSource = contacts;
            }
            progressBar.Visibility = Visibility.Collapsed;
            if (CharacterUtil.Words2Anim.words2Anim.Count == 0)
                await CharacterUtil.Words2Anim.LoadVocabulary();
        }

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            // Remove the UI from the title bar if in-app back stack is empty.
            SystemNavigationManager.GetForCurrentView().AppViewBackButtonVisibility =
                AppViewBackButtonVisibility.Collapsed;
        }

        private void App_BackRequested(object sender, BackRequestedEventArgs e)
        {
            Frame rootFrame = Window.Current.Content as Frame;
            if (rootFrame == null)
                return;

            // Navigate back if possible, and if the event has not 
            // already been handled .
            if (rootFrame.CanGoBack && e.Handled == false)
            {
                Frame.GoBack();
                e.Handled = true;
            }
        }

        private void profileButton_Click(object sender, RoutedEventArgs e)
        {
            Frame.Navigate(typeof(ProfilePage), App.mainSkype.selfProfile);
        }

        private void recentListView_ItemClick(object sender, ItemClickEventArgs e)
        {
            Skype4Sharp.Chat chat = (Skype4Sharp.Chat)e.ClickedItem;
            enterInChat(chat);
        }

        private void peopleListView_ItemClick(object sender, ItemClickEventArgs e)
        {
            Skype4Sharp.User user = (Skype4Sharp.User)e.ClickedItem;
            foreach (Skype4Sharp.Chat chat in recent)
            {
                if (chat.ChatLink.Contains(user.Username))
                {
                    enterInChat(chat);
                    return;
                }
            }
            Frame.Navigate(typeof(ChatPage), user);
        }

        private void enterInChat(Skype4Sharp.Chat chat)
        {
            chat.Unread = false;
            if (App.unreadRecord.ContainsKey(chat.ID))
                App.unreadRecord.Remove(chat.ID);
            refreshUnreadCount();

            recentListView.ItemsSource = null;
            recentListView.ItemsSource = recent;
            App.recentNeedUpdate = false;
            Frame.Navigate(typeof(ChatPage), chat);
        }

        private async void messageReceived(Skype4Sharp.ChatMessage pMessage)
        {
            recent = await App.mainSkype.GetRecent();
            await CharacterUtil.CharacterManager.GetCharAvatarUrlsForChats(recent);
            if (App.unreadRecord.ContainsKey(pMessage.Chat.ID))
                App.unreadRecord[pMessage.Chat.ID]++;
            else
                App.unreadRecord[pMessage.Chat.ID] = 1;
            
            foreach (Skype4Sharp.Chat chat in recent)
            {
                if (App.unreadRecord.ContainsKey(chat.ID))
                    chat.Unread = true;
            }
            await dispatcher.RunAsync(CoreDispatcherPriority.Normal, () => {
                recentListView.ItemsSource = recent;
                App.recentNeedUpdate = false;
                refreshUnreadCount();
            });
        }

        private void refreshUnreadCount()
        {
            int totalUnreadCount = 0;
            foreach (KeyValuePair<string, int> entry in App.unreadRecord)
                totalUnreadCount += entry.Value;
            unreadCountBlock.Text = totalUnreadCount.ToString();
            if (totalUnreadCount == 0)
                unreadCountLabel.Visibility = Visibility.Collapsed;
            else
                unreadCountLabel.Visibility = Visibility.Visible;
        }
    }

    public class UnreadToColorConverter : IValueConverter
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
