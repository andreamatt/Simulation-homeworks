using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

namespace GeRaF
{
    class Relay
    {
        public int id = -1;
        public Vector2 location = Vector2.Zero;
        public double range = -1;
        public List<Relay> neighbours = new List<Relay>();
        public bool awake = true;
        public bool contending = false; // è in contesa per un pacchetto
        public HashSet<Transmission> activeTransmissions = new HashSet<Transmission>();
        public HashSet<Transmission> finishedTransmissions = new HashSet<Transmission>();
        public Packet packetToSend = null;

        // iteration status
        public int regionIndex = 0;
        public int COL_count = 0;
        public int SENSE_count = 0;

        // sensing status
        public bool isSensing = false;
        public bool hasSensed = false;
    }
}
