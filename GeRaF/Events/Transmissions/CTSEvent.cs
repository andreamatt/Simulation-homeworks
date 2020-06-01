using GeRaF.Network;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GeRaF.Events.Transmissions
{
	class StartCTSEvent : StartTransmissionEvent
	{
		[JsonIgnore]
		public Relay requesterRelay;
		public int requesterRelayId => requesterRelay.id;
		public override void Handle(Simulation sim) {
			relay.status = RelayStatus.Transmitting;

			var transmissions = sendTransmissions(TransmissionType.CTS, requesterRelay);

			// schedule CTS_end
			var end = new EndCTSEvent();
			end.relay = relay;
			end.requesterRelay = requesterRelay;
			end.transmissions = transmissions;
			end.time = sim.clock + sim.protocolParameters.t_signal;
			sim.eventQueue.Add(end);
		}
	}

	class EndCTSEvent : EndTransmissionEvent
	{
		[JsonIgnore]
		public Relay requesterRelay;
		public int requesterRelayId => requesterRelay.id;
		public override void Handle(Simulation sim) {
			relay.status = RelayStatus.Awaiting_Signal;

			foreach (var t in transmissions) {
				var n = t.destination;
				// remove transmission
				n.activeTransmissions.Remove(t);
				relay.activeTransmissions.Remove(t);
				if (n == requesterRelay) {
					// move cts to finished
					n.finishedTransmissions.Add(t);
				}
			}
		}
	}
}
