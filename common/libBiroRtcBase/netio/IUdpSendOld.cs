using si.birokrat.rtc.netio.model;

namespace si.birokrat.rtc.netio {
	public interface IUdpSendOld {
        #region // public //
        bool Start();
		bool Stop();
		void Send(MessageModel message);
        #endregion
    }
}
