from enum import Enum
import random
import numpy as np
from numpy import mean, min, max, median, quantile, array
from math import ceil, sqrt, floor
from scipy.stats import expon
from typing import Optional as Opt, List, Tuple, Union, Dict
import heapq


class EventType(Enum):
	Start = 0
	End = 1
	Debug = 2
	# start events: 1. set carrier busy 2. set collisions on active transmissions 3. add to active transmissions
	# end events: 1. set carrier to free 2. move to finished transmission/delete (CTSs are just moved, others deleted)
	SENSE_start = 3,  # start sensing
	SENSE_end = 4,  # check that last active pulse is older than relative sense_start. If free, RTS_start. if busy, backoff to SENSE_start
	RTS_start = 5,  # node, time, region index. 1.select active nodes in that area and add to scheduled RTS_end (keep them awake)
	RTS_end = 6,  # node, time, region index, receiving nodes. 1.schedule CTS_start for selected nodes 2.schedule COL_check in cts_time time (plus some delta to be sure)
	CTS_start = 7,  # :
	CTS_end = 8,  # :
	COL_check = 9,
	# 0. no response => next region
	# source, sink, node, ...: 1. no collision => PKT_start with first CTS sender; any collision => COL_start OR (next_region and RTS_start) depending on collision count
	# modified 1. at least 1 non-overlapping => PKT_start with first CTS sender; all overlapping => COL_start
	# modified 2. select between non-overlapping using probabilities of success
	# modified 3. no collision => PKT_start with probabilities; any collision => COL_start
	# modified 4. send collision message only for collided CTSs (others stay silent)
	COL_start = 10,
	COL_end = 11,  # ... : .. schedule COL_check in max_backoff time. 2.schedule CTS_start for available nodes with backoff
	PKT_start = 12,  # nodes not selected can go to sleep, schedule ACK_check
	PKT_end = 13,  # if packet has no collision schedule ACK_start, else stay
	ACK_start = 14,
	ACK_end = 15,
	ACK_check = 16  # if no collision, finished. Else, schedule PKT_start again


class Node:

	def __init__(self, id: int, location: Tuple[float, float] = (0, 0), range: float = 0):
		self.id = id
		self.location = (0, 0)
		self.range = 0
		self.neighbours: Tuple['Node', ...] = tuple()
		self.awake = True
		self.sensing = set() # set of packets that is sensing

	def __str__(self):
		return f'{"node_("}{self.location[0]: .3f}, {self.location[1]: .3f}{")"}'

	def __repr__(self):
		return self.__str__()


class Event:

	def __init__(self, time: float, eventType: EventType):
		self.time = time
		self.type = eventType

	def __str__(self):
		return f'{"{"}{self.time : .3f}; {str(self.type)[10:]}{"}"}'

	def __repr__(self):
		return self.__str__()

	def __lt__(self, other: 'Event'):
		return self.time < other.time


class Transmission(Event):

	def __init__(self, time, eventType, source=None, destination=None):
		super().__init__(time, eventType)
		self.source = source
		self.destination = destination


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
		self.avg_speed: float = 0


class ProtocolParameters:

	def __init__(self):
		self.duty_cycle = 0.1  # NOT IN SECONDS: active time percentage (t_l / (t_l + t_s))
		self.t_sense = 0.0521  # carrier sense duration
		self.t_backoff = 0.219  # backoff interval length (constant?)
		self.t_listen = 0.016  # listening time
		self.t_sleep = self.t_listen * ((1 / self.duty_cycle) - 1)  # 0.144000, sleep time
		self.t_data = 0.0521  # data transmission time
		self.t_signal = 0.00521  # signal packet transmission time (RTS and CTS ?)
		self.n_regions = 4  # number of priority regions
		self.n_max_attempts = 50  # number of attempts for searching a relay
		self.n_max_coll = 6  # number of attempts for solving a collision


class Simulation:

	def __init__(self, transmissions: int, side: float, t_range: float, n_bins: int, nodes: int, debug_interval: float):
		self.max_time = 1000
		self.transmissions = transmissions
		self.side = side
		self.node_range = t_range
		self.n_bins = n_bins
		self.distances: Dict[int, Dict[int, float]] = {}

		# system state
		self.nodes: List[Node] = [Node(i) for i in range(nodes)]

		# simulation state
		self.clock: float = 0
		self.event_queue = EventQueueHeap()
		self.event_queue.add(Event(0, EventType.Start))
		self.event_queue.add(Event(self.max_time, EventType.End))
		for debug_time in np.arange(0, self.max_time, debug_interval)[1:]:
			self.event_queue.add(Event(debug_time, EventType.Debug))

		# stat counters
		self.event_times = []
		self.stuck = 0
		self.finished_steps = []
		# debugs
		self.debugStats: List[DebugStats] = []

	def run(self):
		while (True):
			event = self.event_queue.pop()
			assert (event != None)

			# update counters

			# instant counters
			self.event_times.append(event.time)

			# update system state
			self.clock = event.time

			# execute event
			if event.type == EventType.End:
				break
			elif event.type == EventType.Start:
				# init nodes
				for node in self.nodes:
					node.location = np.random.rand(2) * self.side
					node.range = self.node_range
				# calculate neighbours
				for n1 in self.nodes:
					neighbours = []
					self.distances[n1.id] = {}
					for n2 in self.nodes:
						x1, y1 = n1.location
						x2, y2 = n2.location
						d = sqrt((x1 - x2)**2 + (y1 - y2)**2)
						self.distances[n1.id][n2.id] = d
						if n1 != n2 and d < n1.range:
							neighbours.append(n2)
					n1.neighbours = tuple(neighbours)
				# add transmissions
				for i in range(self.transmissions):
					source, sink = np.random.choice(self.nodes, 2, replace=False)
					t = Event(np.random.rand() * self.max_time, EventType.Transmission, source, sink)
					self.event_queue.add(t)
			elif event.type == EventType.Debug:
				ds = DebugStats()
				ds.event_time = event.time
				self.debugStats.append(ds)
			elif event.type == EventType.Transmission:
				steps = self.transmission(event)
				if steps == None:
					self.stuck += 1
				else:
					self.finished_steps.append(steps)

	def transmission(self, event):
		current_node = event.source
		steps = 0
		while (True):
			if event.sink in current_node.neighbours:
				return steps + 1
			# calculate bins of available nodes
			regions = self.priority_regions(current_node, event.sink)
			regions.reverse()
			# find first non-empty bin
			chosen_region = None
			for r in regions:
				if len(r) > 0:
					chosen_region = r
					break
			if chosen_region == None:  # stuck
				return None
			else:
				steps += 1
				current_node = np.random.choice(chosen_region)

	# def priority_regions(self, source, sink):
	# 	# keep only the ones advancing the packet geographically (closer to sink than the source)
	# 	source_dist = self.distance(source, sink)
	# 	nodes: List[Tuple[Node, float]] = []
	# 	for n in source.neighbours:
	# 		d = self.distance(n, sink)
	# 		if d < source_dist:
	# 			nodes.append((n, d))
	# 	# bins
	# 	bins = [[] for i in range(self.n_bins)]
	# 	bin_width = source.range / self.n_bins
	# 	for n, d in nodes:
	# 		# calc distance using farthest bin as maximum
	# 		bin_dist = d - (source_dist - source.range)
	# 		i = floor(bin_dist / bin_width)
	# 		bins[i].append(n)

	# 	return bins

	def distance(self, n1: Node, n2: Node) -> float:
		return self.distances[n1.id][n2.id]


print(ProtocolParameters().t_sleep)
# sim = Simulation(100, 100, 30, 4, 100, 1)
# sim.run()
