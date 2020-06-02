using GeRaF.Events.Check;
using GeRaF.Network;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GeRaF.Events.Transmissions
{
	class StartRTSEvent : TransmissionEvent
	{
		public override void Handle() {
			relay.status = RelayStatus.Transmitting;
			relay.transmissionType = TransmissionType.RTS;

			StartTransmission();

			sim.eventQueue.Add(new EndRTSEvent() {
				relay = relay,
				time = sim.clock + sim.protocolParameters.t_signal,
				sim = sim,
				previous = this
			});
		}
	}

	class EndRTSEvent : TransmissionEvent
	{
		public override void Handle() {
			relay.status = RelayStatus.Awaiting_Signal; // waits for CTS

			EndTransmission();

			// time + CTS_time + time delta to make sure CTS events come before COL check
			sim.eventQueue.Add(new CheckCOLEvent() {
				relay = relay,
				time = sim.clock + sim.protocolParameters.t_signal + sim.protocolParameters.t_delta,
				sim = sim,
				previous = this
			});
		}
	}
}
