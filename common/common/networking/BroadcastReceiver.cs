using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Text;

namespace si.birokrat.next.common.networking {
    public class BroadcastReceiver {

        private readonly string _message = string.Empty;
        private readonly int _port = 0;
        private UdpClient _client = null;
        private IPEndPoint _endpoint = null;
        private readonly int _delay = 0;
        private readonly bool _verbose = false;
        private bool running = false;

        public BroadcastReceiver(int port = 10000) {
            _port = port;
            _client = new UdpClient();
            _endpoint = new IPEndPoint(IPAddress.Any, port);
            _client.Client.Bind(_endpoint);
        }

        public string Receive(int timeoutSeconds) {
            
            var timeToWait = TimeSpan.FromSeconds(timeoutSeconds);
            var udpClient = _client;

            var asyncResult = udpClient.BeginReceive(null, null);
            asyncResult.AsyncWaitHandle.WaitOne(timeToWait);
            if (asyncResult.IsCompleted) {
                try {
                    IPEndPoint remoteEP = null;
                    byte[] receivedData = udpClient.EndReceive(asyncResult, ref remoteEP);
                    string result = Encoding.UTF8.GetString(receivedData);
                    _client.Close();
                    return result;
                } catch (Exception ex) {
                    _client.Close();
                    throw ex;
                }
            } else {
                _client.Close();
                return null;
            }

        }
    }
}
