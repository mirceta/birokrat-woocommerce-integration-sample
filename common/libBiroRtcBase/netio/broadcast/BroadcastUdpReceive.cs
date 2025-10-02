using System;
using System.Net;
using System.Net.Sockets;
using System.Text;

using Newtonsoft.Json;

using si.birokrat.rtc.common;
using si.birokrat.rtc.netio.model;

namespace si.birokrat.rtc.netio.broadcast {
    public class BroadcastUdpReceive : IUdpReceiveOld{

        #region // locals //
        private readonly IPAddress ipAddress = IPAddress.Broadcast;
        private readonly int ipPort;
        bool running = false;
        UdpClient net = null;
        #endregion

        #region // events //
        public event EhExceptionInfo ExceptionInfoEvent;
        public event EhMessageReceived MessageReceived;
        #endregion

        #region  // constructor //
        public BroadcastUdpReceive(int ipPort) {
            this.ipPort = ipPort;
        }
        ~BroadcastUdpReceive() {
            Stop();
        }
        #endregion

        #region // public //
        public bool Running() {
            return running;
        }
        public bool Start() {
            if (running)
                return true;
            running = true;

            try {
                IPEndPoint ipe_broadcast = new IPEndPoint(ipAddress, ipPort);
                net = new UdpClient(AddressFamily.InterNetwork);
				net.Client.ReceiveBufferSize = Globals.RECEIVE_BUFFER;
				net.Client.SendBufferSize = Globals.SEND_BUFFER;
				net.Client.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.ReuseAddress, true);
                net.Client.Bind(new IPEndPoint(IPAddress.Any, ipPort));
                net.BeginReceive(
                    receiveAsync,
                    new object[] {
                    net,
                    ipe_broadcast});
            } catch (Exception ex) {
                running = false;
                ExceptionInfo exi = ExceptionInfo.Create(ex);
                throw exi;
            }

            return running;
        }
        public bool Stop() {
            if (!running)
                return true;
            try { net.Close(); } finally { net = null; }
            running = false;
            return true;
        }
        #endregion

        #region // private //
        private void receiveAsync(IAsyncResult result) {
            object[] state = null;
            UdpClient net = null;
            IPEndPoint ipe = null;
            byte[] byte_data = null;

            try {
                state = (object[])result.AsyncState;
                net = (UdpClient)state[0];
                ipe = (IPEndPoint)state[1];
                byte_data = net.EndReceive(result, ref ipe);
            } catch (Exception ex) {
                running = false;
                ExceptionInfo exi = ExceptionInfo.Create(ex, "end.receive");
                ExceptionInfoEvent?.Invoke(exi);
                return;
            }

            try {
                string string_data = Encoding.UTF8.GetString(byte_data);
                MessageModel message = JsonConvert.DeserializeObject<MessageModel>(string_data);
                MessageReceived?.Invoke(message);
            } catch (Exception ex) {
                ExceptionInfo exi = ExceptionInfo.Create(ex, "json.convert");
                ExceptionInfoEvent?.Invoke(exi);
            }

            try {
                if (running)
                    net.BeginReceive(
                        receiveAsync,
                        state);
            } catch(Exception ex) {
                running = false;
                ExceptionInfo exi = ExceptionInfo.Create(ex, "begin.receive");
                ExceptionInfoEvent?.Invoke(exi);
            }
        }
        #endregion

    }
}
