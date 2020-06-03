import * as express from 'express'
import * as d3 from 'd3'
import { JSDOM } from 'jsdom'
import * as fs from 'fs'
var app = express()

let htmlStub: string = fs.readFileSync('public/home.html', 'utf8') //'<html><head></head><body><div id="dataviz-container"></div><script src="js/d3.v3.min.js"></script></body></html>' // html file skull with a container div for the d3 dataviz
let page_jsdom = new JSDOM(htmlStub)
let document: Document = page_jsdom.window.document
let body = d3.select(document).select('body')

app.use(express.static('public'))

app.get('/', function (req: express.Request, res: express.Response) {
  body
    .select(".target")
    .style("stroke-width", 6)

  res.send(document.documentElement.innerHTML)
})

app.listen(3000, function () {
  console.log('Server listening on port 3000')
})