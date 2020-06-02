using GeRaF.Network;
using Newtonsoft.Json;
using System.Collections.Generic;

namespace GeRaF.Events.DebugStats
{
	class DebugStats
	{
		[JsonIgnore]
		public static bool first = true;
		public double time;
		public Event currentEvent;
		public List<Relay> relays;
		public List<Packet> finishedPackets;
	}
}