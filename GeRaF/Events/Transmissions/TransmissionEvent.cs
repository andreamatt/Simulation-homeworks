using GeRaF.Events;
using GeRaF.Network;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GeRaF.Events.Transmissions
{
	abstract class TransmissionEvent : Event
	{
		[JsonIgnore]
		public Relay relay;
		public int relayId => relay.id;

		[JsonIgnore]
		public Relay actualDestination;
		public int actualDestinationId => actualDestination == null ? -1 : actualDestination.id;

		protected void StartTransmission() {
			relay.transmissionDestinationId = actualDestinationId;
			foreach (var neigh in relay.neighbours) {
				neigh.StartReceiving(relay);
			}
		}

		protected void EndTransmission() {
			foreach (var neigh in relay.neighbours) {
				neigh.EndReceiving(this, relay.transmissionType, relay, actualDestination);
			}
		}
	}
}
