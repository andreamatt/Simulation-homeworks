using Priority_Queue;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GeRaF
{
    public enum EventType
    {
        Start = 0,
        End = 1,
        Debug = 2,
        // start events: 1. set carrier busy 2. set collisions on active transmissions 3. add to active transmissions CHECK SENDER STATUS TOO
        // end events: 1. set carrier to free 2. move to finished transmission/delete (CTSs are just moved, others deleted)
        SENSE_start = 3,  // start sensing
        SENSE_end = 4,  // check that last active pulse is older than relative sense_start. If free, RTS_start. if busy, backoff to SENSE_start
        RTS_start = 5,  // node, time, region index. 1.select active nodes in that area and add to scheduled RTS_end (keep them awake)
        RTS_end = 6,  // node, time, region index, receiving nodes. 1.schedule CTS_start for selected nodes 2.schedule COL_check in cts_time time (plus some delta to be sure)
        CTS_start = 7,  // :
        CTS_end = 8,  // :
        COL_check = 9,
        // 0. no response => next region
        // source, sink, node, ...: 1. no collision => PKT_start with first CTS sender; any collision => COL_start OR (next_region and RTS_start) depending on collision count
        // modified 1. at least 1 non-overlapping => PKT_start with first CTS sender; all overlapping => COL_start
        // modified 2. select between non-overlapping using probabilities of success
        // modified 3. no collision => PKT_start with probabilities; any collision => COL_start
        // modified 4. send collision message only for collided CTSs (others stay silent)
        COL_start = 10,
        COL_end = 11,  // ... : .. schedule COL_check in max_backoff time. 2.schedule CTS_start for available nodes with backoff
        PKT_start = 12,  // nodes not selected can go to sleep, schedule ACK_check
        PKT_end = 13,
        //if packet has no collision schedule ACK_start, else stay
        ACK_start = 14,
        ACK_end = 15,
        ACK_check = 16  // if no collision, finished. Else, schedule PKT_start again
    }

    abstract class Event : FastPriorityQueueNode
    {
        public double time;
        //public EventType type;

        public abstract void Handle(Simulation sim);
    }

    class StartEvent : Event
    {
        public StartEvent() {
            time = 0;
        }

        public override void Handle(Simulation sim) {
            // add initial packet arrival
            var p = new PacketGenerationEvent();
            p.time = RNG.rand_expon(sim.packet_rate);
            sim.eventQueue.Add(p);
        }
    }

    class EndEvent : Event
    {
        public EndEvent(double max_time) {
            time = max_time;
        }

        public override void Handle(Simulation sim) {
            // dump debugstats to file???
            // clear event queue
            sim.eventQueue.Clear();
            return;
        }
    }

    class PacketGenerationEvent : Event
    {
        public override void Handle(Simulation sim) {
            // schedule next packet arrival
            var p = new PacketGenerationEvent();
            p.time = sim.clock + RNG.rand_expon(sim.packet_rate);
            sim.eventQueue.Add(p);

            // give this packet to some relay or discard it if none are available
            var freeRelays = sim.relays.Where(r => r.awake && !r.contending).ToList();
            // no nodes available???
            var chosen = freeRelays[RNG.rand_int(0, freeRelays.Count)];

            // choose random sink
            var otherRelays = sim.relays.Where(r => r != chosen).ToList();
            var sink = otherRelays[RNG.rand_int(0, otherRelays.Count)];

            var packet = new Packet();
            packet.generationTime = sim.clock;
            packet.startRelay = chosen;
            packet.sink = sink;
            chosen.packetToSend = packet;

            // schedule startSensingEvent
            var sensing = new StartSensingEvent();
            sensing.time = sim.clock;
            sensing.relay = chosen;
            sim.eventQueue.Add(sensing);
        }
    }

    class StartSensingEvent : Event
    {
        public Relay relay;
        public override void Handle(Simulation sim) {
            // enable sensing on relay
            relay.isSensing = true;
            //relay.hasSensed = false;

            // check if sensingCount is less than max allowed ???
            // schedule sensing end
            var end = new EndSensingEvent();
            end.relay = relay;
            end.time = sim.clock + sim.protocolParameters.t_sense;
            sim.eventQueue.Add(end);
        }
    }

    class EndSensingEvent : Event
    {
        public Relay relay;
        public override void Handle(Simulation sim) {
            if (relay.hasSensed) {
                // reschedule with linear backoff
                var backOffSize = sim.protocolParameters.t_backoff;
                var start = new StartSensingEvent();
                start.time = sim.clock + backOffSize * RNG.rand;
                start.relay = relay;
                sim.eventQueue.Add(start);

                // increase sensing count
                relay.SENSE_count++;
            }
            else {
                // schedule RTS_start
                var RTS_start = new StartRTSEvent();
                RTS_start.time = sim.clock;
                RTS_start.relay = relay;
                sim.eventQueue.Add(RTS_start);

                // set sensing count to 0
                relay.SENSE_count = 0;
            }
            relay.isSensing = false;
            relay.hasSensed = false;
        }
    }

    class StartRTSEvent : Event
    {
        public Relay relay;

        public override void Handle(Simulation sim) {
            var transmissions = new List<Transmission>();
            // search awake nodes
            foreach (var n in relay.neighbours) {
                // add transmission to that node (even asleep ones, in case they wake up)
                var t = new Transmission();
                transmissions.Add(t);
                t.source = relay;
                t.destination = n;
                t.transmissionType = TransmissionType.RTS;
                if (n.activeTransmissions.Count > 0) {
                    // set other active transmissions to failed
                    foreach (var active_t in n.activeTransmissions) {
                        active_t.failed = true;
                    }
                    t.failed = true;
                }
                n.activeTransmissions.Add(t);
                relay.activeTransmissions.Add(t);

                if (n.awake) {
                    // if they are sensing, set to sensed
                    if (n.isSensing) {
                        n.hasSensed = true;
                    }
                }
            }

            // schedule RTS_end
            var end = new EndRTSEvent();
            end.relay = relay;
            end.transmissions = transmissions;
            end.time = sim.clock + sim.protocolParameters.t_signal;
            sim.eventQueue.Add(end);
        }
    }

    class EndRTSEvent : Event
    {
        public Relay relay;
        public List<Transmission> transmissions;
        public override void Handle(Simulation sim) {
            var sink = relay.packetToSend.sink;
            var sourceToSink = sim.distances[relay.id][sink.id];
            var limitToSink = sourceToSink - relay.range;
            var regionWidth = relay.range / sim.protocolParameters.n_regions;
            var minDistance = limitToSink + regionWidth * relay.regionIndex;
            var maxDistance = minDistance + regionWidth;

            foreach (var t in transmissions) {
                var n = t.destination;
                if (n.awake) {
                    // if they are sensing, set to sensed
                    if (n.isSensing) {
                        n.hasSensed = true;
                    }

                    // if neigh relay is in correct region AND transmission has not failed AND is not in another contention, schedule CTS
                    var dist = sim.distances[n.id][sink.id];
                    if (dist > minDistance && dist < maxDistance && t.failed == false && n.contending == false) {
                        n.contending = true; // also when EndCOLEvent. Remember to reset when packetStart arrives ???
                        var CTS_start = new StartCTSEvent();
                        CTS_start.relay = n;
                        CTS_start.requester = relay;
                        CTS_start.time = sim.clock;
                        sim.eventQueue.Add(CTS_start);
                    }
                }
                // delete transmission
                n.activeTransmissions.Remove(t);
                relay.activeTransmissions.Remove(t);
            }

            // schedule COL_check
            var COL_check = new CheckCOLEvent();
            COL_check.relay = relay;
            // time + CTS_time + time delta to make sure CTS events come before COL check
            COL_check.time = sim.clock + sim.protocolParameters.t_signal + sim.protocolParameters.t_delta;
            sim.eventQueue.Add(COL_check);
        }
    }

    class StartCTSEvent : Event
    {
        public Relay relay;
        public Relay requester;
        public override void Handle(Simulation sim) {
            var transmissions = new List<Transmission>();
            // search awake nodes
            foreach (var n in relay.neighbours) {
                // add transmission to that node (even asleep ones, in case they wake up)
                var t = new Transmission();
                transmissions.Add(t);
                t.source = relay;
                t.destination = n;
                t.transmissionType = TransmissionType.CTS;
                if (n.activeTransmissions.Count > 0) {
                    // set other active transmissions to failed
                    foreach (var active_t in n.activeTransmissions) {
                        active_t.failed = true;
                    }
                    t.failed = true;
                }
                n.activeTransmissions.Add(t);
                relay.activeTransmissions.Add(t);

                if (n.awake) {
                    // if they are sensing, set to sensed
                    if (n.isSensing) {
                        n.hasSensed = true;
                    }
                }
            }

            // schedule CTS_end
            var end = new EndCTSEvent();
            end.relay = relay;
            end.requester = requester;
            end.transmissions = transmissions;
            end.time = sim.clock + sim.protocolParameters.t_signal;
            sim.eventQueue.Add(end);
        }
    }

    class EndCTSEvent : Event
    {
        public Relay relay;
        public Relay requester;
        public List<Transmission> transmissions;
        public override void Handle(Simulation sim) {
            foreach (var t in transmissions) {
                var n = t.destination;
                if (n.awake) {
                    // if they are sensing, set to sensed
                    if (n.isSensing) {
                        n.hasSensed = true;
                    }
                }

                // remove transmission
                n.activeTransmissions.Remove(t);
                relay.activeTransmissions.Remove(t);
                if (n == requester) {
                    // move cts to finished
                    n.finishedTransmissions.Add(t);
                }
            }
        }
    }

    class CheckCOLEvent : Event
    {
        public Relay relay;
        public override void Handle(Simulation sim) {

        }
    }

    class DebugEvent : Event
    {

    }

    class TransmissionEvent : Event
    {

    }
}
