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
				sim = sim,
				previous = this
			});
		}
	}

	class EndACKEvent : TransmissionEvent
	{
		public override void Handle() {

			EndTransmission();

			// copy packet from sender (actual destination of ACK)
			var packet = Packet.copy(actualDestination.packetToSend, sim.packetGenerator);
			if (sim.simulationParameters.debugType != DebugType.Never) {
				sim.debugWriter.WriteLine($"P;{packet}");
			}
			packet.hopsIds.Add(relay.id);

			relay.packetToSend = packet;

			// if reached sink, stop
			if (relay.packetToSend.sink == relay) {
				relay.packetToSend.Finish(Result.Success, sim);
				relay.FreeNow(this);
			}
			else {
				// become sender
				relay.FreeNow(this);
				relay.packetToSend = packet;
				relay.SelfReserve();
				sim.eventQueue.Add(new StartSensingEvent {
					time = sim.clock,
					relay = relay,
					sim = sim,
					previous = this
				});
			}
		}
	}
}
