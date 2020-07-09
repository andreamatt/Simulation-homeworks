using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GeRaF.StatsGeneration.Donut
{
	static class DonutVersionCompare
	{
		static public List<DonutStat> Generate(ProtocolParameters pp, SimulationParameters sp) {
			var simulationNumber = Program.simulationNumber;
			var options = new ParallelOptions() { MaxDegreeOfParallelism = Program.maxParallel };

			var versions = Enum.GetValues(typeof(ProtocolVersion)).Cast<ProtocolVersion>().ToList();
			var totalSimulations = simulationNumber * versions.Count;

			var stats = new List<DonutStat>();
			foreach (var version in versions) {
				var new_pp = (ProtocolParameters)pp.Clone();
				new_pp.protocolVersion = version;
				var new_sp = (SimulationParameters)sp.Clone();

				var stat = new DonutStat() {
					protocolVersion = version
				};

				// simulate
				Parallel.For(0, simulationNumber, options, i => {
					var sim = new Simulation(new_sp, new_pp);
					sim.Run();

					var packetsSuccess = sim.packetsFinished.Where(p => p.result == Network.Result.Success).ToList();
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

					lock (stat) {
						stat.success.Add(success);
						stat.delay.Add(delay);
						stat.energy.Add(energy);
						Console.WriteLine($"Simulating DL ... {(stats.Count * simulationNumber + stat.success.Count) * 100f / totalSimulations:##.00}%, v={version}");
					}
				});

				stats.Add(stat);
			}

			return stats;
		}
	}
}
