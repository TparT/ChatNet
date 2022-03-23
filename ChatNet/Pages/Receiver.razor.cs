using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Components;
using ChatNet.Services;

namespace ChatNet.Pages
{
    public partial class Receiver
    {
        //[Inject]
        //private ServerConnectionClient _client { get; set; } = default!;

        [Inject]
        private AudioPlayerManager _player { get; set; } = default!;

        TcpClient client;
        NetworkStream stream;

        protected override async Task OnAfterRenderAsync(bool firstRender)
        {
            if (firstRender)
            {
                //await _client.Initialize();
                await _player.Initialize();
            }
        }

        private async void Connect()
        {
            client = new TcpClient();
            await client.ConnectAsync(System.Net.IPAddress.Parse(IpAddress), 6969);
            Console.WriteLine("Connected!");
        }


        protected override async Task OnInitializedAsync()
        {
            Console.WriteLine("[SERVER] INITIALIZED SERVER PAGE!");



            //string address = DeviceInfo.Platform == DevicePlatform.Android ? "10.0.2.2" : "127.0.0.1";

            //Buffer for reading data
            byte[] bytes = new byte[1024];
            string data = null;

            //await Task.Factory.StartNew(async () =>
            //{
            //    Task.Delay(500);
            //    StateHasChanged();
            //});

            await Task.Factory.StartNew(async () =>
            {
                // Enter the listening loop.
                //await _client.Initialize();



                while (true)
                {

                    data = null;

                    // Perform a blocking call to accept requests.
                    // You could also use server.AcceptSocket() here.


                    // Get a stream object for reading and writing
                    stream = client.GetStream();

                    int i;

                    // Loop to receive all the data sent by the client.
                    while ((i = stream.Read(bytes, 0, bytes.Length)) != -1)
                    {
                        await _player.WriteAsync(bytes, 0, i);

                        // Translate data bytes to a ASCII string.
                        data = System.Text.Encoding.ASCII.GetString(bytes, 0, i);
                        Console.WriteLine("Received: {0}", data);
                        //_ += Environment.NewLine + data;
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
    }
}
