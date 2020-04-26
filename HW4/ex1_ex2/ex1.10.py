from lib import Simulation
import numpy as np
from numpy import mean, min, max, median, quantile, sqrt
from matplotlib import pyplot as plt
from scipy.stats import expon, norm, erlang
from time import time

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

	ρ = λ / µ
	theor_avg_q_size = ρ / (1 - ρ)
	f.plot([0, max_time], [theor_avg_q_size] * 2, 'k-', linewidth=1)
	for sim in simulations:
		debug_times = [ds.event_time for ds in sim.debugStats]
		cum_avg_loads = [ds.cum_avg_load for ds in sim.debugStats]
		avg_loads = [ds.avg_load for ds in sim.debugStats]
		f.plot(debug_times, cum_avg_loads, '-', c=np.random.rand(3,), linewidth=1)
plt.show()
