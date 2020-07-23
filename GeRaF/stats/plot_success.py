from matplotlib import pyplot as plt, patches
import numpy as np
from classes import *
from collections import OrderedDict


def plot_success_over_lambda_and_duty(runResults):
	duty_cycles = list(OrderedDict.fromkeys([stat.duty for stat in runResults.DLstats]))
	protocol_versions = list(OrderedDict.fromkeys([stat.version for stat in runResults.DLstats]))
	
	legend_patches = []
	colors = ["red", "blue", "green", "grey", "black"] # for duties
	line_markers = ["-", ":", "-."]	# for protocol versions
	
	for i in range(len(protocol_versions)):
		version = protocol_versions[i]
		marker = line_markers[i]
		for j in range(len(duty_cycles)):
			duty = duty_cycles[j]
			color = colors[j]
			avg_succ = []
			lambdas = []

			stats = list(filter(lambda s : s.duty == duty and s.version == version, runResults.DLstats))

			for stat in stats:
				avg_succ.append(np.mean(stat.success))
				lambdas.append(stat.lam)

			line = plt.plot(lambdas, avg_succ, marker, marker=".", lw=1, color=color)
			
			if i==0:
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

