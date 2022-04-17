using Android.App;
using Android.Net.Wifi;
using Java.Net;
using Java.Util;
using System.Net;

namespace ChatNet.Services
{
    public partial class NetworkingService
    {
        internal WifiManager.MulticastLock multicastLock;
        public partial string GetIPAddress()
        {
            WifiManager wifiManager = (WifiManager)Android.App.Application.Context.GetSystemService(Service.WifiService);
            int ip = wifiManager.ConnectionInfo.IpAddress;
            IPAddress ipAddr = new IPAddress(ip);

            //  System.out.println(host);
            return ipAddr.ToString();
        }


        public partial void EnableMulticasting()
        {
            WifiManager wifiManager = (WifiManager)Android.App.Application.Context.GetSystemService(Service.WifiService);
            multicastLock = wifiManager.CreateMulticastLock("multicastLock");
            multicastLock.SetReferenceCounted(true);
            multicastLock.Acquire();
        }

        public partial void DisableMulticasting()
        {
            if (multicastLock != null)
            {

                multicastLock.Release();
                multicastLock = null;
            }
        }
    }
}
