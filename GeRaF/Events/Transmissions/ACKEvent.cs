using GeRaF.Events.Intermediate;
using GeRaF.Network;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GeRaF.Events.Transmissions
{
	class StartACKEvent : TransmissionEvent
	{
		public override void Handle() {
			relay.status = RelayStatus.Transmitting;
			relay.transmissionType = TransmissionType.ACK;

			StartTransmission();

			sim.eventQueue.Add(new EndACKEvent() {
				time = sim.clock + sim.protocolParameters.t_signal,
				relay = relay,
				actualDestination = actualDestination,
				sim = sim
			});
		}
	}

	class EndACKEvent : TransmissionEvent
	{
		public override void Handle() {

			EndTransmission();

			// copy packet from sender (actual destination of ACK)
			var packet = Packet.copy(actualDestination.packetToSend);

			// free relay
			relay.FreeNow();

			relay.packetToSend = packet;

			// if reached sink, stop
			if (relay.packetToSend.sink == relay) {
				relay.packetToSend.Finish(Result.Success, sim);
			}
			else {
				// become sender
				relay.SelfReserve();
				sim.eventQueue.Add(new StartSensingEvent {
					time = sim.clock,
					relay = relay,
					sim = sim
				});
			}
		}
	}
}
