using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Threading.Tasks;
using System.Diagnostics;
using Windows.Foundation;
using Windows.Foundation.Collections;
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
        private WinRTBridge.WinRTBridge _bridge;

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
        }

        private void exitButton_Click(object sender, RoutedEventArgs e)
        {
            Frame.GoBack();
        }

        private async void Page_Loaded(object sender, RoutedEventArgs e)
        {
            await waitForLevel();
            unityMask.Visibility = Visibility.Collapsed;
            progressBar.Visibility = Visibility.Collapsed;
        }

        private async Task<bool> waitForLevel()
        {
            while (!Interoperation.levelLoaded)
                await Task.Delay(1000);
            return Interoperation.levelLoaded;
        }
    }
}
