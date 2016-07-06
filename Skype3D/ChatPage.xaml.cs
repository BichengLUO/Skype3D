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
        }
        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            if (e.Parameter is Skype4Sharp.Chat)
            {
                chat = (Skype4Sharp.Chat)e.Parameter;
                chatTopicBlock.Text = chat.Topic;
            }
            else if (e.Parameter is Skype4Sharp.User)
            {
                user = (Skype4Sharp.User)e.Parameter;
                chatTopicBlock.Text = user.DisplayName;
            }
        }

        private void exitButton_Click(object sender, RoutedEventArgs e)
        {
            Frame.GoBack();
        }

        private async void Page_Loaded(object sender, RoutedEventArgs e)
        {
            senderBubble.Visibility = Visibility.Collapsed;
            receiverBubble.Visibility = Visibility.Collapsed;
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
            string messageText = messageTextBox.Text;
            if (chat != null)
                await App.mainSkype.SendMessage(chat, messageText);
            else if (user != null)
                await App.mainSkype.SendMessage(user, messageText);
            messageTextBox.Text = "";
            sentMessageBlock.Text = messageText;
            senderNameBlock.Text = App.mainSkype.selfProfile.DisplayName;
            senderBubble.Visibility = Visibility.Visible;
        }

        private async void messageReceived(Skype4Sharp.ChatMessage pMessage)
        {
            if ((chat != null && pMessage.Chat.ID == chat.ID) ||
                (user != null && pMessage.Sender.Username == user.Username))
            {
                await dispatcher.RunAsync(CoreDispatcherPriority.Normal, () =>
                {
                    receivedMessageBlock.Text = pMessage.getBody();
                    receiverNameBlock.Text = pMessage.Sender.DisplayName;
                    receiverBubble.Visibility = Visibility.Visible;
                });
            }
        }
    }
}
