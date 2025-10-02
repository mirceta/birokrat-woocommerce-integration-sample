using System.Collections.Generic;
using si.birokrat.rtc.common;

namespace si.birokrat.rtc.netio.model {
	public class PeerInfo {
		#region -- properties --
		public bool broadcastSupport { get; set; }
		public string broadcastIpAddress { get; set; }
		public int broadcastPort { get; set; }
		public bool multicastSupport { get; set; }
		public string multicastIpAddress { get; set; }
		public int multicastPort { get; set; }
		public bool udpSupport { get; set; }
		public string udpIpAddress { get; set; }
		public int udpPort { get; set; }
		public Dictionary<string, string> capabilities { get; set; } = new Dictionary<string, string>();
		#endregion
		#region -- methods --
		public string ToJson() {
			return Serialization.Serialize(this);
		}
		public static PeerInfo FromJson(string jsonData) {
			return Serialization.Deserialize<PeerInfo>(jsonData);
		}
		#endregion
	}
}
