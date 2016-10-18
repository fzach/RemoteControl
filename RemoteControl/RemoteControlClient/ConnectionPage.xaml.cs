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
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;

// The Blank Page item template is documented at http://go.microsoft.com/fwlink/?LinkID=390556

namespace RemoteControlClient
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class ConnectionPage : Page
    {
        RemoteControlClientProtocol ControlProtocol; 

        public ConnectionPage()
        {
            this.InitializeComponent();
        }

        /// <summary>
        /// Invoked when this page is about to be displayed in a Frame.
        /// </summary>
        /// <param name="e">Event data that describes how this page was reached.
        /// This parameter is typically used to configure the page.</param>
        protected override void OnNavigatedTo(NavigationEventArgs e)
        {

        }


        private async void Connect_Click(object sender, RoutedEventArgs e)
        {
            ControlProtocol = new RemoteControlClientProtocol(ServerHostName.Text, ServerPort.Text);
            this.ProgressRing.IsActive = true;
            this.MainGrid.Opacity = 0.5;
            this.ProgressGrid.Visibility = Visibility.Visible;
            bool connected = await ControlProtocol.Connect();
            if (connected)
            {
                StatusText.Text = "Connected";
            }
            else StatusText.Text = "Error while connecting";
            this.ProgressRing.IsActive = false;
            this.ProgressGrid.Visibility = Visibility.Collapsed;
            this.MainGrid.Opacity = 1;
        }

        private void ReturnB_Click(object sender, RoutedEventArgs e)
        {
            Frame.Navigate(typeof(MainPage), ControlProtocol);
        }
    }
}
