using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GeRaF
{
	class CheckSINKCOLEvent : Event
	{
		public Relay relay;
		public override void Handle(Simulation sim) {
			// no CTS or CTS failed => back to sensing
			if (relay.finishedTransmissions.Count == 0 || relay.finishedTransmissions.First().failed) {
				var sense = new StartSensingEvent();
				sense.time = sim.clock;
				sense.relay = relay;
				sim.eventQueue.Add(sense);
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
