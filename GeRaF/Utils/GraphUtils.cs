using GeRaF.Network;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GeRaF.Utils
{
	struct Position
	{
		public double X;
		public double Y;
	}

	class GraphUtils
	{
		static public List<Relay> Movable(List<Relay> relays, Dictionary<int, Dictionary<int, double>> distances, double min_distance) {
			var connected = new HashSet<Relay>();
			var too_close = new HashSet<Relay>();
			var to_check = new HashSet<Relay>() {
				relays[0]
			};
			while (to_check.Count > 0) {
				var r = to_check.First();
				to_check.Remove(r);
				connected.Add(r);
				foreach (var r2 in r.neighbours) {
					if (connected.Contains(r2) == false) {
						to_check.Add(r2);
					}
				}
			}
			var disconnected = relays.Where(r => !connected.Contains(r));
			foreach (var r1 in relays) {
				if (!too_close.Contains(r1)) {
					foreach (var r2 in r1.neighbours) {
						if (distances[r1.id][r2.id] < min_distance) {
							too_close.Add(r2);
						}
					}
				}
			}
			return disconnected.Union(too_close).ToList();
		}

		static public bool Connected(List<Relay> relays) {
			var connected = new HashSet<Relay>();
			var to_check = new HashSet<Relay>() {
				relays[0]
			};
			while (to_check.Count > 0) {
				var r = to_check.First();
				to_check.Remove(r);
				connected.Add(r);
				foreach (var r2 in r.neighbours) {
					if (connected.Contains(r2) == false) {
						to_check.Add(r2);
					}
				}
			}
			return connected.Count == relays.Count;
		}

		static public Dictionary<int, Dictionary<int, double>> Distances(List<Relay> relays) {
			var distances = new Dictionary<int, Dictionary<int, double>>();
			foreach (var r1 in relays) {
				distances[r1.id] = new Dictionary<int, double>();
				foreach (var r2 in relays) {
					if (r1 != r2) {
						var dist = Math.Sqrt(Math.Pow(r1.position.X - r2.position.X, 2) + Math.Pow(r1.position.Y - r2.position.Y, 2));
						distances[r1.id][r2.id] = dist;
					}
				}
			}
			return distances;
		}

		static public void SetNeighbours(List<Relay> relays, Dictionary<int, Dictionary<int, double>> distances) {
			foreach (var r1 in relays) {
				r1.neighbours.Clear();
				foreach (var r2 in relays) {
					if (r1 != r2) {
						if (distances[r1.id][r2.id] <= r1.range) {
							r1.neighbours.Add(r2);
						}
					}
				}
			}
		}

		static public void FloydWarshall(List<Relay> relays, Dictionary<int, Dictionary<int, double>> distances) {
			var pathLength = new Dictionary<int, Dictionary<int, double>>();
			var maxLength = relays.Count;
			var nextHop = new Dictionary<int, Dictionary<int, Relay>>();
			foreach (var r1 in relays) {
				pathLength[r1.id] = new Dictionary<int, double>();
				nextHop[r1.id] = new Dictionary<int, Relay>();
				foreach (var r2 in relays) {
					if (r1 == r2) {
						pathLength[r1.id][r1.id] = 0;
						nextHop[r1.id][r1.id] = r1;
					}
					else {
						pathLength[r1.id][r2.id] = maxLength;
						if (r1.neighbours.Contains(r2)) {
							pathLength[r1.id][r2.id] = 1;
							nextHop[r1.id][r2.id] = r2;
						}
					}
				}
			}

			foreach (var k in relays) {
				foreach (var i in relays) {
					foreach (var j in relays) {
						if (pathLength[i.id][j.id] > pathLength[i.id][k.id] + pathLength[k.id][j.id]) {
							pathLength[i.id][j.id] = pathLength[i.id][k.id] + pathLength[k.id][j.id];
							nextHop[i.id][j.id] = nextHop[i.id][k.id];
						}
					}
				}
			}

			foreach (var r1 in relays) {
				foreach (var r2 in relays) {
					if (r1 != r2 && !r1.neighbours.Contains(r2)) {
						var distToSink = distances[r1.id][r2.id];
						var hop = nextHop[r1.id][r2.id];
						var distToHop = distances[r1.id][hop.id];

						var dx = r2.position.X - r1.position.X;
						var dy = r2.position.Y - r1.position.Y;

						// aim in the same direction as nextHop, but at sink distance (different region radius/shape)
						var aimX = r1.position.X + dx * distToSink / distToHop;
						var aimY = r1.position.Y + dy * distToSink / distToHop;
						r1.directionForSink[r2] = new Position() {
							X = aimX,
							Y = aimY
						};
					}
				}
			}
		}
	}
}
