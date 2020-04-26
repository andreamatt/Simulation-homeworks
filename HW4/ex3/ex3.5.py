from lib3 import Simulation
import numpy as np
from numpy import mean, min, max, median, quantile, sqrt
from matplotlib import pyplot as plt
from scipy.stats import expon, norm, erlang
from time import time

def std(values):
	return np.std(values, ddof=1)


def mean_confidence_asymptotic(values, confidence):
	# when variables iid and high n
	n = len(values)
	z = norm.ppf((1 + confidence) / 2)
	s = std(values)
	m = mean(values)
	j = m - z * s / sqrt(n)
	k = m + z * s / sqrt(n)
	return (j, k)


max_time = 1000000
side = 1000
nodes = 10
debug_interval = max_time/20
Ntr = 100
# plot bins
bins = 20

axs, figs = plt.subplots(2)

min_speed = 0
max_speed = 10
# RUN SIMULATIONS for point 5
simulations = []
for i in range(Ntr):
	if i % 50 == 0:
		print(f"Running simulation {i}")
	sim = Simulation(max_time, side, min_speed, max_speed, nodes, debug_interval)
	sim.run()
	simulations.append(sim)


t_interval = debug_interval
ts = np.arange(0, max_time, t_interval)[1:]

hist_per_t = []
bin_edges_per_t = []
means_per_t = []
std_per_t = []
CI_per_t = []
std_factor = 1.5
confidence = 0.95
for i in range(len(ts)):
	values = [sim.debugStats[i].avg_speed for sim in simulations]
	s = std(values)
	m = mean(values)
	hist, bin_edges = np.histogram(values, bins=bins, density=True)
	hist_per_t.append(hist)
	bin_edges_per_t.append(bin_edges)
	means_per_t.append(m)
	std_per_t.append(s)
	CI_per_t.append(mean_confidence_asymptotic(values, confidence))

scale_factor = max([max(h) for h in hist_per_t]) * 2

for i in range(len(ts)):
	t = ts[i]
	m = means_per_t[i]
	s = std_per_t[i]
	hist = hist_per_t[i] / scale_factor
	bin_edges = bin_edges_per_t[i][:-1]
	bin_w = bin_edges[1] - bin_edges[0]
	CI = CI_per_t[i]
	min_h = max((m - std_factor * s * 1.5, 0))
	max_h = m + std_factor * s * 1.5
	valid_indexes = np.argwhere(np.logical_and(bin_edges > min_h, bin_edges < max_h))[:, 0]
	# truncate
	hist = hist[valid_indexes]
	bin_edges = bin_edges[valid_indexes]
	figs[0].plot(t - hist * t_interval, bin_edges + bin_w / 2, 'b-', linewidth=1, zorder=3)
	figs[0].plot([t, t], [min_h - bin_w / 2, max_h + bin_w / 2], 'b-', linewidth=0.5)
	figs[0].plot([t, t], CI, 'r-', linewidth=2)
	figs[0].plot([t], [m], 'ko', markersize=3)


# min_speed = 1
# # RUN SIMULATIONS for point 6
# simulations = []
# for i in range(Ntr):
# 	if i % 50 == 0:
# 		print(f"Running simulation {i}")
# 	sim = Simulation(max_time, side, min_speed, max_speed, nodes, debug_interval)
# 	sim.run()
# 	simulations.append(sim)


# t_interval = debug_interval
# ts = np.arange(0, max_time, t_interval)[1:]

# hist_per_t = []
# bin_edges_per_t = []
# means_per_t = []
# std_per_t = []
# CI_per_t = []
# std_factor = 1.5
# confidence = 0.95
# for i in range(len(ts)):
# 	values = [sim.debugStats[i].avg_speed for sim in simulations]
# 	s = std(values)
# 	m = mean(values)
# 	hist, bin_edges = np.histogram(values, bins=bins, density=True)
# 	hist_per_t.append(hist)
# 	bin_edges_per_t.append(bin_edges)
# 	means_per_t.append(m)
# 	std_per_t.append(s)
# 	CI_per_t.append(mean_confidence_asymptotic(values, confidence))

# scale_factor = max([max(h) for h in hist_per_t]) * 2

# for i in range(len(ts)):
# 	t = ts[i]
# 	m = means_per_t[i]
# 	s = std_per_t[i]
# 	hist = hist_per_t[i] / scale_factor
# 	bin_edges = bin_edges_per_t[i][:-1]
# 	bin_w = bin_edges[1] - bin_edges[0]
# 	CI = CI_per_t[i]
# 	min_h = max((m - std_factor * s * 1.5, 0))
# 	max_h = m + std_factor * s * 1.5
# 	valid_indexes = np.argwhere(np.logical_and(bin_edges > min_h, bin_edges < max_h))[:, 0]
# 	# truncate
# 	hist = hist[valid_indexes]
# 	bin_edges = bin_edges[valid_indexes]
# 	figs[1].plot(t - hist * t_interval, bin_edges + bin_w / 2, 'b-', linewidth=1, zorder=3)
# 	figs[1].plot([t, t], [min_h - bin_w / 2, max_h + bin_w / 2], 'b-', linewidth=0.5)
# 	figs[1].plot([t, t], CI, 'r-', linewidth=2)
# 	figs[1].plot([t], [m], 'ko', markersize=3)

plt.show()

