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

		public override void Handle(Simulation sim) {
			// no CTS or CTS failed => back to sensing
			if (relay.finishedTransmissions.Count == 0 || relay.finishedTransmissions.First().failed) {
				if (relay.SINK_RTS_count < sim.protocolParameters.n_max_sink_rts) {
					var sink_rts_start = new StartSINKRTSEvent();
					sink_rts_start.time = sim.clock + RNG.rand() * sim.protocolParameters.t_backoff;
					sink_rts_start.relay = relay;
					sim.eventQueue.Add(sink_rts_start);
				}
				else {
					relay.packetToSend.Finish(Result.Abort_max_sink_rts, sim);
					relay.FreeNow(sim);
				}
			}
			// only sink replied successfully
			else {
				// send packet to sink
				var chosen = relay.packetToSend.sink;
				var PKT_start = new StartPKTEvent();
				PKT_start.time = sim.clock;
				PKT_start.relay = relay;
				PKT_start.chosenRelay = chosen;
				sim.eventQueue.Add(PKT_start);
			}

			// clear finished
			relay.finishedTransmissions.Clear();
		}
	}
}
