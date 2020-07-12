﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GeRaF.StatsGeneration.Base
{
	class LambdaN   // change generation rate and number of nodes
	{
		static public List<LambdaNStat> Generate(ProtocolParameters pp, SimulationParameters sp, List<double> lambdas, List<int> Ns) {
			var simulationNumber = Program.simulationNumber;
			var options = new ParallelOptions() { MaxDegreeOfParallelism = Program.maxParallel };

			var versions = Enum.GetValues(typeof(ProtocolVersion)).Cast<ProtocolVersion>().ToList();
			var totalSimulations = simulationNumber * lambdas.Count * Ns.Count * versions.Count;

			var stats = new List<LambdaNStat>();
			foreach (var version in versions) {
				var new_pp = (ProtocolParameters)pp.Clone();
				new_pp.protocolVersion = version;
				foreach (var l in lambdas) {
					foreach (var n in Ns) {
						var new_sp = (SimulationParameters)sp.Clone();
						new_sp.packet_rate = l;
						new_sp.n_nodes = n;

						var stat = new LambdaNStat() {
							lambda = l,
							N = n,
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
								Console.WriteLine($"Simulating LN ... {(stats.Count * simulationNumber + stat.success.Count) * 100f / totalSimulations:##.00}%, l={l} n={n} v={version}");
							}
						});

						stats.Add(stat);
					}
				}
			}

			return stats;
		}
	}
}
