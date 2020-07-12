from matplotlib import pyplot as plt, patches
import numpy as np
from classes import *

def plot_heatmaps(runResults: RunResult):
    n_protocol_versions = len(runResults.DonutStats)
    fig, axs = plt.subplots(4, n_protocol_versions)
    vmin = 0
    vmax = 0
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
        axs[0,i].imshow(traffic, vmin=vmin)
        axs[1,i].imshow(failures, vmax=vmax)
        
    for i in range(n_protocol_versions):
        stat = runResults.SquareStats[i]
        traffic = np.array(stat.traffic).T
        failures = np.array(stat.failurePoints).T
        axs[2,i].imshow(traffic, vmin=vmin)
        axs[3,i].imshow(failures, vmax=vmax)
        
    plt.show()

