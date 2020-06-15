from matplotlib import pyplot as plt
from scipy.stats import binned_statistic


def plot_success_over_distance(sim_stats):
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