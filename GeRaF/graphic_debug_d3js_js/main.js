var data = [5, 10, 12];
var width = 200,
	scaleFactor = 10,
	barHeight = 20;

var graph = d3.select("#mainplot")
	.attr("width", width)
	.attr("height", barHeight * data.length);

var bar = graph.selectAll("g")
	.data(data)
	.enter()
	.append("g")
	.attr("transform", function (d, i) {
		return "translate(0," + i * barHeight + ")";
	});

bar.append("rect")
	.attr("width", function (d) {
		return d * scaleFactor;
	})
	.attr("height", barHeight - 1);

bar.append("text")
	.attr("x", function (d) { return (d * scaleFactor); })
	.attr("y", barHeight / 2)
	.attr("dy", ".35em")
	.text(function (d) { return d; });

function readSingleFile(e) {
	var file = e.target.files[0];
	if (!file) {
		return;
	}
	var reader = new FileReader();
	reader.onload = function (e) {
		var contents = e.target.result;
		displayContents(contents);
	};
	reader.readAsText(file);
}

function displayContents(contents) {
	var element = document.getElementById('file-content');
	element.textContent = contents.length;
}

document.getElementById('file-input')
	.addEventListener('change', readSingleFile, false);
