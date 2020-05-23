using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GeRaF
{
	[JsonConverter(typeof(EnumJsonConverter))]
	enum TransmissionType
	{
		SINK_RTS,
		RTS,
		CTS,
		PKT,
		COL,
		ACK
	}

	class Transmission
	{
		[JsonIgnore]
		static private int max_id = 0;

		[JsonIgnore]
		private int _id;
		public int Id => _id;

		[JsonIgnore]
		public Relay source;
		public int sourceId => source.id;

		[JsonIgnore]
		public Relay destination;
		public int destinationId => destination.id;

		public TransmissionType transmissionType;
		public bool failed;
		public bool actualDestination;

		public Transmission() {
			_id = max_id;
			max_id++;
		}
	}
}
