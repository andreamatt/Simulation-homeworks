using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GeRaF.StatsGeneration.Donut
{
	static class VersionCompare
	{
		static public List<DutyLambdaStat> Generate(ProtocolParameters pp, SimulationParameters sp) {
			var simulationNumber = Program.simulationNumber;
			var options = new ParallelOptions() { MaxDegreeOfParallelism = Program.maxParallel };
			var totalSimulations = simulationNumber * dutyCycles.Count * lambdas.Count;

			var stats = new List<DutyLambdaStat>();
			foreach (var d in dutyCycles) {
				var new_pp = (ProtocolParameters)pp.Clone();
				new_pp.duty_cycle = d;
				foreach (var l in lambdas) {
					var new_sp = (SimulationParameters)sp.Clone();
					new_sp.packet_rate = l;

					var stat = new DutyLambdaStat() {
						duty = d,
						lambda = l
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
							Console.WriteLine($"Simulating DL ... {(stats.Count * simulationNumber + stat.success.Count) * 100f / totalSimulations:##.00}%, d={d} l={l}");
						}
					});

					stats.Add(stat);
				}
			}

			return stats;
		}
	}
}
