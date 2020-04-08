import numpy as np
from matplotlib import pyplot as plt
from numpy import mean, min, max, median, quantile
from scipy.stats import binom, norm, t as student, expon, poisson, chi2
from math import sqrt, pow, floor, ceil, exp, log, sin, pi
from statsmodels.api import qqplot

def std(values):
    return np.std(values, ddof=1)

def mean_confidence_asymptotic(values, confidence):
    # when variables iid and high n
    n = len(values)
    z = norm.ppf((1+confidence)/2)
    s = std(values)
    m = mean(values)
    j = m - z*s/sqrt(n)
    k = m + z*s/sqrt(n)
    return (j, k)

P = [[0.75, 0.25, 0, 0], [0.25, 0.50, 0.25, 0], [0, 0.40, 0.40, 0.20], [0, 0, 0.25, 0.75]]

states = [0, 1, 2, 3]

Ntr = 10000
counts = np.array([0, 0, 0, 0])
current_state = np.random.choice(states)
through_Mbit = np.array([1500, 1000, 250, 50])
through_items = []

fig, axs = plt.subplots(2)

for i in range(Ntr):
	probs = P[current_state]
	current_state = np.random.choice(states, p=probs)
	counts[current_state] += 1
	through_items.append(through_Mbit[current_state])
	if i % 100 == 50:
		axs[0].scatter([i] * 4, counts / i, c=['r', 'b', 'k', 'g'], s=0.5)
		interval = mean_confidence_asymptotic(through_items, 0.95)
		m = mean(through_items)
		axs[1].scatter([i], [m], c='k', s=0.5)
		axs[1].plot([i, i], [interval[0], interval[1]], 'b-', linewidth=0.2)


# print(mean(through_items))
interval = mean_confidence_asymptotic(through_items, 0.95)
# print(f"Mean CI at level 0.95: [{interval[0]: .0f}, {interval[1]: .0f}]")

axs[0].plot([0, Ntr], [0.32, 0.32], 'r-')
axs[0].plot([0, Ntr], [0.32, 0.32], 'b-')
axs[0].plot([0, Ntr], [0.20, 0.20], 'k-')
axs[0].plot([0, Ntr], [0.16, 0.16], 'g-')
plt.show()