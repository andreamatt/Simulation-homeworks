using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GeRaF
{
    class ProtocolParameters
    {
        public double duty_cycle = 0.1; // NOT IN SECONDS: active time percentage (t_l / (t_l + t_s))
        public double t_sense = 0.0521; // carrier sense duration
        public double t_backoff = 0.219; // backoff interval length (constant?)
        public double t_listen = 0.016; // listening time
        public double t_sleep = 0.144; // t_listen * ((1 / duty_cycle) - 1); // 0.144000, sleep time
        public double t_data = 0.0521; // data transmission time
        public double t_signal = 0.00521; // signal packet transmission time (RTS and CTS ?)
        public int n_regions = 4; // number of priority regions
        public int n_max_attempts = 50; // number of attempts for searching a relay
        public int n_max_coll = 6; // number of attempts for solving a collision
        public int n_max_sensing = 10; // maybe join max_sensing and max_attempts, when all regions fail go back to sensing instead of RTS ???

        public double t_delta = 0.00001; // small time delta
    }
}
