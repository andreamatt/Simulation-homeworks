using GeRaF.Events.Check;
using GeRaF.Network;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GeRaF.Events.Transmissions
{
	class StartSINKRTSEvent : TransmissionEvent
	{
		public override void Handle() {
			relay.status = RelayStatus.Transmitting;
			relay.transmissionType = TransmissionType.SINK_RTS;

			relay.SINK_RTS_count++;

			StartTransmission();

			sim.eventQueue.Add(new EndSINKRTSEvent {
				time = sim.clock + sim.protocolParameters.t_signal,
				relay = relay,
				actualDestination = actualDestination,
				sim = sim,
				previous = this
			});
		}
	}

	class EndSINKRTSEvent : TransmissionEvent
	{
		public override void Handle() {
			relay.status = RelayStatus.Awaiting_Signal; // waits for CTS

			EndTransmission();

			sim.eventQueue.Add(new CheckSINKCOLEvent {
				relay = relay,
				// time + CTS_time + time delta to make sure CTS events come before COL check
				time = sim.clock + sim.protocolParameters.t_signal + sim.protocolParameters.t_delta,
				sim = sim,
				previous = this
			});
		}
	}
}
