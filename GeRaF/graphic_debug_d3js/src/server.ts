import * as express from 'express'
import * as d3 from 'd3'
import { JSDOM } from 'jsdom'
import { Plot, loadJSON } from './graphic_debugger'
import * as fs from 'fs'
const app = express()

const htmlStub: string = fs.readFileSync('public/home.html', 'utf8') //'<html><head></head><body><div id="dataviz-container"></div><script src="js/d3.v3.min.js"></script></body></html>' // html file skull with a container div for the d3 dataviz

app.use(express.static('public'))

app.get('/', function (req: express.Request, res: express.Response) {
  const page_jsdom = new JSDOM(htmlStub)
  const document: Document = page_jsdom.window.document
  const body = d3.select(document).select('body')

  let data = loadJSON("../debug.json")
  const plot = new Plot(data)
  

  body
    .select(".target")
    .style("stroke-width", 6)

  res.send(document.documentElement.innerHTML)
})

app.listen(3000, function () {
  console.log('Server listening on port 3000')
})