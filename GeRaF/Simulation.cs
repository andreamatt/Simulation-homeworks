using GeRaF.Events;
using GeRaF.Events.DebugStats;
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

			// init relays
			relays = new List<Relay>();
			// generate first batch
			for (int i = 0; i < simulationParameters.n_nodes; i++) {
				var relay = new Relay();
				relay.id = i;
				relay.range = simulationParameters.range;
				relay.status = RelayStatus.Free;
				relays.Add(relay);
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

			// init sim state
			clock = 0;
			eventQueue = new EventQueue();
			eventQueue.Add(new StartEvent());
			if (simulationParameters.debugType == DebugType.Interval) {
				eventQueue.Add(new DebugEvent() {
					time = simulationParameters.debug_interval
				});
			}
			eventQueue.Add(new EndEvent(simulationParameters.max_time));

			// init debug state
			debugWriter = new StreamWriter(simulationParameters.debug_file);
			debugWriter.Write("{\n");
			debugWriter.Write($"\"SimulationParameters\": {JsonConvert.SerializeObject(simulationParameters)},\n");
			debugWriter.Write($"\"ProtocolParameters\": {JsonConvert.SerializeObject(protocolParameters)},\n");
			debugWriter.Write($"\"Distances\": {JsonConvert.SerializeObject(distances)},\n");
			debugWriter.Write($"\"Frames\": [\n");
		}

		public void Run() {
			while (eventQueue.isEmpty == false) {
				if (simulationParameters.debugType == DebugType.Always) {
					DebugEvent.DebugNow(this, false);
				}
				var e = eventQueue.Pop();
				this.clock = e.time;
				e.Handle(this);
			}

			// stats
			Console.WriteLine($"Number of packets: {finishedPackets.Count}");
			var tot = (float)finishedPackets.Count;
			var success = finishedPackets.Count(p => p.Result == Result.Success);
			var channel_busy = finishedPackets.Count(p => p.Result == Result.Abort_max_sensing);
			var max_cycle = finishedPackets.Count(p => p.Result == Result.Abort_max_region_cycle);
			var no_start = finishedPackets.Count(p => p.Result == Result.No_start_relays);
			Console.WriteLine($"Number of packets success: {success}, {success / tot * 100:F3}%");
			Console.WriteLine($"Number of packets channel_busy: {channel_busy}, {channel_busy / tot * 100:F3}%");
			Console.WriteLine($"Number of packets max_cycle: {max_cycle}, {max_cycle / tot * 100:F3}%");
			Console.WriteLine($"Number of packets start_relays: {no_start}, {no_start / tot * 100:F3}%");

			debugWriter.Write("\n]\n}");
			debugWriter.Close();

			//Console.WriteLine("Rewriting");

			//dynamic allFile;
			//using (var reader = new StreamReader(simulationParameters.debug_file)) {
			//	allFile = JsonConvert.DeserializeObject(reader.ReadToEnd());
			//}

			//var formatted = JsonConvert.SerializeObject(allFile, Formatting.Indented);
			//using (var writer = new StreamWriter(simulationParameters.debug_file)) {
			//	writer.Write(formatted);
			//}

			//Console.WriteLine("Compressing");
			//using (var writer = new StreamWriter(simulationParameters.debug_file_compressed)) {
			//	writer.Write(StringCompress.Compress(formatted));
			//}
		}
	}
}
