from matplotlib import pyplot as plt, patches
import seaborn as sns
import numpy as np
from classes import *


def plot_heatmaps(runResults: RunResult):
    protocol_versions = list(
        set([stat.version for stat in runResults.ShapeStats]))
    shapes = list(set([stat.shape for stat in runResults.ShapeStats]))
    fig, axs = plt.subplots(len(shapes)*2+1, len(protocol_versions) + 1)
    vmin = 0
    vmax = 0

    props = dict(boxstyle='round', facecolor='wheat', alpha=0.5)

    vmin = 0
    vmax = 0
    for stat in runResults.ShapeStats:
        traffic = np.array(stat.traffic).T
        failures = np.array(stat.failurePoints).T
        vmax = max([vmax, np.ndarray.max(traffic), np.ndarray.max(failures)])

    for i in range(len(shapes)):
        shape = shapes[i]
        stats = list(filter(lambda s: s.shape == shape, runResults.ShapeStats))
        for k in range(len(protocol_versions)):
            version = protocol_versions[k]
            stat = stats[k]
            traffic = np.array(stat.traffic).T
            failures = np.array(stat.failurePoints).T
            axs[1+i*2, k+1].imshow(traffic, vmin=vmin)
            axs[2+i*2, k+1].imshow(failures, vmax=vmax)

    for i in range(len(shapes)):
        axs[1+i*2][0].axis('off')
        axs[1+i*2][0].text(0.05, 0.95, "traffic", transform=axs[1+i*2][0].transAxes, fontsize=14,
                           verticalalignment='top')

        axs[2+i*2][0].axis('off')
        axs[2+i*2][0].text(0.05, 0.95, "failures", transform=axs[2+i*2][0].transAxes, fontsize=14,
                           verticalalignment='top')

    for i in range(len(protocol_versions)):
        axs[0][1+i].axis('off')
        axs[0][1+i].text(0.05, 0.95, protocol_versions[i], transform=axs[0][1+i].transAxes, fontsize=14,
                         verticalalignment='top')

    textstr = '\n'.join((
        "dati",
        "generali"))

    axs[0][0].axis('off')
    axs[0][0].text(0.05, 0.95, textstr, transform=axs[0][0].transAxes, fontsize=14,
                   verticalalignment='top')

    plt.tight_layout(h_pad=1)
    plt.show()


def plot_heatmaps_hist(runResults):
    n_protocol_versions = len(runResults.DonutStats)
    fig, axs = plt.subplots(2, 2)

    for i in range(n_protocol_versions):
        donut = runResults.DonutStats[i]
        square = runResults.SquareStats[i]
        version = runResults.DonutStats[i].version

        d_traffic = np.array(donut.traffic).flatten()
        d_failures = np.array(donut.failurePoints).flatten()
        s_traffic = np.array(square.traffic).flatten()
        s_failures = np.array(square.failurePoints).flatten()

        sns.kdeplot(d_traffic, ax=axs[0, 0], label=version)
        sns.kdeplot(d_failures, ax=axs[0, 1], label=version)
        sns.kdeplot(s_traffic, ax=axs[1, 0], label=version)
        sns.kdeplot(s_failures, ax=axs[1, 1], label=version)

        # print(ax.get_color())

    axs[0, 0].legend()

    plt.show()
