﻿using GeRaF.Events.DutyCycle;
using GeRaF.Utils;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

namespace GeRaF.Network
{
	[JsonConverter(typeof(EnumJsonConverter))]
	enum RelayStatus
	{
		Asleep,
		Free,   // awake with no task
		Awaiting_region,
		Transmitting,
		Sensing,
		Awaiting_Signal,
		Backoff_Sensing,
		Backoff_CTS,
		Backoff_SinkRTS
	}

	partial class Relay
	{
		[JsonIgnore]
		public Simulation sim;

		public int id = -1;
		public Position position;
		public double range = -1;
		public Position current_aim = new Position();

		//public override bool Equals(object obj) {
		//	return id == (obj as Relay).id;
		//}

		//public override int GetHashCode() {
		//	return id;
		//}

		[JsonIgnore]
		public HashSet<Relay> neighbours = new HashSet<Relay>();
		public List<int> neighboursIds => neighbours.Select(n => n.id).ToList();
		public Dictionary<Relay, Position> directionForSink = new Dictionary<Relay, Position>();

		[JsonIgnore]
		public Queue<int> passedPacketsQueue = new Queue<int>();
		[JsonIgnore]
		public HashSet<int> passedPacketsSet = new HashSet<int>();

		public void OnPacketFinished() {
			if (sim.protocolParameters.avoid_back_flow) {
				if (packetToSend.result == Result.Passed) {
					if (this.passedPacketsQueue.Count > sim.protocolParameters.passed_packets_memory) {
						var toDelete = this.passedPacketsQueue.Dequeue();
						this.passedPacketsSet.Remove(toDelete);
					}
					this.passedPacketsQueue.Enqueue(packetToSend.content_id);
					this.passedPacketsSet.Add(packetToSend.content_id);
				}
			}
		}

		// for transmitting
		public TransmissionType transmissionType = TransmissionType.RTS;
		public int transmissionDestinationId;

		// for receiving
		// dictionary with [relay, hasFailed]
		private Dictionary<Relay, bool> receivingTransmissions = new Dictionary<Relay, bool>();
		public List<Transmission> finishedCTSs = new List<Transmission>();
		public bool receivedACK = false;

		public Packet packetToSend = null;

		public RelayStatus status;

		// iteration status
		public int REGION_index = 0;
		public int COL_count = 0;   // number of collisions in that region during this attempt
		public int SENSE_count = 0;
		public int SINK_RTS_count = 0;
		public int PKT_count = 0;
		public int REGION_cycle = 0;

		// sensing status
		public bool isSensing = false;
		public bool hasSensed = false;

		public string ToFrameString() {
			var res = "";
			res += $"{id}|{(int)status}|{(ShouldBeAwake ? 1 : 0)}";
			if (status != RelayStatus.Asleep && status != RelayStatus.Free && (status == RelayStatus.Transmitting || packetToSend != null)) {
				res += $"|{(int)transmissionType}|{transmissionDestinationId}";
				if (packetToSend != null) {
					res += $"|{REGION_index}|{COL_count}|{SENSE_count}|{SINK_RTS_count}";
					res += $"|{PKT_count}|{REGION_cycle}|{BusyWithId}";
					res += $"|{packetToSend.content_id}|{packetToSend.copy_id}";
					res += $"|{current_aim.X}|{current_aim.Y}";
				}
			}
			return res;
		}
	}
}
