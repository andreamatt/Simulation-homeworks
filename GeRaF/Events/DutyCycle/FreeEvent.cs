using GeRaF.Network;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GeRaF.Events.DutyCycle
{
	class FreeEvent : Event
	{
		[JsonIgnore]
		public Relay relay;
		public int relayId => relay.id;
		public override void Handle() {
			relay.Free(this);

			// NEW IMPLEMENTATION
			// if time since awake is bigger than dutyCycle awake time, go to sleep
			// else Free, keep sleep schedule

			// !!!!!
			// how to free if transmissions incoming?
		}
	}
}
