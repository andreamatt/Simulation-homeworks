using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GeRaF
{
	abstract class StartTransmissionEvent : SensingTriggerEvent
	{
		protected List<Transmission> sendTransmissions(TransmissionType type) {
			var transmissions = new List<Transmission>();
			// search awake nodes
			foreach (var n in relay.neighbours) {
				// add transmission to that node (even asleep ones, in case they wake up)
				var t = new Transmission();
				transmissions.Add(t);
				t.source = relay;
				t.destination = n;
				t.transmissionType = type;
				if (n.activeTransmissions.Count > 0) {
					// set other active transmissions to failed
					foreach (var active_t in n.activeTransmissions) {
						active_t.failed = true;
					}
					t.failed = true;
				}
				n.activeTransmissions.Add(t);
				relay.activeTransmissions.Add(t);
			}

			return transmissions;
		}
	}

	abstract class EndTransmissionEvent : SensingTriggerEvent
	{
		public List<Transmission> transmissions;
	}
}
