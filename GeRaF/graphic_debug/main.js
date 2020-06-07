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

console.log("Debug_data size: " + debug_data.length)
let plot = new Plot(debug_data)
d3.select("#frame_slider").attr('max', plot.frame_lines.length - 1)
// $("#frame_slider").max = plot.frame_lines.length - 1 // no work
// document.getElementById("frame_slider").max = plot.frame_lines.length - 1 // works, how?
// console.log($("#frame_slider"))
// $("#frame_slider")[0].max = plot.frame_lines.length - 1 // no work

plot.plot()