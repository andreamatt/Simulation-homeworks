using GeRaF.Events.Intermediate;
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
	class CheckCOLEvent : Event
	{
		[JsonIgnore]
		public Relay relay;
		public int relayId => relay.id;

		public override void Handle() {
			// check CTS amount
			if (relay.finishedCTSs.Count == 0) {
				sim.eventQueue.Add(new RegionProgressEvent() {
					time = sim.clock,
					relay = relay,
					sim = sim,
					previous = this
				});
			}
			else {
				// if any have failed send COL or go to next region
				if (relay.finishedCTSs.Any(t => t.failed)) {
					if (relay.COL_count < sim.protocolParameters.n_max_coll) {
						relay.COL_count++;
						sim.eventQueue.Add(new StartCOLEvent() {
							time = sim.clock,
							relay = relay,
							sim = sim,
							previous = this
						});
					}
					else {
						sim.eventQueue.Add(new RegionProgressEvent() {
							time = sim.clock,
							relay = relay,
							sim = sim,
							previous = this
						});
					}
				}
				// else choose relay and send packet
				else {
					// choose random relay (choose first one, they are already in random order because of backoff (multiple CTSs without coll implies backoff))
					var chosen = relay.finishedCTSs.First().source;
					sim.eventQueue.Add(new StartPKTEvent() {
						time = sim.clock,
						relay = relay,
						actualDestination = chosen,
						sim = sim,
						previous = this
					});
				}
			}

			// clear finished
			relay.finishedCTSs.Clear();
		}
	}
}
