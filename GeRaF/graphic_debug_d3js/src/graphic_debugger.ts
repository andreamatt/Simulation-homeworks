import * as fs from 'fs'
import * as d3 from 'd3'

enum info_modality {
  off = 0,
  active_transmissions = 1,
  finished_transmissions = 2,
  packet_info = 3
}

class SimulationParameters {
  max_time: number
  area_side: number
  range: number
  min_distance: number
  n_nodes: number
  packet_rate: number
  debug_interval: number
  debugType: number
  debug_file: string

  constructor(data: any) {
    this.max_time = data["max_time"]
    this.area_side = data["area_side"]
    this.range = data["range"]
    this.min_distance = data["min_distance"]
    this.n_nodes = data["n_nodes"]
    this.packet_rate = data["packet_rate"]
    this.debug_interval = data["debug_interval"]
    this.debugType = data["debugType"]
    this.debug_file = data["debug_file"]
  }
}

class ProtocolParameters {
  duty_cycle: number
  t_sense: number
  t_backoff: number
  t_listen: number
  t_sleep: number
  t_data: number
  t_signal: number
  t_busy: number
  n_regions: number
  n_max_coll: number
  n_max_sensing: number
  n_max_sink_rts: number
  n_max_pkt: number
  n_max_region_cycle: number
  t_delta: number
  protocolVersion: string

  constructor(data: any) {
    this.duty_cycle = data["duty_cycle"]
    this.t_sense = data["t_sense"]
    this.t_backoff = data["t_backoff"]
    this.t_listen = data["t_listen"]
    this.t_sleep = data["t_sleep"]
    this.t_data = data["t_data"]
    this.t_signal = data["t_signal"]
    this.t_busy = data["t_busy"]
    this.n_regions = data["n_regions"]
    this.n_max_coll = data["n_max_coll"]
    this.n_max_sensing = data["n_max_sensing"]
    this.n_max_sink_rts = data["n_max_sink_rts"]
    this.n_max_pkt = data["n_max_pkt"]
    this.n_max_region_cycle = data["n_max_region_cycle"]
    this.t_delta = data["t_delta"]
    this.protocolVersion = data["protocolVersion"]
  }
}

class Transmission {
  Type: string
  failed: boolean
  id: number
  sourceId: number
  destinationId: number
  actualDestination: boolean

  constructor(data: any) {
    this.Type = data['transmissionType']
    this.failed = data['failed']
    this.id = data['Id']
    this.sourceId = data['sourceId']
    this.destinationId = data['destinationId']
    this.actualDestination = data['actualDestination']
  }

  // __str__()
  // __repr__()
}

class Packet {
  generationTime: string
  Id: string
  startRelayId: string
  sinkId: string
  result: string

  constructor(data: any) {
    this.generationTime = data['generationTime']
    this.Id = data['Id']
    this.startRelayId = data['startRelayId']
    this.sinkId = data['sinkId']
    this.result = data['result']
  }

  // __str__()
  // __repr__()
}

class Relay {
  id: string
  X: string
  Y: string
  range: string
  status: string
  transmissionType: string
  transmissionDestinationId: string
  REGION_index: string
  COL_count: string
  SENSE_count: string
  SINK_RTS_count: string
  PKT_count: string
  REGION_cycle: string
  isSensing: string
  hasSensed: string
  neighboursIds: string[]
  BusyWithId: string
  finishedCTSs: Transmission[]
  packetToSend: Packet

  constructor(data: any) {
    this.id = data['id']
    this.X = data['X']
    this.Y = data['Y']
    this.range = data['range']
    this.status = data['status']
    this.transmissionType = data['transmissionType']
    this.transmissionDestinationId = data['transmissionDestinationId']
    this.REGION_index = data['REGION_index']
    this.COL_count = data['COL_count']
    this.SENSE_count = data['SENSE_count']
    this.SINK_RTS_count = data['SINK_RTS_count']
    this.PKT_count = data['PKT_count']
    this.REGION_cycle = data['REGION_cycle']
    this.isSensing = data['isSensing']
    this.hasSensed = data['hasSensed']
    this.neighboursIds = data['neighboursIds']
    this.BusyWithId = data['BusyWithId']

    if (data['packetToSend'] != null) {
      this.packetToSend = new Packet(data['packetToSend'])
    }

    for (const item of data['finishedCTSs']) {
      this.finishedCTSs.push(new Transmission(item))
    }

    // __str__()
    // __repr__()
    // details(modality)
  }
}

export class Plot {
  dot_radius: number = 1.6
  protocolParameters: ProtocolParameters
  sim_params: SimulationParameters
  distances: {}
  //relay_details: any
  //relay_markers: {}
  show_Ids: boolean
  frame_index: number
  frames: any[]
  show_dutyCicle: boolean
  frame_plotter: Frame_plotter

  relay_colors = {
    "Asleep": '#aaaaaa',  // grey
    "Free": "#50bbaa",  // green
    "Awaiting_region": 'brown',
    "Transmitting": 'blue',
    "Sensing": '#3399ff',  // lightblue
    "Awaiting_Signal": 'yellow',
    "Backoff_Sensing": 'red',
    "Backoff_CTS": '#ff9900',  // orange
    "Backoff_SinkRTS": '#ffb909'  // orange
  }

  signal_colors = {
    "SINK_RTS": '#7bd1e2',  // lightblue
    "RTS": 'blue',
    "CTS": '#66ff66',  // light green
    "PKT": '#ff00ff',  // fucsia
    "SINK_COL": 'orange',
    "COL": 'red',
    "ACK": '#009933'  // dark green
  }

  constructor(data: any) {
    this.protocolParameters = new ProtocolParameters(data["ProtocolParameters"])
    this.sim_params = new SimulationParameters(data["SimulationParameters"])
    this.distances = data['Distances']
    //this.relay_details = { i: 0 for i in range(Plot.sim_params.n_nodes) }
    //this.relay_markers = {}
    this.show_Ids = false
    this.show_dutyCicle = false
    this.frames = data["Frames"]
    this.frame_index = 0

    this.frame_plotter = new Frame_plotter(this.frames[this.frame_index])
  }

}

class Frame_plotter {
  time: number
  //currentEvent
  finishedPackets: any
  relays: Relay[]

  constructor(frame: any) {
    this.time = frame["time"]
    //this.currentEvent = frame['currentEvent']
    this.finishedPackets = frame['finishedPackets']
    this.relays = []

    for (const item of frame['relays']) {
      this.relays.push(new Relay(item))
    }

  }
}

export const loadJSON = (path: string) => {
  let data_file: Buffer = fs.readFileSync(path)
  let data = JSON.parse(data_file.toString())
  return data
}