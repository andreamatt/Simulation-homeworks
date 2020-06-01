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
		No_start_relays,
		//Abort_max_attempts,
		Abort_max_region_cycle,
		Abort_max_sensing,
		Abort_max_sink_rts,
		Abort_no_ack
	}

	class Packet
	{
		static private int max_id = 0;
		private int _id;
		public int Id => _id;
		public double generationTime;

		[JsonIgnore]
		public Relay startRelay;
		public int startRelayId => startRelay == null ? -1 : startRelay.id;

		[JsonIgnore]
		public Relay sink;
		public int sinkId => sink == null ? -1 : sink.id;

		public Result result = Result.None;

		public Packet() {
			_id = max_id;
			max_id++;
		}

		private Packet(int id) {
			_id = id;
		}

		public static Packet copy(Packet packet) {
			return new Packet(packet._id) {
				generationTime = packet.generationTime,
				startRelay = packet.startRelay,
				sink = packet.sink,
				result = packet.result
			};
		}

		public void Finish(Result result, Simulation sim) {
			this.result = result;
			sim.finishedPackets.Add(this);
		}
	}
}
