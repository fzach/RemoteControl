using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.Networking.Sockets;

namespace RemoteControlClient
{


    public class RemoteControlClientProtocol 
    {
        private string _IPAddress;
        private string _port;

        private StreamSocket _clientSocket;
        private Windows.Networking.HostName _serverHost;
        private bool _connected = false;
        private bool _closing = false;
        Stream _streamOut = null;
        Stream _streamIn = null;
        StreamWriter writer = null;
        StreamReader reader = null;

        public RemoteControlClientProtocol(string ip, string port)
        {
            _IPAddress = ip;
            _port = port;
            _clientSocket = new StreamSocket();
        }

        private async Task<bool> Reconnect()
        {
            try
            {
                _serverHost = new Windows.Networking.HostName(_IPAddress);
                // Try to connect to the 
                _clientSocket.Dispose();
                _clientSocket = new StreamSocket();
                await _clientSocket.ConnectAsync(_serverHost, _port);
                return true;
            }
            catch (Exception ex)
            {
                // If this is an unknown status, 
                // it means that the error is fatal and retry will likely fail.
                if (SocketError.GetStatus(ex.HResult) == SocketErrorStatus.Unknown)
                {
                    throw;
                }

                // Could retry the connection, but for this simple example
                // just close the socket.

                _closing = true;
                // the Close method is mapped to the C# Dispose
                _clientSocket.Dispose();
                _clientSocket = null;
                return false;
            }

        }

        public async Task<bool> Connect()
        {
            if (_connected)
            {
                return false;
            }


            try
            {
                _serverHost = new Windows.Networking.HostName(_IPAddress);
                _clientSocket.Control.KeepAlive = true;
                // Try to connect to the 
                await _clientSocket.ConnectAsync(_serverHost, _port);

                _connected = true;

            }
            catch (Exception exception)
            {
                // If this is an unknown status, 
                // it means that the error is fatal and retry will likely fail.
                if (SocketError.GetStatus(exception.HResult) == SocketErrorStatus.Unknown)
                {
                    throw;
                }
                // Could retry the connection, but for this simple example
                // just close the socket.

                _closing = true;
                // the Close method is mapped to the C# Dispose
                _clientSocket.Dispose();
                _clientSocket = null;
                return false;

            }
            if (_connected)
            {
                return true;
            }

            try
            {
                _clientSocket.Control.KeepAlive = true;

                _serverHost = new Windows.Networking.HostName(_IPAddress);
                // Try to connect to the 
                await _clientSocket.ConnectAsync(_serverHost, _port);

                _connected = true;
                return _connected;


            }
            catch (Exception exception)
            {
                // If this is an unknown status, 
                // it means that the error is fatal and retry will likely fail.
                if (SocketError.GetStatus(exception.HResult) == SocketErrorStatus.Unknown)
                {
                    throw;
                }
                // Could retry the connection, but for this simple example
                // just close the socket.

                _closing = true;
                // the Close method is mapped to the C# Dispose
                _clientSocket.Dispose();
                _clientSocket = null;
                return false;

            }
        }
        public async Task<String> ProcessCommandWithResponse(string request)
        {
            string result = null;
            try
            {
                await Connect();
                await SendCommand(request);
                result = await ReadResponseAsString();
                return result;
            }
            catch (Exception ex)
            {
                return null;
            }

        }

        private async Task<bool> SendCommand(string request)
        {
            bool result = await Reconnect();
            try
            {

                _streamOut = _clientSocket.OutputStream.AsStreamForWrite();
                writer = new StreamWriter(_streamOut);
                await writer.WriteLineAsync(request);
                await writer.FlushAsync();
                string socketPort = _clientSocket.Information.LocalPort;
                writer.Dispose();

            }
            catch (Exception exception)
            {
                // If this is an unknown status, 
                // it means that the error is fatal and retry will likely fail.
                if (SocketError.GetStatus(exception.HResult) == SocketErrorStatus.Unknown)
                {
                    throw;
                }
                // Could retry the connection, but for this simple example
                // just close the socket.

                _closing = true;
                _clientSocket.Dispose();
                _clientSocket = null;
                _connected = false;

            }
            return true;
        }

        public async Task<bool> ProcessCommand(string request)
        {
            bool result = false;
            try
            {
                result = await Connect();
                result = await SendCommand(request);
                result = await ReadResponse();
            }
            catch(Exception ex)
            {
                return false;
            }

            return true;
            
        }

        private async Task<String> ReadResponseAsString()
        {
            try
            {
                string result = null;
                _streamIn = _clientSocket.InputStream.AsStreamForRead();
                reader = new StreamReader(_streamIn);
                result = await reader.ReadToEndAsync();
                return result;
            }
            catch(Exception ex)
            {
                _closing = true;
                _clientSocket.Dispose();
                _clientSocket = null;
                _connected = false;
                return null;
            }
            
        }
        private async Task<bool> ReadResponse()
        {
            try
            {

                //string socketPort = _clientSocket.Information.LocalPort;

                _streamIn = _clientSocket.InputStream.AsStreamForRead();
                reader = new StreamReader(_streamIn);
                char[] buff = new char[4096];
                int response = await reader.ReadAsync(buff, 0, 15);
                return true;
            }
            catch (Exception exception)
            {
                // If this is an unknown status, 
                // it means that the error is fatal and retry will likely fail.
                if (SocketError.GetStatus(exception.HResult) == SocketErrorStatus.Unknown)
                {
                    throw;
                }
                // Could retry, but for this simple example
                // just close the socket.

                _closing = true;
                _clientSocket.Dispose();
                _clientSocket = null;
                _connected = false;
                return false;

            }
        }
    }
}
