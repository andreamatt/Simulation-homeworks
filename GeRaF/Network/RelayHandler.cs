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
		private void HandleRTS(Relay sender) {
			bool failed = this.activeTransmissions[sender];
			// if free or awaiting for ITS correct region
			if (failed == false && (this.status == RelayStatus.Free || (this.status == RelayStatus.Awaiting_region && this.busyWith == sender))) {
				var sink = sender.packetToSend.sink;
				var sourceToSink = sim.distances[sender.id][sink.id];
				var limitToSink = sourceToSink - sender.range;
				var regionWidth = sender.range / sim.protocolParameters.n_regions;
				var dist = sim.distances[this.id][sink.id];
				var myIndex = (int)Math.Floor((dist - limitToSink) / regionWidth);

				// if I'm in the correct region
				if (myIndex == sender.REGION_index) {
					this.Reserve(sender);
					sim.eventQueue.Add(new StartCTSEvent() {
						relay = this,
						time = sim.clock,
						actualDestination = sender,
						sim = sim
					});
				}
				// i'm early
				else if (myIndex > sender.REGION_index) {
					this.Reserve(sender);
					this.status = RelayStatus.Awaiting_region;
				}
			}
		}

		private void HandleCTS(Relay sender, Relay actualDestination) {
			// if it is meant for me
			if (actualDestination == this) {
				// only save in finishedCTS
				this.finishedCTSs.Add(new Transmission() {
					transmissionType = TransmissionType.CTS,
					source = sender,
					destination = this,
					failed = this.activeTransmissions[sender],
					actualDestination = actualDestination == this
				});
			}
		}

		private void HandlePKT(Relay sender, Relay actualDestination) {
			bool failed = this.activeTransmissions[sender];
			if (failed == false && this.BusyWith == sender) {
				// if chosen one, schedule ACK
				if (actualDestination == this) {
					sim.eventQueue.Add(new StartACKEvent() {
						time = sim.clock,
						relay = this,
						actualDestination = sender,
						sim = sim
					});
				}
				else {
					// ??? what exactly?
					this.FreeNow();
				}
			}
		}

		private void HandleACK(Relay sender, Relay actualDestination) {
			bool failed = this.activeTransmissions[sender];
			// if it is meant for me
			if (failed == false && actualDestination == this) {
				// simply set receivedACK to true???
				receivedACK = true;
			}
		}

		private void HandleCOL(Relay sender) {
			bool failed = this.activeTransmissions[sender];
			var sink = sender.packetToSend.sink;
			var sourceToSink = sim.distances[sender.id][sink.id];
			var limitToSink = sourceToSink - sender.range;
			var regionWidth = sender.range / sim.protocolParameters.n_regions;
			var dist = sim.distances[this.id][sink.id];
			var myIndex = (int)Math.Floor((dist - limitToSink) / regionWidth);

			// if i'm busy with sender it means I did send CTS
			// check region to be sure I'm still valid (could have missed my CTS, next RTS, other CTSs and this is their COL)
			if (failed == false && this.BusyWith == sender && myIndex == sender.REGION_index) {
				this.Reserve(sender);
				this.status = RelayStatus.Backoff_CTS;

				var backoffSize = sim.protocolParameters.t_backoff * Math.Pow(2, sender.COL_count);
				sim.eventQueue.Add(new StartCTSEvent() {
					relay = this,
					time = sim.clock + backoffSize * RNG.rand(),
					actualDestination = sender,
					sim = sim
				});
			}
		}

		private void HandleSINKRTS(Relay sender, Relay actualDestination) {
			bool failed = this.activeTransmissions[sender];
			// if free and sink
			if (failed == false && this.status == RelayStatus.Free && actualDestination == this) {
				this.Reserve(sender);
				sim.eventQueue.Add(new StartCTSEvent {
					relay = this,
					actualDestination = sender,
					time = sim.clock,
					sim = sim
				});
			}
		}

		private void HandleSINKCOL(Relay sender, Relay actualDestination) {
			bool failed = this.activeTransmissions[sender];
			if (failed == false && this.busyWith == sender && actualDestination == this) {
				this.Reserve(sender);
				sim.eventQueue.Add(new StartCTSEvent {
					relay = this,
					actualDestination = sender,
					time = sim.clock,
					sim = sim
				});
			}
		}
	}
}
