using System;
using System.Net.Sockets;
using System.Net;
using System.Text;
using Newtonsoft.Json;
using si.birokrat.rtc.common;
using si.birokrat.rtc.netio.model;

namespace si.birokrat.rtc.netio.udp {
	public interface IUdpReceive {
		#region -- events --
		event EhExceptionInfo ExceptionInfoEvent;
		event EhMessageReceived MessageReceived;
		#endregion
		#region -- public --
		bool Running();
		bool Start();
		bool Stop();
		#endregion
	}
	public class UdpReceive : IUdpReceive {
		#region -- locals --
		private readonly IPAddress _ipAddress;
		private readonly int _ipPort;
		bool _running;
		UdpClient _net;
		#endregion
		#region -- events --
		public event EhExceptionInfo ExceptionInfoEvent;
		public event EhMessageReceived MessageReceived;
		#endregion
		#region -- ctor --
		public UdpReceive(IPAddress ipAddress, int ipPort) {
			_ipAddress = ipAddress;
			_ipPort = ipPort;
		}
		~UdpReceive() {
			Stop();
		}
		#endregion
		#region -- methods --
		public bool Running() {
			return _running;
		}
		public bool Start() {
			if (_running) return true;
			_running = true;
			try {
				IPEndPoint ipeBroadcast = new IPEndPoint(_ipAddress, _ipPort);
				_net = new UdpClient(AddressFamily.InterNetwork);
				_net.Client.ReceiveBufferSize = Globals.RECEIVE_BUFFER;
				_net.Client.SendBufferSize = Globals.SEND_BUFFER;
				_net.Client.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.ReuseAddress, true);
				_net.Client.Bind(new IPEndPoint(IPAddress.Any, _ipPort));
				_net.BeginReceive(
					receiveAsync,
					new object[] { _net, ipeBroadcast });
			} catch (Exception ex) {
				_running = false;
				ExceptionInfo exi = ExceptionInfo.Create(ex);
				throw exi;
			}
			return _running;
		}
		public bool Stop() {
			if (!_running) return true;
			try { _net.Close(); } finally { _net = null; }
			_running = false;
			return true;
		}
		#endregion
		#region -- private --
		private void receiveAsync(IAsyncResult result) {
			object[] state;
			UdpClient netLocal;
			byte[] byteData;
			try {
				state = (object[])result.AsyncState;
				netLocal = (UdpClient)state[0];
				IPEndPoint ipe = (IPEndPoint)state[1];
				byteData = netLocal.EndReceive(result, ref ipe);
			} catch (Exception ex) {
				_running = false;
				ExceptionInfo exi = ExceptionInfo.Create(ex, "end.receive");
				ExceptionInfoEvent?.Invoke(exi);
				return;
			}
			try {
				string dataString = Encoding.UTF8.GetString(byteData);
				MessageModel msg = JsonConvert.DeserializeObject<MessageModel>(dataString);
				MessageReceived?.Invoke(msg);
			} catch (Exception ex) {
				ExceptionInfo exi = ExceptionInfo.Create(ex, "json.convert");
				ExceptionInfoEvent?.Invoke(exi);
			}
			try {
				if (_running)
					netLocal.BeginReceive(
						receiveAsync,
						state);
			} catch (Exception ex) {
				_running = false;
				ExceptionInfo exi = ExceptionInfo.Create(ex, "begin.receive");
				ExceptionInfoEvent?.Invoke(exi);
			}
		}
		#endregion
	}
}
