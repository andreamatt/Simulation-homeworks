using GeRaF.Events;
using GeRaF.Events.DebugStats;
using GeRaF.Events.DutyCycle;
using GeRaF.Network;
using GeRaF.Utils;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

namespace GeRaF
{
	class Simulation
	{
		public SimulationParameters simulationParameters;
		public ProtocolParameters protocolParameters;

		// system state
		public List<Relay> relays;
		public Dictionary<int, Relay> relayById;
		public Dictionary<int, Dictionary<int, double>> distances;
		public List<Packet> packetsFinished = new List<Packet>();
		public PacketGenerator packetGenerator = new PacketGenerator();

		// simulation state
		public double clock;
		public EventQueue eventQueue;

		// stat counters
		public StreamWriter debugWriter;

		public Simulation(SimulationParameters simulationParameters, ProtocolParameters protocolParameters) {
			this.simulationParameters = simulationParameters;
			this.protocolParameters = protocolParameters;

			// init sim state
			clock = 0;
			eventQueue = new EventQueue();

			var initialCycleDuration = protocolParameters.t_sleep + protocolParameters.t_listen;
			eventQueue.Add(new StartEvent() {
				time = initialCycleDuration + protocolParameters.t_delta, // must start AFTER first duty cycle events
				sim = this,
				previous = null
			});
			eventQueue.Add(new EndEvent() {
				time = simulationParameters.max_time,
				sim = this,
				previous = null
			});

			if (simulationParameters.debugType == DebugType.Interval) {
				eventQueue.Add(new DebugEvent() {
					time = simulationParameters.debug_interval,
					sim = this,
					previous = null
				});
			}

			// init relays as asleep and schedule random awake
			relays = new List<Relay>();
			relayById = new Dictionary<int, Relay>();
			for (int i = 0; i < simulationParameters.n_nodes; i++) {
				var relay = new Relay {
					id = i,
					range = simulationParameters.range,
					status = RelayStatus.Asleep,
					sim = this
				};
				relays.Add(relay);
				relayById[relay.id] = relay;
				eventQueue.Add(new AwakeEvent() {
					relay = relay,
					// could be at any point of its duty cycle
					time = RNG.rand() * initialCycleDuration,
					sim = this,
					previous = null
				});
			}

			// move disconnected
			//Console.WriteLine("Placing relays");
			var connected = false;
			int n_slots = simulationParameters.binsCount;
			while (!connected) {
				// check wether the grid slot is available
				var slotBusy = new bool[n_slots, n_slots];
				foreach (var relay in relays) {
					var X_slot = 0;
					var Y_slot = 0;
					var validRegion = false;
					do {
						X_slot = RNG.rand_int(0, n_slots);
						Y_slot = RNG.rand_int(0, n_slots);

						// assign actual position
						relay.position.X = slotToX(X_slot);
						relay.position.Y = slotToX(Y_slot);

						// calculate if in region
						var X = relay.position.X;
						var Y = relay.position.Y;
						var c = simulationParameters.area_side / 2;
						var dx = X - c;
						var dy = Y - c;
						switch (simulationParameters.emptyRegionType) {
							case EmptyRegionType.None:
								validRegion = true;
								break;
							case EmptyRegionType.Circle:
								var distanceToCenter = Math.Sqrt(dx * dx + dy * dy);
								var radius = simulationParameters.emptyRegionSize;
								validRegion = distanceToCenter >= radius;
								break;
							case EmptyRegionType.Square:
								var side = simulationParameters.emptyRegionSize;
								validRegion = (Math.Abs(dx) >= side / 2) || (Math.Abs(dy) >= side / 2);
								break;
							case EmptyRegionType.Lines:
								var side_w = simulationParameters.emptyRegionSize;
								var side_h = Math.Max(relay.range + 2, side_w / 8);
								var validFirstLine = (X <= c - side_w / 2) || (X >= c + side_w / 2) || (Y <= c * 3 / 2 - side_h / 2) || (Y >= c * 3 / 2 + side_h / 2);
								var validSecondLine = (X <= c - side_w / 2) || (X >= c + side_w / 2) || (Y <= c * 1 / 2 - side_h / 2) || (Y >= c * 1 / 2 + side_h / 2);
								validRegion = validFirstLine && validSecondLine;
								break;
							case EmptyRegionType.Holes:
								radius = simulationParameters.emptyRegionSize;
								var d1 = Math.Sqrt(Math.Pow(X - c / 2, 2) + Math.Pow(Y - c / 2, 2));
								var d2 = Math.Sqrt(Math.Pow(X - c / 2, 2) + Math.Pow(Y - 3 * c / 2, 2));
								var d3 = Math.Sqrt(Math.Pow(X - 3 * c / 2, 2) + Math.Pow(Y - c / 2, 2));
								var d4 = Math.Sqrt(Math.Pow(X - 3 * c / 2, 2) + Math.Pow(Y - 3 * c / 2, 2));
								validRegion = d1 >= radius && d2 >= radius && d3 >= radius && d4 >= radius;
								break;
						}
					}
					while (slotBusy[X_slot, Y_slot] || !validRegion);

					// take slot
					slotBusy[X_slot, Y_slot] = true;
				}

				//Console.WriteLine("Checking connected relays");

				// calculate new neighbours
				distances = GraphUtils.Distances(relays);
				GraphUtils.SetNeighbours(relays, distances);
				connected = GraphUtils.Connected(relays);
			}
			if (protocolParameters.protocolVersion == ProtocolVersion.Plus) {
				GraphUtils.RepeatedBFS(relays, distances);
			}
			//Console.WriteLine("Placed relays");

			// init debug state
			if (simulationParameters.debugType != DebugType.Never) {
				debugWriter = new StreamWriter(simulationParameters.debug_file);
				debugWriter.Write("debug_data=`");
				debugWriter.WriteLine($"{JsonConvert.SerializeObject(simulationParameters)}");
				debugWriter.WriteLine("#");
				debugWriter.WriteLine($"{JsonConvert.SerializeObject(protocolParameters)}");
				debugWriter.WriteLine("#");
				foreach (var r in relays) {
					debugWriter.WriteLine($"{r.id};{r.position.X};{r.position.Y};{r.range}");
				}
				debugWriter.WriteLine("#");
				debugWriter.WriteLine($"{JsonConvert.SerializeObject(distances)}");
				debugWriter.WriteLine("#");
			}
		}

		public double progressRate => this.clock / simulationParameters.max_time;

		public void Run() {
			while (eventQueue.isEmpty == false) {
				var e = eventQueue.Pop();
				if (e.time < this.clock) {
					throw new Exception("No time travel plz");
				}
				this.clock = e.time;
				e.Handle();
				if (simulationParameters.debugType == DebugType.Always) {
					DebugEvent.DebugNow(this, false, e);
				}
			}

			// force update awake time
			foreach (var r in relays) {
				r.UpdateAwakeTime();
			}

			if (simulationParameters.debugType != DebugType.Never) {
				debugWriter.Write("`");
				debugWriter.Close();
			}
		}

		public double slotToX(int slot) {
			return slot * simulationParameters.min_distance + simulationParameters.min_distance;
		}

		public int xToSlot(double x) {
			return (int)Math.Round((x - simulationParameters.min_distance) / simulationParameters.min_distance);
		}
	}
}
