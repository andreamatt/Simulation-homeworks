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

			// calculate next hop direction
			var version = sim.protocolParameters.protocolVersion;
			var sink = relay.packetToSend.sink;
			if (version == ProtocolVersion.BFS || version == ProtocolVersion.BFS_half) {
				// Calculate regions based on next hop direction instead of sink direction
				var direction = relay.directionForSink[sink];
				relay.packetToSend.current_aim.X = direction.X;
				relay.packetToSend.current_aim.Y = direction.Y;
			}
			else if (version == ProtocolVersion.Rx && relay.REGION_cycle > sim.protocolParameters.n_max_region_cycle / 2) {
				// aim 90 degrees to the right, only after half the region_cycles have failed
				relay.packetToSend.current_aim.X = relay.position.X + (sink.position.Y - relay.position.Y);
				relay.packetToSend.current_aim.Y = relay.position.Y - (sink.position.X - relay.position.X);
			}
			else if (version == ProtocolVersion.Rx_plus) {
				if (relay.REGION_cycle > sim.protocolParameters.n_max_region_cycle / 2) {
					// set right_jumps
					relay.packetToSend.right_jump_index = sim.protocolParameters.n_right_jumps;
				}
				// aim (90 degrees on max index, then slowly converge to 0) degrees to the right
				var dist = sim.distances[relay.id, sink.id];
				var angle = Math.Atan((sink.position.Y - relay.position.Y) / (sink.position.X - relay.position.X));
				if (relay.packetToSend.right_jump_index > 0) {
					var angle_fraction = ((float)Math.PI / 2) / sim.protocolParameters.n_right_jumps;
					angle -= angle_fraction * relay.packetToSend.right_jump_index;
				}
				float flip = relay.position.X < sink.position.X ? 1 : -1;
				relay.packetToSend.current_aim.X = relay.position.X + flip * (float)Math.Cos(angle) * dist;
				relay.packetToSend.current_aim.Y = relay.position.Y + flip * (float)Math.Sin(angle) * dist;
			}
			else {
				relay.packetToSend.current_aim.X = sink.position.X;
				relay.packetToSend.current_aim.Y = sink.position.Y;
			}

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
