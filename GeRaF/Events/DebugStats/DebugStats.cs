using GeRaF.Network;
using Newtonsoft.Json;
using System.Collections.Generic;
using System.Linq;

namespace GeRaF.Events.DebugStats
{
	class DebugStats
	{
		public double time;
		public Event currentEvent;
		public List<Relay> relays;
		public List<Packet> finishedPackets;

		public string ConvertToFrameLine() {
			var line = $"{time};{JsonConvert.SerializeObject(currentEvent)}";
			foreach (var r in relays) {
				line += ";" + r.ToFrameString();
			}
			return line;
		}
	}
}