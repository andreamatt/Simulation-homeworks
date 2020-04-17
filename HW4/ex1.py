from enum import Enum
from numpy.random import rand
import numpy as np
from matplotlib import pyplot as plt
from numpy import mean, min, max, median, quantile
from scipy.stats import binom, norm, t as student, expon, poisson, chi2
from math import sqrt, pow, floor, ceil, exp, log, sin, pi
from statsmodels.api import qqplot


class EventType(Enum):
	Start = 0
	End = 1
	Arrival = 2
	Departure = 3
	Debug = 4


class Event:

	def __init__(self, time=0, type: EventType = EventType.Debug):
		self.time = time
		self.type: EventType = type
		self.next = None

	def __str__(self):
		return f'{"{"}{self.time : .3f}; {str(self.type)[10:]}{"}"}'

	def __repr__(self):
		return self.__str__


class Queue:

	def __init__(self):
		self.head = None
		self.tail = None

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

	def pop(self):
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


class Simulation:

	def __init__(self, max_time, arrival_rate, departure_rate):
		self.max_time = max_time
		self.arrival_rate = arrival_rate
		self.departure_rate = departure_rate

		# system state
		self.server_free = True
		self.packets_in_queue = 0
		self.last_event_time = 0

		# simulation state
		self.clock = 0
		self.queue = Queue()
		self.queue.add(Event(0, EventType.Start))
		self.queue.add(Event(max_time, EventType.End))

		# stat counters
		self.areaQ = 0
		self.areaU = 0
		self.loadX = []
		self.loadY = []

	def arrival(self, event):
		# schedule another arrival
		new_arrival_time = event.time + expon.ppf(rand(), scale=1 / self.arrival_rate)
		new_arrival = Event(time=new_arrival_time, type=EventType.Arrival)
		self.queue.add(new_arrival)

		# handle this arrival
		if self.server_free:
			self.server_free = False
			# schedule another departure
			new_departure_time = event.time + expon.ppf(rand(), scale=1 / self.departure_rate)
			new_departure = Event(time=new_departure_time, type=EventType.Departure)
			self.queue.add(new_departure)
		else:
			self.packets_in_queue += 1

	def departure(self, event):
		if self.packets_in_queue == 0:
			self.server_free = True
		else:
			self.packets_in_queue -= 1
			# schedule another departure
			new_departure_time = event.time + expon.ppf(rand(), scale=1 / self.departure_rate)
			new_departure = Event(time=new_departure_time, type=EventType.Departure)
			self.queue.add(new_departure)

	def run(self):
		while (True):
			# print(f'Q: {self.queue}, Free: {self.server_free}, Pack_Q: {self.packets_in_queue}')

			node = self.queue.pop()

			# update counters
			# packets_in_system = self.packets_in_queue# + (0 if self.server_free else 1) sum at the end
			self.areaQ += self.packets_in_queue * (node.time - self.clock)
			self.areaU += (0 if self.server_free else 1) * (node.time - self.clock)
			self.loadX.append(node.time)
			self.loadY.append(self.packets_in_queue + (0 if self.server_free else 1))

			# update system state
			self.clock = node.time
			# altro??

			if node.type == EventType.End:
				break
			elif node.type == EventType.Start:
				# self.arrival(node)  # is start arrival?
				# schedule another arrival
				new_arrival_time = node.time + expon.ppf(rand(), scale=1 / self.arrival_rate)
				new_arrival = Event(time=new_arrival_time, type=EventType.Arrival)
				self.queue.add(new_arrival)
			elif node.type == EventType.Debug:
				print(self.queue)
			elif node.type == EventType.Arrival:
				self.arrival(node)
			elif node.type == EventType.Departure:
				self.departure(node)


lam = 1
mu = 1.01
max_time = mu * 10000
sim = Simulation(max_time, lam, mu)
sim.run()

ro = lam / mu
theor_avg = ro / (1 - ro)

plt.plot(sim.loadX, sim.loadY, 'b-', linewidth=0.5)
plt.show()
# Y = np.array(sim.ser)
# unique, counts = np.unique(Y, return_counts=True)
# counts = counts / len(Y)
# plt.scatter(unique, counts)
# plt.show()

# print(f'Theoretical: {theor_avg}, empirical: {mean(sim.area)}, events: {(len(Y)-2)/2}')
print(f'Active server time %: {sim.areaU / max_time}, theoric: {ro}')
print(f'Average q size: {(sim.areaQ+sim.areaU) / max_time}, theoric: {theor_avg}')