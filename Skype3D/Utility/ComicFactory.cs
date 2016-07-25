using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.Foundation;
using Windows.UI;
using Windows.UI.Text;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Shapes;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Media.Imaging;
using Windows.Storage;

namespace Skype3D.Utility
{
    class ComicFactory
    {
        private static Random rnd = new Random();
        public async static Task fillGridWithMessage(Grid grid, Skype4Sharp.ChatMessage message)
        {
            int charID = await CharacterUtil.CharacterManager.GetCharIDForUser(message.Sender);
            string animName = CharacterUtil.Words2Anim.convertToAnim(message.Body);
            string fileName = "Assets/comics/" + charID.ToString() + "_" + animName + "_" + rnd.Next(5) + ".png";
            string fileName2 = @"Assets\comics\" + charID.ToString() + "_" + animName + "_" + rnd.Next(5) + ".png";
            if (!await fileExists(fileName2))
                fileName = "Assets/comics/" + charID.ToString() + "_embar_00_" + rnd.Next(5) + ".png";

            Rectangle rect = new Rectangle();
            rect.Stroke = new SolidColorBrush(Colors.DarkGray);
            rect.StrokeThickness = 3;
            ImageBrush brush = new ImageBrush();
            brush.ImageSource = new BitmapImage(new Uri("ms-appx:///" + fileName));
            brush.Stretch = Stretch.UniformToFill;
            rect.Fill = brush;
            rect.Margin = new Thickness(5);

            TextBlock content = new TextBlock();
            content.TextWrapping = TextWrapping.Wrap;
            content.FontWeight = FontWeights.SemiBold;
            content.Text = message.Body;
            content.Margin = new Thickness(10);

            StackPanel panel = new StackPanel();
            panel.Background = new SolidColorBrush(Colors.Orange);
            panel.Opacity = 0.8;
            panel.Children.Add(content);

            Polygon tri = new Polygon();
            tri.Fill = new SolidColorBrush(Colors.Orange);
            tri.Opacity = 0.8;
            tri.HorizontalAlignment = HorizontalAlignment.Center;

            Grid panelGrid = new Grid();
            panelGrid.MaxWidth = 200;
            panelGrid.Margin = new Thickness(20);
            panelGrid.Children.Add(panel);
            panelGrid.Children.Add(tri);

            int pos = rnd.Next(2);
            panel.Margin = new Thickness(0, 15, 0, 0);
            tri.VerticalAlignment = VerticalAlignment.Top;
            tri.Points.Add(new Point(-10, 15));
            tri.Points.Add(new Point(10, 15));
            switch (pos)
            {
                case 0:
                    panelGrid.VerticalAlignment = VerticalAlignment.Bottom;
                    panelGrid.HorizontalAlignment = HorizontalAlignment.Left;
                    tri.Points.Add(new Point(15, 0));
                    break;
                case 1:
                    panelGrid.VerticalAlignment = VerticalAlignment.Bottom;
                    panelGrid.HorizontalAlignment = HorizontalAlignment.Right;
                    tri.Points.Add(new Point(-15, 0));
                    break;
            }
            
            grid.Children.Add(rect);
            grid.Children.Add(panelGrid);
        }

        public static async Task<bool> fileExists(string fileName)
        {
            StorageFolder appInstalledFolder = Windows.ApplicationModel.Package.Current.InstalledLocation;
            try { StorageFile file = await appInstalledFolder.GetFileAsync(fileName); }
            catch { return false; }
            return true;
        }
    }
}
