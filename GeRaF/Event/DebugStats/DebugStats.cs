﻿using Newtonsoft.Json;
using System.Collections.Generic;

namespace GeRaF
{
	class DebugStats
	{
		public static bool first = true;
		public double time;
		public List<Event> events;
		public List<Relay> relays;
		public List<Packet> finishedPackets;
	}
}