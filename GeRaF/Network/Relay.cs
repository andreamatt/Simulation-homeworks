using GeRaF.Events.DutyCycle;
using GeRaF.Utils;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

namespace GeRaF.Network
{
	[JsonConverter(typeof(EnumJsonConverter))]
	enum RelayStatus
	{
		Asleep,
		Free,   // awake with no task
		Awaiting_region,
		Transmitting,
		Sensing,
		Awaiting_Signal,
		Backoff_Sensing,
		Backoff_CTS,
		Backoff_SinkRTS
	}

	partial class Relay
	{
		[JsonIgnore]
		public Simulation sim;

		public int id = -1;
		public double X;
		public double Y;
		public double range = -1;

		[JsonIgnore]
		public HashSet<Relay> neighbours = new HashSet<Relay>();
		public List<int> neighboursIds => neighbours.Select(n => n.id).ToList();

		public RelayStatus status = RelayStatus.Free;
		// if transmitting, what is it transmitting?
		public TransmissionType transmissionType = TransmissionType.RTS;

		//[JsonIgnore]
		public Packet packetToSend = null;
		//public int packetToSendId => packetToSend == null ? -1 : packetToSend.Id;

		//public HashSet<Transmission> activeTransmissions = new HashSet<Transmission>();
		//public HashSet<Transmission> finishedTransmissions = new HashSet<Transmission>();

		// dictionary with [relay, hasFailed]
		private Dictionary<Relay, bool> activeTransmissions = new Dictionary<Relay, bool>();
		public List<Transmission> finishedCTSs = new List<Transmission>();
		public bool receivedACK = false;

		// iteration status
		public int REGION_index = 0;
		public int COL_count = 0;   // number of collisions in that region during this attempt
		public int SENSE_count = 0;
		public int SINK_RTS_count = 0;
		//public int ATTEMPT_count = 0;
		public int PKT_count = 0;
		public int REGION_cycle = 0;

		// sensing status
		public bool isSensing = false;
		public bool hasSensed = false;

		// contention status
		[JsonIgnore]
		private Relay busyWith = null; // è in contesa per un pacchetto

		[JsonIgnore]
		public Relay BusyWith => busyWith;
		public int BusyWithId => busyWith == null ? -1 : busyWith.id;

		[JsonIgnore]
		private FreeEvent freeEvent = null;

		public void SelfReserve() {
			busyWith = this;
		}

		// called only if relay is either free or already busy with the reserver
		public void Reserve(Relay reserver) {
			if (reserver == this) {
				throw new Exception("Can't use Reserve on self, use SelfReserve instead");
			}
			else {
				if (busyWith == null) {
					busyWith = reserver;
					freeEvent = new FreeEvent {
						time = sim.clock + sim.protocolParameters.t_busy,
						relay = this,
						sim = sim
					};
					sim.eventQueue.Add(freeEvent);
				}
				else {
					freeEvent.time = sim.clock + sim.protocolParameters.t_busy;
					sim.eventQueue.Reschedule(freeEvent);
				}
			}
		}

		// called by FreeEvent
		public void Free() {
			busyWith = null;
			freeEvent = null;
			status = RelayStatus.Free;
			Reset();
		}

		public void FreeNow() {
			if (busyWith != this) {
				sim.eventQueue.Remove(freeEvent);
			}
			Free();
		}

		public void Reset() {
			packetToSend = null;
			REGION_index = 0;
			COL_count = 0;
			SENSE_count = 0;
			PKT_count = 0;
			SINK_RTS_count = 0;
			REGION_cycle = 0;
		}

		public void StartReceiving(Relay sender) {
			bool interested = false;
			switch (this.status) {
				case RelayStatus.Sensing:
					this.hasSensed = true;
					break;
				case RelayStatus.Free:
					interested = true;
					break;
				case RelayStatus.Awaiting_Signal:
					interested = true;
					break;
				case RelayStatus.Awaiting_region:
					interested = true;
					break;
				case RelayStatus.Asleep:
					break;
				case RelayStatus.Transmitting:
					break;
				case RelayStatus.Backoff_Sensing:
					break;
				case RelayStatus.Backoff_CTS:
					break;
				case RelayStatus.Backoff_SinkRTS:
					break;
				default:
					throw new Exception("Unexpected relay status");
			}

			if (interested) {
				bool failed = activeTransmissions.Count > 0;
				// every one else failes
				foreach (var transmitter in activeTransmissions.Keys.ToList()) {
					activeTransmissions[transmitter] = false;
				}
				this.activeTransmissions[sender] = failed;
			}
		}

		public void EndReceiving(TransmissionType transmissionType, Relay sender, Relay actualDestination = null) {
			// if i'm interested ( I was when started receiving )
			if (this.activeTransmissions.ContainsKey(sender)) {
				// decide what to do
				switch (transmissionType) {
					case TransmissionType.RTS:
						this.HandleRTS(sender);
						break;
					case TransmissionType.SINK_RTS:
						this.HandleSINKRTS(sender, actualDestination);
						break;
					case TransmissionType.CTS:
						this.HandleCTS(sender, actualDestination);
						break;
					case TransmissionType.PKT:
						this.HandlePKT(sender, actualDestination);
						break;
					case TransmissionType.ACK:
						this.HandleACK(sender, actualDestination);
						break;
					case TransmissionType.COL:
						this.HandleCOL(sender);
						break;
					case TransmissionType.SINK_COL:
						this.HandleSINKCOL(sender, actualDestination);
						break;
					default:
						break;
				}

				this.activeTransmissions.Remove(sender);
			}
		}
	}
}
