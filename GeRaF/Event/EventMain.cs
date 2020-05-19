using Priority_Queue;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GeRaF
{
	public enum EventType
	{
		Start = 0,
		End = 1,
		Debug = 2,
		// start events: 1. set carrier busy 2. set collisions on active transmissions 3. add to active transmissions CHECK SENDER STATUS TOO
		// end events: 1. set carrier to free 2. move to finished transmission/delete (CTSs are just moved, others deleted)
		SENSE_start = 3,  // start sensing
		SENSE_end = 4,  // check that last active pulse is older than relative sense_start. If free, RTS_start. if busy, backoff to SENSE_start
		RTS_start = 5,  // node, time, region index. 1.select active nodes in that area and add to scheduled RTS_end (keep them awake)
		RTS_end = 6,  // node, time, region index, receiving nodes. 1.schedule CTS_start for selected nodes 2.schedule COL_check in cts_time time (plus some delta to be sure)
		SINK_RTS_start,
		SINK_RTS_end,
		CTS_start = 7,  // :
		CTS_end = 8,  // :
		COL_check = 9,
		// 0. no response => next region
		// source, sink, node, ...: 1. no collision => PKT_start with first CTS sender; any collision => COL_start OR (next_region and RTS_start) depending on collision count
		// modified 1. at least 1 non-overlapping => PKT_start with first CTS sender; all overlapping => COL_start
		// modified 2. select between non-overlapping using probabilities of success
		// modified 3. no collision => PKT_start with probabilities; any collision => COL_start
		// modified 4. send collision message only for collided CTSs (others stay silent)
		COL_start = 10,
		COL_end = 11,  // ... : .. schedule COL_check in max_backoff time. 2.schedule CTS_start for available nodes with backoff
		PKT_start = 12,  // nodes not selected can go to sleep, schedule ACK_check
		PKT_end = 13,
		//if packet has no collision schedule ACK_start, else stay
		ACK_start = 14,
		ACK_end = 15,
		ACK_check = 16  // if no collision, finished. Else, schedule PKT_start again
	}

	abstract class Event
	{
		public double time;
		public string type => this.GetType().Name;

		public abstract void Handle(Simulation sim);
	}

	class StartEvent : Event
	{
		public StartEvent() {
			time = 0;
		}

		public override void Handle(Simulation sim) {
			// add initial packet arrival
			var p = new PacketGenerationEvent();
			p.time = RNG.rand_expon(sim.simulationParameters.packet_rate);
			sim.eventQueue.Add(p);
		}
	}

	class EndEvent : Event
	{
		public EndEvent(double max_time) {
			time = max_time;
		}

		public override void Handle(Simulation sim) {
			// dump debugstats to file
			// clear event queue
			sim.eventQueue.Clear();
			return;
		}
	}

	class PacketGenerationEvent : Event
	{
		public override void Handle(Simulation sim) {
			// schedule next packet arrival
			var p = new PacketGenerationEvent();
			p.time = sim.clock + RNG.rand_expon(sim.simulationParameters.packet_rate);
			sim.eventQueue.Add(p);

			// give this packet to some relay or discard it if none are available
			var freeRelays = sim.relays.Where(r => r.status == RelayStatus.Free).ToList();
			if (freeRelays.Count > 0) {
				var chosen = freeRelays[RNG.rand_int(0, freeRelays.Count)];

				// choose random sink
				var otherRelays = sim.relays.Where(r => r != chosen).ToList();
				var sink = otherRelays[RNG.rand_int(0, otherRelays.Count)];

				var packet = new Packet();
				packet.generationTime = sim.clock;
				packet.startRelay = chosen;
				packet.sink = sink;
				chosen.packetToSend = packet;
				chosen.SelfReserve();

				// schedule startSensingEvent
				var sensing = new StartSensingEvent();
				sensing.time = sim.clock;
				sensing.relay = chosen;
				sim.eventQueue.Add(sensing);
			}
			// no nodes available
			else {
				var packet = new Packet();
				packet.generationTime = sim.clock;
				packet.Finish(Result.No_start_relays, sim);
			}
		}
	}

	class FreeRelayEvent : Event
	{
		public Relay relay;
		public override void Handle(Simulation sim) {
			relay.Free();
		}
	}

	class RegionProgressEvent : Event
	{
		public Relay relay;
		public override void Handle(Simulation sim) {
			// no more regions
			if (relay.regionIndex == sim.protocolParameters.n_regions - 1) {
				if (relay.ATTEMPT_count < sim.protocolParameters.n_max_attempts) {
					relay.ATTEMPT_count++;
					relay.regionIndex = 0;
					// schedule sensing immediately
					var SENSE_start = new StartSensingEvent();
					SENSE_start.time = sim.clock;
					SENSE_start.relay = relay;
					sim.eventQueue.Add(SENSE_start);
				}
				else {
					relay.packetToSend.Finish(Result.Abort_max_attempts, sim);
					relay.FreeNow(sim);
				}
			}
			// go next region
			else {
				relay.regionIndex++;
				// schedule RTS
				var RTS_start = new StartRTSEvent();
				RTS_start.time = sim.clock;
				RTS_start.relay = relay;
				sim.eventQueue.Add(RTS_start);
			}
		}
	}
}
