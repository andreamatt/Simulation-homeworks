from matplotlib import pyplot as plt, patches
import numpy as np
from classes import *

def plot_heatmaps(runResults: RunResult):
    n_protocol_versions = len(runResults.DonutStats)
    fig, axs = plt.subplots(5, n_protocol_versions +1)
    vmin = 0
    vmax = 0

    props = dict(boxstyle='round', facecolor='wheat', alpha=0.5)
    
    for i in range(5):
        for k in range(n_protocol_versions):
            stat = runResults.DonutStats[k] if i < 3 else runResults.SquareStats[k]
            if i == 0:
                textstr = f"version: {stat.version}"
                axs[i][k+1].axis('off')
                axs[i][k+1].text(0.05, 0.95, textstr, transform=axs[i][k+1].transAxes, fontsize=14,
                    verticalalignment='top', bbox=props)
                      
            if k == 0 and i != 0:
                textstr = 'traffic' if i % 2 != 0 else 'failures'   
                axs[i][k].axis('off')
                axs[i][k].text(0.05, 0.95, textstr, transform=axs[i][k].transAxes, fontsize=14,
                        verticalalignment='top', bbox=props)

    textstr = '\n'.join((
        "dati",
        "generali"))

    axs[0][0].axis('off')
    axs[0][0].text(0.05, 0.95, textstr, transform=axs[0][0].transAxes, fontsize=14,
        verticalalignment='top', bbox=props)

    for i in range(n_protocol_versions):
        stat = runResults.DonutStats[i]
        traffic = np.array(stat.traffic).T
        failures = np.array(stat.failurePoints).T
        vmax = max([vmax, np.ndarray.max(traffic), np.ndarray.max(failures)])
        stat = runResults.SquareStats[i]
        traffic = np.array(stat.traffic).T
        failures = np.array(stat.failurePoints).T
        vmax = max([vmax, np.ndarray.max(traffic), np.ndarray.max(failures)])

    for i in range(n_protocol_versions):
        stat = runResults.DonutStats[i]
        traffic = np.array(stat.traffic).T
        failures = np.array(stat.failurePoints).T
        axs[1,i+1].imshow(traffic, vmin=vmin)
        axs[2,i+1].imshow(failures, vmax=vmax)
        axs[2,i+1].set_title(f"Max: {np.max(failures)}")

        
    for i in range(n_protocol_versions):
        stat = runResults.SquareStats[i]
        traffic = np.array(stat.traffic).T
        failures = np.array(stat.failurePoints).T
        axs[3,i+1].imshow(traffic, vmin=vmin)
        axs[4,i+1].imshow(failures, vmax=vmax)
        axs[4,i+1].set_title(f"Max: {np.max(failures)}")

   


        
    plt.tight_layout(h_pad=1)
    plt.show()

