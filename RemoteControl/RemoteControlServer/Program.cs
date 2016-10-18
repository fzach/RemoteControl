using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Net.Sockets;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Linq;

namespace RemoteControlServer
{



    public class Program
    {
        private static int WM_SYSCOMMAND = 0x112;
        private static int SC_MONITORPOWER = 0xF170;

        private static int APPCOMMAND_VOLUME_MUTE = 0x80000;
        private static int APPCOMMAND_VOLUME_UP = 0xA0000;
        private static int APPCOMMAND_VOLUME_DOWN = 0x90000;
        private static int WM_APPCOMMAND = 0x319;
        private static int MOUSEEVENTF_MOVE = 0x0001;

        private static int MOUSEEVENTF_LEFTDOWN = 0x0002;
        private static int MOUSEEVENTF_LEFTUP = 0x0004;
        private const int MOUSEEVENTF_RIGHTDOWN = 0x0008;
        private const int MOUSEEVENTF_RIGHTUP = 0x0010;

        public enum MonitorState
        {
            MonitorStateOn = -1,
            MonitorStateOff = 2,
            MonitorStateStandBy = 1
        }

        private IntPtr currentWindow;

        private static int currentMouseX;
        private static int currentMouseY;

        private static string[] coords;

        private const string videosPath = @"E:\Downloads";

        object _lock = new Object(); // sync lock 
        List<Task> _connections = new List<Task>(); // pending connections

        [DllImport("user32.dll", CharSet = CharSet.Auto)]
        private static extern int SendMessage(IntPtr hWnd, int wMsg, IntPtr wParam, IntPtr lParam);

        [DllImport("user32.dll", CharSet = CharSet.Auto)]
        static extern void mouse_event(Int32 dwFlags, Int32 dx, Int32 dy, Int32 dwData, UIntPtr dwExtraInfo);




        // The core server task
        private async Task StartListener()
        {
            var tcpListener = TcpListener.Create(8888);
            tcpListener.Start();
            
            while (true)
            {
                var tcpClient = await tcpListener.AcceptTcpClientAsync();
                Console.WriteLine("[Server] Client has connected");
                var task = StartHandleConnectionAsync(tcpClient);
                // if already faulted, re-throw any error on the calling context
                if (task.IsFaulted)
                    task.Wait();
            }
        }

        // Register and handle the connection
        private async Task StartHandleConnectionAsync(TcpClient tcpClient)
        {
            // start the new connection task
            var connectionTask = HandleConnectionAsync(tcpClient);

            // add it to the list of pending task 
            lock (_lock)
                _connections.Add(connectionTask);

            // catch all errors of HandleConnectionAsync
            try
            {
                await connectionTask;
                // we may be on another thread after "await"
            }
            catch (Exception ex)
            {
                // log the error
                Console.WriteLine(ex.ToString());
            }
            finally
            {
                // remove pending task
                lock (_lock)
                    _connections.Remove(connectionTask);
            }
        }

        // Handle new connection
        private async Task HandleConnectionAsync(TcpClient tcpClient)
        {
            await Task.Yield();
            // continue asynchronously on another threads

            using (var networkStream = tcpClient.GetStream())
            {
                
                var buffer = new byte[4096];
                //Console.WriteLine("[Server] Reading from client");
                var byteCount = await networkStream.ReadAsync(buffer, 0, buffer.Length);
                var request = Encoding.UTF8.GetString(buffer, 0, byteCount);
                //Console.WriteLine("[Server] Client wrote {0} . Request is being processed...", request);
                //Console.WriteLine("[Server] Processing client request...");
                string response = await ProcessRequest(request.Remove(request.Length-2));
                var serverResponseBytes = Encoding.UTF8.GetBytes(response);
                await networkStream.WriteAsync(serverResponseBytes, 0, serverResponseBytes.Length);
                //Console.WriteLine("[Server] Response has been written");
                //tcpClient.Close();
            }
        }

        private async static Task<String> ProcessRequest(string request)
        {
            switch (request)
            {
                case "Bye": ShutDown(); break;

                case "Youtube": PlayYoutubeVideo(); break;

                case "TurnOffMonitor": TurnOffMonitor(); break;

                case "TurnOnMonitor": TurnOnMonitor(); break;

                case "VolumeUp": VolumeUp(); break;

                case "VolumeDown": VolumeDown(); break;

                case "VolumeMute": VolumeMute(); break;

                case "LBClick": MouseLBClick(); break;

                case "RBClick": MouseRBClick(); break;

                case "SynchronizeVideos": return SynchronizeVideos(); break;


                default: break;
            }

            if (request.Contains("youtube"))
            {
                //TODO
            }
            if (request.Contains("Move"))
            {
                MouseMove(request);
            }
            if (request.Contains("PlayMPC"))
            {
                PlayMPC(request);
            }

            return "OK";
        }

        private static void MouseMove(string data)
        {
            coords = data.Split(':');
            currentMouseX = (int)Double.Parse(coords[1],CultureInfo.InvariantCulture);
            currentMouseY = (int)Double.Parse(coords[2],CultureInfo.InvariantCulture);
            mouse_event(MOUSEEVENTF_MOVE,currentMouseX,currentMouseY, 0, UIntPtr.Zero);
        }

        private static void MouseLBClick()
        {
            mouse_event(MOUSEEVENTF_LEFTDOWN, currentMouseX, currentMouseY, 0, UIntPtr.Zero);
            mouse_event(MOUSEEVENTF_LEFTUP, currentMouseX, currentMouseY, 0, UIntPtr.Zero);

        }

        private static void MouseRBClick()
        {
            mouse_event(MOUSEEVENTF_RIGHTDOWN, currentMouseX, currentMouseY, 0, UIntPtr.Zero);
            mouse_event(MOUSEEVENTF_RIGHTUP, currentMouseX, currentMouseY, 0, UIntPtr.Zero);

        }

        private static void TurnOffMonitor()
        {
            IntPtr handle = Process.GetCurrentProcess().MainWindowHandle;
            SendMessage(handle, WM_SYSCOMMAND, (IntPtr)SC_MONITORPOWER, (IntPtr)MonitorState.MonitorStateOff);

        }

        private static void TurnOnMonitor()
        {
            IntPtr handle = Process.GetCurrentProcess().MainWindowHandle;
            SendMessage(handle, WM_SYSCOMMAND, (IntPtr)SC_MONITORPOWER, (IntPtr)MonitorState.MonitorStateOn);
            mouse_event(MOUSEEVENTF_MOVE, 0, 1, 0, UIntPtr.Zero);
            System.Threading.Thread.Sleep(40);
            mouse_event(MOUSEEVENTF_MOVE, 0, -1, 0, UIntPtr.Zero);

        }

        private static void VolumeUp()
        {
            IntPtr handle = Process.GetCurrentProcess().MainWindowHandle;
            SendMessage(handle, WM_APPCOMMAND, handle,
                (IntPtr)APPCOMMAND_VOLUME_UP);
        }

        private static void VolumeDown()
        {
            IntPtr handle = Process.GetCurrentProcess().MainWindowHandle;
            SendMessage(handle, WM_APPCOMMAND, handle,
                (IntPtr)APPCOMMAND_VOLUME_DOWN);
        }

        private static void VolumeMute()
        {
            IntPtr handle = Process.GetCurrentProcess().MainWindowHandle;
            SendMessage(handle, WM_APPCOMMAND, handle,
                (IntPtr)APPCOMMAND_VOLUME_MUTE);
        }

        private static void PlayYoutubeVideo()
        {
            ProcessStartInfo startInfo = new ProcessStartInfo();
            startInfo.FileName = "chrome.exe";
            startInfo.Arguments = @"youtube.com/watch?v=gCYcHz2k5x0";
            Process.Start(startInfo);
        }
        private static void PlayMPC(string fileName)
        {
            try
            {
                String[] separated = fileName.Split('#');
                string name = separated[1];
                ProcessStartInfo startInfo = new ProcessStartInfo();
                startInfo.FileName = @"C:\Program Files\MPC-HC\mpc-hc64.exe";

                startInfo.Arguments = "\"" + name +"\"" + " /play";
                Process.Start(startInfo);
            }

            catch(Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
        }

        private static void ShutDown()
        {
            var psi = new ProcessStartInfo("shutdown", "/s /t 0");
            psi.CreateNoWindow = true;
            psi.UseShellExecute = false;
            Process.Start(psi);
        }

        private static String SynchronizeVideos()
        {
            List<String> dirs = new List<string>();
            string result = null;
            try
            {
                
                dirs = Directory.GetFiles(videosPath, ".",SearchOption.AllDirectories)
                                .Where(x => x.ToLower()
                                .EndsWith("mp4") || x.ToLower().EndsWith("avi")).ToList();
            }
            catch(Exception ex)
            {
                Console.WriteLine(ex);
            }

            foreach(string fn in dirs)
            {
                result += "#" + fn;
            }

            return result;
            
        }

        // The entry point of the console app
        static void Main(string[] args)
        {
            Console.WriteLine("Hit Ctrl-C to exit.");

            new Program().StartListener().Wait();

        }
    }
}