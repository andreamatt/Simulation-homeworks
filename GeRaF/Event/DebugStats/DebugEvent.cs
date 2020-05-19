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
				finishedPackets = sim.finishedPackets,
				relays = sim.relays
			};
			sim.debugStats.Add(stats);
		}
	}
}
