//using System;
//using System.Collections.Generic;
//using System.Linq;
//using System.Net;
//using System.Net.Sockets;
//using System.Text;
//using System.Threading.Tasks;

//namespace ChatNet.Services
//{
//    //// State object for receiving data from remote device.
//    //public class StateObject
//    //{
//    //    public Socket WorkSocket = null;
//    //    public const int BufferSize = 250;
//    //    public byte[] Buffer = new byte[BufferSize];
//    //}

//    public class TcpConnection
//    {
//        public bool IsOpen = false;

//        public event Action<string, byte[]> NewDataEvent;
//        public Socket Listener;

//        private readonly IPAddress _ipAddress;
//        private readonly AutoResetEvent _isConnected;
//        private readonly int _tcpPort;
//        private readonly bool _binaryTicket;
//        private string _ticket = "";

//        /// <summary>
//        ///     constructor
//        /// </summary>
//        /// <param name="ip"></param>
//        /// <param name="newData"> </param>
//        /// <param name="tcpPort"> </param>
//        /// <param name="isConnected"> </param>
//        /// <param name="binaryTicket"></param>
//        public TcpConnection(IPAddress ip, int tcpPort, AutoResetEvent isConnected, bool binaryTicket)
//        {
//            this._binaryTicket = binaryTicket;

//            _isConnected = isConnected;
//            _tcpPort = tcpPort;
//            _ipAddress = ip;

//            Listener = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);

//            // Disable the Nagle Algorithm for this tcp socket.
//            Listener.NoDelay = true;

//        }

//        public void Open()
//        {
//            Console.WriteLine("TCP - starting server. Waiting for client: " + _ipAddress + ":" + _tcpPort);

//            // Generate State object for buffer and socket information for async receive
//            StateObject state = new StateObject();

//            try
//            {
//                TcpListener Server = new TcpListener(_ipAddress, _tcpPort);
//                Server.Start();
//                IAsyncResult socketRes = Server.BeginAcceptSocket(null, null);
//                bool socketSuccess = socketRes.AsyncWaitHandle.WaitOne(10000, true);
//                if (socketSuccess)
//                    Listener = Server.EndAcceptSocket(socketRes);

//                //Open socket and cancel if no connection could be made after 5 seconds
//                IAsyncResult result = Listener.BeginConnect(new IPEndPoint(_ipAddress, _tcpPort), null, null);
//                bool success = result.AsyncWaitHandle.WaitOne(10000, true);
//                if (success)
//                    Listener.EndConnect(result);
//                else
//                {
//                    Listener.Close();
//                    throw new SocketException(10060); // Connection timed out.
//                }
//                state.WorkSocket = Listener;
//                Listener.BeginReceive(state.Buffer, 0, StateObject.BufferSize, SocketFlags.None, new AsyncCallback(ReceiveData), state);

//                // Send the caller a signal that the socket is connected
//                _isConnected!.Set();

//                Console.WriteLine("TCP - Server connected on:" + _ipAddress + " on Port: " + _tcpPort);
//                IsOpen = true;
//            }
//            catch (Exception e)
//            {
//                Console.WriteLine(e.ToString());
//            }
//        }

//        /// <summary>
//        ///     Close the Serial listener, Aborts all threads
//        /// </summary>
//        public void Close()
//        {
//            if (Listener != null && Listener.Connected)
//                Listener.Shutdown(SocketShutdown.Both);

//            while (Listener != null && Listener.Connected)
//            { Task.Delay(1); }

//            if (Listener != null)
//            {
//                Listener.Close();
//                Listener.Dispose();
//            }
//        }

//        /// <summary>
//        ///     Write order to serial
//        ///     waits until reader has recieved answer ticket
//        /// </summary>
//        /// <param name="str"> </param>
//        /// <param name="text"> </param>
//        /// <param name="byteArray"></param>
//        /// <returns></returns>
//        public bool Write(byte[] byteArray)
//        {
//            try
//            {
//                if (Listener != null && Listener.Connected)
//                {
//                    // Disable the Nagle Algorithm for this tcp socket.
//                    Listener.NoDelay = true;
//                    Listener.SendBufferSize = byteArray.Length;
//                    Listener.Send(byteArray, 0, byteArray.Length, SocketFlags.None);
//                }
//            }
//            catch (Exception e)
//            {
//                Console.WriteLine("TCP Writer error: " + e);
//                return false;
//            }
//            return true;
//        }

//        /// <summary>
//        ///     Write order to serial
//        ///     waits until reader has recieved answer ticket
//        /// </summary>
//        /// <param name="str"> </param>
//        /// <param name="text"> </param>
//        /// <param name="byteArray"></param>
//        /// <returns></returns>
//        public async Task<bool> WriteAsync(byte[] byteArray)
//        {
//            try
//            {
//                if (Listener != null && Listener.Connected)
//                {
//                    // Disable the Nagle Algorithm for this tcp socket.
//                    Listener.NoDelay = true;
//                    Listener.SendBufferSize = byteArray.Length;
//                    await Listener.SendAsync(byteArray.AsMemory(), SocketFlags.None);
//                }
//            }
//            catch (Exception e)
//            {
//                Console.WriteLine("TCP Writer error: " + e);
//                return false;
//            }
//            return true;
//        }

//        /// <summary>
//        ///     Write order to serial
//        ///     waits until reader has received answer ticket
//        /// </summary>
//        /// <param name="str"> </param>
//        /// <param name="text"> </param>
//        /// <returns></returns>
//        public bool Write(string str)
//        {
//            byte[] byteArray = Encoding.ASCII.GetBytes(str);
//            try
//            {
//                if (Listener != null && Listener.Connected)
//                {
//                    // Disable the Nagle Algorithm for this tcp socket.
//                    Listener.NoDelay = true;
//                    Listener.SendBufferSize = byteArray.Length;
//                    Listener.Send(byteArray, 0, byteArray.Length, SocketFlags.None);
//                }
//            }
//            catch (Exception e)
//            {
//                Console.WriteLine("TCP Writer error: " + e);
//                return false;
//            }
//            return true;
//        }

//        /// <summary>
//        ///     Write order to serial
//        ///     waits until reader has received answer ticket
//        /// </summary>
//        /// <param name="str"> </param>
//        /// <param name="text"> </param>
//        /// <returns></returns>
//        public async Task<bool> WriteAsync(string str)
//        {
//            byte[] byteArray = Encoding.ASCII.GetBytes(str);
//            return await WriteAsync(byteArray);
//        }


//        private void RecieveBinary(IAsyncResult ar)
//        {

//            StateObject state = (StateObject)ar.AsyncState;
//            Socket handler = state.WorkSocket;

//            if (!handler.Connected)
//                return;
//            try
//            {
//                int recv = handler.EndReceive(ar);
//                NewDataEvent.Invoke("", state.Buffer);

//                // Receive again and wait
//                handler.BeginReceive(state.Buffer, 0, StateObject.BufferSize, SocketFlags.None, new AsyncCallback(ReceiveData), state);

//            }
//            catch (Exception e)
//            {
//                Console.WriteLine(e);
//            }
//        }

//        private void ReceiveData(IAsyncResult ar)
//        {
//            if (_binaryTicket)
//            {
//                RecieveBinary(ar);
//                return;
//            }

//            StateObject state = (StateObject)ar.AsyncState;
//            Socket handler = state.WorkSocket;

//            if (!handler.Connected)
//                return;
//            try
//            {
//                int recv = handler.EndReceive(ar);

//                // Append recevied ticket
//                _ticket += Encoding.ASCII.GetString(state.Buffer, 0, recv);

//                // Interpret all full tickets containing a < and a >
//                while (_ticket.IndexOf(">") > -1)
//                {
//                    var start = _ticket.IndexOf("<");
//                    var end = _ticket.IndexOf(">");

//                    if (end > 0 && start >= 0 && end > start)
//                    {
//                        String firstTicket = _ticket.Substring(start, end + 1);

//                        // Send this ticket to interpreter
//                        NewDataEvent.Invoke(firstTicket, new byte[] { });

//                        // Trim renaming string
//                        _ticket = _ticket.Remove(start, end + 1);
//                    }

//                    // Trim renaming string
//                    else if (end > 0)
//                        _ticket = _ticket.Remove(0, end + 1);
//                }

//                // Receive again and wait
//                handler.BeginReceive(state.Buffer, 0, StateObject.BufferSize, SocketFlags.None, new AsyncCallback(ReceiveData), state);
//            }
//            catch (Exception e)
//            {
//                Console.WriteLine(e);
//            }
//        }
//    }
//}
