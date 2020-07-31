class GraphUtils{
	// static RepeatedBFS(relays, distances, version){
	// 	let nextHop = {}
	// 	for(let start of relays) {
	// 		nextHop[start.id] = {}
	// 		let queue = []
	// 		let visited = new Set()
	// 		visited.add(start.id)
	// 		for(let n of start.neighbours){
	// 			visited.add(n.id)
	// 			queue.push([n, n])
	// 		}

	// 		while(queue.length > 0){
	// 			let v = queue.shift()
	// 			let r = v[0]
	// 			let origin = v[1]
	// 			for(let n of r.neighbours){
	// 				if(!visited.has(n.id)){
	// 					visited.add(n.id)
	// 					nextHop[start.id][n.id] = origin
	// 					queue.push([n, origin])
	// 				}
	// 			}
	// 		}
	// 	}

	// 	for(let r1 of relays){
	// 		for(let r2 of relays){
	// 			if (r1 != r2 && !r1.neighbours.has(r2)) {
	// 				let distToSink = distances[r1.id][r2.id];
	// 				let hop = nextHop[r1.id][r2.id];
	// 				let distToHop = distances[r1.id][hop.id];

	// 				let dx = hop.X - r1.X;
	// 				let dy = hop.Y - r1.Y;

	// 				let aimX;
	// 				let aimY;
	// 				if (version == "BFS") {
	// 					// aim in the same direction as nextHop, but at sink distance (different region radius/shape)
	// 					aimX = r1.position.X + dx * distToSink / distToHop;
	// 					aimY = r1.position.Y + dy * distToSink / distToHop;
	// 				}
	// 				else {
	// 					// aim at an angle between sink and nextHop
	// 					let CX = r1.position.X + dx * distToSink / distToHop;
	// 					let CY = r1.position.Y + dy * distToSink / distToHop;
	// 					let DX = r1.position.X + dx * distToSink / distToHop;
	// 					let DY = r1.position.Y + dy * distToSink / distToHop;
	// 					let d_r1_D = GraphUtils.Distance(r1.position.X, r1.position.Y, DX, DY);
	// 					aimX = r1.position.X + (DX - r1.position.X) * distToSink / d_r1_D;
	// 					aimY = r1.position.Y + (DY - r1.position.Y) * distToSink / d_r1_D;
	// 				}
	// 				r1.directionForSink[r2] = {
	// 					X: aimX,
	// 					Y: aimY
	// 				};
	// 			}
	// 		}
	// 	}
	// }

	static SetNeighbours(relays, distances){
		for(let r1 of relays){
			for(let r2 of relays){
				if(r1!=r2 && distances[r1.id][r2.id] < r1.range){
					r1.neighbours.add(r2)
				}
			}
		}
	}

	static Distance(x1, y1, x2, y2){
		return Math.sqrt(Math.pow(x1 - x2, 2) + Math.pow(y1 - y2, 2));
	}
}