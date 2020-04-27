from lib import Simulation
import numpy as np
from numpy import mean, min, max, median, quantile, sqrt
from matplotlib import pyplot as plt
from scipy.stats import expon, norm, erlang
from time import time
from math import factorial as fact

# 10: compare
c = 1  # number of servers
max_time = 1000  #2000 / µ
debug_interval = max_time / 200
Ntr = 3  # number of simulations to run

parameters = [(1, 15), (10, 15), (14, 15)]
axs, figs = plt.subplots(len(parameters))
for i in range(len(parameters)):
	λ, µ = parameters[i]
	f = figs[i]

	# RUN SIMULATIONS
	simulations = []
	for i in range(Ntr):
		print(f"Running simulation {i}")
		sim = Simulation(max_time, λ, µ, c, debug_interval)
		sim.run()
		simulations.append(sim)
	print(f'Finished simulating')

	ρ = λ / (c * µ)
	pi_0 = 1 / (sum([(c * ρ)**k / fact(k) for k in range(0, c)]) + (c * ρ)**c / (fact(c) * (1 - ρ)))
	pi_c_plus = (c * ρ)**c / (fact(c) * (1 - ρ)) * pi_0
	theor_avg_load = c * ρ + ρ / (1 - ρ) * pi_c_plus

	f.plot([0, max_time], [theor_avg_load] * 2, 'k-', linewidth=1)
	for sim in simulations:
		debug_times = [ds.event_time for ds in sim.debugStats]
		cum_avg_loads = [ds.cum_avg_load for ds in sim.debugStats]
		f.plot(debug_times, cum_avg_loads, '-', c=np.random.rand(3,), linewidth=1)
plt.show()
