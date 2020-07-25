using GeRaF.Network;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GeRaF.StatsGeneration
{
	static class General
	{
		static public List<BaseStat> Generate(string name, SimulationParameters sp, ProtocolParameters pp, GeneralParameters parameters) {
			var options = new ParallelOptions() { MaxDegreeOfParallelism = Program.maxParallel };

			// set default values
			if (parameters.lambdas.Count == 0) parameters.lambdas.Add(sp.packet_rate);
			if (parameters.relay_densities.Count == 0) parameters.relay_densities.Add(sp.n_nodes);
			if (parameters.dutyCycles.Count == 0) parameters.dutyCycles.Add(pp.duty_cycle);
			if (parameters.versions.Count == 0) parameters.versions.Add(pp.protocolVersion);
			if (parameters.emptyRegionTypes.Count == 0) parameters.emptyRegionTypes.Add(sp.emptyRegionType);

			var totalSimulations = parameters.simulations * parameters.lambdas.Count * parameters.relay_densities.Count * parameters.dutyCycles.Count * parameters.versions.Count * parameters.emptyRegionTypes.Count;

			var stats = new List<BaseStat>();
			var new_pp = (ProtocolParameters)pp.Clone();
			var new_sp = (SimulationParameters)sp.Clone();

			foreach (var l in parameters.lambdas) {
				new_sp.packet_rate = l;
				foreach (var density in parameters.relay_densities) {
					foreach (var d in parameters.dutyCycles) {
						new_pp.duty_cycle = d;
						foreach (var version in parameters.versions) {
							new_pp.protocolVersion = version;
							foreach (var emptyRegionType in parameters.emptyRegionTypes) {
								new_sp.emptyRegionType = emptyRegionType;
								var area = Math.Pow(new_sp.area_side, 2);
								switch (emptyRegionType) {
									case EmptyRegionType.Circle:
										new_sp.emptyRegionSize = new_sp.area_side / 6; // radius
										area = area - Math.PI * Math.Pow(new_sp.emptyRegionSize, 2);
										break;
									case EmptyRegionType.Square:
										new_sp.emptyRegionSize = new_sp.area_side / 3; // side
										area = area - Math.Pow(new_sp.emptyRegionSize, 2);
										break;
									case EmptyRegionType.Lines:
										new_sp.emptyRegionSize = new_sp.area_side / 10 * 8; // long side = 80% area side
										area = area - new_sp.emptyRegionSize * Math.Max(new_sp.emptyRegionSize / 8, new_sp.range + 2) * 2;
										break;
									case EmptyRegionType.Holes:
										new_sp.emptyRegionSize = new_sp.area_side / 6; // half radius of circle
										area = area - Math.PI * Math.Pow(new_sp.emptyRegionSize, 2) * 4;
										break;
								}
								new_sp.n_nodes = (int)Math.Floor(area * density / 10000);
								//new_sp.n_nodes = (int)Math.Floor(new_sp.n_nodes * area / Math.Pow(new_sp.area_side, 2));

								var outcomesNumber = Enum.GetValues(typeof(Result)).Length;
								var stat = new BaseStat() {
									lambda = l,
									N = density,
									duty = d,
									protocolVersion = version,
									shape = emptyRegionType,
									averageOutcomes = new List<double>(new double[outcomesNumber])
								};
								//for (int x = 0; x < new_sp.binsCount; x++) {
								//	stat.traffic.Add(new List<double>(new double[new_sp.binsCount]));
								//	stat.failurePoints.Add(new List<double>(new double[new_sp.binsCount]));
								//}
								var trafficList = new List<List<List<int>>>();
								var failureList = new List<List<List<int>>>();
								for (int x = 0; x < new_sp.binsCount; x++) {
									trafficList.Add(new List<List<int>>());
									failureList.Add(new List<List<int>>());
									for (int y = 0; y < new_sp.binsCount; y++) {
										trafficList[x].Add(new List<int>());
										failureList[x].Add(new List<int>());
									}
								}

								// simulate
								Parallel.For(0, parameters.simulations, options, parallel_iteration_index => {
									var sim = new Simulation(new_sp, new_pp);
									sim.Run();

									var outcomes = new List<double>(new double[outcomesNumber]);
									foreach (var p in sim.packetsFinished) {
										outcomes[(int)p.result]++;
									}
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
									var energy = sim.relays.Average(r => {
										var awakeIdle = r.totalAwake - r.totalTransmitting;
										return sp.asleepEnergy * r.totalSleep + sp.idleEnergy * awakeIdle + sp.transmissionEnergy * r.totalTransmitting;
									});

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
											var xBin = sim.xToSlot(relay.position.X);
											var yBin = sim.xToSlot(relay.position.Y);
											traffic[xBin][yBin]++;
										}
									}
									foreach (var p in packetsForFailures) {
										var relay = sim.relayById[p.hopsIds.Last()];
										var xBin = sim.xToSlot(relay.position.X);
										var yBin = sim.xToSlot(relay.position.Y);
										failures[xBin][yBin]++;
									}

									lock (stat) {
										stat.success.Add(success);
										stat.delay.Add(delay);
										stat.energy.Add(energy);
										for (int i = 0; i < outcomes.Count; i++) {
											stat.averageOutcomes[i] += outcomes[i];
										}
										Console.WriteLine($"Simulating general {name} ... {(stats.Count * parameters.simulations + stat.success.Count) * 100f / totalSimulations:##.00}%, l={l} n={new_sp.n_nodes} d={d} v={version} shape={emptyRegionType}");
									}

									lock (trafficList) {
										lock (failureList) {
											for (int x = 0; x < new_sp.binsCount; x++) {
												for (int y = 0; y < new_sp.binsCount; y++) {
													trafficList[x][y].Add(traffic[x][y]);// / (double)packetsForTraffic.Count;
													failureList[x][y].Add(failures[x][y]);// / (double)packetsForFailures.Count;
												}
											}
										}
									}
								});

								for (int x = 0; x < new_sp.binsCount; x++) {
									stat.traffic.Add(new List<double>(new double[new_sp.binsCount]));
									stat.failurePoints.Add(new List<double>(new double[new_sp.binsCount]));
									for (int y = 0; y < new_sp.binsCount; y++) {
										stat.traffic[x][y] = trafficList[x][y].Average();
										stat.failurePoints[x][y] = failureList[x][y].Average();
									}
								}

								var tot = stat.averageOutcomes.Sum();
								for (int i = 0; i < stat.averageOutcomes.Count; i++) {
									stat.averageOutcomes[i] = stat.averageOutcomes[i] / tot;
								}

								stats.Add(stat);
							}
						}
					}
				}
			}

			return stats;
		}
	}
}
