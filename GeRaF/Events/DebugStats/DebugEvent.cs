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
		public override void Handle() {
			DebugNow(sim, false, null);

			// schedule next debug
			sim.eventQueue.Add(new DebugEvent {
				time = sim.clock + sim.simulationParameters.debug_interval,
				sim = sim,
				previous = this
			});
		}

		public static void DebugNow(Simulation sim, bool ending, Event current) {
			// dump simulation to file
			var stats = new DebugStats() {
				time = sim.clock,
				currentEvent = current,
				relays = sim.relays
			};

			if (ending) {
				stats.finishedPackets = sim.finishedPackets;
			}

			var stats_to_string = stats.ConvertToFrameLine();
			sim.debugWriter.WriteLine(stats_to_string);
		}
	}
}
