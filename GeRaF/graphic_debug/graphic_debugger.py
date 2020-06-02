import shapely.geometry as sg
import matplotlib.pyplot as plt
import descartes
import matplotlib.patches as patch
import json
from enum import Enum
import time
from matplotlib.widgets import Button


class info_modality(Enum):
	off = 0
	active_transmissions = 1
	finished_transmissions = 2
	packet_info = 3
	

class SimulationParameters:

	def __init__(self, data):
		self.max_time = data["max_time"]
		self.area_side = data["area_side"]
		self.range = data["range"]
		self.min_distance = data["min_distance"]
		self.n_nodes = data["n_nodes"]
		self.packet_rate = data["packet_rate"]
		self.debug_interval = data["debug_interval"]
		self.debugType = data["debugType"]
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
		self.n_max_coll = data["n_max_coll"]
		self.n_max_sensing = data["n_max_sensing"]
		self.n_max_sink_rts = data["n_max_sink_rts"]
		self.n_max_pkt = data["n_max_pkt"]
		self.n_max_region_cycle = data["n_max_region_cycle"]
		self.t_delta = data["t_delta"]
		self.protocolVersion = data["protocolVersion"]


class Transmission:

	def __init__(self, data):
		self.Type = data['transmissionType']
		self.failed = data['failed']
		self.id = data['Id']
		self.sourceId = data['sourceId']
		self.destinationId = data['destinationId']
		self.actualDestination = data['actualDestination']

	def __str__(self):
		return f"{'{'}{self.Type};{self.failed};{self.id};S_{self.sourceId};D_{self.destinationId}{'}'}"

	def __repr__(self):
		return str(self)

class Packet:
	def __init__(self, data):
		self.generationTime = data['generationTime']
		self.Id = data['Id']
		self.startRelayId = data['startRelayId']
		self.sinkId = data['sinkId']
		self.result = data['result']

	def __str__(self) -> str:
		return f"{'{'}{self.Id};SINK_{self.sinkId};{self.result}{'}'}"

	def __repr__(self) -> str:
		return str(self)

class Relay:

	def __init__(self, data):
		# data
		self.id = data['id']
		self.X = data['X']
		self.Y = data['Y']
		self.range = data['range']
		self.status = data['status']
		self.transmissionType = data['transmissionType']
		self.transmissionDestinationId = data['transmissionDestinationId']
		self.REGION_index = data['REGION_index']
		self.COL_count = data['COL_count']
		self.SENSE_count = data['SENSE_count']
		self.SINK_RTS_count = data['SINK_RTS_count']
		self.PKT_count = data['PKT_count']
		self.REGION_cycle = data['REGION_cycle']
		self.isSensing = data['isSensing']
		self.hasSensed = data['hasSensed']
		self.neighboursIds = data['neighboursIds']
		self.BusyWithId = data['BusyWithId']
		self.finishedCTSs = []
		self.packetToSend = None if data['packetToSend']==None else Packet(data['packetToSend'])

		for item in data['finishedCTSs']:
			transmission = Transmission(item)
			self.finishedCTSs.append(transmission)

	def __str__(self):
		return f"{'{'}{self.id};({self.X} . {self.Y}); ACTIVE: {'}'}"

	def __repr__(self):
		return str(self)

	def details(self, modality):
		if modality == info_modality.active_transmissions.value:
			return f"ACTIVE: "
		if modality == info_modality.finished_transmissions.value:
			return f"FINISHED: {self.finishedCTSs}"
		if modality == info_modality.packet_info.value:
			return f"PACKET: {self.packetToSend}"
		

class Plot:
	# style
	dot_radius = 1.6
	# annot = ax.annotate('aa', xy=(13, 13), xytext=(23, 13), color='black', weight='medium', ha='center', va='bottom', bbox=dict(boxstyle="round", fc="w"))
	# annot.set_visible(False)
	relay_colors = {
		"Asleep": '#aaaaaa',  # grey
		"Free": "#50bbaa",  # green
		"Awaiting_region": 'brown',
		"Transmitting": 'blue',
		"Sensing": '#3399ff',  # lightblue
		"Awaiting_Signal": 'yellow',
		"Backoff_Sensing": 'red',
		"Backoff_CTS": '#ff9900',  # orange
		"Backoff_SinkRTS": '#ffb909'  # orange
	}

	signal_colors = {
		"SINK_RTS": '#7bd1e2',  # lightblue
		"RTS": 'blue',
		"CTS": '#66ff66',  # light green
		"PKT": '#ff00ff',  # fucsia
		"SINK_COL": 'orange',
		"COL": 'red',
		"ACK": '#009933'  # dark green
	}

	@staticmethod
	def Init(data):
		Plot.protocol_params = ProtocolParameters(data["ProtocolParameters"])
		Plot.sim_params = SimulationParameters(data["SimulationParameters"])
		Plot.Distances = data['Distances']
		Plot.relay_details = {i: 0 for i in range(Plot.sim_params.n_nodes)}
		Plot.relay_markers = {}
		Plot.show_Ids = False
		Plot.frames = data["Frames"]
		Plot.frame_index = 0
		for i in range(len(Plot.frames)):
			if Plot.frames[i]['currentEvent']['type']=='PacketGenerationEvent':
				Plot.frame_index = i
				break

		# zoom and drag
		Plot.center = (Plot.sim_params.area_side/2, Plot.sim_params.area_side/2)
		Plot.scale = 1

		# matplotlib
		Plot.last_time = 0
		Plot.fig = plt.figure()
		Plot.ax1 = Plot.fig.add_subplot(1, 1, 1)

		Plot.fig.canvas.mpl_connect('key_press_event', Plot.KeyPress)
		Plot.fig.canvas.mpl_connect('button_press_event', Plot.OnClick)
		Plot.fig.canvas.mpl_connect('scroll_event', Plot.OnScroll)

		Plot.background = None
		Plot.plot()
		plt.show()

	@staticmethod
	def KeyPress(event):
		if(event.key=="2"):
			Plot.increaseIndex()
		elif(event.key=="1"):
			Plot.decreaseIndex()
		elif(event.key == "-"):
			Plot.scale += 0.1
		elif(event.key == "+"):
			Plot.scale = max((0.01, Plot.scale-0.1))
		elif(event.key == "down"):
			Plot.center = (Plot.center[0], Plot.center[1] - 10)
		elif(event.key == "right"):
			Plot.center = (Plot.center[0] + 10, Plot.center[1])
		elif(event.key == "up"):
			Plot.center = (Plot.center[0], Plot.center[1] + 10)
		elif(event.key == "left"):
			Plot.center = (Plot.center[0] - 10, Plot.center[1])
		elif(event.key == "i"):
			Plot.show_Ids = not Plot.show_Ids

		Plot.plot()

	@staticmethod
	def OnScroll(event):
		if (event.button  == "up"):
			Plot.scale = max((0.01, Plot.scale-0.05))
		elif (event.button  == "down"):
			Plot.scale += 0.05
		Plot.plot()	

	@staticmethod
	def OnClick(event):
		if (event.button==1):
			for r_id, marker in Plot.relay_markers.items():
				if marker.contains(event)[0]:
					Plot.relay_details[r_id] = 0 if Plot.relay_details[r_id] == 3 else Plot.relay_details[r_id] + 1
					break
		print('%s click: button=%d, x=%d, y=%d, xdata=%f, ydata=%f' %
		('double' if event.dblclick else 'single', event.button,
		event.x, event.y, event.xdata, event.ydata))

		Plot.plot()

	@staticmethod
	def plot():
		time_before_draw = time.time()
		Plot.fig.clf()
		Plot.ax1 = Plot.fig.add_subplot(1, 1, 1)
		frame_plotter = Frame_plotter(Plot.frames[Plot.frame_index])
		frame_plotter.plot()
		
		Plot.fig.canvas.draw()


		Plot.last_time = time.time() - time_before_draw
		#plt.savefig('sim_instant0.png')  # can set dpi=x

	@staticmethod
	def increaseIndex():
		#print(Plot.sim_params.n_nodes)
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
		self.currentEvent = frame['currentEvent']
		self.finishedPackets = frame['finishedPackets']
		self.relays = {}

		for item in frame['relays']:
			relay = Relay(item)
			self.relays[relay.id] = relay


	def plot_relay(self, relay):
		relay_pos = (relay.X, relay.Y)
		text_pos = (relay.X, relay.Y + 2)
		relay_marker = plt.Circle(relay_pos, radius=Plot.dot_radius, facecolor=Plot.relay_colors[relay.status], edgecolor='black')
		Plot.ax1.add_patch(relay_marker)
		Plot.relay_markers[relay.id] = relay_marker
		
		if Plot.show_Ids:
			Plot.ax1.annotate(relay.id, xy=relay_pos, xytext=text_pos, color='black', weight='medium', ha='center', va='bottom', bbox=dict(boxstyle="round", fc="w"))


		modality = Plot.relay_details[relay.id]
			
		if modality != info_modality.off.value:
			Plot.ax1.annotate(relay.details(modality), xy=relay_pos, xytext=text_pos, color='black', weight='medium', ha='center', va='bottom', bbox=dict(boxstyle="round", fc="w"))		
			if modality == info_modality.packet_info.value and relay.packetToSend != None:
				packet = relay.packetToSend
				sink = self.relays[packet.sinkId]
				Plot.ax1.arrow(relay.X, relay.Y, sink.X - relay.X, sink.Y - relay.Y, length_includes_head=True, head_width=2, head_length=3, capstyle="butt", ls="-")

	def plot_arrow(self, relay):
		if relay.status == "Transmitting" and relay.transmissionType == 'PKT':
			destination = self.relays[relay.transmissionDestinationId]
			Plot.ax1.arrow(relay.X, relay.Y, destination.X - relay.X, destination.Y - relay.Y, length_includes_head=True, head_width=2, head_length=3, capstyle="butt", ls="--")

	def plot_signal(self, relay, signal=None):
		relay_pos = (relay.X, relay.Y)

		if relay.status == "Transmitting":
			color = Plot.signal_colors[relay.transmissionType]
			alpha=0.15
			relay_range = plt.Circle(relay_pos, radius=relay.range, facecolor=color, alpha=alpha, edgecolor='black')
			Plot.ax1.add_patch(relay_range)

	def plot_regions(self, relay):
		if relay.status == "Transmitting" and relay.transmissionType=="RTS":
			regions = Plot.protocol_params.n_regions
			region_index = relay.REGION_index
			sink = self.relays[relay.packetToSend.sinkId]
			# create the circles with shapely
			dist = Plot.Distances[str(relay.id)][str(sink.id)]
			a = sg.Point(sink.X,sink.Y).buffer(dist)
			b = sg.Point(relay.X,relay.Y).buffer(relay.range)
			starting = a.intersection(b)
			for i in range(1, regions+1):
				a = sg.Point(sink.X,sink.Y).buffer(dist - i/regions * relay.range)
				diff = starting.difference(a)
				if region_index==regions-i:
					Plot.ax1.add_patch(descartes.PolygonPatch(diff, fc='#00ff00', ec='k', alpha=0.15))
				else:
					Plot.ax1.add_patch(descartes.PolygonPatch(diff, fc='#000000', ec='k', alpha=0.10))
				starting = starting.intersection(a)

	def plot(self):
		min_x = Plot.center[0] - Plot.sim_params.area_side*Plot.scale - Plot.sim_params.range
		max_x = Plot.center[0] + Plot.sim_params.area_side*Plot.scale + Plot.sim_params.range
		min_y = Plot.center[1] - Plot.sim_params.area_side*Plot.scale - Plot.sim_params.range
		max_y = Plot.center[1] + Plot.sim_params.area_side*Plot.scale + Plot.sim_params.range

		Plot.ax1.add_patch(plt.Rectangle((0, 0), Plot.sim_params.area_side, Plot.sim_params.area_side, fill=None, alpha=1))
		for relay in self.relays.values():
			#if relay.X < max_x and relay.X > min_x and relay.Y < max_y and relay.Y > min_y:
			self.plot_arrow(relay)
			self.plot_signal(relay)
			self.plot_relay(relay)
			self.plot_regions(relay)


		axes = Plot.ax1
		Plot.ax1.axis('scaled')
		axes.set_xlim([min_x, max_x])
		axes.set_ylim([min_y, max_y])
		
		self.plot_legend(min_x, min_y)

	def plot_legend(self, min_x, min_y):
		patches = []
		for k,v in Plot.relay_colors.items():
			p = patch.Patch(color=v, label=k)
			patches.append(p)
		legend_relays = plt.legend(handles=patches, loc=2)


		patches = [
			patch.Patch(label=f"Time: {self.time: 5g}"),
			patch.Patch(label=f"Frame: {Plot.frame_index}/{len(Plot.frames)}"),
			patch.Patch(label=f"Show IDs: {Plot.show_Ids}"),
			patch.Patch(label=f"Plot time: {Plot.last_time: 3g}")
		]
		legend_status = plt.legend(handles=patches, loc=3, handlelength=0, handletextpad=0)

		
		patches = []
		for k,v in Plot.signal_colors.items():
			p = patch.Patch(color=v, label=k)
			patches.append(p)
		plt.legend(handles=patches, loc=1)

		Plot.ax1.add_artist(legend_relays)
		Plot.ax1.add_artist(legend_status)
			

def print_event(ev):
	print("Printevent: ")		

if __name__ == "__main__":	 
	with open('debug.json') as f:
		data = json.load(f)
		Plot.Init(data)
