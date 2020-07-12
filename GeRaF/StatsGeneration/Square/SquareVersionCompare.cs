using GeRaF.Network;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GeRaF.StatsGeneration.Donut
{
	static class SquareVersionCompare
	{
		static public List<BaseStat> Generate(ProtocolParameters pp, SimulationParameters sp) {
			var simulationNumber = Program.simulationNumber;
			var options = new ParallelOptions() { MaxDegreeOfParallelism = Program.maxParallel };

			var versions = Enum.GetValues(typeof(ProtocolVersion)).Cast<ProtocolVersion>().ToList();
			var totalSimulations = simulationNumber * versions.Count;

			var stats = new List<BaseStat>();
			foreach (var version in versions) {
				var new_pp = (ProtocolParameters)pp.Clone();
				new_pp.protocolVersion = version;
				var new_sp = (SimulationParameters)sp.Clone();
				new_sp.emptyRegionType = EmptyRegionType.Square;
				new_sp.emptyRegionSize = new_sp.area_side / 3;  // side

				var stat = new BaseStat() {
					protocolVersion = version
				};
				for (int x = 0; x < new_sp.binsCount; x++) {
					stat.traffic.Add(new List<double>(new double[new_sp.binsCount]));
					stat.failurePoints.Add(new List<double>(new double[new_sp.binsCount]));
				}

				// simulate
				Parallel.For(0, simulationNumber, options, i => {
					var sim = new Simulation(new_sp, new_pp);
					sim.Run();

					var packetsSuccess = sim.packetsFinished.Where(p => p.result == Result.Success).ToList();
					var success = packetsSuccess.Count / (float)sim.packetsFinished.Count;
					double delay = 0;
					if (packetsSuccess.Count > 0) {
						delay = packetsSuccess.Average(p => {
							var startTime = p.receivedTimes.First();
							var endTime = p.receivedTimes.Last();
							var dist = sim.distances[p.startRelayId][p.sinkId];
							return (endTime - startTime) / dist;
						});
					}
					var energy = sim.relays.Average(r => r.totalAwake);

					var traffic = new List<List<int>>();
					var failures = new List<List<int>>();
					for (int x = 0; x < new_sp.binsCount; x++) {
						traffic.Add(new List<int>(new int[new_sp.binsCount]));
						failures.Add(new List<int>(new int[new_sp.binsCount]));
					}

					var packetsForTraffic = sim.packetsFinished.Where(p => p.result != Result.No_start_relays).ToList();
					var packetsForFailures = packetsForTraffic.Where(p => p.result != Result.Success).ToList();
					foreach (var p in packetsForTraffic) {
						foreach (var relayId in p.hopsIds) {
							var relay = sim.relayById[relayId];
							var xBin = sim.xToSlot(relay.X);
							var yBin = sim.xToSlot(relay.Y);
							traffic[xBin][yBin]++;
						}
					}
					foreach (var p in packetsForFailures) {
						var relay = sim.relayById[p.hopsIds.Last()];
						var xBin = sim.xToSlot(relay.X);
						var yBin = sim.xToSlot(relay.Y);
						failures[xBin][yBin]++;
					}

					lock (stat) {
						stat.success.Add(success);
						stat.delay.Add(delay);
						stat.energy.Add(energy);
						for (int x = 0; x < new_sp.binsCount; x++) {
							for (int y = 0; y < new_sp.binsCount; y++) {
								stat.traffic[x][y] += traffic[x][y];// / (double)packetsForTraffic.Count;
								stat.failurePoints[x][y] += failures[x][y];// / (double)packetsForFailures.Count;
							}
						}
						Console.WriteLine($"Simulating Square ... {(stats.Count * simulationNumber + stat.success.Count) * 100f / totalSimulations:##.00}%, v={version}");
					}
				});

				stats.Add(stat);
			}

			return stats;
		}
	}
}
