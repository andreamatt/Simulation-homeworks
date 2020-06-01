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
		public override void Handle(Simulation sim) {
			if (relay.finishedTransmissions.Count == 1) {
				relay.FreeNow(sim);
			}
			else {
				// check ack count and resend
				if (relay.PKT_count < sim.protocolParameters.n_max_pkt) {
					// schedule PKT_start again
					var PKT_start = new StartPKTEvent();
					PKT_start.time = sim.clock;
					PKT_start.relay = relay;
					PKT_start.chosenRelay = chosenRelay;
					sim.eventQueue.Add(PKT_start);
				}
				else {
					relay.packetToSend.Finish(Result.Abort_no_ack, sim);
					relay.FreeNow(sim);
				}
			}
			relay.finishedTransmissions.Clear();
		}
	}
}
