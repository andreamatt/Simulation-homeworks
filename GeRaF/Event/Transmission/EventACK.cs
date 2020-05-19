using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GeRaF
{
	class StartACKEvent : StartTransmissionEvent
	{
		[JsonIgnore]
		public Relay senderRelay;
		public int senderRelayId => senderRelay.id;
		public override void Handle(Simulation sim) {
			relay.status = RelayStatus.Transmitting;

			var transmissions = sendTransmissions(TransmissionType.ACK);
			// schedule ACK_end
			var ACK_end = new EndACKEvent();
			ACK_end.time = sim.clock + sim.protocolParameters.t_signal;
			ACK_end.relay = relay;
			ACK_end.senderRelay = senderRelay;
			ACK_end.transmissions = transmissions;
			sim.eventQueue.Add(ACK_end);
		}
	}

	class EndACKEvent : EndTransmissionEvent
	{
		[JsonIgnore]
		public Relay senderRelay;
		public int senderRelayId => senderRelay.id;
		public override void Handle(Simulation sim) {
			foreach (var t in transmissions) {
				var n = t.destination;
				n.activeTransmissions.Remove(t);
				relay.activeTransmissions.Remove(t);
				if (n == senderRelay && t.failed == false) {
					n.finishedTransmissions.Add(t);
				}
			}

			// free relay
			relay.FreeNow(sim);

			// copy packet
			relay.packetToSend = Packet.copy(senderRelay.packetToSend);

			// if reached sink, stop
			if (relay.packetToSend.sink == relay) {
				relay.packetToSend.Finish(Result.Success, sim);
			}
			else {
				relay.SelfReserve();
				// schedule SENSE_Start
				var SENSE_start = new StartSensingEvent();
				SENSE_start.time = sim.clock;
				SENSE_start.relay = relay;
				sim.eventQueue.Add(SENSE_start);
			}
		}
	}
}
