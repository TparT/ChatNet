using System.Net;
using System.Net.Sockets;

namespace ChatNet.Services.Sockets
{
    public class Server
    {
        public Socket listenerSocket;

        public Action<Client> OnNewClient;
        public Action<Exception> OnError;

        public int Port { get => _port; set => _port = value; }

        private int _port, backlog;

        public Server(int port, int backlog = 32)
        {
            listenerSocket = new Socket(SocketType.Stream, ProtocolType.Tcp);
            this.Port = port;
            this.backlog = backlog;
        }

        public void Listen()
        {
            try
            {
                listenerSocket.Bind(new IPEndPoint(IPAddress.Any, Port));
                listenerSocket.Listen(backlog);
                listenerSocket.BeginAccept(new AsyncCallback(AcceptCallback), listenerSocket);
            }
            catch (Exception ex)
            {
                HandleError(ex);
            }
        }

        private void AcceptCallback(IAsyncResult ar)
        {
            try
            {
                foreach (Delegate d in OnNewClient?.GetInvocationList())
                {
                    d?.DynamicInvoke(new Client(listenerSocket.EndAccept(ar)));
                }
                //OnNewClient?.BeginInvoke(new Client(listenerSocket.EndAccept(ar)), new AsyncCallback(OnNewClientCallback), listenerSocket);
                listenerSocket.BeginAccept(new AsyncCallback(AcceptCallback), listenerSocket);
            }
            catch (Exception ex)
            {
                HandleError(ex);
            }
        }

        private void OnNewClientCallback(IAsyncResult ar)
        {
            try
            {
                OnNewClient?.EndInvoke(ar);
            }
            catch (Exception ex)
            {
                HandleError(ex);
            }
        }

        private void HandleError(Exception ex)
        {
            OnError?.Invoke(ex);
        }
    }
}
