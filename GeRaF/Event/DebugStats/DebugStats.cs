using Newtonsoft.Json;
using System.Collections.Generic;

namespace GeRaF
{
	class DebugStats
	{
		public double time;
		public List<Relay> relays;
		public List<Event> events;
		[JsonIgnore]
		public List<Packet> finishedPackets;
	}
}