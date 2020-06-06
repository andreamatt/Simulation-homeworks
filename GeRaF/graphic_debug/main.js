var data = [5, 10, 12]
var width = 600

// create svg
var graph = d3.select('#mainplot')
	.attr('width', width)
	.attr('height', width)
	.call(d3.zoom().on('zoom', function () {
		graph.attr('transform', d3.event.transform)
	}))
	.append('g')

// create interface controls
var frameIntervalRadio = d3.select('body').append('input')
	.attr('type', 'radio')
	.attr('id', 'frameInterval')
	.attr('name', 'playMode')
	.attr('value', 'frameInterval')
	.property('checked', true)
	.on('click', () => plot.changePlayMode())
d3.select('body').append('label')
	.attr('')

graph.append('g').attr('class', 'area_limit')
graph.append('g').attr('class', 'circle_ranges')
graph.append('g').attr('class', 'regions')
graph.append('g').attr('class', 'circle_dots')
graph.append('g').attr('class', 'sink_arrows')
graph.append('g').attr('class', 'packet_arrows')

let plot = null

function readSingleFile(e) {
	var file = e.target.files[0]
	if (!file) {
		return
	}
	var reader = new FileReader()
	reader.onload = function (e) {
		var contents = e.target.result
		plot = new Plot(contents)
		plot.plot()
	}
	reader.readAsText(file)
}

document.getElementById('file-input')
	.addEventListener('change', readSingleFile, false)

