from enum import Enum
from numpy.random import rand
from numpy import mean, min, max, median, quantile, array
from math import ceil
from scipy.stats import expon


class EventType(Enum):
	Start = 0
	End = 1
	Arrival = 2
	Departure = 3
	Debug = 4


class Packet:

	def __init__(self):
		self.queue_enter = None
		self.queue_exit = None
		self.service_exit = None


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
		self.initPPFs()

		# system state
		self.packets_queue = []
		self.packet_in_server = None
		self.packets_exited = []
		self.packets_finished = []

		# simulation state
		self.last_event_time = 0
		self.clock = 0
		self.event_queue = Queue()
		self.event_queue.add(Event(0, EventType.Start))
		self.event_queue.add(Event(max_time, EventType.End))

		# stat counters
		self.event_times = []
		self.time_diffs = []
		self.server_status = []
		self.q_size = []
		self.load = []
		self.areaQ = 0
		self.areaU = 0
		self.sum_q_time = 0
		self.sum_tot_time = 0
		self.avg_q_times = []
		self.avg_tot_times = []
		# at run finish calculate the following
		self.q_times = []
		self.tot_times = []

	def initPPFs(self):
		ppf_size = ceil(self.max_time * self.departure_rate)*2
		self.arrival_ppfs = expon.ppf(rand(ppf_size), scale=1 / self.arrival_rate).tolist()
		self.departure_ppfs = expon.ppf(rand(ppf_size), scale=1 / self.departure_rate).tolist()

	def get_arrival_ppf(self):
		if len(self.arrival_ppfs) == 0:
			self.initPPFs()
		return self.arrival_ppfs.pop(-1)

	def get_departure_ppf(self):
		if len(self.departure_ppfs) == 0:
			self.initPPFs()
		return self.departure_ppfs.pop(-1)

	def arrival(self, event):
		current_time = event.time

		# schedule another arrival
		new_arrival_time = current_time + self.get_arrival_ppf()
		new_arrival = Event(time=new_arrival_time, type=EventType.Arrival)
		self.event_queue.add(new_arrival)

		# handle this arrival
		packet = Packet()
		packet.queue_enter = current_time
		if self.packet_in_server == None:  # enter/exit queue, enter service server
			self.packet_in_server = packet
			packet.queue_exit = current_time
			# update packets stats
			self.sum_q_time += packet.queue_exit - packet.queue_enter
			self.packets_exited.append(packet)

			# schedule another departure
			new_departure_time = current_time + self.get_departure_ppf()
			new_departure = Event(time=new_departure_time, type=EventType.Departure)
			self.event_queue.add(new_departure)
		else:
			self.packets_queue.append(packet)

	def departure(self, event):
		current_time = event.time

		# register finished packet
		finished_packet = self.packet_in_server
		finished_packet.service_exit = current_time
		# update packets stats
		self.sum_tot_time += finished_packet.service_exit - finished_packet.queue_enter
		self.packets_finished.append(finished_packet)

		if len(self.packets_queue) == 0:  # no new packets
			self.packet_in_server = None
		else:
			# get packet from queue
			packet = self.packets_queue.pop(0)
			packet.queue_exit = current_time
			self.packet_in_server = packet
			# update packets stats
			self.sum_q_time += packet.queue_exit - packet.queue_enter
			self.packets_exited.append(packet)

			# schedule another departure
			new_departure_time = current_time + self.get_departure_ppf()
			new_departure = Event(time=new_departure_time, type=EventType.Departure)
			self.event_queue.add(new_departure)

	def run(self):
		while (True):
			node = self.event_queue.pop()

			# update counters
			time_from_previous = node.time - self.clock
			q_size = len(self.packets_queue)
			server_status = 0 if self.packet_in_server == None else 1
			load = q_size + server_status

			self.event_times.append(node.time)
			self.time_diffs.append(time_from_previous)
			self.server_status.append(server_status)
			self.q_size.append(q_size)
			self.load.append(load)
			self.areaQ += q_size * time_from_previous
			self.areaU += server_status * time_from_previous
			self.avg_q_times.append(self.sum_q_time / len(self.packets_exited) if len(self.packets_exited) > 0 else 0)
			self.avg_tot_times.append(self.sum_tot_time / len(self.packets_finished) if len(self.packets_finished) > 0 else 0)

			# update system state
			self.clock = node.time
			# altro??

			# execute event
			if node.type == EventType.End:
				break
			elif node.type == EventType.Start:
				# self.arrival(node)  # is start arrival?
				# schedule another arrival
				new_arrival_time = node.time + self.get_arrival_ppf()
				new_arrival = Event(time=new_arrival_time, type=EventType.Arrival)
				self.event_queue.add(new_arrival)
			elif node.type == EventType.Debug:
				print(self.event_queue)
			elif node.type == EventType.Arrival:
				self.arrival(node)
			elif node.type == EventType.Departure:
				self.departure(node)

		# calculate others
		for p in self.packets_exited:
			self.q_times.append(p.queue_exit - p.queue_enter)
		for p in self.packets_finished:
			self.tot_times.append(p.service_exit - p.queue_enter)

		# convert to numpy
		self.event_times = array(self.event_times)
		self.time_diffs = array(self.time_diffs)
		self.server_status = array(self.server_status)
		self.q_size = array(self.q_size)
		self.load = array(self.load)
		self.avg_q_times = array(self.avg_q_times)
		self.avg_tot_times = array(self.avg_tot_times)

		self.q_times = array(self.q_times)
		self.tot_times = array(self.tot_times)
