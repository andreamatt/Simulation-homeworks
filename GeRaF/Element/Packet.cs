using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GeRaF
{
	[JsonConverter(typeof(EnumJsonConverter))]
	enum Result
	{
		None,
		No_start_relays,
		Success,
		Abort_max_attempts,
		Abort_channel_busy
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
		private Result result = Result.None;
		public Result Result => result;

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
