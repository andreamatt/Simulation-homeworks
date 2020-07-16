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
			if (parameters.Ns.Count == 0) parameters.Ns.Add(sp.n_nodes);
			if (parameters.dutyCycles.Count == 0) parameters.dutyCycles.Add(pp.duty_cycle);
			if (parameters.versions.Count == 0) parameters.versions.Add(pp.protocolVersion);
			if (parameters.emptyRegionTypes.Count == 0) parameters.emptyRegionTypes.Add(sp.emptyRegionType);

			var totalSimulations = parameters.simulations * parameters.lambdas.Count * parameters.Ns.Count * parameters.dutyCycles.Count * parameters.versions.Count;

			var stats = new List<BaseStat>();
			var new_pp = (ProtocolParameters)pp.Clone();
			var new_sp = (SimulationParameters)sp.Clone();

			foreach (var l in parameters.lambdas) {
				new_sp.packet_rate = l;
				foreach (var n in parameters.Ns) {
					new_sp.n_nodes = n;
					foreach (var d in parameters.dutyCycles) {
						new_pp.duty_cycle = d;
						foreach (var version in parameters.versions) {
							new_pp.protocolVersion = version;
							foreach (var emptyRegionType in parameters.emptyRegionTypes) {
								new_sp.emptyRegionType = emptyRegionType;
								switch(emptyRegionType){
									case EmptyRegionType.Circle:
										new_sp.emptyRegionSize = new_sp.area_side / 6; // radius
										break;
									case EmptyRegionType.Square:
										new_sp.emptyRegionSize = new_sp.area_side / 3; // side
										break;
								}
								var stat = new BaseStat() {
									lambda = l,
									N = n,
									duty = d,
									protocolVersion = version,
									shape = emptyRegionType
								};
								for (int x = 0; x < new_sp.binsCount; x++) {
									stat.traffic.Add(new List<double>(new double[new_sp.binsCount]));
									stat.failurePoints.Add(new List<double>(new double[new_sp.binsCount]));
								}

								// simulate
								Parallel.For(0, parameters.simulations, options, i => {
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
										Console.WriteLine($"Simulating general {name} ... {(stats.Count * parameters.simulations + stat.success.Count) * 100f / totalSimulations:##.00}%, l={l} n={n} d={d} v={version} shape={emptyRegionType}");
									}
								});

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
