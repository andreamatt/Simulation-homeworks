from matplotlib import pyplot as plt, patches
import numpy as np
from classes import *
from collections import OrderedDict
import seaborn as sns
import pandas as pd


def plot_outcomes(runResults):
	shapes = list(OrderedDict.fromkeys([stat.shape for stat in runResults.outcomeStats]))
	protocol_versions = list(OrderedDict.fromkeys([stat.version for stat in runResults.outcomeStats]))
	lam = runResults.outcomeStats[0].lam
	N = runResults.outcomeStats[0].N
	d_cycle = runResults.outcomeStats[0].duty

	fig, axs = plt.subplots(1, len(shapes), figsize=(15,5), sharey=True)
	fig.subplots_adjust(wspace=0.20)

	interesting_outcomes = [1, 4, 5, 6, 7]
	outcomes_names = ["Success","No CTS","Channel busy","No sink CTS","No ack"]

	for i in range(len(shapes)):
		shape = shapes[i]
		data = []
		for j in range(len(protocol_versions)):
			version = protocol_versions[j]

			stat = list(filter(lambda s : s.shape==shape and s.version == version, runResults.outcomeStats))[0] # only 1 per version/shape

			outcomes = np.array(stat.averageOutcomes)[interesting_outcomes]
			for o in range(len(outcomes)):
				data.append([outcomes_names[o], outcomes[o], version])

		data = pd.DataFrame(data).rename(columns={2:"Version"})
		sns.barplot(x=0, y=1, hue="Version", data=data, ax=axs[i])
		axs[i].set_xticklabels(labels=outcomes_names, rotation=30, horizontalalignment='right')
		axs[i].set_xlabel("")
		axs[i].set_ylabel("")
		axs[i].set_title(shape)

	plt.suptitle('Outcomes By Obstacle Type\nN_density=' + str(N) + ",  $\lambda=$" + str(lam) + ",  duty=" + str(d_cycle), y=1.01, fontsize=14)
	plt.savefig("plt_outcomes.png",  dpi=300, bbox_inches = 'tight', pad_inches = 0.15)
	plt.close()


	# for i in range(len(protocol_versions)):
	# 	version = protocol_versions[i]
	# 	marker = line_markers[i]
	# 	for j in range(len(duty_cycles)):
	# 		duty = duty_cycles[j]
	# 		color = colors[j]
	# 		avg_succ = []
	# 		lambdas = []

	# 		stats = list(filter(lambda s : s.duty == duty and s.version == version, runResults.outcomeStats))

	# 		for stat in stats:
	# 			avg_succ.append(np.mean(stat.success))
	# 			lambdas.append(stat.lam)

	# 		line = plt.plot(lambdas, avg_succ, marker, marker=".", lw=1, color=color)
			
	# 		if i==0:
	# 			legend_patches.append(patches.Patch(color=color, label=f'd = {duty}'))
	
	# plt.legend(handles=legend_patches)
	# plt.title('Percentage of packets successfully delivered to the sink, N=?')
	# plt.xlabel('$\lambda$')
	# plt.ylabel('% success delivery')
	# plt.show()