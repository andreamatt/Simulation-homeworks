using GeRaF.Events.Transmissions;
using GeRaF.Network;
using GeRaF.Utils;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GeRaF.Events.Check
{
	class CheckSINKCOLEvent : Event
	{
		[JsonIgnore]
		public Relay relay;
		public int relayId => relay.id;

		public override void Handle() {
			// no CTS or CTS failed => back to sensing
			if (relay.finishedCTSs.Count == 0 || relay.finishedCTSs.First().failed) {
				if (relay.SINK_RTS_count < sim.protocolParameters.n_max_sink_rts) {
					relay.status = RelayStatus.Backoff_SinkRTS;
					sim.eventQueue.Add(new StartSINKRTSEvent {
						time = sim.clock + RNG.rand() * sim.protocolParameters.t_backoff,
						relay = relay,
						actualDestination = relay.packetToSend.sink,
						sim = sim,
						previous = this
					});
				}
				else {
					relay.packetToSend.Finish(Result.Abort_max_sink_rts, sim);
					relay.OnPacketFinished();
					relay.FreeNow(this);
				}
			}
			// sink replied successfully
			else {
				// send packet to sink
				var chosen = relay.packetToSend.sink;
				sim.eventQueue.Add(new StartPKTEvent {
					time = sim.clock,
					relay = relay,
					actualDestination = chosen,
					sim = sim,
					previous = this
				});
			}

			// clear finished
			relay.finishedCTSs.Clear();
		}
	}
}
