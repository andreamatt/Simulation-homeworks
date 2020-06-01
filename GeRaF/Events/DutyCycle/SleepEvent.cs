﻿using GeRaF.Network;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GeRaF.Events.DutyCycle
{
	class SleepEvent : Event
	{
		public Relay relay;

		public override void Handle(Simulation sim) {
			// how to free if transmissions incoming?
		}
	}
}
