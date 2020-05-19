using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GeRaF
{
	class StartPKTEvent : StartTransmissionEvent
	{
		public Relay chosenRelay;

		public override void Handle(Simulation sim) {
			relay.status = RelayStatus.Transmitting;

			var transmissions = sendTransmissions(TransmissionType.PKT);
			// put other interested relays (not chosen) to sleep
			// schedule PKT_end
			var PKT_end = new EndPKTEvent();
			PKT_end.time = sim.clock + sim.protocolParameters.t_data;
			PKT_end.relay = relay;
			PKT_end.chosenRelay = chosenRelay;
			PKT_end.transmissions = transmissions;
			sim.eventQueue.Add(PKT_end);
		}
	}

	class EndPKTEvent : EndTransmissionEvent
	{
		public Relay chosenRelay;

		public override void Handle(Simulation sim) {
			relay.status = RelayStatus.Awaiting_Signal; // waits for ACK

			foreach (var t in transmissions) {
				var n = t.destination;
				n.activeTransmissions.Remove(t);
				relay.activeTransmissions.Remove(t);
				if (t.failed == false && n.BusyWith == relay) {
					// if chosen one, schedule ACK
					if (n == chosenRelay) {
						// update busy
						n.Reserve(relay, sim);
						var ACK_start = new StartACKEvent();
						ACK_start.time = sim.clock;
						ACK_start.relay = n;
						ACK_start.senderRelay = relay;
						sim.eventQueue.Add(ACK_start);
					}
					else {
						n.FreeNow(sim);
					}
				}
			}

			// increase PKT attempts
			relay.ACK_count++;

			// schedule ACK_check
			var ACK_check = new CheckACKEvent();
			ACK_check.time = sim.clock + sim.protocolParameters.t_signal + sim.protocolParameters.t_delta;
			ACK_check.relay = relay;
			ACK_check.chosenRelay = chosenRelay;
			sim.eventQueue.Add(ACK_check);
		}
	}
}
