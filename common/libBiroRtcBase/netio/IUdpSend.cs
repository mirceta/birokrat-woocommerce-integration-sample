using si.birokrat.rtc.netio.model;

namespace si.birokrat.rtc.netio {
	public interface IUdpSend {
        #region // public //
        bool Start();
		bool Stop();
		void Send(MessageModel message);
        #endregion
    }
}
