from matplotlib import pyplot as plt, patches
import numpy as np
from classes import *

def plot_energy_over_lambda_and_duty(runResult):
	duty_cycles = set([stat.duty for stat in runResult.DLstats])

	legend_patches = []
	for duty in duty_cycles:
		avg_energy = []
		lambdas = []
		stats = list(filter(lambda s : s.duty == duty, runResult.DLstats))
		for stat in stats:
			avg_energy.append(np.mean(stat.energy))
			lambdas.append(stat.lam)

		line = plt.plot(lambdas, avg_energy, '-', marker=".", lw=1) 
		color = line[0].get_color()
		legend_patches.append(patches.Patch(color=color, label=f'D = {duty}'))

	plt.legend(handles=legend_patches)
	plt.title('Energy usage factor, N=?')
	plt.xlabel('$\lambda$')
	plt.ylabel('energy usage factor')
	plt.show()

def plot_energy_over_lambda_and_n(runResult):
	Ns = sorted(set([stat.N for stat in runResult.LNstats]))

	legend_patches = []
	for n in Ns:
		avg_energy = []
		lambdas = []
		stats = list(filter(lambda s : s.N == n, runResult.LNstats))
		for stat in stats:
			avg_energy.append(np.mean(stat.energy))
			lambdas.append(stat.lam)

		line = plt.plot(lambdas, avg_energy, '-', marker=".", lw=1) 
		color = line[0].get_color()
		legend_patches.append(patches.Patch(color=color, label=f'N = {n}'))
	
	plt.legend(handles=legend_patches)
	plt.title('Energy usage factor, D=?')
	plt.xlabel('$\lambda$')
	plt.ylabel('energy usage factor')
	plt.show()
