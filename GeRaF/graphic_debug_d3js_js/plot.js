
class Plot {
	// style
	dot_radius = 1.6
	relay_colors = {
		"Asleep": '#aaaaaa',  // grey
		"Free": "#50bbaa",  // green
		"Awaiting_region": 'brown',
		"Transmitting": 'blue',
		"Sensing": '#3399ff',  // lightblue
		"Awaiting_Signal": 'yellow',
		"Backoff_Sensing": 'red',
		"Backoff_CTS": '#ff9900',  // orange
		"Backoff_SinkRTS": '#ffb909'  // orang-ish
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

	constructor(data) {
		let data_sections = data.split("#").map(d => d.trim())

		this.sim_params = new SimulationParameters(JSON.parse(data_sections[0]))
		this.protocol_params = new ProtocolParameters(JSON.parse(data_sections[1]))
		this.relays = {}
		for (let l of data_sections[2].split(/\r?\n/)) {
			l = l.trim()
			let r = new Relay(l)
			this.relays[r.id] = r
		}

		this.distances = JSON.parse(data_sections[3])
		this.frame_lines = data_sections[4].split(/\r?\n/).map(d => d.trim())

		this.packets = {}
		for (let l of data_sections[5].split(/\r?\n/)) {
			let p = JSON.parse(l)
			if (!(p['content_id'] in this.packets)) {
				this.packets[p['content_id']] = {}
			}
			this.packets[p['content_id']][p['copy_id']] = p
		}

		this.relay_details = {}
		for (let i of range(this.sim_params.n_nodes)) {
			this.relay_details[i] = 0
		}
		this.relay_markers = {}
		this.show_Ids = false
		this.show_dutyCicle = true
		this.frame_index = 0

		// zoom and drag
		this.center = (this.sim_params.area_side / 2, this.sim_params.area_side / 2)
		this.scale = 1

		this.last_time = 0

		// d3 selection
		this.svg = d3.select('#mainplot')
	}

	updateIndex(i) {
		if ((i >= 0) && (i < this.frames.length)) {
			this.frame_index = i
		} else {
			console.log("INVALID ARGUMENT: index out of boundaries")
		}
	}

	plot() {
		// update relays based on index
		let frame = this.frame_lines[this.frame_index]
		let frame_sections = frame.split(';')
		this.frame_time = frame_sections[0]
		console.log(this.relays)

		console.log(frame_sections)
		this.frame_event = JSON.parse(frame_sections[1])
		for (let id in this.relays) {
			this.relays[id].update(frame_sections[parseInt(id) + 2])
		}

		let time_before_draw = Date.now()

		// D3
		console.log(Object.values(this.relays))
		let relay_join = this.svg.selectAll('.relay').data(Object.values(this.relays))
		relay_join.enter()
			.append('g')
			.attr('class', 'relay')

		// relay_join = this.svg.selectAll('.relay').data(Object.values(this.relays))
		// relay_join.join()
		// 	.append('circle')
		// 	.attr('fill', 'black')

		this.last_time = Date.now() - time_before_draw
	}
}
