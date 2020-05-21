import matplotlib.pyplot as plt
import matplotlib.patches as patch
import json
from os.path import expanduser as ex


with open('debug.json') as f:
    data = json.load(f)


class Sim_inst:

    def __init__(self, data):

        self.events = data['events']
        self.finishedPackets = data['finishedPackets']
        self.relays = {}

        for item in data['relays']:
            relay = Node(item)
            self.relays[relay.id] = relay


class Transmission:
    def __init__(self, data):
        self.Type = data['transmissionType']
        self.failed = data['failed']
        self.id = data['Id']
        self.sourceId = data['sourceId']
        self.destinationId = data['destinationId']


class Node:
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

    def plot_relay(self, relay):
        relay_pos = (relay.X, relay.Y)
        relay_mark = plt.Circle(relay_pos, radius=self.dot_radius,
                                facecolor=self.relay_colors["awake"] if not relay.status == 0 else self.relay_colors["asleep"], edgecolor='black')
        plt.gca().add_patch(relay_mark)

    def plot_signal(self, relay, signal=None):
        relay_pos = (relay.X, relay.Y)
        relay_range = plt.Circle(
            relay_pos, radius=relay.range, facecolor="000000", alpha=0.03, edgecolor='black')
        plt.gca().add_patch(relay_range)

    def plot_inst(self, data_inst):
        plt.axes()

        for key in data_inst.relays:
            self.plot_relay(data_inst.relays[key])
            # self.plot_signal(data_inst.relays[key])

    '''        
    # REVIEW - TEST annotations
    if (relay["packetToSend"]):  # print packet under relay
        # plt.annotate("M"+ str(relay["packetToSend"]["Id"]), relay_pos, color='black', weight='bold', fontsize=14, ha='center', va='top')
        plt.annotate(
            "P" + str(relay["packetToSend"]["Id"]),
            fontsize=9,
            xy=(relay['X'], relay['Y'] - 1),
            xytext=(relay['X'] + 1, relay['Y'] - 5),
            xycoords='data',
            textcoords='data',
            arrowprops=dict(arrowstyle="->", facecolor='black'),
        )
    '''

    plt.axis('equal')
    plt.show()
    plt.savefig('sim_instant0.png')  # can set dpi=x


instant = Sim_inst(data[0])
plotter = Sim_plotter()
plotter.plot_inst(instant)

'''
for key in instant.relays:
    instant.relays[key].print()
'''
