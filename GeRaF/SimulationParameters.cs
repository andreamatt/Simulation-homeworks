﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GeRaF
{
	public enum DebugType
	{
		Never,
		Interval,
		Always
	}

	class SimulationParameters
	{
		public double max_time;
		public int area_side;
		public double range;
		public double min_distance;
		public int n_nodes;
		public double packet_rate;
		public double debug_interval;
		public DebugType debugType;
		public string debug_file;
	}
}
