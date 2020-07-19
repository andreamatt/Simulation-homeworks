class GraphUtils{
	static RepeatedBFS(relays, distances){
		let nextHop = {}
		for(let start of relays) {
			nextHop[start.id] = {}
			let queue = []
			let visited = new Set()
			visited.add(start.id)
			for(let n of start.neighbours){
				visited.add(n.id)
				queue.push([n, n])
			}

			while(queue.length > 0){
				let v = queue.shift()
				let r = v[0]
				let origin = v[1]
				for(let n of r.neighbours){
					if(!visited.has(n.id)){
						visited.add(n.id)
						nextHop[start.id][n.id] = origin
						queue.push([n, origin])
					}
				}
			}
		}

		for(let r1 of relays){
			for(let r2 of relays){
				if (r1 != r2 && !r1.neighbours.has(r2)) {
					let distToSink = distances[r1.id][r2.id];
					let hop = nextHop[r1.id][r2.id];
					let distToHop = distances[r1.id][hop.id];

					let dx = hop.X - r1.X;
					let dy = hop.Y - r1.Y;

					// aim in the same direction as nextHop, but at sink distance (different region radius/shape)
					let aimX = r1.X + dx * distToSink / distToHop;
					let aimY = r1.Y + dy * distToSink / distToHop;
					r1.directionForSink[r2.id] = [aimX, aimY]
				}
			}
		}
	}

	static SetNeighbours(relays, distances){
		for(let r1 of relays){
			for(let r2 of relays){
				if(r1!=r2 && distances[r1.id][r2.id] < r1.range){
					r1.neighbours.add(r2)
				}
			}
		}
	}
}