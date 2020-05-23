using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GeRaF
{
	class SimulationParameters
	{
		public double max_time;
		public int area_side;
		public double range;
		public int n_nodes;
		public double packet_rate;
		public double debug_interval;
		public bool debug_always;
		public string debug_file;
		public string debug_file_compressed;
	}
}
