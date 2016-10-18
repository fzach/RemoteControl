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
using Windows.Networking.Sockets;
using Windows.Storage.Streams;
using System.Text;
using System.Threading.Tasks;

// The Blank Page item template is documented at http://go.microsoft.com/fwlink/?LinkId=391641

namespace RemoteControlClient
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class MainPage : Page
    {

        private RemoteControlClientProtocol _controlProtocol;

        Point lastTouchedPoint0 = new Point(0, 0);

        private double deltaX = 0.0;
        private double deltaY = 0.0;

        public MainPage()
        {
            this.InitializeComponent();

            this.NavigationCacheMode = NavigationCacheMode.Required;
        }

        /// <summary>
        /// Invoked when this page is about to be displayed in a Frame.
        /// </summary>
        /// <param name="e">Event data that describes how this page was reached.
        /// This parameter is typically used to configure the page.</param>
        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            // TODO: Prepare page for display here.

            // TODO: If your application contains multiple pages, ensure that you are
            // handling the hardware Back button by registering for the
            // Windows.Phone.UI.Input.HardwareButtons.BackPressed event.
            // If you are using the NavigationHelper provided by some templates,
            // this event is handled for you.
            _controlProtocol = e.Parameter as RemoteControlClientProtocol;
        }

        /// <summary>
        /// Handle the btnEcho_Click event by sending text to the echo server 
        /// and outputting the response
        /// </summary>

        private async void ShutDown_Tapped(object sender, TappedRoutedEventArgs e)
        {
            bool result = await _controlProtocol.ProcessCommand("Bye");
        }

        private async void PlayYoutube_Tapped(object sender, TappedRoutedEventArgs e)
        {

            bool result = await _controlProtocol.ProcessCommand("Youtube");
        }

        private async void TurnOffMonitor_Tapped(object sender, TappedRoutedEventArgs e)
        {
            bool result = await _controlProtocol.ProcessCommand("TurnOffMonitor");
        }

        private async void TurnOnMonitor_Tapped(object sender, TappedRoutedEventArgs e)
        {
            bool result = await _controlProtocol.ProcessCommand("TurnOnMonitor");
        }

        private async void VolumeUp_Tapped(object sender, TappedRoutedEventArgs e)
        {
            bool result = await _controlProtocol.ProcessCommand("VolumeUp");
        }

        private async void VolumeDown_Tapped(object sender, TappedRoutedEventArgs e)
        {
            bool result = await _controlProtocol.ProcessCommand("VolumeDown");
        }

        private void Return_Tapped(object sender, TappedRoutedEventArgs e)
        {
            var currentGridiew = ((GridView)((GridViewItem)sender).Parent);
            currentGridiew.Visibility = Visibility.Collapsed;
            this.GeneralGV.Visibility = Visibility.Visible;
        }

        private void PowerSection_Tapped(object sender, TappedRoutedEventArgs e)
        {
            this.GeneralGV.Visibility = Visibility.Collapsed;
            this.PowerGV.Visibility = Visibility.Visible;
        }

        private void EntertainmentSection_Tapped(object sender, TappedRoutedEventArgs e)
        {
            this.GeneralGV.Visibility = Visibility.Collapsed;
            this.EntertainmentGV.Visibility = Visibility.Visible;
        }

        private void InputSection_Tapped(object sender, TappedRoutedEventArgs e)
        {
            this.GeneralGV.Visibility = Visibility.Collapsed;
            this.InputGV.Visibility = Visibility.Visible;
        }

        private void Touchpad_Tapped(object sender, TappedRoutedEventArgs e)
        {
            this.InputGV.Visibility = Visibility.Collapsed;
            this.PowerGV.Visibility = Visibility.Collapsed;
            this.GeneralGV.Visibility = Visibility.Collapsed;
            this.TouchPadGrid.Visibility = Visibility.Visible;
        }

        private void Keyboard_Tapped(object sender, TappedRoutedEventArgs e)
        {

        }

        private void Touchpad_ManipulationStarted(object sender, ManipulationStartedRoutedEventArgs e)
        {
            lastTouchedPoint0 = new Point(e.Position.X, e.Position.Y);

        }

        private async void Touchpad_ManipulationDelta(object sender, ManipulationDeltaRoutedEventArgs e)
        {
            deltaX = (e.Position.X - lastTouchedPoint0.X)*0.1;
            deltaY = (e.Position.Y - lastTouchedPoint0.Y)*0.1;
            //bool result = await ProcessCommand("Move:" + deltaX.ToString() + ":" + deltaY.ToString());
        }

        private async void Touchpad_ManipulationCompleted(object sender, ManipulationCompletedRoutedEventArgs e)
        {
             bool result = await _controlProtocol.ProcessCommand("Move:"+deltaX.ToString()+":"+deltaY.ToString());
        }

        private void Settings_Click(object sender, RoutedEventArgs e)
        {
            Frame.Navigate(typeof(ConnectionPage));
        }

        private async void TouchpadLB_Click(object sender, RoutedEventArgs e)
        {
            bool result = await _controlProtocol.ProcessCommand("LBClick");
        }

        private async void TouchpadRB_Click(object sender, RoutedEventArgs e)
        {
            bool result = await _controlProtocol.ProcessCommand("RBClick");
        }

        private void ReturnB_Click(object sender, RoutedEventArgs e)
        {
            this.EntertainmentGV.Visibility = Visibility.Collapsed;
            this.InputGV.Visibility = Visibility.Collapsed;
            this.PowerGV.Visibility = Visibility.Collapsed;
            this.TouchPadGrid.Visibility = Visibility.Collapsed;
            this.GeneralGV.Visibility = Visibility.Visible;
            
        }

        private async void VolumeMute_Tapped(object sender, TappedRoutedEventArgs e)
        {
            bool result = await _controlProtocol.ProcessCommand("VolumeMute");
        }

        private void Movies_Tapped(object sender, TappedRoutedEventArgs e)
        {
            Frame.Navigate(typeof(ChooseVideo),_controlProtocol);
            
        }
    }
}
