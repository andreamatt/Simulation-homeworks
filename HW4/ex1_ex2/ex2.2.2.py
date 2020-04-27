from lib2 import Simulation
import numpy as np
from numpy import mean, min, max, median, quantile, sqrt
from matplotlib import pyplot as plt
from scipy.stats import expon, norm, erlang
from time import time
from math import factorial as fact


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
λ = 29
µ = 15
c = 5  # number of servers
max_time = 200  #2000 / µ
debug_interval = max_time
Ntr = 100

# RUN SIMULATIONS
simulations = []
for i in range(Ntr):
	if i % 50 == 0:
		print(f"Running simulation {i}")
	sim = Simulation(max_time, λ, µ, c, debug_interval)
	sim.run()
	simulations.append(sim)

ρ = λ / (c * µ)
pi_0 = 1 / (sum([(c * ρ)**k / fact(k) for k in range(0, c)]) + (c * ρ)**c / (fact(c) * (1 - ρ)))
pi_c_plus = (c * ρ)**c / (fact(c) * (1 - ρ)) * pi_0
theor_avg_load = c * ρ + ρ / (1 - ρ) * pi_c_plus
theor_avg_q_time = ρ / (λ * (1 - ρ)) * pi_c_plus


# plot avg system load distributions
means = []
for i in range(c):
	values = [len(sim.servers[i].packets_finished) for sim in simulations]
	m = mean(values)
	plt.plot([i,i], [0,m], 'r-', linewidth=10)

plt.show()