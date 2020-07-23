using GeRaF.StatsGeneration;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace GeRaF
{
	class Program
	{
		public static int maxParallel;

		static void Main(string[] args) {
			System.Globalization.CultureInfo customCulture = (System.Globalization.CultureInfo)System.Threading.Thread.CurrentThread.CurrentCulture.Clone();
			customCulture.NumberFormat.NumberDecimalSeparator = ".";
			System.Threading.Thread.CurrentThread.CurrentCulture = customCulture;
			maxParallel = Environment.ProcessorCount - 1;
			maxParallel = 3;
			Console.WriteLine("Max parallel sims: " + maxParallel);


			var pp = new ProtocolParameters() {
				duty_cycle = 0.9, // NOT IN SECONDS: active time percentage (t_l / (t_l + t_s))
				t_sense = 0.0521, // carrier sense duration (needs to be more than any signal duration)
				t_backoff = 0.0219, // backoff interval length (constant?)
				t_listen = 0.016, // listening time, must be quite higher than t_signal
				t_data = 0.0521, // data transmission time
				t_signal = 0.00521, // signal packet transmission time (RTS and CTS ?)
				n_regions = 4, // number of priority regions
				n_max_coll = 6, // number of attempts for solving a collision
				n_max_sensing = 10,
				n_max_pkt = 3,
				n_max_region_cycle = 5
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
				min_distance = 1,
				asleepEnergy = 2,
				idleEnergy = 10,
				transmissionEnergy = 50,
				emptyRegionType = EmptyRegionType.Lines,
				skipCycleEvents = true
			};

			var versions = Enum.GetValues(typeof(ProtocolVersion)).Cast<ProtocolVersion>().ToList();
			var shapes = Enum.GetValues(typeof(EmptyRegionType)).Cast<EmptyRegionType>().ToList();

			var runResults = new RunResult {
				startTime = DateTime.Now,
				basePP = pp,
				baseSP = sp
			};

			var DLparameters = new GeneralParameters() {
				lambdas = new List<double> { 0.1, 1, 5, 10 },
				dutyCycles = new List<double>() { 0.1, 0.5, 0.9 },
				versions = versions,
				simulations = 200
			};
			var LNparameters = new GeneralParameters() {
				lambdas = new List<double> { 1, 5, 10, 20, 100, 500 },
				Ns = new List<int> { 100, 200, 500 }, // CHANGE TO PROPORTIONAL TO AREA
				versions = versions,
				simulations = 20
			};
			var shapeParameters = new GeneralParameters() {
				//lambdas = new List<double> { 5, 20 },
				versions = versions,
				emptyRegionTypes = shapes,
				simulations = 1000
			};
			var outcomesParameters = new GeneralParameters() {
				versions = versions,
				emptyRegionTypes = shapes,//new List<EmptyRegionType> { EmptyRegionType.None },
				simulations = 100
			};
			var debugParameters = new GeneralParameters() {
				versions = new List<ProtocolVersion> { ProtocolVersion.Base },
				emptyRegionTypes = new List<EmptyRegionType> { EmptyRegionType.Lines },
				simulations = 1
			};

			//runResults.dutyLambdas = General.Generate("DL", sp, pp, DLparameters);
			//runResults.lambdaNs = General.Generate("LN", sp, pp, LNparameters);
			runResults.shapeStats = General.Generate("Shapes", sp, pp, shapeParameters);
			//runResults.outcomeStats = General.Generate("Outcomes", sp, pp, outcomesParameters);

			// debug
			//sp.debugType = DebugType.Always;
			//General.Generate("Debug", sp, pp, debugParameters);

			runResults.endTime = DateTime.Now;

			Console.WriteLine("Finished simulating");

			using (var writer = new StreamWriter("../../stats/runResults.json")) {
				writer.WriteLine(JsonConvert.SerializeObject(runResults, Formatting.Indented));
			}
			Console.WriteLine("Saved results");
			//Console.ReadKey();
		}
	}
}
