﻿using System;
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
        }
        
        protected override async void OnNavigatedTo(NavigationEventArgs e)
        {
            senderBubble.Opacity = receiverBubble.Opacity = 0;
            if (e.Parameter is Skype4Sharp.Chat)
            {
                chat = (Skype4Sharp.Chat)e.Parameter;
                chatTopicBlock.Text = chat.Topic;
                avatarBitmap.UriSource = chat.CharAvatarUri;
                historyButton.Visibility = Visibility.Visible;
                if (chat.LastMessage.Sender.Username == App.mainSkype.selfProfile.Username)
                {
                    if (chat.LastMessage.Body != null)
                    {
                        sentMessageBlock.Text = chat.LastMessage.Body;
                        senderNameBlock.Text = App.mainSkype.selfProfile.DisplayName;
                        senderBubblePop.Begin();
                    }
                }
                else
                {
                    if (chat.LastMessage.Body != null)
                    {
                        receivedMessageBlock.Text = chat.LastMessage.Body;
                        receiverNameBlock.Text = chat.LastMessage.Sender.DisplayName;
                        receiverBubblePop.Begin();
                    }
                }
                user = null;
            }
            else if (e.Parameter is Skype4Sharp.User)
            {
                user = (Skype4Sharp.User)e.Parameter;
                chatTopicBlock.Text = user.DisplayName;
                avatarBitmap.UriSource = user.CharAvatarUri;
                historyButton.Visibility = Visibility.Collapsed;
                comicsButton.Visibility = Visibility.Collapsed;
                chat = null;
            }
            refreshUnread();
            showBackButton();
            await loadCharacter();
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

        private void refreshUnread()
        {
            int totalUnreadCount = 0;
            foreach (KeyValuePair<string, int> entry in App.unreadRecord)
                totalUnreadCount += entry.Value;
            if (totalUnreadCount == 0)
                unreadMark.Visibility = Visibility.Collapsed;
            else
                unreadMark.Visibility = Visibility.Visible;
        }

        private async Task loadCharacter()
        {
            unityMask.Visibility = Visibility.Visible;
            progressBar.Visibility = Visibility.Visible;
            await waitForLevel();
            Interoperation.setCharacterID(await getCharID());
            await waitForCharacterChanged();
            unityMask.Visibility = Visibility.Collapsed;
            progressBar.Visibility = Visibility.Collapsed;
        }

        private async Task<int> getCharID()
        {
            if (chat != null)
            {
                if (chat.Type == Skype4Sharp.Enums.ChatType.Group)
                    return await CharacterUtil.CharacterManager.GetCharIDForUser(chat.LastMessage.Sender);
                else
                    return await CharacterUtil.CharacterManager.GetCharIDForUser(chat.mainParticipant);
            }
            else
                return await CharacterUtil.CharacterManager.GetCharIDForUser(user);
        }

        private void exitButton_Click(object sender, RoutedEventArgs e)
        {
            clearUnreadBeforeGoBack();
            Frame.GoBack();
        }

        private void clearUnreadBeforeGoBack()
        {
            if (chat != null && App.unreadRecord.ContainsKey(chat.ID))
                App.unreadRecord.Remove(chat.ID);
        }

        private async Task waitForLevel()
        {
            while (!Interoperation.levelLoaded)
                await Task.Delay(1000);
        }

        private async Task waitForCharacterChanged()
        {
            while (Interoperation.characterChanged)
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
                int charID = await CharacterUtil.CharacterManager.GetCharIDForUser(pMessage.Sender);
                string animName = CharacterUtil.Words2Anim.convertToAnim(pMessage.Body);
                await dispatcher.RunAsync(CoreDispatcherPriority.Normal, () =>
                {
                    receivedMessageBlock.Text = pMessage.Body;
                    receiverNameBlock.Text = pMessage.Sender.DisplayName;
                    receiverBubblePop.Begin();
                    Interoperation.setCharacterID(charID);
                    Interoperation.setAnimationName(animName);
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
            if (chat != null)
                Frame.Navigate(typeof(HistoryPage), chat);
        }

        private void comicsButton_Click(object sender, RoutedEventArgs e)
        {
            if (chat != null)
                Frame.Navigate(typeof(ComicsPage), chat);
        }
    }
}
