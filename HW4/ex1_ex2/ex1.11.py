from lib import Simulation
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


λ = 10
µ = 15
c = 1  # number of servers
max_time = 1000  # 2000 / µ
debug_interval = max_time
Ntr = 200  # number of simulations to run

# RUN SIMULATIONS
simulations = []
for i in range(Ntr):
	if i % 50 == 0:
		print(f"Running simulation {i}")
	sim = Simulation(max_time, λ, µ, c, debug_interval)
	sim.run()
	simulations.append(sim)
print(f'Finished simulating')

# 11: calculate expected avg q_time at the end
ρ = λ / (c * µ)
pi_0 = 1 / (sum([(c * ρ)**k / fact(k) for k in range(0, c)]) + (c * ρ)**c / (fact(c) * (1 - ρ)))
pi_c_plus = (c * ρ)**c / (fact(c) * (1 - ρ)) * pi_0
theor_avg_load = c * ρ + ρ / (1 - ρ) * pi_c_plus
theor_avg_q_time = ρ / (λ * (1 - ρ)) * pi_c_plus
theor_avg_q_size = ρ / (1 - ρ) * pi_c_plus

values = [mean(sim.q_times) for sim in simulations]
interval = mean_confidence_asymptotic(values, 0.95)
print(f'theor avg q time: {theor_avg_q_time: .4g}, empirical: {mean(values): .4g} with CI at 0.95: [{interval[0]: .4g} - {interval[1]: .4g}]')
