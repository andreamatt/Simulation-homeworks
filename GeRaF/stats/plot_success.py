from matplotlib import pyplot as plt

def plot_success(sim_stats):
	plt.hist([s.mean_rate('Success') for s in sim_stats], bins=10)
	plt.show()