using si.birokrat.next.common.logging;
using si.birokrat.rtc;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Threading;

namespace si.birokrat.next.common_dll {
    public class SimpleBiroDLLToRTCAdapter : IDisposable {

        private BiroRTC _rtc;
        private Object result;
        private bool _resultArrived = false;
        string _guid;

        private static int RequestPollPeriodMs = 100;
        private static int InvitePollPeriodMs = 5000;

        public const string TOO_MANY_WAITS_ERROR = "Waited for more than 10 times. Aborting program.";


        #region [constructor]
        public SimpleBiroDLLToRTCAdapter(string guid) {
            _guid = guid;
            _rtc = new BiroRTC();
            _rtc.MessageReceived += Brtc_MessageReceived;
            _rtc.PeerJoined += (x) => { };
            _rtc.PeerLeft += (x) => { };
            _rtc.PeerTimeout += (x) => { };
            _rtc.Start(guid);
            Logger.Log("RTC Started");
        }

        public void Dispose() {
            _rtc.Stop();
            _rtc = null;
        }
        #endregion

        #region [API]
        public string SendRTCInvite() {

            BiroRTCMessage msg = new BiroRTCMessage();
            msg.type = "DLL";
            msg.command = $"REQ|{_guid}|";
            msg.arguments = $"INVITE";
            _rtc.Send(msg);

            // await result
            Thread.Sleep(InvitePollPeriodMs);
            if (!_resultArrived) {
                return "TIMEOUT";
            } else {
                Object res = result;
                _resultArrived = false;
                return (string)res;
            }
        }

        public string SendRTCMessage(object[] args, int result_count = 0, string method = null) {

            // prepare message
            if (method == null) {
                StackTrace stackTrace = new StackTrace();
                method = stackTrace.GetFrame(1).GetMethod().Name;
            }

            BiroRTCMessage msg = new BiroRTCMessage();
            msg.type = "DLL";
            msg.command = $"REQ|{_guid}|";
            msg.arguments = method + "|" + string.Join("|", args) + "|";

            // send message
            _rtc.Send(msg);
            Logger.Log("Request: " + msg.type + "|" + msg.command + "|" + msg.arguments);

            // await result
            do {
                Thread.Sleep(RequestPollPeriodMs);
            } while (!_resultArrived);


            Object res = result; // copy it actually not just copy ref...
            _resultArrived = false;

            if ((string)res == "WAIT" && result_count < 10) {
                result_count++;
                Thread.Sleep(1000);
                return SendRTCMessage(args, result_count + 1, method);
            }
            else if (result_count >= 10){
                return TOO_MANY_WAITS_ERROR;
            }

            return (string) res;
        }
        #endregion

        #region [event handlers]

        string[] result_parts = null;

        private void Brtc_MessageReceived(BiroRTCMessage message) {

            Console.WriteLine("Received message: " + message.type + "|" + message.command + "|");

            // return if origin is not DLL
            if (message.type != "DLL")
                return;
            string[] cmdparts = message.command.Split('|');
            if (cmdparts[0] != "RES" || cmdparts[1] != _guid)
                return;

            // handle case where message has 1 part
            if (cmdparts[2] == "1.1") {
                result = message.arguments;
                _resultArrived = true;
                return;
            }

            // handle multiple part case
            int[] tmp = cmdparts[2].Split('.').Select((x) => int.Parse(x)).ToArray();
            int page_number = tmp[0];
            int page_count = tmp[1];

            // create or add to message buffer
            if (result_parts == null) {
                result_parts = new string[page_count];
            }
            result_parts[page_number - 1] = message.arguments;

            // if final message -> aggregate the results and move forward
            if (page_number == page_count) {
                result = result_parts.Aggregate("", (x, y) => x + y);
                _resultArrived = true;
                result_parts = null;
            }
            
        }
        #endregion
    }
}
