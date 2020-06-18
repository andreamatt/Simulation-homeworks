from matplotlib import pyplot as plt, patches
import numpy as np
from classes import *

def plot_delay_over_lambda_and_duty(runResult):
	duty_cycles = set([stat.duty for stat in runResult.DLstats])

	legend_patches = []
	for duty in duty_cycles:
		avg_delays = []
		lambdas = []
		stats = list(filter(lambda s : s.duty == duty, runResult.DLstats))
		for stat in stats:
			avg_delays.append(np.mean(stat.delay))
			lambdas.append(stat.lam)

		line = plt.plot(lambdas, avg_delays, '-', marker=".", lw=1) 
		color = line[0].get_color()
		legend_patches.append(patches.Patch(color=color, label=f'D = {duty}'))

	plt.legend(handles=legend_patches)
	plt.title('Average end-to-end delay normalized over distance, N=?')
	plt.xlabel('$\lambda$')
	plt.ylabel('avg delay (s)')
	plt.show()

def plot_delay_over_lambda_and_n(runResult):
	Ns = sorted(set([stat.N for stat in runResult.LNstats]))

	legend_patches = []
	for n in Ns:
		avg_delays = []
		lambdas = []
		stats = list(filter(lambda s : s.N == n, runResult.LNstats))
		for stat in stats:
			avg_delays.append(np.mean(stat.delay))
			lambdas.append(stat.lam)

		line = plt.plot(lambdas, avg_delays, '-', marker=".", lw=1) 
		color = line[0].get_color()
		legend_patches.append(patches.Patch(color=color, label=f'N = {n}'))
	
	plt.legend(handles=legend_patches)
	plt.title('Average end-to-end delay normalized over distance, D=?')
	plt.xlabel('$\lambda$')
	plt.ylabel('avg delay (s)')
	plt.show()
