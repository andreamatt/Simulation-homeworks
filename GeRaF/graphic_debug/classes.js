function range(n) {
	return [...Array(n).keys()]
}

const InfoModality = {
	off: 0,
	active_transmissions: 1,
	finished_transmissions: 2,
	packet_info: 3
}

const RelayStatus = {
	Asleep: 0,
	Free: 1,   // awake with no task
	Awaiting_region: 2,
	Transmitting: 3,
	Sensing: 4,
	Awaiting_Signal: 5,
	Backoff_Sensing: 6,
	Backoff_CTS: 7,
	Backoff_SinkRTS: 8
}

const Result = {
	None: 0,
	Success: 1,
	No_start_relays: 2,
	//Abort_max_attempts,
	Abort_max_region_cycle: 3,
	Abort_max_sensing: 4,
	Abort_max_sink_rts: 5,
	Abort_no_ack: 6
}

const TransmissionType = {
	SINK_RTS: 0,
	RTS: 1,
	CTS: 2,
	PKT: 3,
	SINK_COL: 4,
	COL: 5,
	ACK: 6
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

	constructor(data) {
		let fields = data.split(";")
		this.id = parseInt(fields[0])
		this.X = Number(fields[1].replace(",", "."))
		this.Y = Number(fields[2].replace(",", "."))
		this.range = Number(fields[3].replace(",", "."))

		this.status = 0
		this.ShouldBeAwake = 0
		this.transmissionType = 0
		this.transmissionDestinationId = -1
		this.REGION_index = 0
		this.COL_count = 0
		this.SENSE_count = 0
		this.SINK_RTS_count = 0
		this.PKT_count = 0
		this.REGION_cycle = 0
		this.BusyWithId = -1
		this.packetContentId = -1
		this.packetCopyId = -1
		this.info_modality = InfoModality.off
	}

	update(fields, packets) {
		this.status = parseInt(fields[1])
		this.ShouldBeAwake = parseInt(fields[2])
		if (fields.length > 3) {
			this.transmissionType = parseInt(fields[3])
			this.transmissionDestinationId = parseInt(fields[4])
			if (fields.length > 5) {
				this.REGION_index = parseInt(fields[5])
				this.COL_count = parseInt(fields[6])
				this.SENSE_count = parseInt(fields[7])
				this.SINK_RTS_count = parseInt(fields[8])
				this.PKT_count = parseInt(fields[9])
				this.REGION_cycle = parseInt(fields[10])
				this.BusyWithId = parseInt(fields[11])
				this.packetContentId = fields[12] == "" ? -1 : parseInt(fields[12])
				this.packetCopyId = fields[13] == "" ? -1 : parseInt(fields[13])
			}
		}
		if (this.packetContentId != -1) {
			if (packets[this.packetContentId] == undefined) {
				console.log("Missing packet contentID: " + this.packetContentId)
			}
			else if (packets[this.packetContentId][this.packetCopyId] == undefined) {
				console.log("Missing packet copyID: " + this.packetContentId + "." + this.packetCopyId)
			}
		}
		this.packetToSend = this.packetContentId == -1 ? null : packets[this.packetContentId][this.packetCopyId]
	}

	get actualStatus() {
		if (this.status != 0) return this.status
		return this.ShouldBeAwake
	}

	// def __str__(self):
	// 	return f"{'{'}{self.id};({self.X} . {self.Y}); ACTIVE: {'}'}"

	// def __repr__(self):
	// 	return str(self)

	details() {
		if (this.info_modality == InfoModality.active_transmissions) return `ACTIVE`
		if (this.info_modality == InfoModality.finished_transmissions) return `FINISHED`
		if (this.info_modality == InfoModality.packet_info) return `PACKET: ${this.packetToSend}`

	}
}
