using GeRaF.Events.Transmissions;
using GeRaF.Network;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GeRaF.Events.Check
{
	class CheckACKEvent : Event
	{
		[JsonIgnore]
		public Relay relay;
		public int relayId => relay.id;

		[JsonIgnore]
		public Relay chosenRelay;
		public int chosenRelayId => chosenRelay.id;

		public override void Handle() {
			if (relay.receivedACK) {
				relay.packetToSend.Finish(Result.Passed, sim);
				relay.FreeNow(this);
			}
			else {
				// check ack count and resend OR abort
				if (relay.PKT_count < sim.protocolParameters.n_max_pkt) {
					// schedule PKT_start again
					sim.eventQueue.Add(new StartPKTEvent {
						time = sim.clock,
						relay = relay,
						actualDestination = chosenRelay,
						sim = sim,
						previous = this
					});
				}
				else {
					relay.packetToSend.Finish(Result.Abort_no_ack, sim);
					relay.FreeNow(this);
				}
			}
			relay.receivedACK = false;
		}
	}
}
