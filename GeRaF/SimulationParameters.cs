using GeRaF.Utils;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GeRaF
{
	public enum DebugType
	{
		Never,
		End,
		Interval,
		Always
	}

	[JsonConverter(typeof(EnumJsonConverter))]
	public enum EmptyRegionType
	{
		None,
		Circle,
		Square,
		Lines,
		Holes
	}

	class SimulationParameters : ICloneable
	{
		public double max_time;
		public int area_side;
		public double range;
		public double min_distance;
		public EmptyRegionType emptyRegionType;
		public double emptyRegionSize;
		public int n_nodes;
		public double packet_rate;
		public bool skipCycleEvents;
		public double debug_interval;
		public DebugType debugType;
		public string debug_file;
		public int binsCount => (int)Math.Floor(area_side / min_distance) - 1;

		public object Clone() {
			return MemberwiseClone();
		}
	}
}
