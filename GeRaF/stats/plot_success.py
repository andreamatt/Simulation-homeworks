from matplotlib import pyplot as plt, patches
from matplotlib.lines import Line2D
import numpy as np
from classes import *
from collections import OrderedDict


def plot_success_over_lambda_and_duty(runResults):
	duty_cycles = list(OrderedDict.fromkeys([stat.duty for stat in runResults.DLstats]))
	protocol_versions = list(OrderedDict.fromkeys([stat.version for stat in runResults.DLstats]))
	n_nodes = runResults.DLstats[0].N
	shape_type = runResults.DLstats[0].shape

	colors = ["red", "blue", "green", "grey", "black"] # for duties
	#line_markers = ["-", "-", ":", "-.", '--']	# for protocol versions
	dashes_list = [
		(1, 0),
		(5, 2),
		(2, 1, 4, 3),
		(2, 2, 10, 2),
		(1,3)
	]
	legend_patches = []
	legend_versions = []
	for i, v in enumerate(protocol_versions):
		legend_versions.append(Line2D([0], [0], color='black', linewidth=2, dashes=dashes_list[i], label=v))
	
	for i in range(len(protocol_versions)):
		version = protocol_versions[i]
		for j in range(len(duty_cycles)):
			duty = duty_cycles[j]
			color = colors[j]
			avg_succ = []
			lambdas = []

			stats = list(filter(lambda s : s.duty == duty and s.version == version, runResults.DLstats))

			for stat in stats:
				avg_succ.append(np.mean(stat.success))
				lambdas.append(stat.lam)

			line = plt.plot(lambdas, avg_succ,  marker=".", lw=1, color=color, dashes=dashes_list[i])
			
			if i==0:
				legend_patches.append(patches.Patch(color=color, label=f'd = {duty}'))
	
	legend_1 = plt.legend(handles=legend_patches, loc='upper left', bbox_to_anchor=(1, 1))
	legend_2 =plt.legend(handles=legend_versions, loc='lower left', bbox_to_anchor=(1, 0))
	plt.gca().add_artist(legend_1)
	plt.title('Percentage of packets successfully delivered to the sink\nN=' + str(n_nodes) + ", Shape=" + str(shape_type))
	plt.xlabel('$\lambda$')
	plt.ylabel('% success delivery')
	plt.xlim(0)
	plt.ylim(0,1)
	plt.savefig("plt_success_over_lambda_and_duty.png", dpi=300, pad_inches = 0.05, bbox_inches = 'tight')
	plt.close()


def plot_success_over_lambda_and_n(runResults):
	Ns = list(OrderedDict.fromkeys([stat.N for stat in runResults.LNstats]))
	protocol_versions = list(OrderedDict.fromkeys([stat.version for stat in runResults.LNstats]))
	d_cycle = runResults.LNstats[0].duty
	shape_type = runResults.LNstats[0].shape

	colors = ["red", "blue", "green", "grey", "black"] # for duties
	#line_markers = ["-", "-", ":", "-.", '--']	# for protocol versions
	dashes_list = [
		(1, 0),
		(5, 2),
		(2, 1, 4, 3),
		(2, 2, 10, 2),
		(1,3)
	]
	legend_patches = []
	legend_versions = []
	for i, v in enumerate(protocol_versions):
		legend_versions.append(Line2D([0], [0], color='black', linewidth=2, dashes=dashes_list[i], label=v))
	
	for i in range(len(protocol_versions)):
		version = protocol_versions[i]
		for j in range(len(Ns)):
			N = Ns[j]
			color = colors[j]
			avg_succ = []
			lambdas = []

			stats = list(filter(lambda s : s.N == N and s.version == version, runResults.LNstats))

			for stat in stats:
				avg_succ.append(np.mean(stat.success))
				lambdas.append(stat.lam)

			line = plt.plot(lambdas, avg_succ, marker=".", lw=1, color=color, dashes=dashes_list[i]) 
			
			if i==0:
				legend_patches.append(patches.Patch(color=color, label=f'N = {N}'))
	
	legend_1 = plt.legend(handles=legend_patches, loc='upper left', bbox_to_anchor=(1, 1))
	legend_2 =plt.legend(handles=legend_versions, loc='lower left', bbox_to_anchor=(1, 0))
	plt.gca().add_artist(legend_1)
	plt.title('Percentage of packets successfully delivered to the sink\nd=' + str(d_cycle) + ", Shape=" + str(shape_type))
	plt.xlabel('$\lambda$')
	plt.ylabel('% success delivery')
	plt.xlim(0)
	plt.ylim(0,1)
	plt.savefig("plt_success_over_lambda_and_n.png", dpi=300, pad_inches = 0.05, bbox_inches = 'tight')
	plt.close()