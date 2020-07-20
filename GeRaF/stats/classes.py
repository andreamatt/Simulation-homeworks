from math import sqrt
from enum import Enum

class SimulationParameters:
	def __init__(self, data):
		self.max_time = float(data['max_time'])
		self.area_side = float(data['area_side'])
		self.range = float(data['range'])
		self.min_distance =  float(data['min_distance'])
		self.n_nodes = int(data['n_nodes'])
		self.packet_rate = float(data['packet_rate'])
		self.skipCycleEvents = True if data['skipCycleEvents']=='true' else False
		# useless
		# self.debug_interval = data[7]
		# self.debugType = data[8]
		# self.percentages = data[9]
		# self.debug_file = data[10]

class ProtocolParamaters:
	def __init__(self, data):
		self.duty_cycle = float(data["duty_cycle"])
		self.t_sense = float(data["t_sense"])
		self.t_backoff = float(data["t_backoff"])
		self.t_listen = float(data["t_listen"])
		self.t_data = float(data["t_data"])
		self.t_signal = int(data["t_signal"])
		self.n_regions = int(data["n_regions"])
		self.n_max_coll = int(data["n_max_coll"])
		self.n_max_sensing = int(data["n_max_sensing"])
		self.n_max_sink_rts = int(data['n_max_sink_rts'])
		self.n_max_pkt = int(data['n_max_pkt'])
		self.n_max_region_cycle = int(data['n_max_region_cycle'])
		self.t_delta = float(data['t_delta'])
		self.protocolVersion = data['protocolVersion']
		self.t_sleep = float(data['t_sleep'])
		self.t_cycle = float(data['t_cycle'])
		self.t_busy = float(data['t_busy'])


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
		