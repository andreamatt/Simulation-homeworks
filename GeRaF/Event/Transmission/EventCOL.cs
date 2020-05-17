﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GeRaF
{
	class StartCOLEvent : StartTransmissionEvent
	{
		public override void Handle(Simulation sim) {
			triggerNeighbourSensing();
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
			triggerNeighbourSensing();
			var backoffSize = sim.protocolParameters.t_backoff * Math.Pow(2, relay.COL_count);
			foreach (var t in transmissions) {
				var n = t.destination;
				// remove transmission
				n.activeTransmissions.Remove(t);
				relay.activeTransmissions.Remove(t);
				if (t.failed == false && n.BusyWith == relay) {
					// update busy
					n.Reserve(relay, sim);
					// schedule CTS
					var CTS_start = new StartCTSEvent();
					CTS_start.time = sim.clock + backoffSize * RNG.rand();
					CTS_start.relay = n;
					CTS_start.requester = relay;
					sim.eventQueue.Add(CTS_start);
				}
			}

			// schedule COL_check
			var COL_check = new CheckCOLEvent();
			COL_check.time = sim.clock + backoffSize + sim.protocolParameters.t_signal + sim.protocolParameters.t_delta;
			COL_check.relay = relay;
			sim.eventQueue.Add(COL_check);
		}
	}
}