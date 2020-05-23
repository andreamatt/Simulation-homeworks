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
			//relays = new List<Relay>(){
			//	new Relay(){
			//		id = 0,
			//		X = 10,
			//		Y = 10,
			//		range = 11,
			//		status = RelayStatus.Free
			//	},
			//	new Relay(){
			//		id = 1,
			//		X = 20,
			//		Y = 10,
			//		range = 11,
			//		status = RelayStatus.Free
			//	},
			//	new Relay(){
			//		id = 2,
			//		X = 30,
			//		Y = 10,
			//		range = 11,
			//		status = RelayStatus.Free
			//	}
			//};
			relays = new List<Relay>();
			for (int i = 0; i < simulationParameters.n_nodes; i++) {
				var relay = new Relay();
				relay.id = i;
				relay.X = RNG.rand() * simulationParameters.area_side;
				relay.Y = RNG.rand() * simulationParameters.area_side;
				relay.range = simulationParameters.range;
				relay.status = RelayStatus.Free;
				relays.Add(relay);
			}

			// calculate distances and neighbours
			distances = new Dictionary<int, Dictionary<int, double>>();
			foreach (var r1 in relays) {
				distances[r1.id] = new Dictionary<int, double>();
				foreach (var r2 in relays) {
					if (r1 != r2) {
						var dist = Math.Sqrt(Math.Pow(r1.X - r2.X, 2) + Math.Pow(r1.Y - r2.Y, 2));
						if (dist < r1.range) {
							r1.neighbours.Add(r2);
						}
						distances[r1.id][r2.id] = dist;
					}
				}
			}

			// init sim state
			clock = 0;
			eventQueue = new EventQueue();
			eventQueue.Add(new StartEvent());
			if (simulationParameters.debug_always == false) {
				eventQueue.Add(new DebugEvent());
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
				if (simulationParameters.debug_always) {
					DebugEvent.DebugNow(this);
				}
				var e = eventQueue.Pop();
				this.clock = e.time;
				e.Handle(this);
			}

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
