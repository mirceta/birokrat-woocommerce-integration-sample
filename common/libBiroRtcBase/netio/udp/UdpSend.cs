using System;
using System.Net;
using System.Net.Sockets;
using System.Text;
using Newtonsoft.Json;
using si.birokrat.rtc.common;
using si.birokrat.rtc.netio.model;

namespace si.birokrat.rtc.netio.udp {
	public interface IUdpSend {
		#region -- public --
		bool Start();
		bool Stop();
		void SendData(IPAddress ipAddress, int ipPort, MessageModel msg);
		void SendQuit(IPAddress ipAddress, int ipPort);
		#endregion
	}
	public class UdpSend : IUdpSend {
		#region -- locals --
		private readonly IPAddress _ipAddress = IPAddress.Broadcast;
		private readonly int _ipPort;
		private readonly object _lock = new object();
		private IPEndPoint _ipEndpoint;
		UdpClient _net;
		bool _running;
		#endregion
		#region -- constructor --
		~UdpSend() {
			Stop();
		}
		#endregion
		#region -- public --
		public bool Start() {
			if (_running) return true;
			_running = true;
			try {
				_net = new UdpClient(AddressFamily.InterNetwork);
				_net.Client.ReceiveBufferSize = Globals.RECEIVE_BUFFER;
				_net.Client.SendBufferSize = Globals.SEND_BUFFER;
			} catch (Exception ex) {
				_running = false;
				ExceptionInfo exi = ExceptionInfo.Create(ex);
				throw exi;
			}
			return _running;
		}
		public bool Stop() {
			if (!_running) return true;
			_running = false;
			try {
				_net.Close();
			} finally {
				_net = null;
			}
			return true;
		}
		public void SendData(IPAddress ipAddress, int ipPort, MessageModel msg) {
			if (!_running)
				if (!Start()) {
					ExceptionInfo exi = ExceptionInfo.Create(customInfo: "Send.Start failed");
					throw exi;
				}
			sendPacket(ipAddress, ipPort, msg);
		}
		public void SendQuit(IPAddress ipAddress, int ipPort) {
			sendQuit(ipAddress, ipPort);
		}
		#endregion
		#region -- private --
		private void sendQuit(IPAddress ipAddress, int ipPort) {
			MessageModel quitMsg = new MessageModel() {
				type = MessageType.QUIT
			};
			sendPacket(ipAddress, ipPort, quitMsg);
		}
		private void sendPacket(IPAddress ipAddress, int ipPort, MessageModel msg) {
			if (!_running) return;
			try {
				string data = JsonConvert.SerializeObject(msg);
				byte[] byteData = Encoding.UTF8.GetBytes(data);
				IPEndPoint ipEndPoint = new IPEndPoint(ipAddress, ipPort);
				lock (_lock)
					_net.Send(byteData, byteData.Length, ipEndPoint);
			} catch (Exception ex) {
				ExceptionInfo exi = ExceptionInfo.Create(ex);
				throw exi;
			}
		}
		#endregion
	}
}
