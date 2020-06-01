using GeRaF.Events.Check;
using GeRaF.Network;
using GeRaF.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GeRaF.Events.Transmissions
{
	class StartCOLEvent : StartTransmissionEvent
	{
		public override void Handle(Simulation sim) {
			relay.status = RelayStatus.Transmitting;

			var transmissions = sendTransmissions(TransmissionType.COL);

			// schedule COL_end
			var end = new EndCOLEvent();
			end.relay = relay;
			end.transmissions = transmissions;
			end.time = sim.clock + sim.protocolParameters.t_signal;
			sim.eventQueue.Add(end);
		}
	}

	class EndCOLEvent : EndTransmissionEvent
	{
		public override void Handle(Simulation sim) {
			relay.status = RelayStatus.Awaiting_Signal; // waits for CTS

			var backoffSize = sim.protocolParameters.t_backoff * Math.Pow(2, relay.COL_count);
			foreach (var t in transmissions) {
				var n = t.destination;
				// remove transmission
				n.activeTransmissions.Remove(t);
				relay.activeTransmissions.Remove(t);
				if (t.failed == false && n.BusyWith == relay) {
					relay.status = RelayStatus.Backoff_CTS;
					// update busy
					n.Reserve(relay, sim);
					// schedule CTS
					var CTS_start = new StartCTSEvent();
					CTS_start.time = sim.clock + backoffSize * RNG.rand();
					CTS_start.relay = n;
					CTS_start.requesterRelay = relay;
					sim.eventQueue.Add(CTS_start);
				}
			}

			// schedule COL_check
			if (relay.neighbours.Contains(relay.packetToSend.sink)) {
				var sink_COL_check = new CheckSINKCOLEvent();
				sink_COL_check.relay = relay;
				sink_COL_check.time = sim.clock + sim.protocolParameters.t_signal + sim.protocolParameters.t_delta;
				sim.eventQueue.Add(sink_COL_check);
			}
			else {
				var COL_check = new CheckCOLEvent();
				COL_check.time = sim.clock + backoffSize + sim.protocolParameters.t_signal + sim.protocolParameters.t_delta;
				COL_check.relay = relay;
				sim.eventQueue.Add(COL_check);
			}
		}
	}
}
