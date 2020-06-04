var data = [5, 10, 12]
var width = 200,
	scaleFactor = 10,
	barHeight = 20

var graph = d3.select("#mainplot")
	.attr("width", width)
	.attr("height", barHeight * data.length)

function displayContents(contents) {
	var element = document.getElementById('file-content')
	element.textContent = contents
}

let plot

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

