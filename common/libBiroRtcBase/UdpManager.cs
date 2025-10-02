using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using si.birokrat.rtc.common;
using si.birokrat.rtc.netio.model;
using si.birokrat.rtc.netio.udp;

namespace si.birokrat.rtc {
	public class UdpManager {
		#region -- locals --
		private readonly IPAddress _ipAddressBroadcast = IPAddress.Broadcast;
		private readonly int _ipPortBroadcast = Globals.BROADCAST_IPP;
		private bool _enableBroadcast = true;
		private readonly IPAddress _ipAddressMulticast = Globals.MULTICAST_IPA;

		private IPAddress _ipAddressLocal;
		private int _ipPortUdp = 0;
		private bool _enableUdp = true;

		private string _id = Utils.InstanceIdentifier();
		private PeerInfo _peerInfo;
		private bool _running = false;
		private bool _logger = false;

		private IUdpReceive _receiverBroadcast;
		private IUdpReceive _receiverUdp;
		private IUdpSend _sender;

		private Dictionary<string, PeerModel> _peers = new Dictionary<string, PeerModel>();
		private List<Guid> _seenPackets = new List<Guid>();
		private Task _validatorTask;
		private Task _pingerTask;

		private Dictionary<string, PeerModel> _loggers = new Dictionary<string, PeerModel>();
		private ConcurrentQueue<MessageModel> _loggerQueue = new ConcurrentQueue<MessageModel>();
		private Task _loggerAnnounceTask;
		private Task _loggerSendTask;
		private Task _loggerValidateTask;
		#endregion
		#region -- properties --
		public bool receiverBroadcastFilter { get; set; } = true;
		#endregion
		#region -- events --
		public event EhMessageReceived MessageReceived;
		public event EhExceptionInfo ExceptionInfoRaised;
		public event EhPeerNotification PeerJoined;
		public event EhPeerNotification PeerLeft;
		public event EhPeerNotification PeerTimeout;
		#endregion
		#region -- ctor --
		public UdpManager() {
		}
		~UdpManager() {
			Stop();
		}
		#endregion
		#region -- public --
		public void Start(bool enableUdp = true, bool logger = false) {
			if (_running) return;
			_running = true;

			_enableUdp = enableUdp;
			_logger = logger;

			// -- ips and ports --
			_ipAddressLocal = Utils.GetLanIPAddress();
			_ipPortUdp = Utils.GetLanUdpFreePort();

			// -- load peer info --
			loadPeerInfo();

			// -- receivers --
			_receiverBroadcast = new UdpReceive(_ipAddressBroadcast, _ipPortBroadcast);
			_receiverBroadcast.ExceptionInfoEvent += _receiverBroadcastOnExceptionInfoEvent;
			_receiverBroadcast.MessageReceived += _receiverBroadcastOnMessageReceived;
			_receiverBroadcast.Start();

			if (_enableUdp) {
				_receiverUdp = new UdpReceive(_ipAddressLocal, _ipPortUdp);
				_receiverUdp.ExceptionInfoEvent += _receiverUdpOnExceptionInfoEvent;
				_receiverUdp.MessageReceived += _receiverUdpOnMessageReceived;
				_receiverUdp.Start();
			}

			// -- sender --
			_sender = new UdpSend();
			_sender.Start();

			if (_logger) {
				// -- logger server --
				_loggerAnnounceTask = new Task(loggerAnnounceThread);
				_loggerAnnounceTask.Start();
				// -- validator logger --
				_validatorTask = new Task(validatorLoggerThread);
				_validatorTask.Start();
			} else {
				// -- logger client --
				_loggerSendTask = new Task(loggerSendThread);
				_loggerSendTask.Start();
				// -- validator --
				_validatorTask = new Task(validatorThread);
				_validatorTask.Start();
				// -- pinger --
				_pingerTask = new Task(pingerThread);
				_pingerTask.Start();
			}
		}
		public void Stop() {
			if (!_running) return;
			_running = false;

			// -- validator --
			_validatorTask.Wait();

			if (_logger) {
				// -- logger server --
				_loggerAnnounceTask.Wait();
			} else {
				// -- logger client --
				_loggerSendTask.Wait();
				// -- pinger --
				_pingerTask.Wait();
			}

			// -- receivers --
			_receiverBroadcast.Stop();
			_receiverBroadcast.MessageReceived -= _receiverBroadcastOnMessageReceived;
			_receiverBroadcast.ExceptionInfoEvent -= _receiverBroadcastOnExceptionInfoEvent;
			_receiverBroadcast = null;

			if (_enableUdp) {
				_receiverUdp.Stop();
				_receiverUdp.MessageReceived -= _receiverUdpOnMessageReceived;
				_receiverUdp.ExceptionInfoEvent -= _receiverUdpOnExceptionInfoEvent;
				_receiverUdp = null;
			}

			// -- sender --
			_sender.Stop();
			_sender = null;
		}
		public void Send(MessageModel message) {
			if (_logger) {
				throw ExceptionInfo.Create(null, "Cannot send message in logger mode ...");
				return;
			}
			_seenPackets.Add(message.id);
			message.source = _id;
			message.type = MessageType.MESSAGE;
			sendMessage(message);
		}
		public List<string> GetPeers() {
			List<string> list = new List<string>();
			lock (_peers) {
				foreach (PeerModel peer in _peers.Values) {
					list.Add(peer.identifier);
				}
			}
			return list;
		}
		#endregion
		#region -- private -- broadcast --
		private void _receiverBroadcastOnExceptionInfoEvent(ExceptionInfo exi) {
			ExceptionInfoRaised?.Invoke(exi);
		}
		private void _receiverBroadcastOnMessageReceived(MessageModel msg) {
			if (msg.source == _id) return;
			msg.transport = MessageTransport.BROADCAST;
			if (receiverBroadcastFilter && !_logger) {
				if (string.IsNullOrEmpty(msg.destination) || msg.destination.Equals(_id)) {
					try {
						receivedMessageMain(msg);
					} catch (ExceptionInfo exi) {
						ExceptionInfoRaised?.Invoke(exi);
					} catch (Exception ex) {
						ExceptionInfoRaised?.Invoke(ExceptionInfo.Create(ex));
					}
				}
			} else {
				try {
					receivedMessageMain(msg);
				} catch (ExceptionInfo exi) {
					ExceptionInfoRaised?.Invoke(exi);
				} catch (Exception ex) {
					ExceptionInfoRaised?.Invoke(ExceptionInfo.Create(ex));
				}
			}
		}
		#endregion
		#region -- private -- udp --
		private void _receiverUdpOnExceptionInfoEvent(ExceptionInfo exi) {
			ExceptionInfoRaised?.Invoke(exi);
		}
		private void _receiverUdpOnMessageReceived(MessageModel msg) {
			if (msg.source == _id) return;
			msg.transport = MessageTransport.UDP;
			if (receiverBroadcastFilter && !_logger) {
				if (string.IsNullOrEmpty(msg.destination) || msg.destination.Equals(_id)) {
					try {
						receivedMessageMain(msg);
					} catch (ExceptionInfo exi) {
						ExceptionInfoRaised?.Invoke(exi);
					} catch (Exception ex) {
						ExceptionInfoRaised?.Invoke(ExceptionInfo.Create(ex));
					}
				}
			} else {
				try {
					receivedMessageMain(msg);
				} catch (ExceptionInfo exi) {
					ExceptionInfoRaised?.Invoke(exi);
				} catch (Exception ex) {
					ExceptionInfoRaised?.Invoke(ExceptionInfo.Create(ex));
				}
			}
		}
		#endregion
		#region -- private -- general -- receive --
		private void receivedMessageMain(MessageModel msg) {
			#region .. logger mode ..
			if (_logger) {
				MessageReceived?.Invoke(msg);
				return;
			}
			#endregion
			#region .. logger message ..
			if (msg.type == MessageType.LOGGER) {
				lock (_loggers) {
					if (_loggers.ContainsKey(msg.source)) {
						_loggers[msg.source].Active();
					} else {
						PeerModel peer = new PeerModel(msg.source);
						_loggers.Add(peer.identifier, peer);
					}
				}
				receivedMessageLogger(msg);
				return;
			}
			#endregion
			#region .. peers ..
			if (_peers.ContainsKey(msg.source)) {
				_peers[msg.source].Active();
			} else {
				PeerModel peer = new PeerModel(msg.source);
				lock (_peers) {
					_peers.Add(peer.identifier, peer);
				}
				PeerJoined?.Invoke(peer.identifier);
			}
			#endregion
			switch (msg.type) {
				case MessageType.MESSAGE:
					receivedMessageMessage(msg);
					break;
				case MessageType.ACK:
					receivedMessageAck(msg);
					break;
				case MessageType.PING:
					receivedMessagePing(msg);
					break;
				case MessageType.QUIT:
					receivedMessageQuit(msg);
					break;
			}
		}
		private void receivedMessageMessage(MessageModel msg) {
			if (!_logger)
				sendMessageAck(msg);
			if (!_seenPackets.Contains(msg.id)) {
				MessageReceived?.Invoke(msg);
				_seenPackets.Add(msg.id);
			}
		}
		private void receivedMessageAck(MessageModel msg) {
			if (!_peers.ContainsKey(msg.source)) return;
			_peers[msg.source].Ack(msg);
		}
		private void receivedMessagePing(MessageModel msg) {
			if (!_peers.ContainsKey(msg.source)) {
				lock (_peers) {
					_peers.Add(msg.source, new PeerModel(msg.source));
				}
			}
			if (msg.subType == "INFO") {
				PeerInfo peerInfo = PeerInfo.FromJson(msg.command);
				_peers[msg.source].info = peerInfo;
			}
		}
		private void receivedMessageQuit(MessageModel msg) {
			if (!_peers.ContainsKey(msg.source)) return;
			lock (_peers) {
				_peers.Remove(msg.source);
			}
			PeerLeft?.Invoke(msg.source);
		}
		private void receivedMessageLogger(MessageModel msg) {
			lock (_loggers) {
				if (!_loggers.ContainsKey(msg.source)) {
					_loggers.Add(msg.source, new PeerModel(msg.source));
				}
				if (msg.subType == "INFO") {
					PeerInfo peerInfo = PeerInfo.FromJson(msg.command);
					_loggers[msg.source].info = peerInfo;
				}
			}
		}
		#endregion
		#region -- private -- general -- send --
		private void sendMessage(MessageModel msg) {
			if (string.IsNullOrEmpty(msg.destination)) {
				List<string> peerKeys = _peers.Keys.ToList();
				foreach (string key in peerKeys) {
					try {
						PeerModel peer = _peers[key];
						peer.Message(msg);
					} catch (Exception ex) {
						ExceptionInfoRaised?.Invoke(ExceptionInfo.Create(ex));
					}
				}
			} else {
				if (msg.destination == _id) return;
				if (_peers.ContainsKey(msg.destination)) {
					_peers[msg.destination].Message(msg);
				}
			}
			msg.sent = DateTime.Now;
			_seenPackets.Add(msg.id);
			if (_loggers.Any())
				_loggerQueue.Enqueue(msg);
			if (!string.IsNullOrEmpty(msg.destination)) {
				PeerModel peerDestination;
				_peers.TryGetValue(msg.destination, out peerDestination);
				if (peerDestination == null) {
					ExceptionInfoRaised?.Invoke(new ExceptionInfo() {
						customInfo = "Destination peer not found ..."
					});
					return;
				}
				if (peerDestination.info != null && peerDestination.info.udpSupport && _enableUdp) {
					_sender.SendData(
						IPAddress.Parse(peerDestination.info.udpIpAddress),
						peerDestination.info.udpPort,
						msg);
					return;
				}
				if (peerDestination.info != null && peerDestination.info.broadcastSupport && _enableBroadcast) {
					_sender.SendData(
						IPAddress.Parse(peerDestination.info.broadcastIpAddress),
						peerDestination.info.broadcastPort,
						msg);
					return;
				}
			}
			_sender.SendData(_ipAddressBroadcast, _ipPortBroadcast, msg);
		}
		private void sendMessageAck(MessageModel msg) {
			PeerModel peer = _peers[msg.source];
			MessageModel ack = msg.CloneForAck(_id);
			if (peer.info == null) {
				_sender.SendData(_ipAddressBroadcast, _ipPortUdp, ack);
				return;
			}
			switch (msg.transport) {
				case MessageTransport.UDP:
					if (peer.info.udpSupport && _enableUdp) {
						_sender.SendData(IPAddress.Parse(peer.info.udpIpAddress), peer.info.udpPort, ack);
						return;
					}
					break;
				case MessageTransport.BROADCAST:
					if (peer.info.broadcastSupport && _enableBroadcast) {
						_sender.SendData(IPAddress.Parse(peer.info.broadcastIpAddress), peer.info.broadcastPort, ack);
						return;
					}
					break;
			}
			_sender.SendData(_ipAddressBroadcast, _ipPortBroadcast, ack);
		}
		#endregion
		#region -- private -- general -- info --
		private void loadPeerInfo() {
			_peerInfo = new PeerInfo() {
				broadcastSupport = _enableBroadcast,
				broadcastIpAddress = _ipAddressBroadcast.ToString(),
				broadcastPort = _ipPortBroadcast,
				multicastSupport = false,
				udpSupport = _enableUdp,
				udpIpAddress = _ipAddressLocal.ToString(),
				udpPort = _ipPortUdp
			};
		}
		#endregion
		#region -- validator --
		private void validatorThread() {
			while (_running) {
				try {
					#region .. remove invalid loggers ..
					lock (_peers) {
						List<string> invalidLoggers = new List<string>();
						List<string> loggerKey = _loggers.Keys.ToList();
						foreach (string key in loggerKey) {
							if (!_running) return;
							try {
								PeerModel peer = _loggers[key];
								if ((DateTime.Now - peer.lastActivity) > new TimeSpan(0, 0, 0, 0, Globals.PEER_TIMEOUT))
									invalidLoggers.Add(peer.identifier);
							} catch (Exception ex) {
								ExceptionInfoRaised?.Invoke(ExceptionInfo.Create(ex));
							}
						}
						invalidLoggers.ForEach((x) => {
							if (!_running) return;
							string tempIdentifier = _loggers[x].identifier;
							_loggers[x].Dispose();
							_loggers.Remove(x);
						});
					}
					#endregion
					#region .. remove invalid peers ..
					lock (_peers) {
						List<string> invalidPeers = new List<string>();
						List<string> peerKey = _peers.Keys.ToList();
						foreach (string key in peerKey) {
							if (!_running) return;
							try {
								PeerModel peer = _peers[key];
								if ((DateTime.Now - peer.lastActivity) > new TimeSpan(0, 0, 0, 0, Globals.PEER_TIMEOUT))
									invalidPeers.Add(peer.identifier);
							} catch (Exception ex) {
								ExceptionInfoRaised?.Invoke(ExceptionInfo.Create(ex));
							}
						}
						invalidPeers.ForEach((x) => {
							if (!_running) return;
							string tempIdentifier = _peers[x].identifier;
							_peers[x].Dispose();
							_peers.Remove(x);
							PeerTimeout?.Invoke(tempIdentifier);
						});
					}
					#endregion
					#region .. resend ..
					lock (_peers) {
						List<string> peerKey = _peers.Keys.ToList();
						foreach (string key in peerKey) {
							if (!_running) return;
							try {
								PeerModel peer = _peers[key];
								MessageModel[] messages = peer.Resend();
								if (messages == null) continue;
								lock (messages) {
									foreach (MessageModel message in messages) {
										if (!_running) return;
										sendMessage(message.CloneForResend(peer.identifier));
									}
								}
							} catch (Exception ex) {
								ExceptionInfoRaised?.Invoke(ExceptionInfo.Create(ex));
							}
						}
					}
					#endregion
					#region .. cleanup ..
					if (_seenPackets.Count > Globals.MESSAGE_SEEN_BUFFER) {
						_seenPackets.RemoveRange(0, Globals.MESSAGE_SEEN_BUFFER);
					}
					#endregion
					#region .. delay ..
					for (int rpt = 0; rpt < Globals.VALIDATION_DELAY_REPEAT; rpt++) {
						if (!_running) return;
						Thread.Sleep(Globals.VALIDATION_DELAY_TIME);
					}
					#endregion
				} catch (ExceptionInfo exi) {
					ExceptionInfoRaised?.Invoke(exi);
				} catch (Exception ex) {
					ExceptionInfoRaised?.Invoke(ExceptionInfo.Create(ex));
				}
			}
		}
		private void validatorLoggerThread() {
			while (_running) {
				try {
					#region .. remove invalid loggers ..
					lock (_peers) {
						List<string> invalidLoggers = new List<string>();
						List<string> loggerKey = _loggers.Keys.ToList();
						foreach (string key in loggerKey) {
							if (!_running) return;
							try {
								PeerModel peer = _loggers[key];
								if ((DateTime.Now - peer.lastActivity) > new TimeSpan(0, 0, 0, 0, Globals.PEER_TIMEOUT))
									invalidLoggers.Add(peer.identifier);
							} catch (Exception ex) {
								ExceptionInfoRaised?.Invoke(ExceptionInfo.Create(ex));
							}
						}
						invalidLoggers.ForEach((x) => {
							if (!_running) return;
							string tempIdentifier = _loggers[x].identifier;
							_loggers[x].Dispose();
							_loggers.Remove(x);
						});
					}
					#endregion
					#region .. remove invalid peers ..
					lock (_peers) {
						List<string> invalidPeers = new List<string>();
						List<string> peerKey = _peers.Keys.ToList();
						foreach (string key in peerKey) {
							if (!_running) return;
							try {
								PeerModel peer = _peers[key];
								if ((DateTime.Now - peer.lastActivity) > new TimeSpan(0, 0, 0, 0, Globals.PEER_TIMEOUT))
									invalidPeers.Add(peer.identifier);
							} catch (Exception ex) {
								ExceptionInfoRaised?.Invoke(ExceptionInfo.Create(ex));
							}
						}
						invalidPeers.ForEach((x) => {
							if (!_running) return;
							string tempIdentifier = _peers[x].identifier;
							_peers[x].Dispose();
							_peers.Remove(x);
							PeerTimeout?.Invoke(tempIdentifier);
						});
					}
					#endregion
					#region .. cleanup ..
					if (_seenPackets.Count > Globals.MESSAGE_SEEN_BUFFER) {
						_seenPackets.RemoveRange(0, Globals.MESSAGE_SEEN_BUFFER);
					}
					#endregion
					#region .. delay ..
					for (int rpt = 0; rpt < Globals.VALIDATION_DELAY_REPEAT; rpt++) {
						if (!_running) return;
						Thread.Sleep(Globals.VALIDATION_DELAY_TIME);
					}
					#endregion
				} catch (ExceptionInfo exi) {
					ExceptionInfoRaised?.Invoke(exi);
				} catch (Exception ex) {
					ExceptionInfoRaised?.Invoke(ExceptionInfo.Create(ex));
				}
			}
		}
		#endregion
		#region -- pinger --
		private void pingerThread() {
			while (_running) {
				try {
					MessageModel msg = new MessageModel() {
						type = MessageType.PING,
						subType = "INFO",
						command = _peerInfo.ToJson(),
					};
					_seenPackets.Add(msg.id);
					_sender.SendData(_ipAddressBroadcast, _ipPortBroadcast, msg);
					for (int rpt = 0; rpt < Globals.PING_INTERVAL_REPEAT; rpt++) {
						if (!_running) return;
						Thread.Sleep(Globals.PING_INTERVAL_TIME);
					}
				} catch (ExceptionInfo exi) {
					ExceptionInfoRaised?.Invoke(exi);
				} catch (Exception ex) {
					ExceptionInfoRaised?.Invoke(ExceptionInfo.Create(ex));
				}
			}
		}
		#endregion
		#region -- logger announce --
		private void loggerAnnounceThread() {
			while (_running) {
				try {
					MessageModel msg = new MessageModel() {
						type = MessageType.LOGGER,
						subType = "INFO",
						command = _peerInfo.ToJson(),
					};
					_seenPackets.Add(msg.id);
					_sender.SendData(_ipAddressBroadcast, _ipPortBroadcast, msg);
					for (int rpt = 0; rpt < Globals.PING_INTERVAL_REPEAT; rpt++) {
						if (!_running) return;
						Thread.Sleep(Globals.PING_INTERVAL_TIME);
					}
				} catch (ExceptionInfo exi) {
					ExceptionInfoRaised?.Invoke(exi);
				} catch (Exception ex) {
					ExceptionInfoRaised?.Invoke(ExceptionInfo.Create(ex));
				}
			}
		}
		private void loggerSendThread() {
			while (_running) {
				try {
					if (_loggers.Any()) {
						bool runLoop = true;
						while (runLoop) {
							if (_loggerQueue.TryDequeue(out var msgLog)) {
								foreach (KeyValuePair<string, PeerModel> pair in _loggers) {
									_sender.SendData(
										IPAddress.Parse(pair.Value.info.udpIpAddress),
										pair.Value.info.udpPort,
										msgLog);
								}
							} else {
								runLoop = false;
							}
						}
					}
					Task.Delay(50).Wait();
				} catch (ExceptionInfo exi) {
					ExceptionInfoRaised?.Invoke(exi);
				} catch (Exception ex) {
					ExceptionInfoRaised?.Invoke(ExceptionInfo.Create(ex));
				}
			}
		}
		#endregion
	}
}
