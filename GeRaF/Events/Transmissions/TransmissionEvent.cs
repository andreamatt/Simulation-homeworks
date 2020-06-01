﻿using GeRaF.Network;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GeRaF.Events.Transmissions
{
	abstract class StartTransmissionEvent : Event
	{
		[JsonIgnore]
		public Relay relay;
		public int relayId => relay.id;
		protected List<Transmission> sendTransmissions(TransmissionType type, Relay actualDestination = null) {
			// trigger sensing
			foreach (var n in relay.neighbours) {
				if (n.status == RelayStatus.Sensing) {
					n.hasSensed = true;
				}
			}

			var transmissions = new List<Transmission>();
			// search awake nodes
			foreach (var n in relay.neighbours) {
				if (n.status != RelayStatus.Asleep) {
					// add transmission to that node
					var t = new Transmission();
					transmissions.Add(t);
					t.source = relay;
					t.destination = n;
					t.transmissionType = type;
					if (n == actualDestination) {
						t.actualDestination = true;
					}
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
			}

			return transmissions;
		}
	}

	abstract class EndTransmissionEvent : Event
	{
		[JsonIgnore]
		public Relay relay;
		public int relayId => relay.id;

		[JsonIgnore]
		public List<Transmission> transmissions;
	}
}