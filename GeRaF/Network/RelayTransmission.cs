using GeRaF.Events;
using GeRaF.Events.Transmissions;
using GeRaF.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GeRaF.Network
{
	partial class Relay
	{
		public void StartReceiving(Event cause, Relay sender) {
			// if skipping events, wake up as it receives any transmission (can be optimized to wake up only when actually necessary)
			if (sim.simulationParameters.skipCycleEvents && this.status == RelayStatus.Asleep && this.ShouldBeAwake) {
				this.AwakeMidtime(cause);
			}

			bool interested = false;
			switch (this.status) {
				case RelayStatus.Sensing:
					this.hasSensed = true;
					break;
				case RelayStatus.Free:
					this.PostponeSleep(cause);
					interested = true;
					break;
				case RelayStatus.Awaiting_Signal:
					interested = true;
					break;
				case RelayStatus.Awaiting_region:
					interested = true;
					break;
				case RelayStatus.Asleep:
					break;
				case RelayStatus.Transmitting:
					break;
				case RelayStatus.Backoff_Sensing:
					break;
				case RelayStatus.Backoff_CTS:
					break;
				case RelayStatus.Backoff_SinkRTS:
					break;
				default:
					throw new Exception("Unexpected relay status");
			}

			// every one else failes anyway
			foreach (var transmitter in receivingTransmissions.Keys.ToList()) {
				receivingTransmissions[transmitter] = false;
			}

			if (interested) {
				bool failed = receivingTransmissions.Count > 0;
				this.receivingTransmissions[sender] = failed;
			}
		}

		public void EndReceiving(Event cause, TransmissionType transmissionType, Relay sender, Relay actualDestination = null) {
			// if i'm interested ( I was when started receiving )
			if (this.receivingTransmissions.ContainsKey(sender)) {
				// decide what to do
				switch (transmissionType) {
					case TransmissionType.RTS:
						this.HandleRTS(cause, sender);
						break;
					case TransmissionType.SINK_RTS:
						this.HandleSINKRTS(cause, sender, actualDestination);
						break;
					case TransmissionType.CTS:
						this.HandleCTS(cause, sender, actualDestination);
						break;
					case TransmissionType.PKT:
						this.HandlePKT(cause, sender, actualDestination);
						break;
					case TransmissionType.ACK:
						this.HandleACK(cause, sender, actualDestination);
						break;
					case TransmissionType.COL:
						this.HandleCOL(cause, sender);
						break;
					case TransmissionType.SINK_COL:
						this.HandleSINKCOL(cause, sender, actualDestination);
						break;
					default:
						break;
				}

				this.receivingTransmissions.Remove(sender);
			}
		}

		private void HandleRTS(Event cause, Relay sender) {
			var packetContentID = sender.packetToSend.content_id;
			if (sim.protocolParameters.avoid_back_flow && passedPacketsSet.Contains(packetContentID)) {
				// do not reply
				return;
			}
			bool failed = this.receivingTransmissions[sender];
			// if free or awaiting for ITS correct region
			if (failed == false && (this.status == RelayStatus.Free || (this.status == RelayStatus.Awaiting_region && this.busyWith == sender))) {
				int myIndex = CalculateRegionIndex(sender);
				// if I'm in the correct region
				if (myIndex == sender.REGION_index) {
					this.Reserve(sender, cause);
					sim.eventQueue.Add(new StartCTSEvent() {
						relay = this,
						time = sim.clock,
						actualDestination = sender,
						sim = sim,
						previous = cause
					});
				}
				// i'm early
				else if (myIndex > sender.REGION_index && myIndex < sim.protocolParameters.n_regions) {
					this.Reserve(sender, cause);
					this.status = RelayStatus.Awaiting_region;
				}
			}
		}

		private void HandleCTS(Event cause, Relay sender, Relay actualDestination) {
			// if it is meant for me
			if (actualDestination == this) {
				// only save in finishedCTS
				this.finishedCTSs.Add(new Transmission() {
					transmissionType = TransmissionType.CTS,
					source = sender,
					destination = this,
					failed = this.receivingTransmissions[sender],
					actualDestination = actualDestination == this
				});
			}
		}

		private void HandlePKT(Event cause, Relay sender, Relay actualDestination) {
			bool failed = this.receivingTransmissions[sender];
			if (failed == false && this.busyWith == sender) {
				// if chosen one, schedule ACK
				if (actualDestination == this) {
					sim.eventQueue.Add(new StartACKEvent() {
						time = sim.clock,
						relay = this,
						actualDestination = sender,
						sim = sim,
						previous = cause
					});
				}
				else {
					// ??? what exactly?
					this.FreeNow(cause);
				}
			}
		}

		private void HandleACK(Event cause, Relay sender, Relay actualDestination) {
			bool failed = this.receivingTransmissions[sender];
			// if it is meant for me
			if (failed == false && actualDestination == this) {
				// simply set receivedACK to true???
				receivedACK = true;
			}
		}

		private void HandleCOL(Event cause, Relay sender) {
			bool failed = this.receivingTransmissions[sender];
			int myIndex = CalculateRegionIndex(sender);

			// if i'm busy with sender it means I did send CTS
			// check region to be sure I'm still valid (could have missed my CTS, next RTS, other CTSs and this is their COL)
			if (failed == false && this.busyWith == sender && myIndex == sender.REGION_index) {
				this.Reserve(sender, cause);
				this.status = RelayStatus.Backoff_CTS;

				var backoffSize = sim.protocolParameters.t_backoff * Math.Pow(2, sender.COL_count);
				sim.eventQueue.Add(new StartCTSEvent() {
					relay = this,
					time = sim.clock + backoffSize * RNG.rand(),
					actualDestination = sender,
					sim = sim,
					previous = cause
				});
			}
		}

		private void HandleSINKRTS(Event cause, Relay sender, Relay actualDestination) {
			bool failed = this.receivingTransmissions[sender];
			// if free and sink
			if (failed == false && this.status == RelayStatus.Free && actualDestination == this) {
				this.Reserve(sender, cause);
				sim.eventQueue.Add(new StartCTSEvent {
					relay = this,
					actualDestination = sender,
					time = sim.clock,
					sim = sim,
					previous = cause
				});
			}
		}

		private void HandleSINKCOL(Event cause, Relay sender, Relay actualDestination) {
			bool failed = this.receivingTransmissions[sender];
			if (failed == false && this.busyWith == sender && actualDestination == this) {
				this.Reserve(sender, cause);
				sim.eventQueue.Add(new StartCTSEvent {
					relay = this,
					actualDestination = sender,
					time = sim.clock,
					sim = sim,
					previous = cause
				});
			}
		}

		private int CalculateRegionIndex(Relay sender) {
			var sink = sender.packetToSend.sink;
			var aimX = sender.packetToSend.current_aim.X;
			var aimY = sender.packetToSend.current_aim.Y;
			var sourceToAim = sim.distances[sender.id, sink.id]; // always same as sink, by design
			var limitToAim = sourceToAim - sender.range;
			var regionWidth = sender.range / sim.protocolParameters.n_regions;
			var distToAim = GraphUtils.Distance(position.X, position.Y, aimX, aimY);
			//var distToAim = sender.packetToSend.current_aim_relay == null ? GraphUtils.Distance(position.X, position.Y, aimX, aimY) : sim.distances[this.id, sender.packetToSend.current_aim_relay.id];
			return (int)Math.Floor((distToAim - limitToAim) / regionWidth);
		}
	}
}
