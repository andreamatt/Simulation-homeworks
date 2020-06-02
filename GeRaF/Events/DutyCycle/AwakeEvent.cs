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

		public override void Handle() {
			relay.Awake(this);
		}
	}
}
