from enum import Enum
import random
import numpy as np
from numpy import mean, min, max, median, quantile, array
from math import ceil, sqrt, floor
from scipy.stats import expon
from typing import Optional as Opt, List, Tuple, Union
import heapq


class EventType(Enum):
	Start = 0
	End = 1
	Debug = 2
	Transmission = 3


class Node:

	def __init__(self, location=(0, 0), range=0):
		self.location = (0, 0)
		self.range = 0
		self.neighbours: Tuple['Node', ...] = tuple()

	def __str__(self):
		return f'{"node_("}{self.location[0]: .3f}, {self.location[1]: .3f}{")"}'

	def __repr__(self):
		return self.__str__()


def distance(n1: Node, n2: Node) -> float:
	x1, y1 = n1.location
	x2, y2 = n2.location
	return sqrt((x1 - x2)**2 + (y1 - y2)**2)


class Event:

	def __init__(self, time: float, eventType: EventType, source: Opt[Node] = None, sink: Opt[Node] = None):
		self.time = time
		self.type = eventType
		self.next: Opt['Event'] = None
		self.source: Opt[Node] = source
		self.sink: Opt[Node] = sink

	def __str__(self):
		return f'{"{"}{self.time : .3f}; {str(self.type)[10:]}{"}"}'

	def __repr__(self):
		return self.__str__()

	def __lt__(self, other):
		return self.time < other.time


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


class Simulation:

	def __init__(self, transmissions: int, side: float, t_range: float, n_bins: int, nodes: int, debug_interval: float):
		self.max_time = 1000
		self.transmissions = transmissions
		self.side = side
		self.node_range = t_range
		self.n_bins = n_bins

		# system state
		self.nodes: List[Node] = [Node() for i in range(nodes)]

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
					for n2 in self.nodes:
						if n1 != n2 and distance(n1, n2) < n1.range:
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
				current_node = np.random.choice(r)

	def priority_regions(self, source, sink):
		# keep only the ones advancing the packet geographically (closer to sink than the source)
		source_dist = distance(source, sink)
		nodes: List[Tuple[Node, float]] = []
		for n in source.neighbours:
			d = distance(n, sink)
			if d < source_dist:
				nodes.append((n, d))
		# bins
		bins = [[] for i in range(self.n_bins)]
		bin_width = source.range / self.n_bins
		for n, d in nodes:
			# calc distance using farthest bin as maximum
			bin_dist = d - (source_dist - source.range)
			i = floor(bin_dist / bin_width)
			bins[i].append(n)

		return bins


sim = Simulation(100, 100, 30, 4, 100, 1)
sim.run()
