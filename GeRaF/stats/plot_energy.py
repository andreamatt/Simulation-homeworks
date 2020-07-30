from matplotlib import pyplot as plt, patches
from matplotlib.lines import Line2D
import numpy as np
from classes import *
from collections import OrderedDict


def plot_energy_over_lambda_and_duty(runResults):
	duty_cycles = list(OrderedDict.fromkeys([stat.duty for stat in runResults.DLstats]))
	protocol_versions = list(OrderedDict.fromkeys([stat.version for stat in runResults.DLstats]))
	n_nodes = runResults.DLstats[0].N
	shape_type = runResults.DLstats[0].shape

	colors = ["green", "blue", "red","grey", "black"] # for duties
	line_styles = ["-", "--", ":", "-."]	# for protocol versions
	# dashes_list = [
	# 	(1, 0),
	# 	(5, 2),
	# 	(2, 1, 4, 3),
	# 	(2, 2, 10, 2),
	# 	(1,3)
	# ]
	legend_duties = []
	legend_versions = []

	for i in range(len(protocol_versions)):
		version = protocol_versions[i]
		color = colors[i]
		legend_versions.append(patches.Patch(color=color, label=version))
		for j in range(len(duty_cycles)):
			duty = duty_cycles[j]
			avg_energy = []
			lambdas = []

			stats = list(filter(lambda s : s.duty == duty and s.version == version, runResults.DLstats))

			for stat in stats:
				avg_energy.append(np.mean(stat.energy))
				lambdas.append(stat.lam)

			line = plt.plot(lambdas, avg_energy,  marker=".", lw=1, color=color, ls=line_styles[j])
			
			if i==0:
				legend_duties.append(Line2D([0], [0], color='black', linewidth=2, ls=line_styles[j], label=f'd = {duty}'))
	
	legend_1 = plt.legend(handles=legend_duties, loc='upper left', bbox_to_anchor=(1, 1))
	legend_2 = plt.legend(handles=legend_versions, loc='lower left', bbox_to_anchor=(1, 0))
	plt.gca().add_artist(legend_1)
	plt.title('Average energy over $\lambda$ and duty cycle\n' + "Shape="+ str(shape_type) +', N=' + str(n_nodes))
	plt.xlabel('$\lambda$')
	plt.ylabel('energy')
	plt.xlim(0)
	plt.ylim(0)
	plt.savefig("plt_energy_over_lambda_and_duty.png", dpi=300, pad_inches = 0.05, bbox_inches = 'tight')
	plt.close()

def plot_energy_over_lambda_and_n(runResults):
	Ns = list(OrderedDict.fromkeys([stat.N for stat in runResults.LNstats]))
	protocol_versions = list(OrderedDict.fromkeys([stat.version for stat in runResults.LNstats]))
	d_cycle = runResults.LNstats[0].duty
	shape_type = runResults.LNstats[0].shape
	
	colors = ["green", "blue", "red","grey", "black"] # for duties
	line_styles = ["-", "--", ":", "-."]	# for protocol versions

	legend_Ns = []
	legend_versions = []

	for i in range(len(protocol_versions)):
		version = protocol_versions[i]
		color = colors[i]
		legend_versions.append(patches.Patch(color=color, label=version))
		for j in range(len(Ns)):
			N = Ns[j]
			avg_energy = []
			lambdas = []

			stats = list(filter(lambda s : s.N == N and s.version == version, runResults.LNstats))

			for stat in stats:
				avg_energy.append(np.mean(stat.energy))
				lambdas.append(stat.lam)

			line = plt.plot(lambdas, avg_energy,  marker=".", lw=1, color=color, ls=line_styles[j])
			
			if i==0:
				legend_Ns.append(Line2D([0], [0], color='black', linewidth=2, ls=line_styles[j], label=f'N = {N}'))
	
	legend_1 = plt.legend(handles=legend_Ns, loc='upper left', bbox_to_anchor=(1, 1))
	legend_2 = plt.legend(handles=legend_versions, loc='lower left', bbox_to_anchor=(1, 0))
	plt.gca().add_artist(legend_1)
	plt.title('Average energy over $\lambda$ and N\n' + "Shape="+ str(shape_type) +', d=' + str(d_cycle))
	plt.xlabel('$\lambda$')
	plt.ylabel('energy')
	plt.xlim(0)
	plt.savefig("plt_energy_over_lambda_and_n.png", dpi=300, pad_inches = 0.05, bbox_inches = 'tight')
	plt.close()
