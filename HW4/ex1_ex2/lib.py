from enum import Enum
import random
import numpy as np
from numpy import mean, min, max, median, quantile, array
from math import ceil
from scipy.stats import expon
from typing import Optional as Opt, List, Tuple, Union
import heapq


class EventType(Enum):
	Start = 0
	End = 1
	Arrival = 2
	Departure = 3
	Debug = 4


class Packet:

	def __init__(self):
		self.queue_enter: Opt[float] = None
		self.queue_exit: Opt[float] = None
		self.service_exit: Opt[float] = None


class Server:

	def __init__(self, ID):
		self.ID = ID
		self.packet: Opt[Packet] = None
		self.packets_finished: List[Packet] = []

	def __lt__(self, other):
		return self.ID < other.ID


class Event:

	def __init__(self, time: float, eventType: EventType, departure_server: Opt[Server] = None):
		self.time = time
		self.type = eventType
		self.next: Opt['Event'] = None
		self.departure_server = departure_server

	def __str__(self):
		return f'{"{"}{self.time : .3f}; {str(self.type)[10:]}{"}"}'

	def __repr__(self):
		return self.__str__()

	def __lt__(self, other):
		return self.time < other.time


class EventQueue:

	def __init__(self):
		self.head: Opt[Event] = None
		self.tail: Opt[Event] = None

	def add(self, event: Event):
		if self.tail == None:  # empty queue
			self.head = self.tail = event
			event.next = None
		else:
			current_node = self.head
			prev_node = None
			while (current_node != None and current_node < event):
				prev_node = current_node
				current_node = current_node.next
			if prev_node == None:  # new event at head
				event.next = current_node
				self.head = event
			elif current_node == None:  # new event at tail
				prev_node.next = event
				event.next = current_node
				self.tail = event
			else:  # new event in the middle
				prev_node.next = event
				event.next = current_node

	def pop(self) -> Opt[Event]:
		if self.head == None:
			return None
		res = self.head
		self.head = res.next
		return res

	def __str__(self):
		res = '<'
		current = self.head
		while (current != None):
			res += str(current)
			current = current.next
		return res + '>'

	def __repr__(self):
		return self.__str__()


class EventQueueHeap:

	def __init__(self):
		self.heap: List[Event] = []

	def add(self, event: Event):
		heapq.heappush(self.heap, event)

	def pop(self):
		return heapq.heappop(self.heap)

	def __str__(self):
		res = '<'
		for e in self.heap:
			res += str(e)
		return res + '>'

	def __repr__(self):
		return self.__str__()


class DebugStats:

	def __init__(self):
		self.event_time: float = 0
		# since last debug event
		self.avg_busy_servers: float = 0
		self.avg_q_size: float = 0
		self.avg_load: float = 0
		self.avg_q_time: float = 0
		self.avg_tot_time: float = 0
		#since the beninning
		self.cum_avg_busy_servers: float = 0
		self.cum_avg_q_size: float = 0
		self.cum_avg_load: float = 0
		self.cum_avg_q_time: float = 0
		self.cum_avg_tot_time: float = 0


class Simulation:

	def __init__(self, max_time: float, arrival_rate: float, departure_rate: float, n_servers: int, debug_interval: float):
		self.max_time = max_time
		self.arrival_rate = arrival_rate
		self.departure_rate = departure_rate

		# system state
		self.packets_queue: List[Packet] = []
		self.servers = tuple(Server(i) for i in range(n_servers))
		self.free_servers = list(self.servers)
		self.packets_exited: List[Packet] = []

		# simulation state
		self.clock: float = 0
		self.event_queue = EventQueueHeap()
		self.event_queue.add(Event(0, EventType.Start))
		self.event_queue.add(Event(max_time, EventType.End))
		for debug_time in np.arange(0, max_time, debug_interval)[1:]:
			self.event_queue.add(Event(debug_time, EventType.Debug))

		# stat counters
		self.event_times = []
		self.q_sizes = []
		self.u_sizes = []
		self.areaQ: float = 0
		self.areaU: float = 0
		self.areaQ_debug: float = 0
		self.areaU_debug: float = 0
		# at run finish calculate the following
		self.q_times: List[float] = []
		self.tot_times: List[float] = []
		# debugs
		self.debugStats: List[DebugStats] = []

	def get_arrival_ppf(self):
		return random.expovariate(self.arrival_rate)

	def get_departure_ppf(self):
		return random.expovariate(self.departure_rate)

	def get_free_server(self):
		if len(self.free_servers) == 0:
			return None
		return heapq.heappop(self.free_servers)

	def arrival(self, event: Event):
		current_time = event.time

		# schedule another arrival
		new_arrival_time = current_time + self.get_arrival_ppf()
		new_arrival = Event(new_arrival_time, EventType.Arrival)
		self.event_queue.add(new_arrival)

		# handle this arrival
		packet = Packet()
		packet.queue_enter = current_time
		free_server = self.get_free_server()
		if free_server != None:  # enter/exit queue, enter service server
			# assign packet to server
			packet.queue_exit = current_time
			free_server.packet = packet
			# update packets stats
			self.packets_exited.append(packet)

			# schedule another departure
			new_departure_time = current_time + self.get_departure_ppf()
			new_departure = Event(new_departure_time, EventType.Departure, free_server)
			self.event_queue.add(new_departure)
		else:
			# place packet in queue
			self.packets_queue.append(packet)

	def departure(self, event: Event):
		assert (event.departure_server != None)
		current_time = event.time
		departure_server = event.departure_server
		# register finished packet
		assert (departure_server.packet != None)
		finished_packet = departure_server.packet
		finished_packet.service_exit = current_time
		# free the server
		departure_server.packet = None
		heapq.heappush(self.free_servers, departure_server)
		# update packets stats
		departure_server.packets_finished.append(finished_packet)

		if len(self.packets_queue) == 0:  # no new packets
			self.packet_in_server = None
		else:
			free_server = self.get_free_server()
			assert (free_server != None)  # at least the departure server must be free
			# get packet from queue
			packet = self.packets_queue.pop(0)
			packet.queue_exit = current_time
			free_server.packet = packet
			# update packets stats
			self.packets_exited.append(packet)

			# schedule another departure
			new_departure_time = current_time + self.get_departure_ppf()
			new_departure = Event(new_departure_time, EventType.Departure, free_server)
			self.event_queue.add(new_departure)

	def run(self):
		while (True):
			event = self.event_queue.pop()
			assert (event != None)

			# update counters
			time_from_previous = event.time - self.clock
			q_size = len(self.packets_queue)
			busy_servers = len(self.servers) - len(self.free_servers)

			# instant counters
			self.event_times.append(event.time)
			self.q_sizes.append(q_size)
			self.u_sizes.append(busy_servers)

			# overral counters
			self.areaQ += q_size * time_from_previous
			self.areaU += busy_servers * time_from_previous
			self.areaQ_debug += q_size * time_from_previous
			self.areaU_debug += busy_servers * time_from_previous

			# update system state
			self.clock = event.time

			# execute event
			if event.type == EventType.End:
				break
			elif event.type == EventType.Start:
				# self.arrival(node)  # is start arrival?
				# schedule another arrival
				new_arrival_time = event.time + self.get_arrival_ppf()
				new_arrival = Event(new_arrival_time, EventType.Arrival)
				self.event_queue.add(new_arrival)
			elif event.type == EventType.Debug:
				prev_debug_time = 0 if len(self.debugStats) == 0 else self.debugStats[-1].event_time
				time_diff = event.time - prev_debug_time
				ds = DebugStats()
				self.debugStats.append(ds)
				finished_packets = []
				for s in self.free_servers:
					finished_packets += s.packets_finished
				ds.event_time = event.time
				# since last debug event
				ds.avg_busy_servers = self.areaU_debug / time_diff
				ds.avg_q_size = self.areaQ_debug / time_diff
				ds.avg_load = ds.avg_q_size + ds.avg_busy_servers
				q_times_since_last = [p.queue_exit - p.queue_enter for p in self.packets_exited if p.queue_exit > prev_debug_time]
				tot_times_since_last = [p.service_exit - p.queue_enter for p in finished_packets if p.service_exit > prev_debug_time]
				ds.avg_q_time = mean(q_times_since_last) if len(q_times_since_last) > 0 else 0
				ds.avg_tot_time = mean(tot_times_since_last) if len(tot_times_since_last) > 0 else 0
				# since the beninning
				ds.cum_avg_busy_servers = self.areaU / ds.event_time
				ds.cum_avg_q_size = self.areaQ / ds.event_time
				ds.cum_avg_load = ds.cum_avg_busy_servers + ds.cum_avg_q_size
				q_times = [p.queue_exit - p.queue_enter for p in self.packets_exited]
				tot_times = [p.service_exit - p.queue_enter for p in finished_packets]
				# print("q_times", q_times)
				ds.cum_avg_q_time = mean(q_times) if len(q_times) > 0 else 0
				ds.cum_avg_tot_time = mean(tot_times) if len(tot_times) > 0 else 0

				# reset counters
				self.areaU_debug = 0
				self.areaQ_debug = 0
			elif event.type == EventType.Arrival:
				self.arrival(event)
			elif event.type == EventType.Departure:
				self.departure(event)

		# calculate others
		for p in self.packets_exited:
			self.q_times.append(p.queue_exit - p.queue_enter)
		for s in self.free_servers:
			for p in s.packets_finished:
				self.tot_times.append(p.service_exit - p.queue_enter)

		# convert to numpy
		self.event_times = np.array(self.event_times)
		self.q_sizes = np.array(self.q_sizes)
		self.u_sizes = np.array(self.u_sizes)
		self.q_times = np.array(self.q_times)
		self.tot_times = np.array(self.tot_times)
