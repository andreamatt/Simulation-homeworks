using GeRaF.Events.Transmissions;
using GeRaF.Network;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GeRaF.Events.Intermediate
{
	class RegionProgressEvent : Event
	{
		[JsonIgnore]
		public Relay relay;
		public int relayId => relay.id;
		public override void Handle() {
			// if max region reached
			if (relay.REGION_index == sim.protocolParameters.n_regions - 1) {
				// reset region, increase region cycle
				relay.REGION_index = 0;
				relay.REGION_cycle++;

				if (relay.REGION_cycle < sim.protocolParameters.n_max_region_cycle) {
					// schedule RTS immediately
					sim.eventQueue.Add(new StartRTSEvent {
						time = sim.clock,
						relay = relay,
						sim = sim,
						previous = this
					});
				}
				else {
					relay.packetToSend.Finish(Result.Abort_max_region_cycle, sim);
					if (relay.packetToSend.hopsIds.Count > 1) {
						var previousRelayId = relay.packetToSend.hopsIds[relay.packetToSend.hopsIds.Count - 2];
						var previousRelay = sim.relayById[previousRelayId];
						relay.failuresFromNeighbour[previousRelay]++;
					}
					relay.FreeNow(this);
				}
			}
			// go next region
			else {
				relay.REGION_index++;

				sim.eventQueue.Add(new StartRTSEvent {
					time = sim.clock,
					relay = relay,
					sim = sim,
					previous = this
				});
			}
		}
	}
}
