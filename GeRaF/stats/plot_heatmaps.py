from matplotlib import pyplot as plt, patches
from mpl_toolkits.axes_grid1 import make_axes_locatable
from collections import OrderedDict
import seaborn as sns
import numpy as np
from classes import *


def plot_heatmaps(runResults: RunResult):
	protocol_versions = list(OrderedDict.fromkeys([stat.version for stat in runResults.ShapeStats]))
	shapes = list(OrderedDict.fromkeys([stat.shape for stat in runResults.ShapeStats]))
	lam = runResults.ShapeStats[0].lam
	N = runResults.ShapeStats[0].N
	d_cycle = runResults.ShapeStats[0].duty

	fig, axs = plt.subplots(len(protocol_versions), len(shapes)*2, figsize=(15,15))
	growth = lambda x: 3.38*10**(-3)*x**3 -3.954*10**(-2)*x**2 + 0.1625*x - 1.082
	fig.subplots_adjust(hspace= growth(len(protocol_versions)))
	fig.subplots_adjust(wspace=0.03)

	vmin = 0
	vmax = [0]*len(shapes)*2

	luminosity = 1
	res = 1
	for i in range(len(shapes)):
		shape = shapes[i]
		stats = list(filter(lambda s: s.shape == shape, runResults.ShapeStats))
		vmax[i*2] = np.max([np.percentile(s.traffic,99) for s in stats]) / luminosity
		vmax[i*2+1] = np.max([np.percentile(s.failurePoints, 99) for s in stats]) / luminosity
		for k in range(len(protocol_versions)):
			version = protocol_versions[k]
			stat = stats[k]
			traffic = stat.traffic / vmax[i*2]
			failures = stat.failurePoints  / vmax[i*2+1]

			axs[k,i*2].imshow(traffic, vmin=vmin, vmax=1, cmap='afmhot')
			im = axs[k, 1+i*2].imshow(failures, vmin=vmin, vmax=1, cmap='afmhot')
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

	cbar_ax = fig.add_axes([0.91, 0.31, 0.01, 0.37])
	fig.colorbar(im, cax=cbar_ax)
	#plt.tight_layout(h_pad=0.08, w_pad=0.08, rect=(0.3, 0.3, 0.97, 0.97))
	plt.suptitle('Relative Traffic Flow\nN_density=' + str(N) + ",  $\lambda=$" + str(lam) + ",  Duty_cycle=" + str(d_cycle), fontsize=16, y=0.74)
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