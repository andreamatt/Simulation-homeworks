﻿using GeRaF.Events;
using GeRaF.Events.DutyCycle;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GeRaF.Network
{
	partial class Relay
	{
		// contention status
		[JsonIgnore]
		public Relay busyWith { get; private set; }
		public int BusyWithId => busyWith == null ? -1 : busyWith.id;

		private FreeEvent freeEvent = null;
		private SleepEvent sleepEvent = null;
		private double awakeSince = 0;
		private double lastSleep = 0;
		public double startTransmissionTime = 0;
		public double totalAwake = 0;
		public double totalTransmitting = 0;
		public double totalSleep => sim.clock - totalAwake;

		public bool ShouldBeAwake {
			get {
				var since_sleep = (sim.clock - lastSleep) % sim.protocolParameters.t_cycle;
				return since_sleep >= sim.protocolParameters.t_sleep;
			}
		}

		public void SelfReserve() {
			busyWith = this;
			// remove sleep (handle sleep when Free occurs)
			if (sleepEvent != null) {
				sim.eventQueue.Remove(sleepEvent);
				sleepEvent = null;
			}
		}

		// called only if relay is either free or already busy with the reserver
		public void Reserve(Relay reserver, Event cause) {
			if (this.status == RelayStatus.Free || this.busyWith == reserver) {
				// remove sleep (handle sleep when Free occurs)
				if (sleepEvent != null) {
					sim.eventQueue.Remove(sleepEvent);
					sleepEvent = null;
				}

				if (reserver == this) {
					throw new Exception("Can't use Reserve on self, use SelfReserve instead");
				}
				else {
					if (busyWith == null) {
						busyWith = reserver;
						freeEvent = new FreeEvent {
							time = sim.clock + sim.protocolParameters.t_busy,
							relay = this,
							sim = sim,
							previous = cause
						};
						sim.eventQueue.Add(freeEvent);
					}
					else {
						freeEvent.time = sim.clock + sim.protocolParameters.t_busy;
						sim.eventQueue.Reschedule(freeEvent);
					}
				}
			}
			else {
				throw new Exception("Cannot reserve this relay");
			}
		}

		// called by FreeEvent
		public void Free(Event cause) {
			Reset();
			status = RelayStatus.Free;
			// reschedule sleep
			sleepEvent = new SleepEvent() {
				time = Math.Max(awakeSince + sim.protocolParameters.t_listen, sim.clock),
				relay = this,
				sim = sim,
				previous = cause
			};
			sim.eventQueue.Add(sleepEvent);

			// OLD CODE
			// check sleep schedule
			//if (sim.clock - awakeSince > 999) {//sim.protocolParameters.t_sleep) {
			//	Sleep(cause);
			//}
			//else {
			//	// free and reschedule sleep
			//	Reset();
			//	status = RelayStatus.Free;
			//	sleepEvent = new SleepEvent() {
			//		time = awakeSince + sim.protocolParameters.t_listen,// THIS???
			//		relay = this,
			//		sim = sim,
			//		previous = cause
			//	};
			//	sim.eventQueue.Add(sleepEvent);
			//}
		}

		public void FreeNow(Event cause) {
			// if self-reserved there is no freeEvent
			if (busyWith != this) {
				sim.eventQueue.Remove(freeEvent);
			}
			Free(cause);
		}

		private void Reset() {
			busyWith = null;
			freeEvent = null;
			sleepEvent = null;

			packetToSend = null;

			REGION_index = 0;
			COL_count = 0;
			SENSE_count = 0;
			PKT_count = 0;
			SINK_RTS_count = 0;
			REGION_cycle = 0;
		}

		public void UpdateAwakeTime() {
			var n_cycles = (int)Math.Floor((sim.clock - lastSleep) / sim.protocolParameters.t_cycle);
			totalAwake += n_cycles * sim.protocolParameters.t_listen;
		}

		public void Awake(Event cause) {
			// no need for UpdateAwakeTime because the cause is a sleep event
			if (cause.GetType() != typeof(StartEvent) && cause.GetType() != typeof(AwakeEvent)) {
				throw new Exception("Awake with no reason");
			}
			// set relay as free, schedule sleep, set awake_time (used in FreeEvent)
			status = RelayStatus.Free;
			awakeSince = sim.clock;
			sleepEvent = new SleepEvent() {
				time = sim.clock + sim.protocolParameters.t_listen,
				relay = this,
				sim = sim,
				previous = cause
			};
			sim.eventQueue.Add(sleepEvent);
		}

		public void AwakeMidtime(Event cause) {
			// update awake time, useful only when using skipCycle
			UpdateAwakeTime();

			status = RelayStatus.Free;
			var n_cycles = Math.Floor((sim.clock - lastSleep) / sim.protocolParameters.t_cycle);
			lastSleep = lastSleep + n_cycles * sim.protocolParameters.t_cycle;
			awakeSince = lastSleep + sim.protocolParameters.t_sleep;
			sleepEvent = new SleepEvent() {
				time = awakeSince + sim.protocolParameters.t_listen,
				relay = this,
				sim = sim,
				previous = cause
			};
			if (sleepEvent.time < sim.clock) {
				throw new Exception("no time travel plz");
			}
			sim.eventQueue.Add(sleepEvent);
		}

		public void Sleep(Event cause) {
			lastSleep = sim.clock;
			sleepEvent = null;
			// how to free if transmissions incoming?
			Reset();
			receivingTransmissions.Clear();
			status = RelayStatus.Asleep;

			// skip duty cycle events improves simulation performance dramatically
			if (sim.simulationParameters.skipCycleEvents == false) {
				// schedule awake
				sim.eventQueue.Add(new AwakeEvent() {
					relay = this,
					time = sim.clock + sim.protocolParameters.t_sleep,
					sim = sim,
					previous = cause
				});
			}

			// update awake time stat
			totalAwake += sim.clock - awakeSince;
		}

		public void PostponeSleep(Event cause) {
			if (sleepEvent != null && busyWith == null) {
				sleepEvent.time = sim.clock + sim.protocolParameters.t_signal + sim.protocolParameters.t_delta;
				sim.eventQueue.Reschedule(sleepEvent);
			}
		}
	}
}
