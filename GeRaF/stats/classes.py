from math import sqrt
from enum import Enum

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


class BaseStat:
	def __init__(self, data):
		self.shape = data['shape']
		self.version = data["protocolVersion"]
		self.duty = data["duty"]
		self.lam = float(data['lambda'])
		self.N = float(data['N'])
		self.delay = [float(s) for s in data['delay']]
		self.success = [float(s) for s in data['success']]
		self.energy = [float(s) for s in data['energy']]
		self.traffic = data['traffic']
		self.failurePoints = data['failurePoints']
	


class RunResult:
	def __init__(self, data):
		self.basePP = ProtocolParamaters(data['basePP'])
		self.baseSP = SimulationParameters(data['baseSP'])
		self.DLstats = [BaseStat(s) for s in data['dutyLambdas']]
		self.LNstats = [BaseStat(s) for s in data['lambdaNs']]
		self.ShapeStats = [BaseStat(s) for s in data['shapeStats']]
		