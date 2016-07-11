using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI;
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
    public sealed partial class HistoryPage : Page
    {
        private Skype4Sharp.Chat chat;
        private List<Skype4Sharp.ChatMessage> messages;
        public HistoryPage()
        {
            this.InitializeComponent();
        }

        protected override async void OnNavigatedTo(NavigationEventArgs e)
        {
            int totalUnreadCount = 0;
            foreach (KeyValuePair<string, int> entry in App.unreadRecord)
                totalUnreadCount += entry.Value;
            if (totalUnreadCount == 0)
                unreadMark.Visibility = Visibility.Collapsed;
            else
                unreadMark.Visibility = Visibility.Visible;
            if (e.Parameter is Skype4Sharp.Chat)
            {
                chat = (Skype4Sharp.Chat)e.Parameter;
                chatTopicBlock.Text = chat.Topic;
                avatarBitmap.UriSource = chat.AvatarUri;
                messages = await chat.getMessageHistory();
            }
            
            historyListView.ItemsSource = messages;
            historyListView.ScrollIntoView(historyListView.Items[historyListView.Items.Count - 1]);
            progressBar.Visibility = Visibility.Collapsed;
        }

        private void exitButton_Click(object sender, RoutedEventArgs e)
        {
            Frame.GoBack();
        }
    }

    public class SelfColorConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, string language)
        {
            var user = (Skype4Sharp.User)value;
            return user.Username == App.mainSkype.selfProfile.Username ? new SolidColorBrush(Colors.PowderBlue) : new SolidColorBrush(Colors.LightCyan);
        }

        // No need to implement converting back on a one-way binding 
        public object ConvertBack(object value, Type targetType,
            object parameter, string language)
        {
            throw new NotImplementedException();
        }
    }

    public class SelfAlignmentConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, string language)
        {
            var user = (Skype4Sharp.User)value;
            return user.Username == App.mainSkype.selfProfile.Username ? HorizontalAlignment.Right : HorizontalAlignment.Left;
        }

        // No need to implement converting back on a one-way binding 
        public object ConvertBack(object value, Type targetType,
            object parameter, string language)
        {
            throw new NotImplementedException();
        }
    }

    public class SelfBubblePointsConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, string language)
        {
            var points = new PointCollection();
            var user = (Skype4Sharp.User)value;
            if (user.Username == App.mainSkype.selfProfile.Username)
            {
                points.Add(new Point(160, 0));
                points.Add(new Point(180, 0));
                points.Add(new Point(180, 20));
            }
            else
            {
                points.Add(new Point(20, 0));
                points.Add(new Point(40, 0));
                points.Add(new Point(20, 20));
            }
            return points;
        }

        // No need to implement converting back on a one-way binding 
        public object ConvertBack(object value, Type targetType,
            object parameter, string language)
        {
            throw new NotImplementedException();
        }
    }
}
