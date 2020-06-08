using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GeRaF
{
	class Program
	{
		static void Main(string[] args) {
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
				debugType = DebugType.Always,
				debug_file = "../../graphic_debug/debug_data.js",
				max_time = 20,
				n_nodes = 200,
				packet_rate = 5,
				range = 20,
				min_distance = 2,
				percentages = 200,
				skipCycleEvents = true
			};
			var sim = new Simulation(sp, pp);
			sim.Run();
			Console.WriteLine("Finished");
			Console.ReadKey();
		}
	}
}
