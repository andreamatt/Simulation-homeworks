var data = [5, 10, 12]
var width = 600,
	height = 600

var graph = d3.select("#mainplot")
	.attr("width", width)
	.attr("height", height)

graph.append('g').attr('class', 'circle_ranges')
graph.append('g').attr('class', 'regions')
graph.append('g').attr('class', 'circle_dots')
graph.append('g').attr('class', 'sink_arrows')
graph.append('g').attr('class', 'packet_arrows')

function displayContents(contents) {
	var element = document.getElementById('file-content')
	element.textContent = contents
}

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

