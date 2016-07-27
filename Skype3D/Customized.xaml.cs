using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media.Imaging;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;

// The Blank Page item template is documented at http://go.microsoft.com/fwlink/?LinkId=234238

namespace Skype3D
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class Customized : Page
    {
        private int currentClothesInd = 0;
        public Customized()
        {
            this.InitializeComponent();
        }

        private void exitButton_Click(object sender, TappedRoutedEventArgs e)
        {
            Frame.GoBack();
        }

        private void updateSelectionForImg(int ind)
        {
            selection.Margin = new Thickness((ind + 0.5) * 100 - 10, 0, 0, 0);
            selectionPop.Begin();
        }

        private void Image_Tapped(object sender, TappedRoutedEventArgs e)
        {
            Image characterImg = (Image)sender;
            currentClothesInd = clothesSelectionPanel.Children.IndexOf(characterImg);
            currentClothesImage.Source = new BitmapImage(new Uri(string.Format("ms-appx:///Assets/customized/Boy_{0:00}.png", currentClothesInd + 1)));
            updateSelectionForImg(currentClothesInd);
        }
    }
}
