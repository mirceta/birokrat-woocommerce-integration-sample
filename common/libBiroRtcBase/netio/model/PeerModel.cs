using System;
using System.Collections.Generic;
using System.Linq;
using si.birokrat.rtc.common;

namespace si.birokrat.rtc.netio.model {
    public class PeerModel : IDisposable {
        #region -- constructor --
        public PeerModel(string identifier) {
            this.identifier = identifier;
            this.lastActivity = DateTime.Now;
            this.messages = new Dictionary<Guid, MessageModel>();
        }
        #endregion
        #region -- properties --
        public string identifier { get; set; }
        public DateTime lastActivity { get; set; }
        public Dictionary<Guid, MessageModel> messages { get; set; }
        public PeerInfo info { get; set; }
		#endregion
		#region -- public --
		public void Active() {
			lastActivity = DateTime.Now;
		}
		public void Ack(MessageModel message) {
			lock (messages) {
				if (messages.ContainsKey(message.id)) {
                    messages.Remove(message.id);
                }
            }
        }
        public void Message(MessageModel message) {
			lock (messages) {
				messages[message.id] = message;
            }
        }
        public MessageModel[] Resend() {
            MessageModel[] result;
            lock(messages) {
				var oldIds =
					(from val in messages.Values
					 where (DateTime.Now - val.created) > new TimeSpan(0, 0, 0, 0, Globals.MESSAGE_TIMEOUT)
					 select val.id);
				Guid[] idArray = oldIds.ToArray();
				foreach (Guid id in idArray)
					messages.Remove(id);
				var query =
					from val in messages.Values
					where (DateTime.Now - val.sent) > new TimeSpan(0, 0, 0, 0, Globals.RESEND_TIMEOUT)
					select val;

				result = query.ToArray();
			}
            return result;
        }
        #endregion
        #region -- IDisposable --
        public void Dispose() {
            messages.Clear();
            messages = null;
        }
        #endregion
    }
}
