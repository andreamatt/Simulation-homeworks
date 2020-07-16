using GeRaF.Events.DutyCycle;
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
		public Dictionary<Relay, int> successesFromNeighbour = new Dictionary<Relay, int>();
		public Dictionary<Relay, int> failuresFromNeighbour = new Dictionary<Relay, int>();
		public Dictionary<Relay, float> markovFromNeighbour = new Dictionary<Relay, float>();

		public void OnPacketFinished() {
			if (packetToSend.hopsIds.Count > 1) {
				var previousRelayId = packetToSend.hopsIds[packetToSend.hopsIds.Count - 2];
				var previousRelay = sim.relayById[previousRelayId];
				var val = markovFromNeighbour[previousRelay];
				switch (packetToSend.result) {
					case Result.None:
						break;
					case Result.Success:
						break;
					case Result.Passed:
						successesFromNeighbour[previousRelay]++;
						markovFromNeighbour[previousRelay] = val + (1 - val) / 2;
						break;
					default:
						failuresFromNeighbour[previousRelay]++;
						markovFromNeighbour[previousRelay] = val / 2;
						break;
				}
			}
		}

		public void UpdateMarkov() {
			if (packetToSend.hopsIds.Count > 1) {
				var previousRelayId = packetToSend.hopsIds[packetToSend.hopsIds.Count - 2];
				var previousRelay = sim.relayById[previousRelayId];

				var ctsSenders = finishedCTSs.Select(t => t.source);
				var markovValues = ctsSenders.Select(r => r.markovFromNeighbour[this]);

				var val = markovFromNeighbour[previousRelay];
				markovFromNeighbour[previousRelay] = (val + markovValues.Average()) / 2;
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

		public string ToFrameString() {
			var res = "";
			res += $"{id}|{(int)status}|{(ShouldBeAwake ? 1 : 0)}|{string.Join("_", markovFromNeighbour.Select(m => $"{m.Key.id}:{m.Value}"))}";
			if (status != RelayStatus.Asleep && status != RelayStatus.Free && (status == RelayStatus.Transmitting || packetToSend != null)) {
				res += $"|{(int)transmissionType}|{transmissionDestinationId}";
				if (packetToSend != null) {
					res += $"|{REGION_index}|{COL_count}|{SENSE_count}|{SINK_RTS_count}";
					res += $"|{PKT_count}|{REGION_cycle}|{BusyWithId}";
					res += $"|{packetToSend.content_id}|{packetToSend.copy_id}";
				}
			}
			return res;
		}
	}
}
