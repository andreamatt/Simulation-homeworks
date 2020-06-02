using GeRaF.Network;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GeRaF.Events.Transmissions
{
	class StartCTSEvent : TransmissionEvent
	{
		public override void Handle() {
			relay.status = RelayStatus.Transmitting;
			relay.transmissionType = TransmissionType.CTS;

			StartTransmission();

			// schedule CTS_end
			sim.eventQueue.Add(new EndCTSEvent() {
				relay = relay,
				actualDestination = actualDestination,
				time = sim.clock + sim.protocolParameters.t_signal,
				sim = sim,
				previous = this
			});
		}
	}

	class EndCTSEvent : TransmissionEvent
	{
		public override void Handle() {
			relay.status = RelayStatus.Awaiting_Signal;

			EndTransmission();
		}
	}
}
