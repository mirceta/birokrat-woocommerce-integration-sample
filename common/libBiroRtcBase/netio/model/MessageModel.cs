using System;
using Newtonsoft.Json;
using si.birokrat.rtc.common;

namespace si.birokrat.rtc.netio.model {
    public class MessageModel {
        #region -- properties --
        public Guid id { get; set; } = Guid.NewGuid();
		public DateTime created { get; set; } = DateTime.Now;
		public DateTime sent { get; set; } = DateTime.Now;
        public string source { get; set; } = Utils.InstanceIdentifier();
        public string destination { get; set; } = string.Empty;
        public MessageType type { get; set; } = MessageType.MESSAGE;
        public string subType { get; set; } = string.Empty;
        public string command { get; set; } = string.Empty;
        public string arguments { get; set; } = string.Empty;
        [JsonIgnore]
        public MessageTransport transport { get; set; } = MessageTransport.UNKNOWN;
		#endregion
		#region -- public --
		public MessageModel CloneForAck(string source) {
            MessageModel temp = new MessageModel() {
                id = this.id,
                created = this.created,
                sent = this.sent,
                source = source,
                destination = this.source,
                type = MessageType.ACK,
                subType = string.Empty,
                command = string.Empty,
                arguments = string.Empty,
                transport = transport
            };
            return temp;
        }
        public MessageModel CloneForResend(string destination) {
            MessageModel temp = new MessageModel() {
                id = this.id,
                created = this.created,
                sent = this.sent,
                source = this.source,
                destination = destination,
                type = this.type,
                subType = this.subType,
                command = this.command,
                arguments = this.arguments,
				transport = this.transport
			};
            return temp;
        }
        #endregion
    }
}
