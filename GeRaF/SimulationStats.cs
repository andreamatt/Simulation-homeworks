using GeRaF.Network;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GeRaF
{
	class RelayInfo
	{
		public int id;
		public double X;
		public double Y;
		public double awakeTime;
		public double asleepTime;

		public override string ToString() {
			return $"{id}|{X}|{Y}|{awakeTime}|{asleepTime}";
		}
	}

	class SimulationStats
	{
		public string relayInfos;
		public string finishedPackets;
	}
}
