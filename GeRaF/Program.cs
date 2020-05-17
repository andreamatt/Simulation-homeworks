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
			var sim = new Simulation(10, 100, 10000, 3, 1, pp);
			sim.Run();
		}
	}
}
