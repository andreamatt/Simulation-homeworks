import matplotlib.pyplot as plt
import matplotlib.patches as patch
import json

with open('bin/Debug/debug.txt') as f:
	data = json.load(f)


class Sim_inst:

	def __init__(self, data_inst):
		self.relays = data_inst['relays']
		self.events = data_inst['events']
		self.finishedPackets = data_inst['finishedPackets']


class Sim_plotter:
	dot_radius = 1.2
	relay_colors = {
	    "asleep": '#aaaaaa',  #grey
	    "awake": "#50bbaa",  #green
	    "transmitting": 'blue',
	    "sensing": '',  #lightblue
	    "busy": 'yellow',
	    "in_backoff": 'red'
	}

	signal_colors = {
	    "SinkRTS": '',  # lightblue
	    "RTS": 'blue',
	    "CTS": '',  # light green
	    "PKT": '',  # fucsia
	    "COL": 'red',
	    "ACK": ''  # dark green 
	}

	def plot_relay(self, relay):
		relay_pos = (relay['X'], relay['Y'])
		relay_mark = plt.Circle(relay_pos, radius=self.dot_radius, facecolor=self.relay_colors["awake"] if relay['awake'] else self.relay_colors["awake"], edgecolor='black')
		plt.gca().add_patch(relay_mark)

	def plot_signal(self, relay, signal=None):
		relay_pos = (relay['X'], relay['Y'])
		relay_range = plt.Circle(relay_pos, radius=relay["range"] / 50, facecolor="000000", alpha=0.03, edgecolor='black')
		plt.gca().add_patch(relay_range)

	def plot_inst(self, data_inst):
		plt.axes()

		for relay in data_inst.relays:
			self.plot_relay(relay)
			self.plot_signal(relay)

			# REVIEW - TEST annotations
			if (relay["packetToSend"]):  # print packet under relay
				#plt.annotate("M"+ str(relay["packetToSend"]["Id"]), relay_pos, color='black', weight='bold', fontsize=14, ha='center', va='top')
				plt.annotate(
				    "P" + str(relay["packetToSend"]["Id"]),
				    fontsize=9,
				    xy=(relay['X'], relay['Y'] - 1),
				    xytext=(relay['X'] + 1, relay['Y'] - 5),
				    xycoords='data',
				    textcoords='data',
				    arrowprops=dict(arrowstyle="->", facecolor='black'),
				)

			plt.annotate(relay['id'], xy=(relay['X'] - 0.2, relay['Y'] + 1), color='black', weight='medium', fontsize=self.dot_radius * 8, ha='center', va='bottom')
			# maybe add other annotations to indicate some relay statuses
			print(relay)

		plt.axis('scaled')
		#plt.show()
		plt.savefig('sim_instant0.png') # can set dpi=x


instant = Sim_inst(data[0])
plotter = Sim_plotter()
plotter.plot_inst(instant)