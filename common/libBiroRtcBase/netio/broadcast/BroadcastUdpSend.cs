using System;
using System.Net;
using System.Net.Sockets;
using System.Text;

using Newtonsoft.Json;

using si.birokrat.rtc.common;
using si.birokrat.rtc.netio.model;

namespace si.birokrat.rtc.netio.broadcast {
    public class BroadcastUdpSend : IUdpSendOld {

        #region // locals //
        private readonly IPAddress ipAddress = IPAddress.Broadcast;
        private readonly int ipPort;
        private IPEndPoint ipEndpoint;
        UdpClient net;
        bool running = false;
        #endregion

        #region // constructor //
        public BroadcastUdpSend(int ipPort) {
            this.ipPort = ipPort;
        }
        ~BroadcastUdpSend() {
            Stop();
        }
        #endregion

        #region // public //
        public bool Start() {
            if (running)
                return true;
            running = true;
            try {
                net = new UdpClient(AddressFamily.InterNetwork);
				net.Client.ReceiveBufferSize = Globals.RECEIVE_BUFFER;
				net.Client.SendBufferSize = Globals.SEND_BUFFER;
                ipEndpoint = new IPEndPoint(ipAddress, ipPort);
            } catch(Exception ex) {
                running = false;
                ExceptionInfo exi = ExceptionInfo.Create(ex);
                throw exi;
            }
            return running;
        }
        public bool Stop() {
            if (!running)
                return true;
            running = false;
            try { sendQuit(); } catch { }
            try { net.Close(); } finally { net = null; }
            return true;
        }
        public void Send(MessageModel message) {
            if(!running)
                if(!Start()) {
                    ExceptionInfo exi = ExceptionInfo.Create(customInfo: "Send.Start failed");
                    throw exi;
                }
            sendPacket(message);
        }
        #endregion

        #region // private //
        private void sendQuit() {
            MessageModel quit_message = new MessageModel() {
                type = MessageType.QUIT
            };
            sendPacket(quit_message);
        }
        private void sendPacket(MessageModel message) {
            if (!running)
                return;
            try {
                string data = JsonConvert.SerializeObject(message);
                byte[] byte_data = Encoding.UTF8.GetBytes(data);
                net.Send(byte_data, byte_data.Length, ipEndpoint);
            } catch(Exception ex) {
                ExceptionInfo exi = ExceptionInfo.Create(ex);
                throw exi;
            }
        }
        #endregion

    }
}
