using System;
using si.birokrat.rtc.netio.model;

namespace si.birokrat.rtc.common {
  #region // delegates internal //
  public delegate void EhExceptionInfo(ExceptionInfo exi);
  public delegate void EhMessageReceived(MessageModel message);
  #endregion

  #region // delegates internal //
  public delegate void EhPeerNotification(string peer);
  public delegate void EhException(string source, Exception ex);
  #endregion

  #region // delegates com //
  public delegate void EhComVoid();
  public delegate void EhComException(string source, string exception);
  public delegate void EhComPeerNotification(string peer);
  #endregion
}
