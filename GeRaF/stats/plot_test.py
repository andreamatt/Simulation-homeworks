from matplotlib import pyplot as plt
import numpy as np
from classes import *

def plot_avgdelay_over_lambda_and_duty(runResult):
	duty_cycles = [dl_stat.duty for dl_stat in runResult.DLstats]

	for duty in duty_cycles:
		avg_delays = []
		lambdas = []
		dl_stats = list(filter(lambda s : s.duty == duty, runResult.DLstats))
		for dl_stat in dl_stats:
			avg_delays.append(np.mean(dl_stat.delay))
			lambdas.append(dl_stat.lam)

		plt.scatter(lambdas, avg_delays) # print avg_delay over lambda 

	plt.show()
