using System.Net.NetworkInformation;
using System.Net.Sockets;

namespace ChatNet.Services
{
    public partial class NetworkingService
    {
        public partial string GetIPAddress()
        {
            string ipAddress = "";

            foreach (var netInterface in NetworkInterface.GetAllNetworkInterfaces())
            {
                if (netInterface.NetworkInterfaceType == NetworkInterfaceType.Wireless80211 ||
                    netInterface.NetworkInterfaceType == NetworkInterfaceType.Ethernet)
                {
                    foreach (var addrInfo in netInterface.GetIPProperties().UnicastAddresses)
                    {
                        if (addrInfo.Address.AddressFamily == AddressFamily.InterNetwork)
                        {
                            ipAddress = addrInfo.Address.ToString();
                        }
                    }
                }
            }

            return ipAddress;
        }

        public partial void EnableMulticasting()
        {

        }

        public partial void DisableMulticasting()
        {

        }
    }
}
