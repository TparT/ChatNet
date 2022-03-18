using ChatNet.Services;
using Microsoft.AspNetCore.Components;
using System.Net.Sockets;


namespace ChatNet.Pages
{
    public partial class Server
    {

        [Inject]
        private ServerConnectionClient _client { get; set; } = default!;

        private string Messages;

        TcpClient client;
        NetworkStream stream;

        public event Action<string> DataReceived;

        //Services.Sockets.Server Listener = new Services.Sockets.Server(6969);
        //Services.Sockets.Client Client = new Services.Sockets.Client();

        protected override async Task OnAfterRenderAsync(bool firstRender)
        {
            if (firstRender)
            {
                //if (!_client.IsOpen)
                //{
                //    _client.Open();
                //    _client.NewDataEvent += Received;
                //}
                await _client.Initialize();
                DataReceived += (data) =>
                {

                };
            }
        }

        protected override async Task OnInitializedAsync()
        {
            Console.WriteLine("[SERVER] INITIALIZED SERVER PAGE!");



            //string address = DeviceInfo.Platform == DevicePlatform.Android ? "10.0.2.2" : "127.0.0.1";

            //Buffer for reading data
            byte[] bytes = new byte[256];
            string data = null;

            //await Task.Factory.StartNew(async () =>
            //{
            //    Task.Delay(500);
            //    StateHasChanged();
            //});

            await Task.Factory.StartNew(async () =>
            {
                // Enter the listening loop.
                await _client.Initialize();
                while (true)
                {
                    Console.Write("Waiting for a connection... ");

                    data = null;

                    // Perform a blocking call to accept requests.
                    // You could also use server.AcceptSocket() here.
                    //if (_client.Server.Pending())
                    //{
                    client = await _client.Server.AcceptTcpClientAsync();
                    Console.WriteLine("Connected!");

                    // Get a stream object for reading and writing
                    stream = client.GetStream();
                    //}

                    int i;

                    // Loop to receive all the data sent by the client.
                    while ((i = stream.Read(bytes, 0, bytes.Length)) != -1)
                    {
                        // Translate data bytes to a ASCII string.
                        data = System.Text.Encoding.ASCII.GetString(bytes, 0, i);
                        Console.WriteLine("Received: {0}", data);
                        Messages += Environment.NewLine+data;
                        //DataReceived?.Invoke(data);
                        //// Process the data sent by the client.
                        //data = data.ToUpper();

                        //byte[] msg = System.Text.Encoding.ASCII.GetBytes(data);

                        //// Send back a response.
                        //stream.Write(msg, 0, msg.Length);
                        //Console.WriteLine("Sent: {0}", data);
                    }

                    // Shutdown and end connection
                    client.Close();
                }
            });
        }

        private void Received(string data, byte[] buffer)
        {
            //Translate data bytes to a ASCII string.
            //string data = System.Text.Encoding.ASCII.GetString(bytes);
            Console.WriteLine("Received: {0}", data);
            Messages += data + "\n";
            StateHasChanged();
        }
    }
}