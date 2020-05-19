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
				debug_interval = 0.1,
				debug_always = true,
				debug_file = "../../debug.json",
				max_time = 10,
				n_nodes = 3,
				packet_rate = 1,
				range = 1000
			};
			var sim = new Simulation(sp, pp);
			sim.Run();
			Console.WriteLine("Finished");
			//Console.ReadKey();
		}
	}
}
