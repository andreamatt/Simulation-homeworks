﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GeRaF
{
	class StartRTSEvent : StartTransmissionEvent
	{
		public override void Handle(Simulation sim) {
			triggerNeighbourSensing();
			var transmissions = sendTransmissions(TransmissionType.RTS);

			// schedule RTS_end
			var end = new EndRTSEvent();
			end.relay = relay;
			end.transmissions = transmissions;
			end.time = sim.clock + sim.protocolParameters.t_signal;
			sim.eventQueue.Add(end);
		}
	}

	class EndRTSEvent : EndTransmissionEvent
	{
		public override void Handle(Simulation sim) {
			triggerNeighbourSensing();

			var sink = relay.packetToSend.sink;
			var sourceToSink = sim.distances[relay.id][sink.id];
			var limitToSink = sourceToSink - relay.range;
			var regionWidth = relay.range / sim.protocolParameters.n_regions;
			var minDistance = limitToSink + regionWidth * relay.regionIndex;
			var maxDistance = minDistance + regionWidth;

			foreach (var t in transmissions) {
				var n = t.destination;
				if (n.awake) {
					// if neigh relay is in correct region AND transmission has not failed AND is not in another contention, schedule CTS
					var dist = sim.distances[n.id][sink.id];
					if (dist > minDistance && dist < maxDistance && t.failed == false && n.BusyWith == null) {
						n.Reserve(relay, sim);
						var CTS_start = new StartCTSEvent();
						CTS_start.relay = n;
						CTS_start.requester = relay;
						CTS_start.time = sim.clock;
						sim.eventQueue.Add(CTS_start);
					}
				}
				// delete transmission
				n.activeTransmissions.Remove(t);
				relay.activeTransmissions.Remove(t);
			}

			// schedule COL_check
			var COL_check = new CheckCOLEvent();
			COL_check.relay = relay;
			// time + CTS_time + time delta to make sure CTS events come before COL check
			COL_check.time = sim.clock + sim.protocolParameters.t_signal + sim.protocolParameters.t_delta;
			sim.eventQueue.Add(COL_check);
		}
	}
}