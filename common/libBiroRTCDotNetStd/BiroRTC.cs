using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Threading;
using System.Threading.Tasks;

using Newtonsoft.Json;

using si.birokrat.rtc.common;
using si.birokrat.rtc.netio.model;

namespace si.birokrat.rtc {

    public delegate void EhComMessage(BiroRTCMessage message);

    [ComVisible(true)]
    [Guid("5E85658A-9458-4E41-8552-FB67DC49A17F")]
    [InterfaceType(ComInterfaceType.InterfaceIsIDispatch)]
    public interface _BiroRTCEvents {
        void ExceptionRaised(string source, string exception);
        void MessageReceived(BiroRTCMessage message);
        void PeerJoined(string identifier);
        void PeerLeft(string identifier);
        void PeerTimeout(string identifier);
		void RtcLiveCheck();
		void RtcQuit();
    }

    [ComVisible(true)]
    [Guid("970A4AA9-5883-4E95-A2CD-9FFEC8FF0247")]
    [InterfaceType(ComInterfaceType.InterfaceIsIDispatch)]
    public interface _BiroRTC {
        [DispId(1)]
        bool Start();
		[DispId(2)]
		bool StartWithId(string id);
		[DispId(3)]
		bool StartNoEvents();
		[DispId(4)]
		bool StartNoEventsWithId(string id);
		[DispId(5)]
		bool EnableUdp(bool enabled);

		[DispId(10)]
		void Stop();

		[DispId(20)]
		bool Send(BiroRTCMessage message);

		[DispId(30)]
		string[] GetPeers();
		[DispId(35)]
		BiroRTCMessage GetNextMessage();
		[DispId(40)]
		string GetNextPeerLeft();
		[DispId(45)]
		string GetNextPeerJoined();
		[DispId(50)]
		string GetNextPeerTimeout();

		[DispId(60)]
		void ConfirmLive();
		[DispId(70)]
		void TimeoutInSeconds(int timeout);
		[DispId(80)]
		bool IsLogger(bool enabled);
		[DispId(100)]
		bool IncommingFilter(bool enabled);
	}
	
	[ComVisible(true)]
    [Guid("8829CCF8-E085-4690-B5BC-D28B3875A492")]
    [ComSourceInterfaces(typeof(_BiroRTCEvents))]
    [ClassInterface(ClassInterfaceType.None)]
    [ProgId("Birokrat.BiroRTC")]
    public class BiroRTC : _BiroRTC {
		#region -- locals --
		DateTime isRtcLiveLast = DateTime.MinValue;
		Task isRtcLiveThread = null;
        UdpManager manager = null;
		bool rtcThreadsRunning = false;
		bool noEvents = false;
		bool incomingFilter = true;
		bool udpEnabled = true;
		bool isLogger = false;
		Task DispatcherTask;
		#endregion
		#region -- events --
		public event EhComException ExceptionRaised;
        public event EhComMessage MessageReceived;
        public event EhComPeerNotification PeerJoined;
        public event EhComPeerNotification PeerLeft;
        public event EhComPeerNotification PeerTimeout;
		public event EhComVoid RtcLiveCheck;
		public event EhComVoid RtcQuit;
        #endregion
        #region -- constructor --
        public BiroRTC() {
        }
        ~BiroRTC() {
            Stop();
        }
		#endregion
		#region -- public --
		public bool Start() {
			return StartWithId("");
		}
		public bool StartWithId(string id = "") {
            Globals.ID = id;
			rtcThreadsRunning = true;
            if (manager != null) return true;

            manager = new UdpManager();
			manager.receiverBroadcastFilter = incomingFilter;
            
			manager.MessageReceived += managerOnMessageReceived;
            manager.ExceptionInfoRaised += managerOnExceptionInfoRaised;
            manager.PeerJoined += managerOnPeerJoined;
            manager.PeerLeft += managerOnPeerLeft;
            manager.PeerTimeout += managerOnPeerTimeout;

            try {
                manager.Start(udpEnabled, isLogger);
				if (RtcLiveCheck != null) {
					isRtcLiveLast = DateTime.Now;
					liveCheckStart();
				}
            } catch (Exception ex) {
                ExceptionInfo exi = ExceptionInfo.Create(ex);
                managerOnExceptionInfoRaised(exi);
                manager = null;
                return false;
            }
            return true;
        }
		public bool StartNoEvents() {
			return StartNoEventsWithId("");
		}
		public bool StartNoEventsWithId(string id = "") {
			noEvents = true;
			Globals.ID = id;
			if (manager != null)
				return true;
			manager = new UdpManager();
			manager.receiverBroadcastFilter = incomingFilter;
			manager.MessageReceived += managerOnMessageReceived;
			//manager.ExceptionInfoRaised += Manager_ExceptionInfoRaised;
			manager.PeerJoined += managerOnPeerJoined;
			manager.PeerLeft += managerOnPeerLeft;
			manager.PeerTimeout += managerOnPeerTimeout;
			try {
				manager.Start(udpEnabled, isLogger);
				if (RtcLiveCheck != null) {
					isRtcLiveLast = DateTime.Now;
					liveCheckStart();
				}
			} catch (Exception ex) {
				ExceptionInfo exi = ExceptionInfo.Create(ex);
				managerOnExceptionInfoRaised(exi);
				manager = null;
				return false;
			}
			return true;
		}
		public void Stop() {
			rtcThreadsRunning = false;
            if (manager != null)
                manager.Stop();
            manager = null;
        }
        public bool Send(BiroRTCMessage message) {
			isRtcLiveLast = DateTime.Now;
            if (manager == null) return false;
            try {
                if (manager != null) {
                    MessageModel msgout = BiroRTCMessage.BirToMul(message);
                    manager.Send(msgout);
                }
            } catch(ExceptionInfo exi) {
                managerOnExceptionInfoRaised(exi);
                return false;
            } catch(Exception ex) {
                ExceptionInfo exi = ExceptionInfo.Create(ex);
                managerOnExceptionInfoRaised(exi);
                return false;
            }
            return true;
        }
        public string[] GetPeers() {
			isRtcLiveLast = DateTime.Now;
            if (manager != null)
                return manager.GetPeers()?.ToArray<string>();
            else
                return null;
        }
		public BiroRTCMessage GetNextMessage() {
			if (messageReceivedQueue.Count > 0) {
				BiroRTCMessage message = messageReceivedQueue.Dequeue();
				if (message != null)
					return message;
			}
			return null;
		}
		public string GetNextPeerLeft() {
			if(peerLeftQueue.Count > 0)
				return peerLeftQueue.Dequeue();
			return "";
		}
		public string GetNextPeerJoined() {
			if (peerJoinedQueue.Count > 0)
				return peerJoinedQueue.Dequeue();
			return "";
		}
		public string GetNextPeerTimeout() {
			if (peerTimeoutQueue.Count > 0)
				return peerTimeoutQueue.Dequeue();
			return "";
		}
		public bool IncommingFilter(bool enabled) {
			incomingFilter = enabled;
			if (manager != null)
				manager.receiverBroadcastFilter = incomingFilter;
			return true;
		}
		public bool EnableUdp(bool enabled) {
			udpEnabled = enabled;
			return true;
		}
		public bool IsLogger(bool enabled) {
			isLogger = enabled;
			return true;
		}
		#endregion
		#region -- private event lock --
		bool eventLockFreeBool = true;
		DateTime eventLockDate = DateTime.MaxValue;
		private bool eventLock() {
			if(!eventLockFreeBool) {
				if (eventLockDate < DateTime.Now.AddSeconds(-10))
					eventLockFreeBool = true;
			}
			if (eventLockFreeBool) {
				eventLockFreeBool = false;
				return true;
			}
			return false;
		}
		private void eventUnlock() {
			eventLockFreeBool = true;
			eventLockDate = DateTime.MaxValue;
		}
		#endregion
		#region -- live check --
		public void ConfirmLive() {
			isRtcLiveLast = DateTime.Now;
		}
		public void TimeoutInSeconds(int timeout) {
			Globals.LIVE_TIMEOUT_INTERVAL = timeout;
		}
		private void liveCheckStart() {
			isRtcLiveThread = new Task(() => liveCheckLoop());
			isRtcLiveThread.Start();
		}
		private void liveCheckLoop() {
			while (true) {
				if((DateTime.Now - isRtcLiveLast).Seconds > Globals.LIVE_TIMEOUT_INTERVAL) {
					Stop();
					if(!noEvents)
						RtcQuit?.Invoke();
					break;
				}

				try {
					if (!noEvents) {
						while (!eventLock()) { Thread.Sleep(10); }
						RtcLiveCheck?.Invoke();
						eventUnlock();
					}
				} finally {
				}

				for (int i = 0; i < Globals.LIVE_CHECK_INTERVAL_RPT; i++) {
					if (!rtcThreadsRunning) return;
					Thread.Sleep(Globals.LIVE_CHECK_INTERVAL_TIME);
				}
			}
		}
		#endregion
		#region -- event handlers - message received --
		Queue<BiroRTCMessage> messageReceivedQueue = new Queue<BiroRTCMessage>();
        private void managerOnMessageReceived(MessageModel message) {
            try {
				if (message == null) return;
				BiroRTCMessage msgout = BiroRTCMessage.MulToBir(message);
				lock(messageReceivedQueue)
					messageReceivedQueue.Enqueue(msgout);
				if (!noEvents)
					dispatchTaskStart();
			} finally { }
        }
        #endregion
        #region -- event handlers - exception raised --
        Queue<ExceptionInfo> exceptionInfoRaisedQueue = new Queue<ExceptionInfo>();
        private void managerOnExceptionInfoRaised(ExceptionInfo exi) {
            try {
				if (!noEvents) {
					exceptionInfoRaisedQueue.Enqueue(exi);
					dispatchTaskStart();
				}
			} finally { }
        }
        #endregion
        #region -- event handlers - peer joined --
        Queue<string> peerJoinedQueue = new Queue<string>();
        private void managerOnPeerJoined(string peer) {
            try {
				peerJoinedQueue.Enqueue(peer);
				if (!noEvents)
					dispatchTaskStart();
			} finally { }
        }
        #endregion
        #region -- event handlers - peer left --
        Queue<string> peerLeftQueue = new Queue<string>();
        private void managerOnPeerLeft(string peer) {
            try {
				peerLeftQueue.Enqueue(peer);
				if (!noEvents)
					dispatchTaskStart();
			} finally { }
        }
        #endregion
        #region -- event handlers - peer timeout --
        Queue<string> peerTimeoutQueue = new Queue<string>();
        private void managerOnPeerTimeout(string peer) {
            try {
				peerTimeoutQueue.Enqueue(peer);
				if (!noEvents)
					dispatchTaskStart();
			} finally { }
        }
		#endregion
		#region -- dispatcher --
		private void dispatchTaskStart() {
			if (DispatcherTask != null) return;
			DispatcherTask = new Task(() => dispatchLoop());
			DispatcherTask.Start();
		}
		private void dispatchLoop() {
			while (rtcThreadsRunning) {
				try {
					// messages
					while (messageReceivedQueue.Count > 0) {
						if (MessageReceived == null) Stop();
						BiroRTCMessage message;
						lock (messageReceivedQueue)
							message = messageReceivedQueue.Dequeue();
						if (message != null)
							MessageReceived?.Invoke(message);
					}
					// joined
					while (peerJoinedQueue.Count > 0) {
						string message = peerJoinedQueue.Dequeue();
						if (!string.IsNullOrEmpty(message))
							PeerJoined?.Invoke(message);
					}
					// left
					while (peerLeftQueue.Count > 0) {
						string message = peerLeftQueue.Dequeue();
						if (!string.IsNullOrEmpty(message))
							PeerLeft?.Invoke(message);
					}
					// timeout
					while (peerTimeoutQueue.Count > 0) {
						string message = peerTimeoutQueue.Dequeue();
						if (!string.IsNullOrEmpty(message))
							PeerTimeout?.Invoke(message);
					}
					// exceptions
					while (exceptionInfoRaisedQueue.Count > 0) {
						ExceptionInfo msg = exceptionInfoRaisedQueue.Dequeue();
						if (msg != null) {
							string json = JsonConvert.SerializeObject(msg);
							ExceptionRaised?.Invoke("RTC", json);
						}
					}
				} catch(Exception ex) {
					ExceptionInfo exi = ExceptionInfo.Create(ex);
					exceptionInfoRaisedQueue.Enqueue(exi);
				}
				for (int i = 1; i < 500; i++) {
					if (messageReceivedQueue.Count > 0) break;
					if (peerJoinedQueue.Count > 0) break;
					if (peerLeftQueue.Count > 0) break;
					if (peerTimeoutQueue.Count > 0) break;
					if (exceptionInfoRaisedQueue.Count > 0) break;
					if (!rtcThreadsRunning) break;
					Thread.Sleep(10);
				}
			}
		}
		#endregion
	}
}
