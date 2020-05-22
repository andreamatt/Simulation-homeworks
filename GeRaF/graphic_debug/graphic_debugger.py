import matplotlib.pyplot as plt
import matplotlib.patches as patch
import json


class SimulationParameters:

	def __init__(self, data):
		self.max_time = data["max_time"]
		self.area_side = data["area_side"]
		self.range = data["range"]
		self.n_nodes = data["n_nodes"]
		self.packet_rate = data["packet_rate"]
		self.debug_interval = data["debug_interval"]
		self.debug_always = data["debug_always"]
		self.debug_file = data["debug_file"]


class ProtocolParameters:

	def __init__(self, data):
		self.duty_cycle = data["duty_cycle"]
		self.t_sense = data["t_sense"]
		self.t_backoff = data["t_backoff"]
		self.t_listen = data["t_listen"]
		self.t_sleep = data["t_sleep"]
		self.t_data = data["t_data"]
		self.t_signal = data["t_signal"]
		self.t_busy = data["t_busy"]
		self.n_regions = data["n_regions"]
		self.n_max_attempts = data["n_max_attempts"]
		self.n_max_coll = data["n_max_coll"]
		self.n_max_sensing = data["n_max_sensing"]
		self.n_max_ack = data["n_max_ack"]
		self.t_delta = data["t_delta"]
		self.protocolVersion = data["protocolVersion"]


class Transmission:

	def __init__(self, data):
		self.Type = data['transmissionType']
		self.failed = data['failed']
		self.id = data['Id']
		self.sourceId = data['sourceId']
		self.destinationId = data['destinationId']

	def __str__(self):
		return f"{'{'}{self.Type};{self.failed};{self.id};S_{self.sourceId};D_{self.destinationId}{'}'}"

	def __repr__(self):
		return str(self)


class Relay:

	def __init__(self, data):
		# data
		self.id = data['id']
		self.X = data['X']
		self.Y = data['Y']
		self.range = data['range']
		self.status = data['status']
		self.COL_count = data['COL_count']
		self.SENSE_count = data['SENSE_count']
		self.ATTEMPT_count = data['ATTEMPT_count']
		self.ACK_count = data['ACK_count']
		self.isSensing = data['isSensing']
		self.hasSensed = data['hasSensed']
		self.neighboursIds = data['neighboursIds']
		self.packetToSendId = data['packetToSendId']
		self.BusyWithId = data['BusyWithId']
		self.activeTransmissions = []
		self.finishedTransmissions = []

		for item in data['activeTransmissions']:
			transmission = Transmission(item)
			self.activeTransmissions.append(transmission)

		for item in data['finishedTransmissions']:
			transmission = Transmission(item)
			self.finishedTransmissions.append(transmission)

	def __str__(self):
		return f"{'{'}{self.id};({self.X} . {self.Y}); ACTIVE: {self.activeTransmissions}{'}'}"

	def __repr__(self):
		return str(self)


class Plot:
	# style
	dot_radius = 1.6
	# annot = ax.annotate('aa', xy=(13, 13), xytext=(23, 13), color='black', weight='medium', ha='center', va='bottom', bbox=dict(boxstyle="round", fc="w"))
	# annot.set_visible(False)
	relay_colors = {
	    "Asleep": '#aaaaaa',  # grey
	    "Free": "#50bbaa",  # green
	    "Transmitting": 'blue',
	    "Sensing": '#3399ff',  # lightblue
	    "Awaiting_Signal": 'yellow',
	    "Backoff_Sensing": 'red',
	    "Backoff_CTS": '#ff9900'  # orange
	}

	signal_colors = {
	    "SINK_RTS": '#7bd1e2',  # lightblue
	    "RTS": 'blue',
	    "CTS": '#66ff66',  # light green
	    "PKT": '#ff00ff',  # fucsia
	    "COL": 'red',
	    "ACK": '#009933'  # dark green
	}

	@staticmethod
	def Init(data):
		Plot.protocol_params = ProtocolParameters(data["ProtocolParameters"])
		Plot.sim_params = SimulationParameters(data["SimulationParameters"])
		Plot.relay_details = {i: False for i in range(Plot.sim_params.n_nodes)}
		Plot.relay_markers = {}
		
		Plot.frames = data["Frames"]
		Plot.frame_index = 5

		# matplotlib
		Plot.fig = plt.figure()
		Plot.ax = Plot.fig.add_subplot(1, 1, 1)

		Plot.fig.canvas.mpl_connect('key_press_event', Plot.KeyPress)
		Plot.fig.canvas.mpl_connect('button_press_event', Plot.OnClick)

		Plot.plot()
		plt.show()

	@staticmethod
	def KeyPress(event):
		if(event.key=="right"):
			Plot.increaseIndex()
			Plot.plot()
		elif(event.key=="left"):
			Plot.decreaseIndex()
			Plot.plot()
		

	@staticmethod
	def OnClick(event):
		if (event.button==1):
			for r_id, marker in Plot.relay_markers.items():
				if marker.contains(event)[0]:
					Plot.relay_details[r_id] = not Plot.relay_details[r_id]
					break
		print('%s click: button=%d, x=%d, y=%d, xdata=%f, ydata=%f' %
		('double' if event.dblclick else 'single', event.button,
		event.x, event.y, event.xdata, event.ydata))

		Plot.plot()

	@staticmethod
	def plot():
		Plot.fig.clf()
		Plot.ax = Plot.fig.add_subplot(1, 1, 1)
		frame_plotter = Frame_plotter(Plot.frames[Plot.frame_index])
		frame_plotter.plot()
		Plot.fig.canvas.draw()
		#plt.savefig('sim_instant0.png')  # can set dpi=x

	@staticmethod
	def increaseIndex():
		print(Plot.sim_params.n_nodes)
		if Plot.frame_index < len(Plot.frames)-1:
			Plot.frame_index += 1
		else:
			print("End of simulation")

	@staticmethod
	def decreaseIndex():
		if Plot.frame_index > 0:
			Plot.frame_index -= 1
		else:
			print("Start of simulation")


class Frame_plotter:
	
	def __init__(self, frame):

		self.time = frame["time"]
		self.events = frame['events']
		self.finishedPackets = frame['finishedPackets']
		self.relays = {}

		for item in frame['relays']:
			relay = Relay(item)
			self.relays[relay.id] = relay


	def plot_relay(self, relay):
		relay_pos = (relay.X, relay.Y)
		relay_marker = plt.Circle(relay_pos, radius=Plot.dot_radius, facecolor=Plot.relay_colors[relay.status], edgecolor='black')
		Plot.ax.add_patch(relay_marker)
		Plot.relay_markers[relay.id] = relay_marker
		
		if Plot.relay_details[relay.id]:
			Plot.ax.annotate(str(relay), xy=(13, 13), xytext=relay_pos, color='black', weight='medium', ha='center', va='bottom', bbox=dict(boxstyle="round", fc="w"))

	def plot_signal(self, relay, signal=None):
		relay_pos = (relay.X, relay.Y)

		#if relay.status == 2: # status is a string
		print(relay.activeTransmissions)
		color = "#000000"
		transmissions = tuple(t for t in relay.activeTransmissions if t.sourceId==relay.id)

		print(relay.id, ":", transmissions)
		if len(transmissions) != 0:
			color = Plot.signal_colors[transmissions[0].Type]

		relay_range = plt.Circle(relay_pos, radius=relay.range, facecolor=color, alpha=0.15, edgecolor='black')
		Plot.ax.add_patch(relay_range)

	def plot(self):
		for key in self.relays:
			relay = self.relays[key]
			self.plot_signal(relay)
			self.plot_relay(relay)

		plt.axis('equal')
		axes = plt.gca()
		axes.set_xlim([-Plot.sim_params.range, Plot.sim_params.area_side + Plot.sim_params.range])
		axes.set_ylim([-Plot.sim_params.range, Plot.sim_params.area_side + Plot.sim_params.range])

		self.plot_legend()

	def plot_legend(self):
		patches = []
		for k,v in Plot.relay_colors.items():
			p = patch.Patch(color=v, label=k)
			patches.append(p)
		legend1 = plt.legend(handles=patches, loc=2)

		patches = []
		for k,v in Plot.signal_colors.items():
			p = patch.Patch(color=v, label=k)
			patches.append(p)
		plt.legend(handles=patches, loc=1)
		Plot.ax.add_artist(legend1)


if __name__ == "__main__":
	with open('debug.json') as f:
		data = json.load(f)
		Plot.Init(data)
