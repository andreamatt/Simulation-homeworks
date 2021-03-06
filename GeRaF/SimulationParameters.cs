﻿using GeRaF.Utils;
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
		Square,
		Holes,
		Lines,
		Cross
	}

	class SimulationParameters : ICloneable
	{
		public double max_time;
		public int area_side;
		public float range;
		public float min_distance;
		public EmptyRegionType emptyRegionType;
		public float emptyRegionSize;
		public int n_nodes;
		public float n_density;
		public double packet_rate;
		public double asleepEnergy;
		public double idleEnergy;
		public double transmissionEnergy;
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
