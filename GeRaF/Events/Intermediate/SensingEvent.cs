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
		public override void Handle() {
			relay.status = RelayStatus.Sensing;

			relay.SENSE_count++;

			// enable sensing on relay
			relay.isSensing = true;
			if (relay.neighbours.Any(n => n.status == RelayStatus.Transmitting)) {
				relay.hasSensed = true;
			}

			// schedule sensing end
			sim.eventQueue.Add(new EndSensingEvent {
				relay = relay,
				time = sim.clock + sim.protocolParameters.t_sense,
				sim = sim
			});
		}
	}

	class EndSensingEvent : Event
	{
		[JsonIgnore]
		public Relay relay;
		public int relayId => relay.id;
		public override void Handle() {
			if (relay.hasSensed) {
				// check if sensingCount is less than max allowed
				if (relay.SENSE_count < sim.protocolParameters.n_max_sensing) {
					relay.status = RelayStatus.Backoff_Sensing;
					sim.eventQueue.Add(new StartSensingEvent {
						time = sim.clock + sim.protocolParameters.t_backoff * RNG.rand(),
						relay = relay,
						sim = sim
					});
				}
				else {
					// finish and free
					relay.packetToSend.Finish(Result.Abort_max_sensing, sim);
					relay.FreeNow();
				}
			}
			else {
				// check if sink is in range
				if (relay.neighbours.Contains(relay.packetToSend.sink)) {
					sim.eventQueue.Add(new StartSINKRTSEvent {
						time = sim.clock,
						relay = relay,
						actualDestination = relay.packetToSend.sink,
						sim = sim
					});
				}
				else {
					sim.eventQueue.Add(new StartRTSEvent {
						time = sim.clock,
						relay = relay,
						sim = sim
					});
				}
			}

			// reset sensing vars
			relay.isSensing = false;
			relay.hasSensed = false;
		}
	}
}
