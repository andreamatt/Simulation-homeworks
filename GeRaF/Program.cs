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
			var pp = new ProtocolParameters();
			var sp = new SimulationParameters() {
				area_side = 100,
				debug_interval = 1,
				debugType = DebugType.Never,
				debug_file = "../../debug.json",
				max_time = 100000,
				n_nodes = 100,
				packet_rate = 2,
				range = 20,
				min_distance = 7
			};
			var sim = new Simulation(sp, pp);
			sim.Run();
			Console.WriteLine("Finished");
			Console.ReadKey();
		}
	}
}
