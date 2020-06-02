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
		public Dictionary<int, Dictionary<int, double>> distances;
		public List<Packet> finishedPackets = new List<Packet>();

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
			eventQueue.Add(new StartEvent() {
				time = 0,
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
			var initialCycleDuration = protocolParameters.t_sleep + protocolParameters.t_listen;
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
			Console.WriteLine("Placing relays");
			var connected = false;
			while (!connected) {
				foreach (var relay in relays) {
					relay.X = RNG.rand() * simulationParameters.area_side;
					relay.Y = RNG.rand() * simulationParameters.area_side;
				}

				// calculate new neighbours
				distances = GraphUtils.Distances(relays);
				GraphUtils.SetNeighbours(relays, distances);
				connected = GraphUtils.Connected(relays);
			}
			Console.WriteLine("Placed relays");

			// init debug state
			debugWriter = new StreamWriter(simulationParameters.debug_file);
			debugWriter.Write("{\n");
			debugWriter.Write($"\"SimulationParameters\": {JsonConvert.SerializeObject(simulationParameters)},\n");
			debugWriter.Write($"\"ProtocolParameters\": {JsonConvert.SerializeObject(protocolParameters)},\n");
			debugWriter.Write($"\"Distances\": {JsonConvert.SerializeObject(distances)},\n");
			debugWriter.Write($"\"Frames\": [\n");

		}

		public void Run() {
			double lastPrintPercentage = 0;
			while (eventQueue.isEmpty == false) {
				var e = eventQueue.Pop();
				if (e.time < this.clock) {
					throw new Exception("WTF");
				}
				this.clock = e.time;
				var percentage = this.clock / simulationParameters.max_time;
				if (percentage > lastPrintPercentage) {
					Console.WriteLine($"Simulating ... {lastPrintPercentage * 100:F3}%");
					lastPrintPercentage += 1 / (double)simulationParameters.percentages;
				}
				e.Handle();
				if (simulationParameters.debugType == DebugType.Always) {
					DebugEvent.DebugNow(this, false, e);
				}
			}

			// stats
			Console.WriteLine($"Number of packets: {finishedPackets.Count}");
			var tot = (float)finishedPackets.Count;
			var success = finishedPackets.Count(p => p.result == Result.Success);
			var max_sensing = finishedPackets.Count(p => p.result == Result.Abort_max_sensing);
			var max_cycle = finishedPackets.Count(p => p.result == Result.Abort_max_region_cycle);
			var no_start = finishedPackets.Count(p => p.result == Result.No_start_relays);
			var max_sink_rts = finishedPackets.Count(p => p.result == Result.Abort_max_sink_rts);
			var no_ack = finishedPackets.Count(p => p.result == Result.Abort_no_ack);
			Console.WriteLine($"Number of packets success: {success}, {success / tot * 100:F3}%");
			Console.WriteLine($"Number of packets no_start_relays: {no_start}, {no_start / tot * 100:F3}%");
			Console.WriteLine($"Number of packets max_cycle: {max_cycle}, {max_cycle / tot * 100:F3}%");
			Console.WriteLine($"Number of packets max_sensing: {max_sensing}, {max_sensing / tot * 100:F3}%");
			Console.WriteLine($"Number of packets max_sink_rts: {max_sink_rts}, {max_sink_rts / tot * 100:F3}%");
			Console.WriteLine($"Number of packets no_ack: {no_ack}, {no_ack / tot * 100:F3}%");

			debugWriter.Write("\n]\n}");
			debugWriter.Close();
		}
	}
}
