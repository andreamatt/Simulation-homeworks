using GeRaF.Utils;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GeRaF.Network
{
	[JsonConverter(typeof(EnumJsonConverter))]
	enum Result
	{
		None,
		Success,
		Passed,
		No_start_relays,
		//Abort_max_attempts,
		Abort_max_region_cycle,
		Abort_max_sensing,
		Abort_max_sink_rts,
		Abort_no_ack
	}

	class Packet
	{
		public int content_id = 0;
		public int copy_id;
		public double generationTime;

		[JsonIgnore]
		public Relay startRelay;
		public int startRelayId => startRelay == null ? -1 : startRelay.id;

		[JsonIgnore]
		public Relay sink;
		public int sinkId => sink == null ? -1 : sink.id;

		public List<int> hopsIds = new List<int>();
		public List<double> receivedTimes = new List<double>();

		public Result result = Result.None;

		private Packet() { }

		public Packet(PacketGenerator gen) {
			content_id = gen.next_content_id;
			gen.next_content_id++;
			copy_id = 0;
			gen.next_copy_id[content_id] = 1;
		}

		public static Packet copy(Packet packet, PacketGenerator gen) {
			var p = new Packet() {
				content_id = packet.content_id,
				generationTime = packet.generationTime,
				startRelay = packet.startRelay,
				sink = packet.sink,
				result = Result.None,
				copy_id = gen.next_copy_id[packet.content_id],
				hopsIds = packet.hopsIds.ToList(),
				receivedTimes = packet.receivedTimes.ToList()
			};
			gen.next_copy_id[p.content_id]++;
			return p;
		}

		public void Finish(Result result, Simulation sim) {
			this.result = result;
			sim.packetsFinished.Add(this);
		}

		public override string ToString() {
			return $"{content_id}|{copy_id}|{generationTime}|{startRelayId}|{sinkId}|{String.Join(",", hopsIds)}|{String.Join(",", receivedTimes)}|{(int)result}";
		}
	}

	class PacketGenerator
	{
		public int next_content_id = 0;
		public Dictionary<int, int> next_copy_id = new Dictionary<int, int>();
	}
}
