using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GeRaF
{
	class Program
	{
		static void Main(string[] args) {
			System.Globalization.CultureInfo customCulture = (System.Globalization.CultureInfo)System.Threading.Thread.CurrentThread.CurrentCulture.Clone();
			customCulture.NumberFormat.NumberDecimalSeparator = ".";
			System.Threading.Thread.CurrentThread.CurrentCulture = customCulture;


			var pp = new ProtocolParameters() {
				duty_cycle = 0.5, // NOT IN SECONDS: active time percentage (t_l / (t_l + t_s))
				t_sense = 0.0521, // carrier sense duration (needs to be more than any signal duration)
				t_backoff = 0.0219, // backoff interval length (constant?)
				t_listen = 0.016, // listening time, must be quite higher than t_signal
				t_data = 0.0521, // data transmission time
				t_signal = 0.00521, // signal packet transmission time (RTS and CTS ?)
				n_regions = 4, // number of priority regions
				n_max_coll = 6, // number of attempts for solving a collision
				n_max_sensing = 10,
				n_max_pkt = 3,
				n_max_region_cycle = 1
			};
			var sp = new SimulationParameters() {
				area_side = 200,
				debug_interval = 1,
				debugType = DebugType.Never,
				debug_file = "../../graphic_debug/debug_data.js",
				max_time = 100,
				n_nodes = 200,
				packet_rate = 5,
				range = 20,
				min_distance = 2,
				percentages = 200,
				skipCycleEvents = true
			};

			int simulationNumber = 100;
			var simResults = new List<SimulationStats>();
			var maxParallel = Environment.ProcessorCount - 1;
			Console.WriteLine("Max parallel sims: " + maxParallel);
			var options = new ParallelOptions() { MaxDegreeOfParallelism = maxParallel };
			Parallel.For(0, simulationNumber, options, i => {
				var id = i;
				var sim = new Simulation(sp, pp);
				var simResult = sim.Run();
				lock (simResults) {
					simResults.Add(simResult);
				}
				Console.WriteLine($"Finished simulation {id}, {simResults.Count * 100f / simulationNumber}%");
			});

			Console.WriteLine("Finished simulating");

			// write results to file
			var runResults = new RunResults() {
				protocolParameters = pp,
				simulationParameters = sp,
				simulationStats = simResults
			};
			using (var writer = new StreamWriter("../../stats/runResults.json")) {
				writer.WriteLine(JsonConvert.SerializeObject(runResults, Formatting.Indented));
			}
			Console.WriteLine("Saved results");
			Console.ReadKey();
		}
	}

	class RunResults
	{
		public ProtocolParameters protocolParameters;
		public SimulationParameters simulationParameters;
		public List<SimulationStats> simulationStats;
	}
}
