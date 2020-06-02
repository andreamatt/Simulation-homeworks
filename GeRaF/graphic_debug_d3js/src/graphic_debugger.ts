// global.d.ts
// import * as _d3 from "d3"

// declare global {
//   const d3: typeof _d3
// }

// console.log("EDDAI")

enum info_modality {
  off = 0,
  active_transmissions = 1,
  finished_transmissions = 2,
  packet_info = 3
}

class SimulationParameters {
  max_time: string
  area_side: string
  range: string
  min_distance: string
  n_nodes: string
  packet_rate: string
  debug_interval: string
  debugType: string
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
  duty_cycle: string
  t_sense: string
  t_backoff: string
  t_listen: string
  t_sleep: string
  t_data: string
  t_signal: string
  t_busy: string
  n_regions: string
  n_max_coll: string
  n_max_sensing: string
  n_max_sink_rts: string
  n_max_pkt: string
  n_max_region_cycle: string
  t_delta: string
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
  failed: string
  id: string
  sourceId: string
  destinationId: string
  actualDestination: string

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
  neighboursIds: string
  BusyWithId: string
  finishedCTSs: string[]
  packetToSend: string

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
    this.finishedCTSs = []
    // this.packetToSend = None if data['packetToSend'] == None else Packet(data['packetToSend'])

    //   for item in data['finishedCTSs']:
    // 		transmission = Transmission(item)
    // 		self.finishedCTSs.append(transmission)
    // }

    // __str__()
    // __repr__()
    // details(modality)
  }
}

// MAIN SCRIPT //
import * as d3 from 'd3'

d3
  .select(".target")
  .style("stroke-width", 6)

d3
  .select("body").append("span")
  .text("Hello, world!")

d3.json("debug.json").then((data) => {
  console.log(data)
})