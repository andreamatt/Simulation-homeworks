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
		let other_lines = data_sections[4].split(/\r?\n/).map(d => d.trim())
		this.frame_lines = other_lines.filter(l => l[0] == 'F').map(l => l.substring(2))

		let packet_lines = other_lines.filter(l => l[0] == 'P').map(l => l.substring(2))
		this.packets = {}
		for (let l of packet_lines) {
			let p = new Packet(l)
			if (!(p.content_id in this.packets)) {
				this.packets[p.content_id] = {}
			}
			this.packets[p.content_id][p.copy_id] = p
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
		this.svg.select('#area_limit')
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
			.attr('markerWidth', 10)
			.attr('markerHeight', 10)
			.attr('refX', 7)
			.attr('refY', 3)
			.attr('orient', 'auto')
			.attr('markerUnits', 'strokeWidth')
			.append('path')
			.attr('d', 'M0,0 L0,6 L9,3 z')
			.attr('fill', 'stroke')


		this.dragHandler = d3.drag()
			.on("drag", function (d) {
				var x = d3.event.x;
				var y = d3.event.y;
				d3.select(this).attr("transform", "translate(" + x + "," + y + ")")
					.select('line')
					.attr('x2', r => r.X * plot.scale - x)
					.attr('y2', r => r.Y * plot.scale - y)
			})
	}

	updateIndex(i) {
		if ((i >= 0) && (i < this.frame_lines.length)) {
			this.frame_index = parseInt(i)	// when "i" comes from slider, it's a string
			d3.select("#frame_slider").property('value', this.frame_index)
			// d3.select("#frame_slider").attr('value', this.frame_index) // updates default value
			// $("#frame_slider").val(this.frame_index) // updates actual value
			// $("#frame_slider")[0].value = this.frame_index // updates actual value
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
		this.time = Number(frame_sections[0].replace(",", "."))

		this.frame_event = JSON.parse(frame_sections[1])
		for (let s of frame_sections.slice(2)) {
			let fields = s.split('|')
			let r_id = parseInt(fields[0])
			this.relays[r_id].update(fields, this.packets)
		}
		let relays_regions = []
		for (let rel of this.relays.filter(r => r.status == RelayStatus.Transmitting && r.transmissionType == TransmissionType.RTS)) {
			for (let reg of range(this.protocol_params.n_regions)) {
				relays_regions.push({ rel: rel, reg: reg })
			}
		}

		let circle_dots = this.svg.select('#circle_dots').selectAll('.circle_dot')
			.data(this.relays, r => r.id)
		let circle_ranges = this.svg.select('#circle_ranges').selectAll('.circle_range')
			.data(this.relays.filter(r => r.actualStatus == RelayStatus.Transmitting), r => r.id)
		let packet_arrows = this.svg.select('#packet_arrows').selectAll('.packet_arrow')
			.data(this.relays.filter(r => r.actualStatus == RelayStatus.Transmitting && r.transmissionType == TransmissionType.PKT), r => r.id)
		let sink_arrows = this.svg.select('#sink_arrows').selectAll('.sink_arrow')
			.data(this.relays.filter(r => r.packetToSend != null && this.relay_details[r.id] == InfoModality.packet_info), r => r.id)
		let regions = this.svg.select('#regions').selectAll('.region')
			.data(relays_regions, t => [t.rel.id, t.reg])
		let id_labels = this.svg.select('#circle_labels').selectAll('.id_label')
			.data(this.relays.filter(r => this.show_Ids), r => r.id)
		let info_tooltips = this.svg.select('#info_tooltips').selectAll('.info_tooltip')
			.data(this.relays.filter(r => r.info_modality != InfoModality.off), r => r.id)


		info_tooltips.exit().remove()

		let infoTool = info_tooltips.enter()
			.append('g')
			.attr('class', 'info_tooltip')
			.attr('transform', r => `translate(${r.X * this.scale}, ${r.Y * this.scale})`)
			.call(this.dragHandler)

		infoTool.append('rect')
			.attr('width', r => r.details().length * 3 * this.scale)
			.attr('height', r => 4.2 * this.scale)
			.style("fill", "lightsteelblue")
			.attr('stroke', 'black')
			.attr('opacity', r => r.actualStatus > 0 ? "ff" : "40")
			.attr('stroke-width', 0.2 * this.scale)

		infoTool.append('text')
			.attr('dy', r => 4 * this.scale)
			.text(r => r.details())

		infoTool.append('line')
			.attr('x1', r => 0)
			.attr('y1', r => 0)
			.attr('x2', r => 0)
			.attr('y2', r => 0)
			.attr('stroke', 'grey')
			.attr('stroke-width', 0.2 * this.scale)
			.attr('position', 'absolute')

		info_tooltips.select('text').text(r => r.details())
		info_tooltips.select('rect').attr('width', r => r.details().length * 3 * this.scale)




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
			.attr('opacity', r => r.actualStatus > 0 ? "ff" : "40")
			.attr('stroke-width', 0.2 * this.scale)
			.on('click', r => {
				r.info_modality = (r.info_modality + 1) % 5
				this.plot()
			})

		circle_dots
			.style('fill', r => this.relay_colors[r.actualStatus])
			.attr('opacity', r => r.actualStatus > 0 ? "ff" : "40")

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
			.attr('relay_id_reg', t => `${t.rel.id}_${t.reg}`)

		let masks = region_groups
			.append('mask')
			.attr('id', t => `mask_${t.rel.id}_${t.reg}`)

		// mask with relay range
		masks.append('circle')
			.attr('cx', t => t.rel.X * this.scale)
			.attr('cy', t => t.rel.Y * this.scale)
			.attr('r', t => t.rel.range * this.scale)
			.style('stroke', 'none')
			.style('fill', '#ffffff')

		// mask with sink range
		masks.append('circle')
			.attr('cx', t => this.relays_dict[t.rel.packetToSend.sinkId].X * this.scale)
			.attr('cy', t => this.relays_dict[t.rel.packetToSend.sinkId].Y * this.scale)
			.attr('r', t => (this.distances[t.rel.id][t.rel.packetToSend.sinkId] - t.rel.range + t.reg * t.rel.range / this.protocol_params.n_regions) * this.scale)
			.style('stroke', 'none')
			.style('fill', '#000000')

		// circle that fills
		region_groups.append('circle')
			.attr('cx', t => this.relays_dict[t.rel.packetToSend.sinkId].X * this.scale)
			.attr('cy', t => this.relays_dict[t.rel.packetToSend.sinkId].Y * this.scale)
			.attr('r', t => (this.distances[t.rel.id][t.rel.packetToSend.sinkId] - t.rel.range + (t.reg + 1) * t.rel.range / this.protocol_params.n_regions) * this.scale)
			.attr('mask', t => `url(#mask_${t.rel.id}_${t.reg})`)
			.style('fill', t => t.rel.REGION_index == t.reg ? '#00e00080' : '#0000e080')


		// LABELS
		id_labels.exit().remove()

		let idLabels = id_labels.enter()
			.append('g')
			.attr('class', 'id_label')
			.attr('transform', r => `translate(${r.X * this.scale}, ${r.Y * this.scale})`)
			.attr('opacity', 0.7)

		idLabels.append('rect')
			// .attr('x', r => r.X * this.scale)
			// .attr('y', r => r.Y * this.scale - 10)
			.attr('width', r => String(r.id).length * 3 * this.scale)
			.attr('height', r => 4.2 * this.scale)
			.style("fill", "lightsteelblue")

		idLabels.append('text')
			// .attr('dx', r => r.X * this.scale)
			.attr('dy', r => 4 * this.scale)
			.text(r => `${r.id}`)


		console.log('render time: ' + (Date.now() - time_before_draw))
	}

	startPlaying() {
		if (this.interval) {
			this.stopPlaying()
		}
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
			if (this.speed == undefined) {
				this.speed = Number(document.getElementById('speed_slider').value)
			}
			let getAwaitTime = () => {
				let t1 = this.time
				let next_line = this.frame_lines[this.frame_index + 1]
				let t2 = Number(next_line.substring(0, next_line.indexOf(';')).replace(",", "."))
				let time = (t2 - t1) * 1000 / this.speed // convert in milliseconds
				// console.log("Await time: " + time)
				return time
			}
			let redoTimeout = () => {
				this.updateIndex(this.frame_index + 1)
				if (this.frame_index == this.frame_lines.length - 1) {
					this.interval.stop()
				}
				else {
					this.interval = d3.timeout(redoTimeout, getAwaitTime())
				}
			}
			this.interval = d3.timeout(time => redoTimeout(), getAwaitTime())
		}
	}

	stopPlaying() {
		if (this.interval) {
			this.interval.stop()
			this.interval = undefined
		}
	}

	updateInfo() {
		this.show_Ids = !this.show_Ids
		this.plot()
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
		this.speed = Number(slider.value)

		// restart playing
		if (this.interval) {
			this.stopPlaying()
			this.startPlaying()
		}
	}

	changePlayMode() {
		this.playMode = d3.select('#frameInterval').property('checked') ? PlayMode.FrameTime : PlayMode.Speed
		if (this.playMode == PlayMode.FrameTime) {
			d3.select('#speed_slider').style('display', 'none')
			d3.select('#speed_output').style('display', 'none')
			d3.select('#frame_time_slider').style('display', '')
			d3.select('#frame_time_output').style('display', '')
		}
		else {
			d3.select('#frame_time_slider').style('display', 'none')
			d3.select('#frame_time_output').style('display', 'none')
			d3.select('#speed_slider').style('display', '')
			d3.select('#speed_output').style('display', '')
		}

		if (this.interval) {
			this.stopPlaying()
			this.startPlaying()
		}
	}
}
