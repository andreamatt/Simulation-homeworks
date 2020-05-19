using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GeRaF
{
	class CheckCOLEvent : Event
	{
		[JsonIgnore]
		public Relay relay;
		public int relayId => relay.id;

		public override void Handle(Simulation sim) {
			// check CTS amount
			if (relay.finishedTransmissions.Count == 0) {
				var regionChange = new RegionProgressEvent();
				regionChange.time = sim.clock;
				regionChange.relay = relay;
				sim.eventQueue.Add(regionChange);
			}
			else {
				// if any have failed send COL or go to next region
				if (relay.finishedTransmissions.Any(t => t.failed)) {
					if (relay.COL_count < sim.protocolParameters.n_max_coll) {
						var COL_start = new StartCOLEvent();
						COL_start.time = sim.clock;
						COL_start.relay = relay;
						sim.eventQueue.Add(COL_start);
						relay.COL_count++;
					}
					else {
						var regionChange = new RegionProgressEvent();
						regionChange.time = sim.clock;
						regionChange.relay = relay;
						sim.eventQueue.Add(regionChange);
					}
				}
				// else choose relay and send packet
				else {
					// choose random relay (choose first one, they are already in random order because of backoff (multiple CTSs without coll implies backoff))
					var chosen = relay.finishedTransmissions.First().source;
					var PKT_start = new StartPKTEvent();
					PKT_start.time = sim.clock;
					PKT_start.relay = relay;
					PKT_start.chosenRelay = chosen;
					sim.eventQueue.Add(PKT_start);
				}
			}

			// clear finished
			relay.finishedTransmissions.Clear();
		}
	}
}
