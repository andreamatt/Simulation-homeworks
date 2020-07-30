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
		public float X;
		public float Y;
	}

	class GraphUtils
	{
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

		static public void Distances(List<Relay> relays, float[,] distances) {
			foreach (var r1 in relays) {
				foreach (var r2 in relays) {
					distances[r1.id, r2.id] = 0;
					if (r1 != r2) {
						distances[r1.id, r2.id] = Distance(r1.position.X, r1.position.Y, r2.position.X, r2.position.Y);
					}
				}
			}
		}

		static public void SetNeighbours(List<Relay> relays, float[,] distances) {
			foreach (var r1 in relays) {
				r1.neighbours.Clear();
				foreach (var r2 in relays) {
					if (r1 != r2) {
						if (distances[r1.id, r2.id] <= r1.range) {
							r1.neighbours.Add(r2);
						}
					}
				}
			}
		}

		static public void RepeatedBFS(List<Relay> relays, float[,] distances, ProtocolVersion version) {
			var nextHop = new Dictionary<int, Dictionary<int, Relay>>();

			foreach (var start in relays) {
				nextHop[start.id] = new Dictionary<int, Relay>();

				var queue = new Queue<(Relay, Relay)>();
				var visited = new HashSet<int>();
				visited.Add(start.id);
				foreach (var n in start.neighbours) {
					visited.Add(n.id);
					queue.Enqueue((n, n));
				}

				while (queue.Count != 0) {
					var v = queue.Dequeue();
					var relay = v.Item1;
					var origin = v.Item2;
					foreach (var neigh in relay.neighbours) {
						if (!visited.Contains(neigh.id)) {
							visited.Add(neigh.id);
							nextHop[start.id][neigh.id] = origin;
							queue.Enqueue((neigh, origin));
						}
					}
				}
			}

			foreach (var r1 in relays) {
				foreach (var r2 in relays) {
					if (r1 != r2 && !r1.neighbours.Contains(r2)) {
						var distToSink = distances[r1.id, r2.id];
						var hop = nextHop[r1.id][r2.id];
						var distToHop = distances[r1.id, hop.id];

						var dx = hop.position.X - r1.position.X;
						var dy = hop.position.Y - r1.position.Y;

						float aimX;
						float aimY;
						if (version == ProtocolVersion.BFS) {
							// aim in the same direction as nextHop, but at sink distance (different region radius/shape)
							aimX = r1.position.X + dx * distToSink / distToHop;
							aimY = r1.position.Y + dy * distToSink / distToHop;
						}
						else {
							// aim at an angle between sink and nextHop
							var CX = r1.position.X + dx * distToSink / distToHop;
							var CY = r1.position.Y + dy * distToSink / distToHop;
							var DX = r1.position.X + dx * distToSink / distToHop;
							var DY = r1.position.Y + dy * distToSink / distToHop;
							var d_r1_D = GraphUtils.Distance(r1.position.X, r1.position.Y, DX, DY);
							aimX = r1.position.X + (DX - r1.position.X) * distToSink / d_r1_D;
							aimY = r1.position.Y + (DY - r1.position.Y) * distToSink / d_r1_D;
						}
						r1.directionForSink[r2] = new Position() {
							X = aimX,
							Y = aimY
						};
					}
				}
			}
		}

		static public double Distance(double x1, double y1, double x2, double y2) => Math.Sqrt(Math.Pow(x1 - x2, 2) + Math.Pow(y1 - y2, 2));

		static public float Distance(float x1, float y1, float x2, float y2) => (float)Distance((double)x1, y1, x2, y2);
	}
}
