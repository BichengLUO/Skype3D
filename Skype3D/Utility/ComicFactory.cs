using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.UI;
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
            rect.Stroke = new SolidColorBrush(Colors.Black);
            rect.StrokeThickness = 3;
            ImageBrush brush = new ImageBrush();
            brush.ImageSource = new BitmapImage(new Uri("ms-appx:///" + fileName));
            brush.Stretch = Stretch.UniformToFill;
            rect.Fill = brush;
            rect.Margin = new Thickness(5);

            grid.Children.Add(rect);
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
