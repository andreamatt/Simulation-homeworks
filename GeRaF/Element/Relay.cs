using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

namespace GeRaF
{
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
		public List<Relay> neighbours = new List<Relay>();
		public List<int> neighboursIds => neighbours.Select(n => n.id).ToList();
		public RelayStatus status = RelayStatus.Free;
		//public bool IsAwake => status != RelayStatus.Asleep;
		public Packet packetToSend = null;

		[JsonIgnore]
		public HashSet<Transmission> activeTransmissions = new HashSet<Transmission>();
		public List<Transmission> activeTransmissionsList => activeTransmissions.ToList();
		[JsonIgnore]
		public HashSet<Transmission> finishedTransmissions = new HashSet<Transmission>();
		public List<Transmission> finishedTransmissionsList => finishedTransmissions.ToList();

		// iteration status
		public int regionIndex = 0;
		public int COL_count = 0;   // number of collisions in that region during this attempt
		public int SENSE_count = 0;
		public int ATTEMPT_count = 0;
		public int ACK_count = 0;

		// sensing status
		public bool isSensing = false;
		public bool hasSensed = false;

		// contention status
		private Relay busyWith = null; // è in contesa per un pacchetto

		[JsonIgnore]
		public Relay BusyWith => busyWith;
		public int BusyWithId => busyWith == null ? -1 : busyWith.id;
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
		}

		public void FreeNow(Simulation sim) {
			if (busyWith != this) {
				sim.eventQueue.Remove(freeEvent);
			}
			Free();
		}
	}
}
