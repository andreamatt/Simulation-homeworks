from enum import Enum
import random
import numpy as np
from numpy import mean, min, max, median, quantile, array
from math import ceil, sqrt
from scipy.stats import expon
from typing import Optional as Opt, List, Tuple, Union


class EventType(Enum):
	Start = 0
	End = 1
	Waypoint_reached = 2
	Debug = 3


class Node:

	def __init__(self):
		# self.locations = []
		# self.destinations = []
		self.speeds = []
		self.iteration = 0


class Event:

	def __init__(self, time: float, eventType: EventType, node: Opt[Node] = None):
		self.time = time
		self.type = eventType
		self.next: Opt['Event'] = None
		self.node: Opt[Node] = node

	def __str__(self):
		return f'{"{"}{self.time : .3f}; {str(self.type)[10:]}{"}"}'

	def __repr__(self):
		return self.__str__


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
			while (current_node != None and current_node.time < event.time):
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
		return self.__str__


class DebugStats:

	def __init__(self):
		self.event_time: float = 0
		self.avg_speed: float = 0


class Simulation:

	def __init__(self, max_time: float, side: float, min_speed: float, max_speed: float, nodes: int, debug_interval: float):
		self.max_time = max_time
		self.side = side
		self.min_speed = min_speed
		self.max_speed = max_speed

		# system state
		self.nodes: List[Node] = [Node() for i in range(nodes)]

		# simulation state
		self.clock: float = 0
		self.event_queue = EventQueue()
		self.event_queue.add(Event(0, EventType.Start))
		self.event_queue.add(Event(max_time, EventType.End))
		for debug_time in np.arange(0, max_time, debug_interval)[1:]:
			self.event_queue.add(Event(debug_time, EventType.Debug))

		# stat counters
		self.event_times = []
		# debugs
		self.debugStats: List[DebugStats] = []

	def get_rand_speed(self):
		return random.random() * (self.max_speed - self.min_speed) + self.min_speed

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
				events = []
				for node in self.nodes:
					time = 0
					location = np.random.rand(2) * self.side
					while(time < self.max_time):
						destination = np.random.rand(2) * self.side
						speed = self.get_rand_speed()
						# update node
						# node.locations.append(location)
						# node.destinations.append(destination)
						node.speeds.append(speed)
								
						# calculate dest time
						distance = sqrt((location[0] - destination[0])**2 + (location[1] - destination[1])**2)
						next_time = time + distance / speed
						node_event = Event(next_time, EventType.Waypoint_reached, node=node)
						# self.event_queue.add(node_event)
						events.append(node_event)

						# update
						location = destination
						time = next_time
				events = sorted(events, key=lambda e: e.time, reverse=True)
				for e in events:
					self.event_queue.add(e)

			elif event.type == EventType.Debug:
				ds = DebugStats()
				ds.event_time = event.time
				ds.avg_speed = mean([node.speeds[node.iteration] for node in self.nodes])
				self.debugStats.append(ds)
			elif event.type == EventType.Waypoint_reached:
				node = event.node
				node.iteration += 1

		# convert to numpy
		self.event_times = np.array(self.event_times)
