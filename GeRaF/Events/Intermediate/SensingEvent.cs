using GeRaF.Events.Transmissions;
using GeRaF.Network;
using GeRaF.Utils;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GeRaF.Events.Intermediate
{
	class StartSensingEvent : Event
	{
		[JsonIgnore]
		public Relay relay;
		public int relayId => relay.id;
		public override void Handle(Simulation sim) {
			relay.status = RelayStatus.Sensing;

			relay.SENSE_count++;

			// enable sensing on relay
			relay.isSensing = true;
			if (relay.neighbours.Any(n => n.status == RelayStatus.Transmitting)) {
				relay.hasSensed = true;
			}

			// schedule sensing end
			var end = new EndSensingEvent();
			end.relay = relay;
			end.time = sim.clock + sim.protocolParameters.t_sense;
			sim.eventQueue.Add(end);
		}
	}

	class EndSensingEvent : Event
	{
		[JsonIgnore]
		public Relay relay;
		public int relayId => relay.id;
		public override void Handle(Simulation sim) {

			if (relay.hasSensed) {
				// check if sensingCount is less than max allowed
				if (relay.SENSE_count < sim.protocolParameters.n_max_sensing) {
					// reschedule with linear backoff
					relay.status = RelayStatus.Backoff_Sensing;
					var backOffSize = sim.protocolParameters.t_backoff;
					var start = new StartSensingEvent();
					start.time = sim.clock + backOffSize * RNG.rand();
					start.relay = relay;
					sim.eventQueue.Add(start);
				}
				else {
					// finish and free
					relay.packetToSend.Finish(Result.Abort_max_sensing, sim);
					relay.FreeNow(sim);
				}
			}
			else {
				// check if sink is in range
				if (relay.neighbours.Contains(relay.packetToSend.sink)) {
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

			// reset sensing vars
			relay.isSensing = false;
			relay.hasSensed = false;
		}
	}
}
