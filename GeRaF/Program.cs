using GeRaF.StatsGeneration.Base;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace GeRaF
{
	class Program
	{
		public static int simulationNumber;
		public static int maxParallel;

		static void Main(string[] args) {
			System.Globalization.CultureInfo customCulture = (System.Globalization.CultureInfo)System.Threading.Thread.CurrentThread.CurrentCulture.Clone();
			customCulture.NumberFormat.NumberDecimalSeparator = ".";
			System.Threading.Thread.CurrentThread.CurrentCulture = customCulture;
			maxParallel = Environment.ProcessorCount - 1;
			Console.WriteLine("Max parallel sims: " + maxParallel);


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
				area_side = 100,
				debug_interval = 1,
				debugType = DebugType.Never,
				debug_file = "../../graphic_debug/debug_data.js",
				max_time = 50,
				n_nodes = 200,
				packet_rate = 5,
				range = 20,
				min_distance = 2,
				skipCycleEvents = true
			};

			simulationNumber = 50;

			var runResults = new RunResult {
				basePP = pp,
				baseSP = sp,
				dutyLambdas = DutyLambda.Generate(pp, sp, new List<double>() { 0.1, 0.5, 0.9 }, new List<double> { 0.1, 1, 5, 10, 20 }),
				lambdaNs = LambdaN.Generate(pp, sp, new List<double> { 1, 5, 10, 20, 100, 500 }, new List<int> { 50, 100, 200, 500 })
			};

			Console.WriteLine("Finished simulating");

			using (var writer = new StreamWriter("../../stats/runResults.json")) {
				writer.WriteLine(JsonConvert.SerializeObject(runResults, Formatting.Indented));
			}
			Console.WriteLine("Saved results");
			Console.ReadKey();
		}
	}
}
