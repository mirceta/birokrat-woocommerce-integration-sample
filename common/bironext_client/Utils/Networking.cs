using System.Linq;
using System.Net.NetworkInformation;

namespace BirokratNext.Utils
{
    public static class Networking
    {
        public static PhysicalAddress GetMacAddress() =>
            NetworkInterface
                .GetAllNetworkInterfaces()
                .Where(networkInterface => networkInterface.NetworkInterfaceType == NetworkInterfaceType.Ethernet)
                .Select(networkInterface => networkInterface.GetPhysicalAddress())
                .FirstOrDefault();
    }
}
