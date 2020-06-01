using GeRaF.Network;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GeRaF.Events.DutyCycle
{
	class AwakeEvent : Event
	{
		public Relay relay;

		public override void Handle(Simulation sim) {
			// set relay as free, schedule sleep, set awake_time (used in FreeEvent)
		}
	}
}
