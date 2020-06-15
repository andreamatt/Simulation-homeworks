from matplotlib import pyplot as plt
import json
from classes import *
import numpy as np
from scipy.stats import binned_statistic

jsonFile = open("runResults.json")
data = json.load(jsonFile)

simulations = data['simulationStats']

sim_params = SimulationParameters(data['simulationParameters'])
protocol_params = ProtocolParamaters(data['protocolParameters'])
sim_stats = [SimulationStat(s) for s in data['simulationStats']]

# plt.hist([s.mean_rate('Success') for s in sim_stats])
# plt.show()

distances = []
results = []
for sim in sim_stats:
	for p in sim.packets:
		if p.result!='No_start_relays':
			distances.append(sim.distances[p.startRelayId][p.sinkId])
			results.append(p.result=='Success')

n_bins = 20
stats, bins, binnumber = binned_statistic(distances, results, statistic=lambda b: list(b).count(True)/len(b), bins=n_bins)

plt.scatter(bins[:-1], stats)
plt.show()