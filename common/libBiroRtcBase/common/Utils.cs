using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Net.NetworkInformation;
using System.Net.Sockets;

namespace si.birokrat.rtc.common {
    public static class Utils {
        #region -- machine --
        public static string InstanceIdentifier() {
            string id = Environment.MachineName + "/" + Environment.UserName + "/";
			id += string.IsNullOrEmpty(Globals.ID) ?
				$"{Process.GetCurrentProcess().Id.ToString()}" :
				Globals.ID;
			return id;
        }
		#endregion
		#region -- ip addresses --
		public static IPAddress GetLanIPAddress() {
			IPAddress ipAddress = IPAddress.None;
			using (Socket socket = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp)) {
				socket.Connect("8.8.8.8", 65530);
				IPEndPoint endPoint = socket.LocalEndPoint as IPEndPoint;
				ipAddress = endPoint.Address;
			};
			return ipAddress;
		}
		public static int GetLanUdpFreePort() {
			IPGlobalProperties ipProperties = IPGlobalProperties.GetIPGlobalProperties();
			IPEndPoint[] udpListeners = ipProperties.GetActiveUdpListeners();
			int[] udpPortsActive =
				(from ul in udpListeners
				 where ul.Port >= Globals.UDP_IPP_MIN &&
								ul.Port <= Globals.UDP_IPP_MAX
				 select ul.Port).ToArray();
			IEnumerable<int> freePorts = Enumerable
				.Range(Globals.UDP_IPP_MIN, Globals.UDP_IPP_MAX - Globals.UDP_IPP_MIN)
				.Except(udpPortsActive);
			Random rnd = new Random(DateTime.Now.Millisecond);
			return freePorts.ElementAt(rnd.Next(freePorts.Count()));
		}
		public static List<IPAddress> MachineIPAddresses() {
            List<IPAddress> list = new List<IPAddress>();
            try {
                foreach (NetworkInterface net_if in NetworkInterface.GetAllNetworkInterfaces()) {
                    if (net_if.OperationalStatus == OperationalStatus.Up
                        && net_if.NetworkInterfaceType != NetworkInterfaceType.Loopback
                        && net_if.SupportsMulticast) {
                        list.AddRange(InterfaceIPAddress(net_if));
                    }
                }
            } catch {

            }
            return list;
        }
        private static List<IPAddress> InterfaceIPAddress(NetworkInterface nif) {
            List<IPAddress> list = new List<IPAddress>();
            try {
                IPInterfaceProperties if_props = nif.GetIPProperties();
                foreach (IPAddressInformation unicast in if_props.UnicastAddresses) {
                    if (unicast.Address.AddressFamily == AddressFamily.InterNetwork)
                        list.Add(unicast.Address);
                }
            } catch {

            }
            return list;
        }
        #endregion
    }
}
