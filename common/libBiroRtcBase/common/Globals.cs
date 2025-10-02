using System.Net;

namespace si.birokrat.rtc.common {
    public static class Globals {
        #region // constants //
        public static string ID = "";
        public static int MODE = 2;

		public static int UDP_IPP_MIN = 43000;
		public static int UDP_IPP_MAX = 44000;
		public static IPAddress MULTICAST_IPA = IPAddress.Parse("224.0.0.42");
        public static int MULTICAST_IPP = 42042;
        public static int BROADCAST_IPP = 42042;

		public static int MESSAGE_SEEN_BUFFER = 1000;
        public static int PEER_TIMEOUT = 17500;
		public static int MESSAGE_TIMEOUT = 5000;
        public static int RESEND_TIMEOUT = 1000;
		public static int RECEIVE_BUFFER = 512000;
		public static int SEND_BUFFER = 512000;

		public static int LIVE_CHECK_INTERVAL_RPT = 500;
		public static int LIVE_CHECK_INTERVAL_TIME = 10;
		public static int LIVE_TIMEOUT_INTERVAL = 30;

		public static int PING_INTERVAL_REPEAT = 500;
		public static int PING_INTERVAL_TIME = 10;

		public static int VALIDATION_DELAY_REPEAT = 50;
		public static int VALIDATION_DELAY_TIME = 10;
		#endregion
	}
}
