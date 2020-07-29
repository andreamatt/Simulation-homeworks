from matplotlib import pyplot as plt, patches
from collections import OrderedDict
import seaborn as sns
import numpy as np
from classes import *


def plot_heatmaps(runResults: RunResult):
	protocol_versions = list(OrderedDict.fromkeys([stat.version for stat in runResults.ShapeStats]))
	shapes = list(OrderedDict.fromkeys([stat.shape for stat in runResults.ShapeStats]))
	
	fig, axs = plt.subplots(len(protocol_versions), len(shapes)*2, figsize=(15,15))
	growth = lambda x: 3.38*10**(-3)*x**3 -3.954*10**(-2)*x**2 + 0.1625*x - 1.082
	fig.subplots_adjust(hspace= growth(len(protocol_versions)))
	fig.subplots_adjust(wspace=0.03)

	vmin = 0
	vmax = [0]*len(shapes)*2

	luminosity = 2
	res = 1
	for i in range(len(shapes)):
		shape = shapes[i]
		stats = list(filter(lambda s: s.shape == shape, runResults.ShapeStats))
		vmax[i*2] = np.max([np.max(s.traffic) for s in stats]) / luminosity
		vmax[i*2+1] = np.max([np.max(s.failurePoints) for s in stats]) / luminosity
		for k in range(len(protocol_versions)):
			version = protocol_versions[k]
			stat = stats[k]
			traffic = np.array(stat.traffic).T
			traffic = np.repeat(np.repeat(traffic, res, axis=0), res, axis=1)
			failures = np.array(stat.failurePoints).T
			failures = np.repeat(np.repeat(failures, res, axis=0), res, axis=1)
			axs[k,i*2].imshow(traffic, vmin=vmin, vmax=vmax[i*2], cmap='afmhot')
			axs[k, 1+i*2].imshow(failures, vmin=vmin, vmax=vmax[i*2+1], cmap='afmhot')
			if i!=0:
				axs[k,i*2].axis('off')
			axs[k, 1+i*2].axis('off')

	for i in range(len(shapes)):
		axs[0][i*2].set_title("traffic")
		axs[0][i*2+1].set_title("failures")
		
	for k in range(len(protocol_versions)):
		axs[k][0].set_ylabel(protocol_versions[k])
		axs[k][0].set_yticklabels([])
		axs[k][0].set_xticklabels([])
		axs[k][0].set_yticks([])
		axs[k][0].set_xticks([])

	#plt.tight_layout(h_pad=0.08, w_pad=0.08, rect=(0.3, 0.3, 0.97, 0.97))
	#plt.suptitle("Traffic Flow", y=0.604, fontsize=20)
	plt.savefig("plt_heatmaps.png", dpi=300, bbox_inches = 'tight', pad_inches = 0.05)
	plt.close()


def plot_heatmaps_hist(runResults):
	protocol_versions = list(OrderedDict.fromkeys([stat.version for stat in runResults.ShapeStats]))
	shapes = list(OrderedDict.fromkeys([stat.shape for stat in runResults.ShapeStats]))

	fig, axs = plt.subplots(2, len(shapes), figsize=(15,10))
	fig.subplots_adjust(hspace=0.1)
	fig.subplots_adjust(wspace=0.25)

	for i in range(len(shapes)):
		shape = shapes[i]
		stats = list(filter(lambda s: s.shape == shape, runResults.ShapeStats))
		for k in range(len(protocol_versions)):
			version = protocol_versions[k]
			stat = stats[k]
			traffic = np.array(stat.traffic).flatten()
			failures = np.array(stat.failurePoints).flatten()

			sns.kdeplot(traffic, ax=axs[0, i], label=version)
			sns.kdeplot(failures, ax=axs[1, i], label=version)
			#ax=axs[0, i].set_xlim(0,0.04)

	axs[0][0].set_ylabel("traffic", fontsize=13)
	axs[1][0].set_ylabel("failures", fontsize=13)
	
	for i in range(len(shapes)):
		axs[0][i].set_title(shapes[i])

	plt.suptitle('Traffic Hist', y=0.94, fontsize=18)
	plt.savefig("plt_heatmaps_hist.png", dpi=300, bbox_inches = 'tight', pad_inches = 0.05)
	plt.close()