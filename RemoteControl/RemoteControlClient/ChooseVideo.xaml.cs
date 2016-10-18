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
    public sealed partial class ChooseVideo : Page
    {

        RemoteControlClientProtocol _controlProtocol = null;
        public ChooseVideo()
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
            _controlProtocol = e.Parameter as RemoteControlClientProtocol;
        }

        private void ReturnB_Click(object sender, RoutedEventArgs e)
        {
            Frame.Navigate(typeof(MainPage),_controlProtocol);
        }


        private async void SynchronizeVideos_Click(object sender, RoutedEventArgs e)
        {
            this.ProgressRing.IsActive = true;
            string result = null;
            result = await _controlProtocol.ProcessCommandWithResponse("SynchronizeVideos");
            String[] movies = result.Split('#');
            VideosLB.ItemsSource = movies.ToList();
            this.ProgressRing.IsActive = false;
            this.ProgressGrid.Visibility = Visibility.Collapsed;
        }


        private async void VideosLB_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            string item = (string)this.VideosLB.SelectedItem;
            bool result = await _controlProtocol.ProcessCommand("PlayMPC:#" + item);
        }
    }
}
