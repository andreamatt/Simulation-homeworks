using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GeRaF.StatsGeneration
{
	class GeneralParameters
	{
		public List<double> lambdas = new List<double>();
		public List<int> relay_densities = new List<int>(); // number of relays for 100x100 area
		public List<double> dutyCycles = new List<double>();
		public List<ProtocolVersion> versions = new List<ProtocolVersion>();
		public List<EmptyRegionType> emptyRegionTypes = new List<EmptyRegionType>();
		public int simulations;
	}
}
