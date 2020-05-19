using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GeRaF
{
	class DebugEvent : Event
	{
		public override void Handle(Simulation sim) {
			DebugNow(sim);

			// schedule next debug
			var dbgEvent = new DebugEvent();
			dbgEvent.time = sim.clock + sim.simulationParameters.debug_interval;
			sim.eventQueue.Add(dbgEvent);
		}

		public static void DebugNow(Simulation sim) {
			// dump simulation to file
			var stats = new DebugStats() {
				time = sim.clock,
				events = sim.eventQueue.ToList(),
				relays = sim.relays,
				finishedPackets = sim.finishedPackets
			};

			var stats_to_string = JsonConvert.SerializeObject(stats, Formatting.Indented);
			if (DebugStats.first) {
				DebugStats.first = false;
				sim.debugWriter.Write(stats_to_string);
			}
			else {
				sim.debugWriter.Write(",\n" + stats_to_string);
			}
		}
	}
}
