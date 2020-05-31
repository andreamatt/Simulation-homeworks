using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

namespace GeRaF
{
	[JsonConverter(typeof(EnumJsonConverter))]
	enum RelayStatus
	{
		Asleep,
		Free,   // awake with no task
		Transmitting,
		Sensing,
		Awaiting_Signal,
		Backoff_Sensing,
		Backoff_CTS
	}

	class Relay
	{
		public int id = -1;
		public double X;
		public double Y;
		public double range = -1;

		[JsonIgnore]
		public HashSet<Relay> neighbours = new HashSet<Relay>();
		public List<int> neighboursIds => neighbours.Select(n => n.id).ToList();

		public RelayStatus status = RelayStatus.Free;

		//[JsonIgnore]
		public Packet packetToSend = null;
		//public int packetToSendId => packetToSend == null ? -1 : packetToSend.Id;

		public HashSet<Transmission> activeTransmissions = new HashSet<Transmission>();
		public HashSet<Transmission> finishedTransmissions = new HashSet<Transmission>();

		// iteration status
		public int regionIndex = 0;
		public int COL_count = 0;   // number of collisions in that region during this attempt
		public int SENSE_count = 0;
		public int ATTEMPT_count = 0;
		public int PKT_count = 0;

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
		private FreeRelayEvent freeEvent = null;

		public void SelfReserve() {
			busyWith = this;
		}

		// called only if relay is either free or already busy with the reserver
		public void Reserve(Relay reserver, Simulation sim) {
			if (reserver == this) {
				throw new Exception("Can't use Reserve on self, use SelfReserve instead");
			}
			else {
				if (busyWith == null) {
					busyWith = reserver;
					freeEvent = new FreeRelayEvent();
					freeEvent.time = sim.clock + sim.protocolParameters.t_busy;
					freeEvent.relay = this;
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

		public void FreeNow(Simulation sim) {
			if (busyWith != this) {
				sim.eventQueue.Remove(freeEvent);
			}
			Free();
		}

		public void Reset() {
			packetToSend = null;
			regionIndex = 0;
			COL_count = 0;
			SENSE_count = 0;
			ATTEMPT_count = 0;
			PKT_count = 0;
		}
	}
}
