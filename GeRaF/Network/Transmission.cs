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
	enum TransmissionType
	{
		SINK_RTS,
		RTS,
		CTS,
		PKT,
		SINK_COL,
		COL,
		ACK
	}

	class Transmission
	{
		[JsonIgnore]
		public Relay source;
		public int sourceId => source.id;

		[JsonIgnore]
		public Relay destination;
		public int destinationId => destination.id;

		public TransmissionType transmissionType;
		public bool failed;
		public bool actualDestination;
	}
}
