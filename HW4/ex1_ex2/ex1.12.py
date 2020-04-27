from lib import Simulation
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


# 12: show number of packets in system over time
λ = 10
µ = 15
c = 1	# number of servers
max_time = 50#2000 / µ
debug_interval = max_time/20
Ntr = 1000

# RUN SIMULATIONS
simulations = []
for i in range(Ntr):
	if i % 50 == 0:
		print(f"Running simulation {i}")
	sim = Simulation(max_time, λ, µ, c, debug_interval)
	sim.run()
	simulations.append(sim)

ρ = λ / µ
theor_avg_load = ρ / (1 - ρ)
theor_avg_q_time = ρ**2 / (λ * (1 - ρ))


t_interval = debug_interval
ts = np.arange(0, max_time, t_interval)[1:]

# plot avg system load distributions
hist_per_t = []
bin_edges_per_t = []
means_per_t = []
std_per_t = []
CI_per_t = []
std_factor = 1.5
confidence = 0.95
for i in range(len(ts)):
	values = [sim.debugStats[i].cum_avg_load for sim in simulations]
	s = std(values)
	m = mean(values)
	hist, bin_edges = np.histogram(values, bins=50, density=True)
	hist_per_t.append(hist)
	bin_edges_per_t.append(bin_edges)
	means_per_t.append(m)
	std_per_t.append(s)
	CI_per_t.append(mean_confidence_asymptotic(values, confidence))

scale_factor = max([max(h) for h in hist_per_t]) * 2

plt.plot([0, max_time], [theor_avg_load, theor_avg_load], 'y-', linewidth=1)
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
	plt.plot(t - hist * t_interval, bin_edges + bin_w / 2, 'b-', linewidth=1, zorder=3)
	plt.plot([t, t], [min_h - bin_w / 2, max_h + bin_w / 2], 'b-', linewidth=0.5)
	plt.plot([t, t], CI, 'r-', linewidth=2)
	plt.plot([t], [m], 'ko', markersize=3)

plt.show()



# plot avg q_time distributions
hist_per_t = []
bin_edges_per_t = []
means_per_t = []
std_per_t = []
CI_per_t = []
std_factor = 1.5
confidence = 0.95
for i in range(len(ts)):
	values = [sim.debugStats[i].cum_avg_q_time for sim in simulations]
	s = std(values)
	m = mean(values)
	hist, bin_edges = np.histogram(values, bins=30, density=True)
	hist_per_t.append(hist)
	bin_edges_per_t.append(bin_edges)
	means_per_t.append(m)
	std_per_t.append(s)
	CI_per_t.append(mean_confidence_asymptotic(values, confidence))

scale_factor = max([max(h) for h in hist_per_t]) * 2

plt.plot([0, max_time], [theor_avg_q_time, theor_avg_q_time], 'y-', linewidth=1)
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
	plt.plot(t - hist * t_interval, bin_edges + bin_w / 2, 'b-', linewidth=1, zorder=3)
	plt.plot([t, t], [min_h - bin_w / 2, max_h + bin_w / 2], 'b-', linewidth=0.5)
	plt.plot([t, t], CI, 'r-', linewidth=2)
	plt.plot([t], [m], 'ko', markersize=3)

# plt.show()