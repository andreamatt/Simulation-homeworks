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
	class StartSinkCOLEvent : TransmissionEvent
	{
		public override void Handle() {
			relay.status = RelayStatus.Transmitting;
			relay.transmissionType = TransmissionType.SINK_COL;

			StartTransmission();

			sim.eventQueue.Add(new EndCOLEvent {
				relay = relay,
				time = sim.clock + sim.protocolParameters.t_signal,
				sim = sim,
				previous = this
			});
		}
	}

	class EndSinkCOLEvent : TransmissionEvent
	{
		public override void Handle() {
			relay.status = RelayStatus.Awaiting_Signal; // waits for CTS

			EndTransmission();

			sim.eventQueue.Add(new CheckSINKCOLEvent {
				relay = relay,
				time = sim.clock + sim.protocolParameters.t_signal + sim.protocolParameters.t_delta,
				sim = sim,
				previous = this
			});

		}
	}
}
