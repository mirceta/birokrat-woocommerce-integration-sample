using si.birokrat.rtc.common;

namespace si.birokrat.rtc.netio {
	public interface IUdpReceiveOld {
        #region // events //
        event EhExceptionInfo ExceptionInfoEvent;
		event EhMessageReceived MessageReceived;
        #endregion

        #region // public //
        bool Running();
        bool Start();
		bool Stop();
        #endregion
    }
}
