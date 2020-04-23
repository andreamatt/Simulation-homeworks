from lib import Simulation
import numpy as np
from numpy import mean, min, max, median, quantile, sqrt
from matplotlib import pyplot as plt
from scipy.stats import expon, norm
from time import time


def variance(values):
	return np.var(values, ddof=1)


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


λ = 10
µ = 15
max_time = 2000 / µ
Ntr = 5000
simulations = []
for i in range(Ntr):
	if i % 50 == 0:
		print(f"Running simulation {i}")
	sim = Simulation(max_time, λ, µ)
	sim.run()
	simulations.append(sim)

# s1 = simulations[0]
# s2 = simulations[1]
# s3 = simulations[2]

# plt.plot(s1.event_times[:100], s1.avg_tot_times[:100], 'b-', linewidth=0.2)
# plt.plot(s2.event_times[:100], s2.avg_tot_times[:100], 'g-', linewidth=0.2)
# plt.plot(s3.event_times[:100], s3.avg_tot_times[:100], 'r-', linewidth=0.2)
# plt.plot([0, s3.event_times[100]], [1 / (µ - λ), 1 / (µ - λ)], 'k-', linewidth=0.4)
# plt.show()

# plt.plot(s1.event_times[-100:], s1.avg_tot_times[-100:], 'b-', linewidth=0.2)
# plt.plot(s2.event_times[-100:], s2.avg_tot_times[-100:], 'g-', linewidth=0.2)
# plt.plot(s3.event_times[-100:], s3.avg_tot_times[-100:], 'r-', linewidth=0.2)
# plt.plot([s3.event_times[-100], s3.event_times[-1]], [1 / (µ - λ), 1 / (µ - λ)], 'k-', linewidth=0.4)
# plt.show()

# plot emp distr of n of packets in the system at time t1

ρ = λ / µ
theor_avg_q_size = ρ / (1 - ρ)
theor_avg_q_time = ρ**2 / (λ * (1 - ρ))
print(f'theor avg q time: {theor_avg_q_time}')

t_interval = max_time / 5
ts = np.arange(0.05 * max_time, max_time, t_interval)
hist_per_t = []
bin_edges_per_t = []
means_per_t = []
std_per_t = []
CI_per_t = []
std_factor = 2
confidence = 0.95
for t in ts:
	values = []
	for sim in simulations:
		index = np.argwhere(sim.event_times < t)[-1][0]
		value = sim.avg_q_times[index]
		values.append(value)

	s = std(values)
	m = mean(values)
	hist, bin_edges = np.histogram(values, bins=100, density=True)  #range=[min_h, max_h]
	hist_per_t.append(hist)
	bin_edges_per_t.append(bin_edges)
	means_per_t.append(m)
	std_per_t.append(s)
	CI_per_t.append(mean_confidence_asymptotic(values, confidence))

scale_factor = max([max(h) for h in hist_per_t]) * 2
print("scale: ", scale_factor)
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

plt.plot([0, max_time], [theor_avg_q_time, theor_avg_q_time], 'y-', linewidth=1)
plt.show()

# # print(f'Theoretical: {theor_avg}, empirical: {mean(sim.area)}, events: {(len(Y)-2)/2}')

# print(f'Active server time %: {sim.areaU / max_time}, theoric: {ρ}')
# print(f'Average q size: {(sim.areaQ+sim.areaU) / max_time}, theoric: {theor_avg_q_size}')
# print(f'Average q waiting time: {mean([2])}, theoric: {theor_avg_q_time}, finished packets: {len(sim.packets_finished)}')
