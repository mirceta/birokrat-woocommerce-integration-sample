using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace si.birokrat.rtc.netio.model {
	public enum MessageTransport {
		UNKNOWN = 0,
		BROADCAST = 10,
		MULTICAST = 20,
		UDP = 30
	}
}
