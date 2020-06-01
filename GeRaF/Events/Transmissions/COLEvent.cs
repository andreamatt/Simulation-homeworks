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
	class StartCOLEvent : TransmissionEvent
	{
		public override void Handle() {
			relay.status = RelayStatus.Transmitting;
			relay.transmissionType = TransmissionType.COL;

			StartTransmission();

			sim.eventQueue.Add(new EndCOLEvent {
				relay = relay,
				time = sim.clock + sim.protocolParameters.t_signal,
				sim = sim
			});
		}
	}

	class EndCOLEvent : TransmissionEvent
	{
		public override void Handle() {
			relay.status = RelayStatus.Awaiting_Signal; // waits for CTS

			EndTransmission();

			var backoffSize = sim.protocolParameters.t_backoff * Math.Pow(2, relay.COL_count);
			sim.eventQueue.Add(new CheckCOLEvent {
				time = sim.clock + backoffSize + sim.protocolParameters.t_signal + sim.protocolParameters.t_delta,
				relay = relay,
				sim = sim
			});
		}
	}
}
