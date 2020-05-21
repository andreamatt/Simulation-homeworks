import matplotlib.pyplot as plt
import matplotlib.patches as patch
import json

with open('debug.json') as f:
    data = json.load(f)


class Transmission:
    def __init__(self, data):
        self.Type = data['transmissionType']
        self.failed = data['failed']
        self.id = data['Id']
        self.sourceId = data['sourceId']
        self.destinationId = data['destinationId']


class Relay:
    def __init__(self, data):
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
        self.marker = None

        for item in data['activeTransmissions']:
            transmission = Transmission(item)
            self.activeTransmissions.append(transmission)

        for item in data['finishedTransmissions']:
            transmission = Transmission(item)
            self.finishedTransmissions.append(transmission)

    def print(self):
        print(f'ID: {self.id}')
        print(f'X: {self.X}, Y: {self.Y}, Range: {self.range}')


class Sim_plotter:
    dot_radius = 1.2
    fig, ax = plt.subplots()
    relay_colors = {
        "asleep": '#aaaaaa',  # grey
        "awake": "#50bbaa",  # green
        "transmitting": 'blue',
        "sensing": '',  # lightblue
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

    annot = ax.annotate('aa', xy=(13, 13), xytext=(23, 13), color='black', weight='medium',
                        ha='center', va='bottom', bbox=dict(boxstyle="round", fc="w"))
    annot.set_visible(False)

    def __init__(self, data):

        self.events = data['events']
        self.finishedPackets = data['finishedPackets']
        self.relays = {}

        for item in data['relays']:
            relay = Relay(item)
            self.relays[relay.id] = relay

    def update_annot(self, relay, pos, ind):
        self.annot.set_position((pos[0] + 2, pos[1] + 2))
        self.annot.set_text(f'Id: {relay.id}')
        self.annot.get_bbox_patch().set_alpha(0.4)

    def onclick(self, event):
        print('')

    def hover(self, event):
        vis = self.annot.get_visible()
        if event.inaxes == self.ax:
            for key in self.relays:
                relay = self.relays[key]
                marker = relay.marker
                pos = (relay.X, relay.Y)
                cont, ind = marker.contains(event)
                if cont:
                    self.update_annot(relay, pos, ind)
                    self.annot.set_visible(True)
                    self.fig.canvas.draw_idle()
                else:
                    if vis:
                        self.annot.set_visible(False)
                        self.fig.canvas.draw_idle()

    def plot_relay(self, relay):
        relay_pos = (relay.X, relay.Y)
        relay_marker = plt.Circle(relay_pos, radius=self.dot_radius,
                                  facecolor=self.relay_colors["awake"] if not relay.status == 0 else self.relay_colors["asleep"], edgecolor='black')
        self.ax.add_patch(relay_marker)
        relay.marker = relay_marker

    def plot_signal(self, relay, signal=None):
        relay_pos = (relay.X, relay.Y)
        if relay.status == 2:
            relay_range = plt.Circle(
                relay_pos, radius=relay.range, facecolor="000000", alpha=0.03, edgecolor='black')
            self.ax.add_patch(relay_range)

    def plot_inst(self):
        for key in self.relays:
            relay = self.relays[key]
            self.plot_relay(relay)
            self.plot_signal(relay)

        self.fig.canvas.mpl_connect('motion_notify_event', self.hover)
        self.fig.canvas.mpl_connect('button_press_event', self.onclick)
        plt.axis('scaled')
        plt.show()
        plt.savefig('sim_instant0.png')  # can set dpi=x


plotter = Sim_plotter(data[32])
plotter.plot_inst()
