from matplotlib import pyplot as plt, patches
import numpy as np
from classes import *

def plot_success_over_lambda_and_duty(runResult):
	duty_cycles = set([stat.duty for stat in runResult.DLstats])
	
	legend_patches = []
	for duty in duty_cycles:
		avg_succ_base = []
		avg_succ_plus = []
		avg_succ_plus_2 = []
		lambdas_base = []
		lambdas_plus = []
		lambdas_plus_2 = []

		stats_base = list(filter(lambda s : s.duty == duty and s.version == 'Base', runResult.DLstats))
		stats_plus = list(filter(lambda s : s.duty == duty and s.version == 'Plus', runResult.DonutStats))
		stats_plus_2 = list(filter(lambda s : s.duty == duty and s.version == 'Plus_2', runResult.SquareStats))
		for stat in stats_base:
			avg_succ_base.append(np.mean(stat.success))
			lambdas_base.append(stat.lam)
		for stat in stats_plus:
			avg_succ_plus.append(np.mean(stat.success))
			lambdas_plus.append(stat.lam)
		for stat in stats_plus_2:
			avg_succ_plus_2.append(np.mean(stat.success))
			lambdas_plus_2.append(stat.lam)

		line_base = plt.plot(lambdas_base, avg_succ_base, '-', marker=".", lw=1)
		color = line_base[0].get_color()
		line_plus = plt.plot(lambdas_plus, avg_succ_plus, ':', marker=".", lw=1, color=color)
		line_plus_2 = plt.plot(lambdas_plus_2, avg_succ_plus_2, '-.', marker=".", lw=1, color=color) 
		
		legend_patches.append(patches.Patch(color=color, label=f'd = {duty}'))
	

	plt.legend(handles=legend_patches)
	plt.title('Percentage of packets successfully delivered to the sink, N=?')
	plt.xlabel('$\lambda$')
	plt.ylabel('% success delivery')
	plt.show()

def plot_success_over_lambda_and_n(ax, LNstats):
	Ns = sorted(set([stat.N for stat in LNstats]))

	legend_patches = []
	for n in Ns:
		avg_succ = []
		lambdas = []
		stats = list(filter(lambda s : s.N == n, runResult.LNstats))
		for stat in stats:
			avg_succ.append(np.mean(stat.success))
			lambdas.append(stat.lam)

		line = plt.plot(lambdas, avg_succ, '-', marker=".", lw=1)
		color = line[0].get_color()
		legend_patches.append(patches.Patch(color=color, label=f'N = {n}'))
	
	ax.legend(handles=legend_patches)
	ax.title('Percentage of packets successfully delivered to the sink, D=?')
	ax.xlabel('$\lambda$')
	ax.ylabel('% success delivery')
	ax.show()



	ax.legend(handles=legend_patches)
	ax.title('Percentage of packets successfully delivered to the sink, N=?')
	ax.xlabel('$\lambda$')
	ax.ylabel('% success delivery')

