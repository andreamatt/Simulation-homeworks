﻿using GeRaF.Events;
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
		public float[,] distances;
		public List<Packet> packetsFinished = new List<Packet>();
		public List<Packet> packetsPassed = new List<Packet>();
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

			// create distance matrix
			distances = new float[relays.Count, relays.Count];

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
							case EmptyRegionType.Cross:
								var w = simulationParameters.emptyRegionSize / Math.Sqrt(2);
								var h = Math.Max(relay.range + 2, simulationParameters.emptyRegionSize / 10) / Math.Sqrt(2);
								var l = simulationParameters.area_side;
								var w1 = (l - w) > (X + Y);
								var w2 = (l + w) < (X + Y);
								var w3 = (w) < (X - Y);
								var w4 = -(w) > (X - Y);
								var h1 = (h) < (X - Y);
								var h2 = -(h) > (X - Y);
								var h3 = (l - h) > (X + Y);
								var h4 = (l + h) < (X + Y);
								validRegion = (w1 || w2 || h1 || h2) && (w3 || w4 || h3 || h4);
								break;
							case EmptyRegionType.Square:
								var side = simulationParameters.emptyRegionSize;
								validRegion = (Math.Abs(dx) >= side / 2) || (Math.Abs(dy) >= side / 2);
								break;
							case EmptyRegionType.Lines:
								var side_w = simulationParameters.emptyRegionSize;
								var side_h = Math.Max(relay.range + 2, side_w / 6);
								var validFirstLine = (X <= c - side_w / 2) || (X >= c + side_w / 2) || (Y <= c * 3 / 2 - side_h / 2) || (Y >= c * 3 / 2 + side_h / 2);
								var validSecondLine = (X <= c - side_w / 2) || (X >= c + side_w / 2) || (Y <= c * 1 / 2 - side_h / 2) || (Y >= c * 1 / 2 + side_h / 2);
								validRegion = validFirstLine && validSecondLine;
								break;
							case EmptyRegionType.Holes:
								var radius = simulationParameters.emptyRegionSize;
								var d1 = GraphUtils.Distance(X, Y, 0.5 * c, 0.5 * c);
								var d2 = GraphUtils.Distance(X, Y, 0.5 * c, 1.5 * c);
								var d3 = GraphUtils.Distance(X, Y, 1.5 * c, 0.5 * c);
								var d4 = GraphUtils.Distance(X, Y, 1.5 * c, 1.5 * c);
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
				GraphUtils.Distances(relays, distances);
				GraphUtils.SetNeighbours(relays, distances);
				connected = GraphUtils.Connected(relays);
			}
			if (protocolParameters.protocolVersion == ProtocolVersion.BFS || protocolParameters.protocolVersion == ProtocolVersion.BFS_half) {
				GraphUtils.RepeatedBFS(relays, distances, protocolParameters.protocolVersion);
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

		public float slotToX(int slot) {
			return slot * simulationParameters.min_distance + simulationParameters.min_distance;
		}

		public int xToSlot(float x) {
			return (int)Math.Round((x - simulationParameters.min_distance) / simulationParameters.min_distance);
		}
	}
}
