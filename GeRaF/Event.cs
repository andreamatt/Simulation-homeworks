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
        SINK_RTS_start,
        SINK_RTS_end,
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

    abstract class Event
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
            // dump debugstats to file
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
            var freeRelays = sim.relays.Where(r => r.awake && r.BusyWith == null).ToList();
            if (freeRelays.Count > 0) {
                var chosen = freeRelays[RNG.rand_int(0, freeRelays.Count)];

                // choose random sink
                var otherRelays = sim.relays.Where(r => r != chosen).ToList();
                var sink = otherRelays[RNG.rand_int(0, otherRelays.Count)];

                var packet = new Packet();
                packet.generationTime = sim.clock;
                packet.startRelay = chosen;
                packet.sink = sink;
                chosen.packetToSend = packet;
                chosen.SelfReserve();

                // schedule startSensingEvent
                var sensing = new StartSensingEvent();
                sensing.time = sim.clock;
                sensing.relay = chosen;
                sim.eventQueue.Add(sensing);
            }
            // no nodes available
            else {
                var packet = new Packet();
                packet.generationTime = sim.clock;
                packet.Finish(Result.No_start_relays, sim);
            }
        }
    }

    class StartSensingEvent : Event
    {
        public Relay relay;
        public override void Handle(Simulation sim) {
            // check if sensingCount is less than max allowed
            if (relay.SENSE_count < sim.protocolParameters.n_max_sensing) {
                relay.SENSE_count++;
                // enable sensing on relay
                relay.isSensing = true;
                //relay.hasSensed = false;

                // schedule sensing end
                var end = new EndSensingEvent();
                end.relay = relay;
                end.time = sim.clock + sim.protocolParameters.t_sense;
                sim.eventQueue.Add(end);
            }
            else {
                // finish and free
                relay.packetToSend.Finish(Result.Abort_channel_busy, sim);
                relay.FreeNow(sim);
            }
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
                start.time = sim.clock + backOffSize * RNG.rand();
                start.relay = relay;
                sim.eventQueue.Add(start);
            }
            else {
                // set sensing count to 0
                relay.SENSE_count = 0;

                // check if sink is in range
                if (sim.distances[relay.id][relay.packetToSend.sink.id] < relay.range) {
                    var SINK_RTS_start = new StartSINKRTSEvent();
                    SINK_RTS_start.time = sim.clock;
                    SINK_RTS_start.relay = relay;
                    sim.eventQueue.Add(SINK_RTS_start);
                }
                else {
                    // schedule RTS_start
                    var RTS_start = new StartRTSEvent();
                    RTS_start.time = sim.clock;
                    RTS_start.relay = relay;
                    sim.eventQueue.Add(RTS_start);
                }
            }
            relay.isSensing = false;
            relay.hasSensed = false;
        }
    }

    abstract class SensingTriggerEvent : Event
    {
        public Relay relay;
        protected void triggerNeighbourSensing() {
            foreach (var n in relay.neighbours) {
                if (n.awake) {
                    // if they are sensing, set to sensed
                    if (n.isSensing) {
                        n.hasSensed = true;
                    }
                }
            }
        }
    }

    abstract class StartTransmissionEvent : SensingTriggerEvent
    {
        protected List<Transmission> sendTransmissions(TransmissionType type) {
            var transmissions = new List<Transmission>();
            // search awake nodes
            foreach (var n in relay.neighbours) {
                // add transmission to that node (even asleep ones, in case they wake up)
                var t = new Transmission();
                transmissions.Add(t);
                t.source = relay;
                t.destination = n;
                t.transmissionType = type;
                if (n.activeTransmissions.Count > 0) {
                    // set other active transmissions to failed
                    foreach (var active_t in n.activeTransmissions) {
                        active_t.failed = true;
                    }
                    t.failed = true;
                }
                n.activeTransmissions.Add(t);
                relay.activeTransmissions.Add(t);
            }

            return transmissions;
        }
    }

    abstract class EndTransmissionEvent : SensingTriggerEvent
    {
        public List<Transmission> transmissions;
    }

    class FreeRelayEvent : Event
    {
        public Relay relay;
        public override void Handle(Simulation sim) {
            relay.Free();
        }
    }

    class StartSINKRTSEvent : StartTransmissionEvent
    {
        public override void Handle(Simulation sim) {
            triggerNeighbourSensing();
            var transmissions = sendTransmissions(TransmissionType.SINK_RTS);

            // schedule SINK_RTS_end
            var end = new EndSINKRTSEvent();
            end.relay = relay;
            end.transmissions = transmissions;
            end.time = sim.clock + sim.protocolParameters.t_signal;
            sim.eventQueue.Add(end);
        }
    }

    class EndSINKRTSEvent : EndTransmissionEvent
    {
        public override void Handle(Simulation sim) {
            triggerNeighbourSensing();

            var sink = relay.packetToSend.sink;
            foreach (var t in transmissions) {
                var n = t.destination;
                if (n.awake && n == sink && n.BusyWith == null) {
                    n.Reserve(relay, sim);
                    var CTS_start = new StartCTSEvent();
                    CTS_start.relay = n;
                    CTS_start.requester = relay;
                    CTS_start.time = sim.clock;
                    sim.eventQueue.Add(CTS_start);
                }
                // delete transmission
                n.activeTransmissions.Remove(t);
                relay.activeTransmissions.Remove(t);
            }

            // schedule COL_check
            var COL_check = new CheckSINKCOLEvent();
            COL_check.relay = relay;
            // time + CTS_time + time delta to make sure CTS events come before COL check
            COL_check.time = sim.clock + sim.protocolParameters.t_signal + sim.protocolParameters.t_delta;
            sim.eventQueue.Add(COL_check);
        }
    }

    class StartRTSEvent : StartTransmissionEvent
    {

        public override void Handle(Simulation sim) {
            triggerNeighbourSensing();
            var transmissions = sendTransmissions(TransmissionType.RTS);

            // schedule RTS_end
            var end = new EndRTSEvent();
            end.relay = relay;
            end.transmissions = transmissions;
            end.time = sim.clock + sim.protocolParameters.t_signal;
            sim.eventQueue.Add(end);
        }
    }

    class EndRTSEvent : EndTransmissionEvent
    {
        public override void Handle(Simulation sim) {
            triggerNeighbourSensing();

            var sink = relay.packetToSend.sink;
            var sourceToSink = sim.distances[relay.id][sink.id];
            var limitToSink = sourceToSink - relay.range;
            var regionWidth = relay.range / sim.protocolParameters.n_regions;
            var minDistance = limitToSink + regionWidth * relay.regionIndex;
            var maxDistance = minDistance + regionWidth;

            foreach (var t in transmissions) {
                var n = t.destination;
                if (n.awake) {
                    // if neigh relay is in correct region AND transmission has not failed AND is not in another contention, schedule CTS
                    var dist = sim.distances[n.id][sink.id];
                    if (dist > minDistance && dist < maxDistance && t.failed == false && n.BusyWith == null) {
                        n.Reserve(relay, sim);
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

    class StartCTSEvent : StartTransmissionEvent
    {
        public Relay requester;
        public override void Handle(Simulation sim) {
            triggerNeighbourSensing();
            var transmissions = sendTransmissions(TransmissionType.CTS);

            // schedule CTS_end
            var end = new EndCTSEvent();
            end.relay = relay;
            end.requester = requester;
            end.transmissions = transmissions;
            end.time = sim.clock + sim.protocolParameters.t_signal;
            sim.eventQueue.Add(end);
        }
    }

    class EndCTSEvent : EndTransmissionEvent
    {
        public Relay requester;
        public override void Handle(Simulation sim) {
            triggerNeighbourSensing();
            foreach (var t in transmissions) {
                var n = t.destination;
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

    class CheckSINKCOLEvent : Event
    {
        public Relay relay;
        public override void Handle(Simulation sim) {
            // no CTS or CTS failed => back to sensing
            if (relay.finishedTransmissions.Count == 0 || relay.finishedTransmissions.First().failed) {
                var sense = new StartSensingEvent();
                sense.time = sim.clock;
                sense.relay = relay;
                sim.eventQueue.Add(sense);
            }
            // only sink replied successfully
            else {
                // send packet to sink
                var chosen = relay.packetToSend.sink;
                var PKT_start = new StartPKTEvent();
                PKT_start.time = sim.clock;
                PKT_start.relay = relay;
                PKT_start.chosenRelay = chosen;
                sim.eventQueue.Add(PKT_start);
            }

            // clear finished
            relay.finishedTransmissions.Clear();
        }
    }

    class CheckCOLEvent : Event
    {
        public Relay relay;
        public override void Handle(Simulation sim) {
            // check CTS amount
            if (relay.finishedTransmissions.Count == 0) {
                var regionChange = new RegionProgressEvent();
                regionChange.time = sim.clock;
                regionChange.relay = relay;
                sim.eventQueue.Add(regionChange);
            }
            else {
                // if any have failed send COL or go to next region
                if (relay.finishedTransmissions.Any(t => t.failed)) {
                    if (relay.COL_count < sim.protocolParameters.n_max_coll) {
                        var COL_start = new StartCOLEvent();
                        COL_start.time = sim.clock;
                        COL_start.relay = relay;
                        sim.eventQueue.Add(COL_start);
                        relay.COL_count++;
                    }
                    else {
                        var regionChange = new RegionProgressEvent();
                        regionChange.time = sim.clock;
                        regionChange.relay = relay;
                        sim.eventQueue.Add(regionChange);
                    }
                }
                // else choose relay and send packet
                else {
                    // choose random relay (choose first one, they are already in random order because of backoff (multiple CTSs without coll implies backoff))
                    var chosen = relay.finishedTransmissions.First().source;
                    var PKT_start = new StartPKTEvent();
                    PKT_start.time = sim.clock;
                    PKT_start.relay = relay;
                    PKT_start.chosenRelay = chosen;
                    sim.eventQueue.Add(PKT_start);
                }
            }

            // clear finished
            relay.finishedTransmissions.Clear();
        }
    }

    class StartCOLEvent : StartTransmissionEvent
    {
        public override void Handle(Simulation sim) {
            triggerNeighbourSensing();
            var transmissions = sendTransmissions(TransmissionType.COL);

            // schedule COL_end
            var end = new EndCOLEvent();
            end.relay = relay;
            end.transmissions = transmissions;
            end.time = sim.clock + sim.protocolParameters.t_signal;
            sim.eventQueue.Add(end);
        }
    }

    class EndCOLEvent : EndTransmissionEvent
    {
        public override void Handle(Simulation sim) {
            triggerNeighbourSensing();
            var backoffSize = sim.protocolParameters.t_backoff * Math.Pow(2, relay.COL_count);
            foreach (var t in transmissions) {
                var n = t.destination;
                // remove transmission
                n.activeTransmissions.Remove(t);
                relay.activeTransmissions.Remove(t);
                if (t.failed == false && n.BusyWith == relay) {
                    // update busy
                    n.Reserve(relay, sim);
                    // schedule CTS
                    var CTS_start = new StartCTSEvent();
                    CTS_start.time = sim.clock + backoffSize * RNG.rand();
                    CTS_start.relay = n;
                    CTS_start.requester = relay;
                    sim.eventQueue.Add(CTS_start);
                }
            }

            // schedule COL_check
            var COL_check = new CheckCOLEvent();
            COL_check.time = sim.clock + backoffSize + sim.protocolParameters.t_signal + sim.protocolParameters.t_delta;
            COL_check.relay = relay;
            sim.eventQueue.Add(COL_check);
        }
    }

    class RegionProgressEvent : Event
    {
        public Relay relay;
        public override void Handle(Simulation sim) {
            // no more regions
            if (relay.regionIndex == sim.protocolParameters.n_regions - 1) {
                if (relay.ATTEMPT_count < sim.protocolParameters.n_max_attempts) {
                    relay.ATTEMPT_count++;
                    relay.regionIndex = 0;
                    // schedule sensing immediately
                    var SENSE_start = new StartSensingEvent();
                    SENSE_start.time = sim.clock;
                    SENSE_start.relay = relay;
                    sim.eventQueue.Add(SENSE_start);
                }
                else {
                    relay.packetToSend.Finish(Result.Abort_max_attempts, sim);
                    relay.FreeNow(sim);
                }
            }
            // go next region
            else {
                relay.regionIndex++;
                // schedule RTS
                var RTS_start = new StartRTSEvent();
                RTS_start.time = sim.clock;
                RTS_start.relay = relay;
                sim.eventQueue.Add(RTS_start);
            }
        }
    }

    class StartPKTEvent : StartTransmissionEvent
    {
        public Relay chosenRelay;

        public override void Handle(Simulation sim) {
            triggerNeighbourSensing();
            var transmissions = sendTransmissions(TransmissionType.PKT);
            // put other interested relays (not chosen) to sleep
            // schedule PKT_end
            var PKT_end = new EndPKTEvent();
            PKT_end.time = sim.clock + sim.protocolParameters.t_data;
            PKT_end.relay = relay;
            PKT_end.chosenRelay = chosenRelay;
            PKT_end.transmissions = transmissions;
            sim.eventQueue.Add(PKT_end);
        }
    }

    class EndPKTEvent : EndTransmissionEvent
    {
        public Relay chosenRelay;

        public override void Handle(Simulation sim) {
            triggerNeighbourSensing();
            foreach (var t in transmissions) {
                var n = t.destination;
                n.activeTransmissions.Remove(t);
                relay.activeTransmissions.Remove(t);
                if (t.failed == false && n.BusyWith == relay) {
                    // if chosen one, schedule ACK
                    if (n == chosenRelay) {
                        // update busy
                        n.Reserve(relay, sim);
                        var ACK_start = new StartACKEvent();
                        ACK_start.time = sim.clock;
                        ACK_start.relay = n;
                        ACK_start.senderRelay = relay;
                        sim.eventQueue.Add(ACK_start);
                    }
                    else {
                        n.FreeNow(sim);
                    }
                }
            }

            // increase PKT attempts
            relay.ACK_count++;

            // schedule ACK_check
            var ACK_check = new CheckACKEvent();
            ACK_check.time = sim.clock + sim.protocolParameters.t_signal + sim.protocolParameters.t_delta;
            ACK_check.relay = relay;
            ACK_check.chosenRelay = chosenRelay;
            sim.eventQueue.Add(ACK_check);
        }
    }

    class StartACKEvent : StartTransmissionEvent
    {
        public Relay senderRelay;
        public override void Handle(Simulation sim) {
            triggerNeighbourSensing();
            var transmissions = sendTransmissions(TransmissionType.ACK);
            // schedule ACK_end
            var ACK_end = new EndACKEvent();
            ACK_end.time = sim.clock + sim.protocolParameters.t_signal;
            ACK_end.relay = relay;
            ACK_end.senderRelay = senderRelay;
            ACK_end.transmissions = transmissions;
            sim.eventQueue.Add(ACK_end);
        }
    }

    class EndACKEvent : EndTransmissionEvent
    {
        public Relay senderRelay;
        public override void Handle(Simulation sim) {
            triggerNeighbourSensing();
            foreach (var t in transmissions) {
                var n = t.destination;
                n.activeTransmissions.Remove(t);
                relay.activeTransmissions.Remove(t);
                if (n == senderRelay && t.failed == false) {
                    n.finishedTransmissions.Add(t);
                }
            }

            // free relay
            relay.FreeNow(sim);

            // copy packet
            relay.packetToSend = Packet.copy(senderRelay.packetToSend);

            // if reached sink, stop
            if (relay.packetToSend.sink == senderRelay) {
                relay.packetToSend.Finish(Result.Success, sim);
            }
            else {
                relay.FreeNow(sim);
                relay.SelfReserve();
                // schedule SENSE_Start
                var SENSE_start = new StartSensingEvent();
                SENSE_start.time = sim.clock;
                SENSE_start.relay = relay;
                sim.eventQueue.Add(SENSE_start);
            }
        }
    }

    class CheckACKEvent : Event
    {
        public Relay relay;
        public Relay chosenRelay;
        public override void Handle(Simulation sim) {
            if (relay.finishedTransmissions.Count == 1) {
                relay.FreeNow(sim);
            }
            else {
                // check ack count and resend
                if (relay.ACK_count < sim.protocolParameters.n_max_ack) {
                    // schedule PKT_start again
                    var PKT_start = new StartPKTEvent();
                    PKT_start.time = sim.clock;
                    PKT_start.relay = relay;
                    PKT_start.chosenRelay = chosenRelay;
                    sim.eventQueue.Add(PKT_start);
                }
                else {
                    relay.ACK_count = 0;
                    if (relay.COL_count < sim.protocolParameters.n_max_coll) {
                        var COL_start = new StartCOLEvent();
                        COL_start.time = sim.clock;
                        COL_start.relay = relay;
                        sim.eventQueue.Add(COL_start);
                        relay.COL_count++;
                    }
                    else {
                        // if sink in range, go back to sensing
                        if (sim.distances[relay.id][relay.packetToSend.sink.id] < relay.range) {
                            var sense = new StartSensingEvent();
                            sense.time = sim.clock;
                            sense.relay = relay;
                            sim.eventQueue.Add(sense);
                        }
                        else {
                            // try going to next region
                            var regionChange = new RegionProgressEvent();
                            regionChange.time = sim.clock;
                            regionChange.relay = relay;
                            sim.eventQueue.Add(regionChange);
                        }
                    }
                }
            }
            relay.finishedTransmissions.Clear();
        }
    }

    //class DebugEvent : Event
    //{

    //}
}
