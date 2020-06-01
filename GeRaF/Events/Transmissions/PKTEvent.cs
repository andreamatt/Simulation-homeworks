using GeRaF.Events.Check;
using GeRaF.Network;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GeRaF.Events.Transmissions
{
	class StartPKTEvent : StartTransmissionEvent
	{
		[JsonIgnore]
		public Relay chosenRelay;
		public int chosenRelayId => chosenRelay.id;

		public override void Handle(Simulation sim) {
			relay.status = RelayStatus.Transmitting;

			relay.PKT_count++;

			var transmissions = sendTransmissions(TransmissionType.PKT, chosenRelay);
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
		[JsonIgnore]
		public Relay chosenRelay;
		public int chosenRelayId => chosenRelay.id;

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

			// schedule ACK_check
			var ACK_check = new CheckACKEvent();
			ACK_check.time = sim.clock + sim.protocolParameters.t_signal + sim.protocolParameters.t_delta;
			ACK_check.relay = relay;
			ACK_check.chosenRelay = chosenRelay;
			sim.eventQueue.Add(ACK_check);
		}
	}
}
