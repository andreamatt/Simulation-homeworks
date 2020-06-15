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
			for (int i = 0; i < simulationParameters.n_nodes; i++) {
				var relay = new Relay {
					id = i,
					range = simulationParameters.range,
					status = RelayStatus.Asleep,
					sim = this
				};
				relays.Add(relay);
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
			int n_slots = (int)Math.Floor(simulationParameters.area_side / simulationParameters.min_distance) - 1;
			while (!connected) {
				// check wether the grid slot is available
				var slotBusy = new bool[n_slots, n_slots];
				foreach (var relay in relays) {
					var X_slot = RNG.rand_int(0, n_slots);
					var Y_slot = RNG.rand_int(0, n_slots);
					// while slot is busy, change slot
					while (slotBusy[X_slot, Y_slot]) {
						X_slot = RNG.rand_int(0, n_slots);
						Y_slot = RNG.rand_int(0, n_slots);
					}
					// take slot
					slotBusy[X_slot, Y_slot] = true;
					// assign actual position
					relay.X = X_slot * simulationParameters.min_distance + simulationParameters.min_distance;
					relay.Y = Y_slot * simulationParameters.min_distance + simulationParameters.min_distance;
				}

				//Console.WriteLine("Checking connected relays");

				// calculate new neighbours
				distances = GraphUtils.Distances(relays);
				GraphUtils.SetNeighbours(relays, distances);
				connected = GraphUtils.Connected(relays);
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
					debugWriter.WriteLine($"{r.id};{r.X};{r.Y};{r.range}");
				}
				debugWriter.WriteLine("#");
				debugWriter.WriteLine($"{JsonConvert.SerializeObject(distances)}");
				debugWriter.WriteLine("#");
			}
		}

		public double progressRate => this.clock / simulationParameters.max_time;

		public SimulationStats Run() {
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

			if (simulationParameters.debugType != DebugType.Never) {
				debugWriter.Write("`");
				debugWriter.Close();
			}

			// stats
			var simStats = new SimulationStats() {
				finishedPackets = String.Join(";", packetsFinished.Select(p => p.ToString())),
				relayInfos = String.Join(";", relays.Select(r => new RelayInfo() {
					id = r.id,
					X = r.X,
					Y = r.Y,
					asleepTime = 0,
					awakeTime = 0
				}.ToString()))
			};

			return simStats;

			//var tot = (float)packetsFinished.Count;
			//var success = packetsFinished.Count(p => p.result == Result.Success);
			//var max_sensing = packetsFinished.Count(p => p.result == Result.Abort_max_sensing);
			//var max_cycle = packetsFinished.Count(p => p.result == Result.Abort_max_region_cycle);
			//var no_start = packetsFinished.Count(p => p.result == Result.No_start_relays);
			//var max_sink_rts = packetsFinished.Count(p => p.result == Result.Abort_max_sink_rts);
			//var no_ack = packetsFinished.Count(p => p.result == Result.Abort_no_ack);
		}
	}
}
