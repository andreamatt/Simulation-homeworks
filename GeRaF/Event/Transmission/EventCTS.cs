using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GeRaF
{
	class StartCTSEvent : StartTransmissionEvent
	{
		public Relay requester;
		public override void Handle(Simulation sim) {
			triggerNeighbourSensing();
			var transmissions = sendTransmissions(TransmissionType.CTS);

			// schedule CTS_end
			var end = new EndCTSEvent();
			end.relay = relay;
			end.requester = requester;
			end.transmissions = transmissions;
			end.time = sim.clock + sim.protocolParameters.t_signal;
			sim.eventQueue.Add(end);
		}
	}

	class EndCTSEvent : EndTransmissionEvent
	{
		public Relay requester;
		public override void Handle(Simulation sim) {
			triggerNeighbourSensing();
			foreach (var t in transmissions) {
				var n = t.destination;
				// remove transmission
				n.activeTransmissions.Remove(t);
				relay.activeTransmissions.Remove(t);
				if (n == requester) {
					// move cts to finished
					n.finishedTransmissions.Add(t);
				}
			}
		}
	}
}
