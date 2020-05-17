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
        public double X;
        public double Y;
        public double range = -1;
        public List<Relay> neighbours = new List<Relay>();
        public bool awake = true;
        public HashSet<Transmission> activeTransmissions = new HashSet<Transmission>();
        public HashSet<Transmission> finishedTransmissions = new HashSet<Transmission>();
        public Packet packetToSend = null;

        // iteration status
        public int regionIndex = 0;
        public int COL_count = 0;   // number of collisions in that region during this attempt
        public int SENSE_count = 0;
        public int ATTEMPT_count = 0;
        public int ACK_count = 0;

        // sensing status
        public bool isSensing = false;
        public bool hasSensed = false;

        // contention status
        private Relay busyWith = null; // è in contesa per un pacchetto
        private FreeRelayEvent freeEvent = null;
        public Relay BusyWith => busyWith;

        public void SelfReserve() {
            busyWith = this;
        }

        // called only if relay is either free or already busy with the reserver
        public void Reserve(Relay reserver, Simulation sim) {
            if (reserver == this) {
                throw new Exception("Can't use Reserve on self, use SelfReserve instead");
            }
            else {
                if (busyWith == null) {
                    busyWith = reserver;
                    freeEvent = new FreeRelayEvent();
                    freeEvent.time = sim.clock + sim.protocolParameters.t_busy;
                    freeEvent.relay = this;
                    sim.eventQueue.Add(freeEvent);
                }
                else {
                    freeEvent.time = sim.clock + sim.protocolParameters.t_busy;
                    sim.eventQueue.Reschedule(freeEvent);
                }
            }
        }

        // called by FreeEvent
        public void Free() {
            busyWith = null;
            freeEvent = null;
        }

        public void FreeNow(Simulation sim) {
            sim.eventQueue.Remove(freeEvent);
            Free();
        }
    }
}
