from math import sqrt

class Relay:
	def __init__(self, data):
		infos = data.split('|')
		self.id = int(infos[0])
		self.X = float(infos[1])
		self.Y = float(infos[2])
		self.asleepTime = float(infos[3])
		self.awakeTime = float(infos[4])

class Packet:
	results = [
		'None',
		'Success',
		'Passed',# not used
		'No_start_relays',
		#Abort_max_attempts,
		'Abort_max_region_cycle',
		'Abort_max_sensing',
		'Abort_max_sink_rts',
		'Abort_no_ack'
	]

	def __init__(self, data):
		infos = data.split('|')
		self.content_id = int(infos[0])
		self.copy_id = int(infos[1])
		self.generationTime = float(infos[2])
		self.startRelayId = int(infos[3])
		self.sinkId = int(infos[4])
		self.hopsIds = [int(i) for i in infos[5].split(',')]
		self.result = Packet.results[int(infos[6])]


class SimulationParameters:
	def __init__(self, data):
		data = list(data.values())
		self.max_time = float(data[0])
		self.area_side = float(data[1])
		self.range = float(data[2])
		self.min_distance =  float(data[3])
		self.n_nodes = int(data[4])
		self.packet_rate = float(data[5])
		self.skipCycleEvents = True if data[6]=='true' else False
		# useless
		# self.debug_interval = data[7]
		# self.debugType = data[8]
		# self.percentages = data[9]
		# self.debug_file = data[10]
		

class ProtocolParamaters:
	def __init__(self, data):
		data = list(data.values())
		self.duty_cycle = float(data[0])
		self.t_sense = float(data[1])
		self.t_backoff = float(data[2])
		self.t_listen = float(data[3])
		self.t_data = float(data[4])
		self.t_signal = int(data[5])
		self.n_regions = int(data[6])
		self.n_max_coll = int(data[7])
		self.n_max_sensing = int(data[8])
		self.n_max_sink_rts = int(data[9])
		self.n_max_pkt = int(data[10])
		self.n_max_region_cycle = int(data[11])
		self.t_delta = float(data[12])
		self.protocolVersion = data[13]
		self.t_sleep = float(data[14])
		self.t_cycle = float(data[15])
		self.t_busy = float(data[16])

class SimulationStat:
	def __init__(self, data):
		self.relays = [Relay(s) for s in data['relayInfos'].split(';')]
		self.relays_dict = {r.id: r for r in self.relays}
		self.packets = [Packet(s) for s in data['finishedPackets'].split(';')]
		self.packets_dict = {}
		self.distances = {}
		for r1 in self.relays:
			self.distances[r1.id] = {}
			for r2 in self.relays:
				if(r1 != r2):
					self.distances[r1.id][r2.id] = sqrt((r1.X-r2.X)**2 + (r1.Y-r2.Y)**2)
		for p in self.packets:
			if p.content_id not in self.packets_dict:
				self.packets_dict[p.content_id] = []
			self.packets_dict[p.content_id].append(p)
		
	def mean_rate(self, result):
		total = len(self.packets_dict)
		res = 0
		for packets in self.packets_dict.values():
			if any(map(lambda p: p.result==result, packets)):
				res += 1
		return res/total


