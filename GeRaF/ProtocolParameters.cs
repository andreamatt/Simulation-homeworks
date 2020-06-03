using GeRaF.Utils;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GeRaF
{
	[JsonConverter(typeof(EnumJsonConverter))]
	enum ProtocolVersion
	{
		Base,
		Plus,
		Plus_2
	}

	class ProtocolParameters
	{
		public double duty_cycle = 0.1; // NOT IN SECONDS: active time percentage (t_l / (t_l + t_s))
		public double t_sense = 0.0521; // carrier sense duration (needs to be more than any signal duration)
		public double t_backoff = 0.0219; // backoff interval length (constant?)
		public double t_listen = 0.016; // listening time, must be quite higher than t_signal
		public double t_sleep => t_listen * ((1 / duty_cycle) - 1); // 0.144000, sleep time
		public double t_cycle => t_sleep + t_listen;
		public double t_data = 0.0521; // data transmission time
		public double t_signal = 0.00521; // signal packet transmission time (RTS and CTS ?)
		public double t_busy => Math.Pow(2, n_max_coll) * t_backoff + 2 * t_signal + t_delta; // slightly more than the max backoff + cts_time + col_time
		public int n_regions = 4; // number of priority regions
								  //public int n_max_attempts = 50; // number of attempts for searching a relay
		public int n_max_coll = 6; // number of attempts for solving a collision
		public int n_max_sensing = 10;
		public int n_max_sink_rts = 10;
		public int n_max_pkt = 3;
		public int n_max_region_cycle = 5;

		public double t_delta = 0.00001; // small time delta

		public ProtocolVersion protocolVersion = ProtocolVersion.Base;
	}
}
