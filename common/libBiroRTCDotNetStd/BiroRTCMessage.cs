using System;
using System.Runtime.InteropServices;

using si.birokrat.rtc.netio.model;

namespace si.birokrat.rtc {
	[ComVisible(true)]
	[Guid("CD39939A-6A96-4885-979E-8A274D4B9993")]
	[InterfaceType(ComInterfaceType.InterfaceIsIDispatch)]
	public interface _BiroRTCMessage {
		[DispId(1)]
		string source { get; set; }
		[DispId(2)]
		string destination { get; set; }
		[DispId(3)]
		string type { get; set; }
		[DispId(4)]
		string command { get; set; }
		[DispId(5)]
		string arguments { get; set; }
	}

	[ComVisible(true)]
	[Guid("AEE9A4FD-40B5-46D7-AD11-62DB6486D907")]
	[ClassInterface(ClassInterfaceType.None)]
	public class BiroRTCMessage : _BiroRTCMessage {
		#region // constructor //
		public BiroRTCMessage() {
			source = string.Empty;
			destination = string.Empty;
			type = string.Empty;
			command = string.Empty;
			arguments = string.Empty;
		}
		#endregion
		#region // properties //
		public string source{ get; set; }
		public string destination{ get; set; }
		public string type{ get; set; }
		public string command{ get; set; }
		public string arguments{ get; set; }
		#endregion
		#region // statics //
		public static BiroRTCMessage MulToBir(MessageModel msgin) {
			BiroRTCMessage msgout = new BiroRTCMessage() {
				source = msgin.source,
				destination = msgin.destination,
				type = msgin.subType,
				command = msgin.command,
				arguments = msgin.arguments
			};
			return msgout;
		}
		public static MessageModel BirToMul(BiroRTCMessage msgin) {
            MessageModel msgout = new MessageModel() {
				source = msgin.source,
				destination = msgin.destination,
				subType = msgin.type,
				command = msgin.command,
				arguments = msgin.arguments
			};
			return msgout;
		}
		#endregion
	}
}
