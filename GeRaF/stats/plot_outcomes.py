from matplotlib import pyplot as plt
from classes import Packet
from numpy import mean

def plot_outcomes(sim_stats):
	result_mean = {}
	for r in Packet.results:
		result_mean[r] = mean([s.mean_rate(r) for s in sim_stats])

	print(result_mean)
	# plt.hist([s.mean_rate('Success') for s in sim_stats], bins=10)
	# plt.show()

	plt.bar(Packet.results, result_mean.values())
	plt.show()