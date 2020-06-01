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
		public override void Handle(Simulation sim) {
			// if max region reached
			if (relay.REGION_index == sim.protocolParameters.n_regions - 1) {
				// reset region, increase region cycle
				relay.REGION_index = 0;
				relay.REGION_cycle++;

				if (relay.REGION_cycle < sim.protocolParameters.n_max_region_cycle) {
					// schedule RTS immediately
					var RTS_start = new StartRTSEvent();
					RTS_start.time = sim.clock;
					RTS_start.relay = relay;
					sim.eventQueue.Add(RTS_start);
				}
				else {
					relay.packetToSend.Finish(Result.Abort_max_region_cycle, sim);
					relay.FreeNow(sim);
				}
			}
			// go next region
			else {
				relay.REGION_index++;
				// schedule RTS
				var RTS_start = new StartRTSEvent();
				RTS_start.time = sim.clock;
				RTS_start.relay = relay;
				sim.eventQueue.Add(RTS_start);
			}
		}
	}
}
