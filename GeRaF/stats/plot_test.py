from matplotlib import pyplot as plt
import numpy as np
from classes import *

def plot_avgdelay_over_lambda_and_duty(runResult):
	duty_cycles = [stat.duty for stat in runResult.DLstats]

	for duty in duty_cycles:
		avg_delays = []
		lambdas = []
		stats = list(filter(lambda s : s.duty == duty, runResult.DLstats))
		for stat in stats:
			avg_delays.append(np.mean(stat.delay))
			lambdas.append(stat.lam)

		plt.plot(lambdas, avg_delays, 'b-') # print avg_delay over lambda

	plt.show()
