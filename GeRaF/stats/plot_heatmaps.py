from matplotlib import pyplot as plt, patches
import seaborn as sns
import numpy as np
from classes import *


def plot_heatmaps(runResults: RunResult):
	protocol_versions = np.unique(np.array([stat.version for stat in runResults.ShapeStats]))
	shapes = np.unique(np.array([stat.shape for stat in runResults.ShapeStats]))
	
	fig, axs = plt.subplots(len(protocol_versions), len(shapes)*2)

	vmin = 0
	vmax = [0]*len(shapes)*2

	res = 1
	for i in range(len(shapes)):
		shape = shapes[i]
		stats = list(filter(lambda s: s.shape == shape, runResults.ShapeStats))
		vmax[i*2] = np.max([np.max(s.traffic) for s in stats])
		vmax[i*2+1] = np.max([np.max(s.failurePoints) for s in stats])
		for k in range(len(protocol_versions)):
			version = protocol_versions[k]
			stat = stats[k]
			traffic = np.array(stat.traffic).T
			traffic = np.repeat(np.repeat(traffic, res, axis=0), res, axis=1)
			failures = np.array(stat.failurePoints).T
			failures = np.repeat(np.repeat(failures, res, axis=0), res, axis=1)
			axs[k,i*2].imshow(traffic, vmin=vmin, vmax=vmax[i*2], cmap='hot')
			axs[k, 1+i*2].imshow(failures, vmin=vmin, vmax=vmax[i*2+1], cmap='hot')
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

	plt.tight_layout(h_pad=0.08, w_pad=0.08, rect=(0.3, 0.3, 0.97, 0.97))
	plt.show()


def plot_heatmaps_hist(runResults):
	protocol_versions = np.unique(np.array([stat.version for stat in runResults.ShapeStats]))
	shapes = np.unique(np.array([stat.shape for stat in runResults.ShapeStats]))

	fig, axs = plt.subplots(2, len(shapes))

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

	axs[0][0].set_ylabel("traffic")
	axs[1][0].set_ylabel("failures")
	
	for i in range(len(shapes)):
		axs[0][i].set_title(shapes[i])

	plt.show()
