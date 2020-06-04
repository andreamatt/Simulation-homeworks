function range(n) {
	return [...Array(n).keys()]
}

const info_modality = {
	off: 0,
	active_transmissions: 1,
	finished_transmissions: 2,
	packet_info: 3
}

class SimulationParameters {

	constructor(data) {
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

	constructor(data) {
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

	constructor(data) {
		this.Type = data['transmissionType']
		this.failed = data['failed']
		this.id = data['Id']
		this.sourceId = data['sourceId']
		this.destinationId = data['destinationId']
		this.actualDestination = data['actualDestination']
	}
	// def __str__(self):
	// 	return f"{'{'}{self.Type};{self.failed};{self.id};S_{self.sourceId};D_{self.destinationId}{'}'}"

	// def __repr__(self):
	// 	return str(self)
}

class Packet {
	constructor(data) {
		this.generationTime = data['generationTime']
		this.Id = data['Id']
		this.startRelayId = data['startRelayId']
		this.sinkId = data['sinkId']
		this.result = data['result']
	}
	// def __str__(self) -> str:
	// 	return f"{'{'}{self.Id};SINK_{self.sinkId};{self.result}{'}'}"

	// def __repr__(self) -> str:
	// 	return str(self)
}

class Relay {

	constructor(data){
		let fields = data.split(";")
		this.id = parseInt(fields[0])
		this.X = Number(fields[1].replace(",","."))
		this.Y = Number(fields[2].replace(",","."))
		this.range = Number(fields[3].replace(",","."))
	}

	update(data) {
		let fields = data.split("|")
		this.status = parseInt(fields[0])
		this.transmissionType = parseInt(fields[1])
		this.transmissionDestinationId = parseInt(fields[2])
		this.REGION_index = parseInt(fields[3])
		this.COL_count = parseInt(fields[4])
		this.SENSE_count = parseInt(fields[5])
		this.SINK_RTS_count = parseInt(fields[6])
		this.PKT_count = parseInt(fields[7])
		this.REGION_cycle = parseInt(fields[8])
		this.isSensing = parseInt(fields[9])
		this.hasSensed = parseInt(fields[10])
		this.BusyWithId = parseInt(fields[11])
		this.ShouldBeAwake = parseInt(fields[12])
		// this.neighboursIds = fields[0]
		// this.finishedCTSs = []
		// this.packetToSend = data['packetToSend'] == None ? None : Packet(data['packetToSend'])

		// for (item of data['finishedCTSs']) {
		// 	this.finishedCTSs.append(Transmission(item))
		// }
	}
	// def __str__(self):
	// 	return f"{'{'}{self.id};({self.X} . {self.Y}); ACTIVE: {'}'}"

	// def __repr__(self):
	// 	return str(self)

	// def details(self, modality):
	// 	if modality == info_modality.active_transmissions.value:
	// 		return f"ACTIVE: {self.ShouldBeAwake}"
	// 	if modality == info_modality.finished_transmissions.value:
	// 		return f"FINISHED: {self.finishedCTSs}"
	// 	if modality == info_modality.packet_info.value:
	// 		return f"PACKET: {self.packetToSend}"
}
