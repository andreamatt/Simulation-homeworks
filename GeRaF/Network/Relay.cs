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
		public double X;
		public double Y;
		public double range = -1;

		[JsonIgnore]
		public HashSet<Relay> neighbours = new HashSet<Relay>();
		public List<int> neighboursIds => neighbours.Select(n => n.id).ToList();

		// for transmitting
		public TransmissionType transmissionType = TransmissionType.RTS;
		public int transmissionDestinationId;

		// for receiving
		// dictionary with [relay, hasFailed]
		private Dictionary<Relay, bool> receivingTransmissions = new Dictionary<Relay, bool>();
		public List<Transmission> finishedCTSs = new List<Transmission>();
		public bool receivedACK = false;

		public Packet packetToSend = null;

		public RelayStatus status = RelayStatus.Free;

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
	}
}
