using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI.Core;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;

// The Blank Page item template is documented at http://go.microsoft.com/fwlink/?LinkId=234238

namespace Skype3D
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class ComicsPage : Page
    {
        private Skype4Sharp.Chat chat;
        private List<Skype4Sharp.ChatMessage> messages;
        private Random rand = new Random();
        public ComicsPage()
        {
            this.InitializeComponent();
        }

        protected override async void OnNavigatedTo(NavigationEventArgs e)
        {
            refreshUnread();
            showBackButton();
            if (e.Parameter is Skype4Sharp.Chat)
            {
                chat = (Skype4Sharp.Chat)e.Parameter;
                chatTopicBlock.Text = chat.Topic;
                avatarBitmap.UriSource = chat.CharAvatarUri;
                messages = await chat.getMessageHistory();
            }
            progressBar.Visibility = Visibility.Collapsed;
            await fillCanvas();
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

        private void exitButton_Click(object sender, RoutedEventArgs e)
        {
            Frame.GoBack();
        }

        private async Task fillCanvas()
        {
            double width = comicsCanvas.ActualWidth;
            double height = comicsCanvas.ActualHeight;
            int rows = 3;
            int cols = 2;
            double y = 0;
            int k = 0;
            for (int i = 0; i < rows; i++)
            {
                double x = 0;
                double rowHeight = height / rows + rand.Next(60) - 30;
                for (int j = 0; j < cols; j++)
                {
                    double cellwidth = width / cols + rand.Next(60) - 30;
                    Grid grid = new Grid();
                    grid.Width = j == cols - 1 ? width - x : cellwidth;
                    grid.Height = i == rows - 1 ? height - y : rowHeight;
                    await Utility.ComicFactory.fillGridWithMessage(grid, messages[k++]);
                    Canvas.SetLeft(grid, x);
                    Canvas.SetTop(grid, y);
                    comicsCanvas.Children.Add(grid);
                    x += cellwidth;
                }
                y += rowHeight;
            }
        }
    }
}
