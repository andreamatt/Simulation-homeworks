using GeRaF.Network;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GeRaF.Utils
{
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
						var dist = Math.Sqrt(Math.Pow(r1.X - r2.X, 2) + Math.Pow(r1.Y - r2.Y, 2));
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
	}
}
