using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GeRaF
{
	class CheckACKEvent : Event
	{
		public Relay relay;
		public Relay chosenRelay;
		public override void Handle(Simulation sim) {
			if (relay.finishedTransmissions.Count == 1) {
				relay.FreeNow(sim);
			}
			else {
				// check ack count and resend
				if (relay.ACK_count < sim.protocolParameters.n_max_ack) {
					// schedule PKT_start again
					var PKT_start = new StartPKTEvent();
					PKT_start.time = sim.clock;
					PKT_start.relay = relay;
					PKT_start.chosenRelay = chosenRelay;
					sim.eventQueue.Add(PKT_start);
				}
				else {
					relay.ACK_count = 0;
					if (relay.COL_count < sim.protocolParameters.n_max_coll) {
						var COL_start = new StartCOLEvent();
						COL_start.time = sim.clock;
						COL_start.relay = relay;
						sim.eventQueue.Add(COL_start);
						relay.COL_count++;
					}
					else {
						// if sink in range, go back to sensing
						if (sim.distances[relay.id][relay.packetToSend.sink.id] < relay.range) {
							var sense = new StartSensingEvent();
							sense.time = sim.clock;
							sense.relay = relay;
							sim.eventQueue.Add(sense);
						}
						else {
							// try going to next region
							var regionChange = new RegionProgressEvent();
							regionChange.time = sim.clock;
							regionChange.relay = relay;
							sim.eventQueue.Add(regionChange);
						}
					}
				}
			}
			relay.finishedTransmissions.Clear();
		}
	}
}
