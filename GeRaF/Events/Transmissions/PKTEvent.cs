using GeRaF.Events.Check;
using GeRaF.Network;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GeRaF.Events.Transmissions
{
	class StartPKTEvent : TransmissionEvent
	{
		public override void Handle() {
			relay.status = RelayStatus.Transmitting;
			relay.transmissionType = TransmissionType.PKT;

			relay.PKT_count++;

			StartTransmission();

			// schedule PKT_end
			sim.eventQueue.Add(new EndPKTEvent() {
				time = sim.clock + sim.protocolParameters.t_data,
				relay = relay,
				actualDestination = actualDestination,
				sim = sim,
				previous = this
			});
		}
	}

	class EndPKTEvent : TransmissionEvent
	{
		public override void Handle() {
			relay.status = RelayStatus.Awaiting_Signal; // waits for ACK

			EndTransmission();

			// schedule ACK_check
			sim.eventQueue.Add(new CheckACKEvent() {
				time = sim.clock + sim.protocolParameters.t_signal + sim.protocolParameters.t_delta,
				relay = relay,
				chosenRelay = actualDestination,
				sim = sim,
				previous = this
			});
		}
	}
}
