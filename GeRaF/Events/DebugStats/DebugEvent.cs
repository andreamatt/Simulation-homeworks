using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GeRaF.Events.DebugStats
{
	class DebugEvent : Event
	{
		public override void Handle(Simulation sim) {
			DebugNow(sim, false);

			// schedule next debug
			var dbgEvent = new DebugEvent();
			dbgEvent.time = sim.clock + sim.simulationParameters.debug_interval;
			sim.eventQueue.Add(dbgEvent);
		}

		public static void DebugNow(Simulation sim, bool ending) {
			// dump simulation to file
			var stats = new DebugStats() {
				time = sim.clock,
				events = sim.eventQueue.ToList(),
				relays = sim.relays
			};

			if (ending) {
				stats.finishedPackets = sim.finishedPackets;
			}

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
