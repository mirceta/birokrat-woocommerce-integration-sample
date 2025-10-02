using System;
using System.Collections.Generic;
using System.Management;
using System.Net.NetworkInformation;
using System.Text;

namespace common_birowoo
{
    public class HardwareIDGenerator
    {
        public static string GetUniqueIdentifier()
        {
            string macAddress = GetMacAddress();
            string combinedId = $"{macAddress}";

            // You might also consider hashing the combined ID for added security
            return combinedId;
        }

        private static string GetMacAddress()
        {
            string macAddress = string.Empty;
            try
            {
                foreach (NetworkInterface nic in NetworkInterface.GetAllNetworkInterfaces())
                {
                    if (nic.NetworkInterfaceType != NetworkInterfaceType.Loopback && nic.OperationalStatus == OperationalStatus.Up)
                    {
                        macAddress = nic.GetPhysicalAddress().ToString();
                        break; // Get the first MAC address found
                    }
                }
            }
            catch (Exception ex)
            {
                // Handle any exceptions here
                Console.WriteLine("Error getting MAC address: " + ex.Message);
            }

            return macAddress;
        }
    }
}
