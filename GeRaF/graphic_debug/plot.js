const PlayMode = {
	FrameTime: 0,
	Speed: 1
}

class Plot {
	// style
	relay_colors = [
		'#aaaaaa',  // grey, Asleep
		'#50bbaa',  // green, Free
		'#996633', // Awaiting_region
		'#1a53ff', //Transmitting
		'#3399ff',  // lightblue Sensing
		'#e6e600', // Awaiting_Signal
		'#e60000', // Backoff_Sensing
		'#ff9900',  // orange Backoff_CTS
		'#ffb909'  // orang-ish Backoff_SinkRTS
	]

	signal_colors = [
		'#7bd1e2',  // lightblue SINK_RTS
		'#1a1aff', // RTS
		'#66ff66',  // light green CTS
		'#ff00ff',  // fucsia PKT
		'#ff751a', // SINK_COL
		'#ff3333', // COL
		'#009933'  // dark green ACK
	]

	constructor(data) {
		let data_sections = data.split('#').map(d => d.trim())

		this.sim_params = new SimulationParameters(JSON.parse(data_sections[0]))
		this.protocol_params = new ProtocolParameters(JSON.parse(data_sections[1]))
		this.relays_dict = {}
		for (let l of data_sections[2].split(/\r?\n/)) {
			l = l.trim()
			let r = new Relay(l)
			this.relays_dict[r.id] = r
		}
		this.relays = Object.values(this.relays_dict)

		this.distances = JSON.parse(data_sections[3])
		this.frame_lines = data_sections[4].split(/\r?\n/).map(d => d.trim())

		this.packets = {}
		for (let l of data_sections[5].split(/\r?\n/)) {
			let p = JSON.parse(l)
			let content_id = parseInt(p['content_id'])
			if (!(content_id in this.packets)) {
				this.packets[content_id] = {}
			}
			this.packets[content_id][parseInt(p['copy_id'])] = p
		}

		this.relay_details = {}
		for (let r of this.relays) {
			this.relay_details[r.id] = InfoModality.off
		}
		this.show_Ids = false
		this.show_dutyCicle = true
		this.frame_index = 0
		this.playMode = PlayMode.FrameTime

		// zoom and drag
		this.scale = width / this.sim_params.area_side

		// d3 selection
		this.svg = graph	// d3.select(#mainplot)

		// limit square
		this.svg.select('.area_limit')
			.append('rect')
			.attr('x', 0)
			.attr('y', 0)
			.attr('width', this.sim_params.area_side * this.scale)
			.attr('height', this.sim_params.area_side * this.scale)
			.style('fill', '#ffffff')
			.style('stroke', 'black')
			.style('stroke-width', 0.2 * this.scale)

		// arrow head marker
		graph.append('marker')
			.attr('id', 'arrow')
			.attr('markerWidth', 1 * this.scale)
			.attr('markerHeight', 1 * this.scale)
			.attr('refX', 0.9 * this.scale)
			.attr('refY', 0.3 * this.scale)
			.attr('orient', 'auto')
			.attr('markerUnits', 'strokeWidth')
			.append('path')
			.attr('d', 'M0,0 L0,6 L9,3 z')
			.attr('fill', 'stroke')
	}

	updateIndex(i) {
		if ((i >= 0) && (i < this.frame_lines.length)) {
			this.frame_index = i
		} else {
			console.log('INVALID ARGUMENT: index out of boundaries')
		}
		console.log('index: ' + this.frame_index)
		this.plot()
	}

	plot() {
		let time_before_draw = Date.now()

		// update relays based on index
		let frame = this.frame_lines[this.frame_index]
		let frame_sections = frame.split(';')
		this.frame_time = frame_sections[0]

		this.frame_event = JSON.parse(frame_sections[1])
		for (let r of this.relays) {
			r.update(frame_sections[parseInt(r.id) + 2], this.packets)
		}
		// console.log(this.relays)

		let circle_dots = this.svg.select('.circle_dots').selectAll('.circle_dot')
			.data(this.relays, r => r.id)
		let circle_ranges = this.svg.select('.circle_ranges').selectAll('.circle_range')
			.data(this.relays.filter(r => r.actualStatus == RelayStatus.Transmitting), r => r.id)
		let packet_arrows = this.svg.select('.packet_arrows').selectAll('.packet_arrow')
			.data(this.relays.filter(r => r.actualStatus == RelayStatus.Transmitting && r.transmissionType == TransmissionType.PKT), r => r.id)
		let sink_arrows = this.svg.select('.sink_arrows').selectAll('.sink_arrow')
			.data(this.relays.filter(r => r.packetToSend != null && this.relay_details[r.id] == InfoModality.packet_info), r => r.id)
		let regions = this.svg.select('.regions').selectAll('.region')
			.data(this.relays.filter(r => r.actualStatus == RelayStatus.Transmitting && r.transmissionType == TransmissionType.RTS), r => r.id)

		// transmission ranges
		circle_ranges.exit().remove()

		circle_ranges.enter()
			.append('circle')
			.attr('class', 'circle_range')
			.attr('relay_id', r => r.id)
			.attr('cx', r => r.X * this.scale)
			.attr('cy', r => r.Y * this.scale)
			.attr('r', r => r.range * this.scale)
			.style('fill', r => this.signal_colors[r.transmissionType] + '20')

		circle_ranges
			.style('fill', r => this.signal_colors[r.transmissionType] + '20')


		// relay dots
		circle_dots.enter()
			.append('circle')
			.attr('class', 'circle_dot')
			.attr('relay_id', r => r.id)
			.attr('cx', r => r.X * this.scale)
			.attr('cy', r => r.Y * this.scale)
			.attr('r', r => 2 * this.scale)
			.style('fill', r => this.relay_colors[r.actualStatus])
			.attr('stroke', 'black')
			.attr('stroke-width', 0.2 * this.scale)

		circle_dots
			.style('fill', r => this.relay_colors[r.actualStatus])

		// pkt arrows
		packet_arrows.exit().remove()

		packet_arrows.enter()
			.append('line')
			.attr('class', 'packet_arrow')
			.attr('relay_id', r => r.id)
			.attr('x1', r => r.X * this.scale)
			.attr('y1', r => r.Y * this.scale)
			.attr('x2', r => this.relays_dict[r.transmissionDestinationId].X * this.scale)
			.attr('y2', r => this.relays_dict[r.transmissionDestinationId].Y * this.scale)
			.attr('stroke', 'black')
			.attr('stroke-width', 0.2 * this.scale)
			.attr('marker-end', 'url(#arrow)');


		// sink arrows
		sink_arrows.exit().remove()

		sink_arrows.enter()
			.append('line')
			.attr('class', 'sink_arrow')
			.attr('relay_id', r => r.id)
			.attr('x1', r => r.X * this.scale)
			.attr('y1', r => r.Y * this.scale)
			.attr('x2', r => this.relays_dict[r.transmissionDestinationId].X * this.scale)
			.attr('y2', r => this.relays_dict[r.transmissionDestinationId].Y * this.scale)
			.attr('stroke', 'grey')
			.attr('stroke-width', 0.2 * this.scale)
			.attr('stroke-dasharray', 0.4 * this.scale)
			.attr('marker-end', 'url(#arrow)');


		// regions
		regions.exit().remove()

		let region_groups = regions.enter()
			.append('g')
			.attr('class', 'region')
			.attr('relay_id', r => r.id)

		let region_indeces = range(this.protocol_params.n_regions)
		for (let i of region_indeces) {
			let masks = region_groups
				.append('mask')
				.attr('id', r => `mask_${r.id}_${i}`)

			// mask with relay range
			masks.append('circle')
				.attr('cx', r => r.X * this.scale)
				.attr('cy', r => r.Y * this.scale)
				.attr('r', r => r.range * this.scale)
				.style('stroke', 'none')
				.style('fill', '#ffffff')

			// mask with sink range
			masks.append('circle')
				.attr('cx', r => this.relays_dict[r.packetToSend.sinkId].X * this.scale)
				.attr('cy', r => this.relays_dict[r.packetToSend.sinkId].Y * this.scale)
				.attr('r', r => (this.distances[r.id][r.packetToSend.sinkId] - r.range + i * r.range / this.protocol_params.n_regions) * this.scale)
				.style('stroke', 'none')
				.style('fill', '#000000')

			// circle that fills
			region_groups.append('circle')
				.attr('cx', r => this.relays_dict[r.packetToSend.sinkId].X * this.scale)
				.attr('cy', r => this.relays_dict[r.packetToSend.sinkId].Y * this.scale)
				.attr('r', r => (this.distances[r.id][r.packetToSend.sinkId] - r.range + (i + 1) * r.range / this.protocol_params.n_regions) * this.scale)
				.attr('mask', r => `url(#mask_${r.id}_${i})`)
				.style('fill', r => r.REGION_index == i ? '#00e00080' : '#0000e080')
		}


		console.log('render time: ' + (Date.now() - time_before_draw))
	}

	startPlaying() {
		if (this.playMode == PlayMode.FrameTime) {
			if (this.frameTime == undefined) {
				this.frameTime = parseInt(document.getElementById('frame_time_slider').value)
			}
			this.interval = d3.interval(time => {
				this.updateIndex(this.frame_index + 1)
				if (this.frame_index == this.frame_lines.length - 1) {
					this.interval.stop()
				}
			}, this.frameTime)
		}
		else {
			if (this.frameTime == undefined) {
				this.frameTime = parseInt(document.getElementById('frame_time_slider').value)
			}

		}
	}

	stopPlaying() {
		this.interval.stop()
		this.interval = undefined
	}

	updateFrameTime() {
		var slider = document.getElementById('frame_time_slider')
		var output = document.getElementById('frame_time_output')
		output.innerHTML = 'Frame time: ' + slider.value
		this.frameTime = parseInt(slider.value)

		// restart playing
		if (this.interval) {
			this.stopPlaying()
			this.startPlaying()
		}
	}

	updateSpeed() {
		var slider = document.getElementById('speed_slider')
		var output = document.getElementById('speed_output')
		output.innerHTML = 'Speed: ' + slider.value + 'x'
		this.frameTime = parseInt(slider.value)

		// restart playing
		if (this.interval) {
			this.stopPlaying()
			this.startPlaying()
		}
	}

	changePlayMode() {
		this.playMode = d3.select('#frameInterval').property('checked') ? PlayMode.FrameTime : PlayMode.Speed
		if (this.playMode == PlayMode.FrameTime) {
			d3.select('#speed_slider').style('visibility', 'hidden')
			d3.select('#speed_output').style('visibility', 'hidden')
			d3.select('#frame_time_slider').style('visibility', 'visible')
			d3.select('#frame_time_output').style('visibility', 'visible')
		}
		else {
			d3.select('#frame_time_slider').style('visibility', 'hidden')
			d3.select('#frame_time_output').style('visibility', 'hidden')
			d3.select('#speed_slider').style('visibility', 'visible')
			d3.select('#speed_output').style('visibility', 'visible')
		}

		if (this.interval) {
			this.stopPlaying()
			this.startPlaying()
		}
	}
}
