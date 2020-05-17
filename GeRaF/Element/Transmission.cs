using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GeRaF
{
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
		static private int max_id = 0;
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

		public Transmission() {
			_id = max_id;
			max_id++;
		}
	}
}
