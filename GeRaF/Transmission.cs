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
        public Relay source;
        public Relay destination;
        public TransmissionType transmissionType;
        public bool failed;
    }
}
