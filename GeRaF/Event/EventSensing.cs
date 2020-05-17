using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GeRaF
{
	class StartSensingEvent : Event
	{
		public Relay relay;
		public override void Handle(Simulation sim) {
			// check if sensingCount is less than max allowed
			if (relay.SENSE_count < sim.protocolParameters.n_max_sensing) {
				relay.SENSE_count++;
				// enable sensing on relay
				relay.isSensing = true;
				//relay.hasSensed = false;

				// schedule sensing end
				var end = new EndSensingEvent();
				end.relay = relay;
				end.time = sim.clock + sim.protocolParameters.t_sense;
				sim.eventQueue.Add(end);
			}
			else {
				// finish and free
				relay.packetToSend.Finish(Result.Abort_channel_busy, sim);
				relay.FreeNow(sim);
			}
		}
	}

	class EndSensingEvent : Event
	{
		public Relay relay;
		public override void Handle(Simulation sim) {
			if (relay.hasSensed) {
				// reschedule with linear backoff
				var backOffSize = sim.protocolParameters.t_backoff;
				var start = new StartSensingEvent();
				start.time = sim.clock + backOffSize * RNG.rand();
				start.relay = relay;
				sim.eventQueue.Add(start);
			}
			else {
				// set sensing count to 0
				relay.SENSE_count = 0;

				// check if sink is in range
				if (sim.distances[relay.id][relay.packetToSend.sink.id] < relay.range) {
					var SINK_RTS_start = new StartSINKRTSEvent();
					SINK_RTS_start.time = sim.clock;
					SINK_RTS_start.relay = relay;
					sim.eventQueue.Add(SINK_RTS_start);
				}
				else {
					// schedule RTS_start
					var RTS_start = new StartRTSEvent();
					RTS_start.time = sim.clock;
					RTS_start.relay = relay;
					sim.eventQueue.Add(RTS_start);
				}
			}
			relay.isSensing = false;
			relay.hasSensed = false;
		}
	}
}
