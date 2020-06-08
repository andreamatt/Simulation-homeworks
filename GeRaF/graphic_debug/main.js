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

graph.append('g').attr('id', 'area_limit')
graph.append('g').attr('id', 'circle_ranges')
graph.append('g').attr('id', 'regions')
graph.append('g').attr('id', 'circle_dots')
graph.append('g').attr('id', 'sink_arrows')
graph.append('g').attr('id', 'packet_arrows')
graph.append('g').attr('id', 'circle_labels')
graph.append('g').attr('id', 'info_tooltips')


console.log("Debug_data size: " + debug_data.length)
let plot = new Plot(debug_data)
d3.select("#frame_slider").attr('max', plot.frame_lines.length - 1)
// $("#frame_slider").max = plot.frame_lines.length - 1 // no work
// document.getElementById("frame_slider").max = plot.frame_lines.length - 1 // works, how?
// console.log($("#frame_slider"))
// $("#frame_slider")[0].max = plot.frame_lines.length - 1 // no work

plot.plot()