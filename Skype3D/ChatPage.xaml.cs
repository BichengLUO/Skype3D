using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Threading.Tasks;
using System.Diagnostics;
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
using Windows.System;
using UnityPlayer;

// The Blank Page item template is documented at http://go.microsoft.com/fwlink/?LinkId=234238

namespace Skype3D
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class ChatPage : Page
    {
        private CoreDispatcher dispatcher;
        private WinRTBridge.WinRTBridge _bridge;
        private Skype4Sharp.Chat chat;
        private Skype4Sharp.User user;

        public ChatPage()
        {
            this.InitializeComponent();
            this.NavigationCacheMode = NavigationCacheMode.Enabled;

            AppCallbacks appCallbacks = AppCallbacks.Instance;
            // Setup scripting bridge
            _bridge = new WinRTBridge.WinRTBridge();
            appCallbacks.SetBridge(_bridge);
            appCallbacks.SetKeyboardTriggerControl(this);
            appCallbacks.SetSwapChainPanel(DXSwapChainPanel);
            appCallbacks.SetCoreWindowEvents(Window.Current.CoreWindow);
            appCallbacks.InitializeD3DXAML();
            dispatcher = CoreWindow.GetForCurrentThread().Dispatcher;
            App.mainSkype.messageReceived += messageReceived;

            SystemNavigationManager.GetForCurrentView().BackRequested += App_BackRequested;
        }
        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            if (e.Parameter is Skype4Sharp.Chat)
            {
                chat = (Skype4Sharp.Chat)e.Parameter;
                chatTopicBlock.Text = chat.Topic;
                avatarBitmap.UriSource = chat.AvatarUri;
            }
            else if (e.Parameter is Skype4Sharp.User)
            {
                user = (Skype4Sharp.User)e.Parameter;
                chatTopicBlock.Text = user.DisplayName;
                avatarBitmap.UriSource = user.AvatarUri;
                historyButton.Visibility = Visibility.Collapsed;
            }
            int totalUnreadCount = 0;
            foreach (KeyValuePair<string, int> entry in App.unreadRecord)
                totalUnreadCount += entry.Value;
            if (totalUnreadCount == 0)
                unreadMark.Visibility = Visibility.Collapsed;
            else
                unreadMark.Visibility = Visibility.Visible;
        }

        private void exitButton_Click(object sender, RoutedEventArgs e)
        {
            clearUnreadBeforeGoBack();
        }

        private void clearUnreadBeforeGoBack()
        {
            if (chat != null && App.unreadRecord.ContainsKey(chat.ID))
                App.unreadRecord.Remove(chat.ID);
            Frame.GoBack();
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
                e.Handled = true;
                clearUnreadBeforeGoBack();
            }
        }

        private async void Page_Loaded(object sender, RoutedEventArgs e)
        {
            await waitForLevel();
            unityMask.Visibility = Visibility.Collapsed;
            progressBar.Visibility = Visibility.Collapsed;
        }

        private async Task waitForLevel()
        {
            while (!Interoperation.levelLoaded)
                await Task.Delay(1000);
        }

        private async void sendButton_Click(object sender, RoutedEventArgs e)
        {
            await sendMessage();
        }

        private async Task sendMessage()
        {
            string messageText = messageTextBox.Text;
            if (messageText != "")
            {
                messageTextBox.Text = "";
                sentMessageBlock.Text = messageText;
                senderNameBlock.Text = App.mainSkype.selfProfile.DisplayName;
                senderBubblePop.Begin();
                if (chat != null)
                    await App.mainSkype.SendMessage(chat, messageText);
                else if (user != null)
                    await App.mainSkype.SendMessage(user, messageText);
                App.recentNeedUpdate = true;
            }
        }

        private async void messageReceived(Skype4Sharp.ChatMessage pMessage)
        {
            if ((chat != null && pMessage.Chat.ID == chat.ID) ||
                (user != null && pMessage.Sender.Username == user.Username))
            {
                await dispatcher.RunAsync(CoreDispatcherPriority.Normal, () =>
                {
                    receivedMessageBlock.Text = pMessage.Body;
                    receiverNameBlock.Text = pMessage.Sender.DisplayName;
                    receiverBubblePop.Begin();
                });
            }
            else
            {
                await dispatcher.RunAsync(CoreDispatcherPriority.Normal, () =>
                {
                    unreadMark.Visibility = Visibility.Visible;
                });
            }
        }

        private async void messageTextBox_KeyDown(object sender, KeyRoutedEventArgs e)
        {
            if (e.Key == VirtualKey.Enter)
            {
                await sendMessage();
            }
        }

        private void historyButton_Click(object sender, RoutedEventArgs e)
        {
            Frame.Navigate(typeof(HistoryPage), chat);
        }
    }
}
