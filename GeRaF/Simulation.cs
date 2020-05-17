using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

namespace GeRaF
{
	class Simulation
	{
		public double max_time;
		public int area_side;
		public double range;
		public int n_nodes;
		public double packet_rate;
		public ProtocolParameters protocolParameters;

		// system state
		public List<Relay> relays;
		public Dictionary<int, Dictionary<int, double>> distances;
		public List<Packet> finishedPackets = new List<Packet>();

		// simulation state
		public double clock;
		public EventQueue eventQueue;

		// stat counters

		// debug stats
		public List<DebugStats> debugStats;

		public Simulation(double max_time, int area_side, double range, int n_nodes, double packet_rate, ProtocolParameters protocolParameters) {
			this.max_time = max_time;
			this.area_side = area_side;
			this.range = range;
			this.n_nodes = n_nodes;
			this.packet_rate = packet_rate;
			this.protocolParameters = protocolParameters;


			// init relays
			relays = new List<Relay>();
			for (int i = 0; i < n_nodes; i++) {
				var relay = new Relay();
				relay.id = i;
				relay.X = RNG.rand() * area_side;
				relay.Y = RNG.rand() * area_side;
				relay.range = this.range;
				relay.awake = true;
				relays.Add(relay);
			}

			// calculate distances and neighbours
			distances = new Dictionary<int, Dictionary<int, double>>();
			foreach (var r1 in relays) {
				distances[r1.id] = new Dictionary<int, double>();
				foreach (var r2 in relays) {
					var dist = Math.Sqrt(Math.Pow(r1.X - r2.X, 2) + Math.Pow(r1.Y - r2.Y, 2));
					if (dist < r1.range) {
						r1.neighbours.Add(r2);
					}
					distances[r1.id][r2.id] = dist;
				}
			}

			// init sim state
			clock = 0;
			eventQueue = new EventQueue();
			eventQueue.Add(new StartEvent());
			eventQueue.Add(new EndEvent(max_time));
		}

		public void Run() {
			while (eventQueue.isEmpty == false) {
				var e = eventQueue.Pop();
				this.clock = e.time;
				e.Handle(this);
			}
		}
	}
}
