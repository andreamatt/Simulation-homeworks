﻿using Newtonsoft.Json;
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
				duty_cycle = 0.1, // NOT IN SECONDS: active time percentage (t_l / (t_l + t_s))
				t_sense = 0.0521, // carrier sense duration (needs to be more than any signal duration)
				t_backoff = 0.0219, // backoff interval length (constant?)
				t_listen = 0.016, // listening time, must be quite higher than t_signal
				t_sleep = 0.144, // t_listen * ((1 / duty_cycle) - 1), // 0.144000, sleep time
				t_data = 0.0521, // data transmission time
				t_signal = 0.00521, // signal packet transmission time (RTS and CTS ?)
									//t_busy, // slightly more than the max backoff + cts_time + col_time
				n_regions = 4, // number of priority regions
				n_max_attempts = 50, // number of attempts for searching a relay
				n_max_coll = 6, // number of attempts for solving a collision
				n_max_sensing = 10,
				n_max_pkt = 3
			};
			var sp = new SimulationParameters() {
				area_side = 100,
				debug_interval = 1,
				debugType = DebugType.Always,
				debug_file = "../../debug.json",
				max_time = 2,
				n_nodes = 100,
				packet_rate = 1,
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
