from matplotlib import pyplot as plt, patches
import numpy as np
from classes import *
from collections import OrderedDict


def plot_energy_over_lambda_and_duty(runResults):
	duty_cycles = list(OrderedDict.fromkeys([stat.duty for stat in runResults.DLstats]))
	protocol_versions = list(OrderedDict.fromkeys([stat.version for stat in runResults.DLstats]))
	n_nodes = runResults.DLstats[0].N
	shape_type = runResults.DLstats[0].shape

	legend_patches = []
	colors = ["red", "blue", "green", "grey", "black"] # for duties
	line_markers = ["-", ":", "-."]	# for protocol versions
	
	for i in range(len(protocol_versions)):
		version = protocol_versions[i]
		marker = line_markers[i]
		for j in range(len(duty_cycles)):
			duty = duty_cycles[j]
			color = colors[j]
			avg_energy = []
			lambdas = []

			stats = list(filter(lambda s : s.duty == duty and s.version == version, runResults.DLstats))

			for stat in stats:
				avg_energy.append(np.mean(stat.energy))
				lambdas.append(stat.lam)

			line = plt.plot(lambdas, avg_energy, marker, marker=".", lw=1, color=color)
			
			if i==0:
				legend_patches.append(patches.Patch(color=color, label=f'd = {duty}'))
	
	plt.legend(handles=legend_patches)
	plt.title('Average energy over $\lambda$ and duty cycle\n' + "Shape="+ str(shape_type) +', N=' + str(n_nodes))
	plt.xlabel('$\lambda$')
	plt.ylabel('energy')
	plt.xlim(0)
	plt.ylim(0)
	plt.savefig("plt_energy_over_lambda_and_duty.png", dpi=300, pad_inches = 0.05)
	plt.close()

def plot_energy_over_lambda_and_n(runResults):
	Ns = list(OrderedDict.fromkeys([stat.N for stat in runResults.LNstats]))
	protocol_versions = list(OrderedDict.fromkeys([stat.version for stat in runResults.LNstats]))
	d_cycle = runResults.LNstats[0].duty
	shape_type = runResults.LNstats[0].shape
	
	legend_patches = []
	colors = ["red", "blue", "green", "grey", "black"] # for duties
	line_markers = ["-", ":", "-."]	# for protocol versions
	
	for i in range(len(protocol_versions)):
		version = protocol_versions[i]
		marker = line_markers[i]
		for j in range(len(Ns)):
			N = Ns[j]
			color = colors[j]
			avg_energy = []
			lambdas = []

			stats = list(filter(lambda s : s.N == N and s.version == version, runResults.LNstats))

			for stat in stats:
				avg_energy.append(np.mean(stat.energy))
				lambdas.append(stat.lam)

			line = plt.plot(lambdas, avg_energy, marker, marker=".", lw=1, color=color)
			
			if i==0:
				legend_patches.append(patches.Patch(color=color, label=f'N = {N}'))
	
	plt.legend(handles=legend_patches)
	plt.title('Average energy over $\lambda$ and N\n' + "Shape="+ str(shape_type) +', d=' + str(d_cycle))
	plt.xlabel('$\lambda$')
	plt.ylabel('energy')
	plt.xlim(0)
	plt.savefig("plt_energy_over_lambda_and_n.png", dpi=300, pad_inches = 0.05)
	plt.close()
