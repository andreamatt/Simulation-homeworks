using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GeRaF
{
	class StartSINKRTSEvent : StartTransmissionEvent
	{
		public override void Handle(Simulation sim) {
			relay.status = RelayStatus.Transmitting;

			var transmissions = sendTransmissions(TransmissionType.SINK_RTS);

			// schedule SINK_RTS_end
			var end = new EndSINKRTSEvent();
			end.relay = relay;
			end.transmissions = transmissions;
			end.time = sim.clock + sim.protocolParameters.t_signal;
			sim.eventQueue.Add(end);
		}
	}

	class EndSINKRTSEvent : EndTransmissionEvent
	{
		public override void Handle(Simulation sim) {
			relay.status = RelayStatus.Awaiting_Signal; // waits for CTS

			var sink = relay.packetToSend.sink;
			foreach (var t in transmissions) {
				var n = t.destination;
				if (n.status == RelayStatus.Free && n == sink) {
					n.Reserve(relay, sim);
					var CTS_start = new StartCTSEvent();
					CTS_start.relay = n;
					CTS_start.requesterRelay = relay;
					CTS_start.time = sim.clock;
					sim.eventQueue.Add(CTS_start);
				}
				// delete transmission
				n.activeTransmissions.Remove(t);
				relay.activeTransmissions.Remove(t);
			}

			// schedule COL_check
			var COL_check = new CheckSINKCOLEvent();
			COL_check.relay = relay;
			// time + CTS_time + time delta to make sure CTS events come before COL check
			COL_check.time = sim.clock + sim.protocolParameters.t_signal + sim.protocolParameters.t_delta;
			sim.eventQueue.Add(COL_check);
		}
	}
}
