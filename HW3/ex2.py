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
	z = norm.ppf((1 + confidence) / 2)
	s = std(values)
	m = mean(values)
	j = m - z * s / sqrt(n)
	k = m + z * s / sqrt(n)
	return (j, k)


P = [[0.75, 0.25, 0, 0], [0.25, 0.50, 0.25, 0], [0, 0.40, 0.40, 0.20], [0, 0, 0.25, 0.75]]

states = [0, 1, 2, 3]

Ntr = 100000
counts = np.array([0, 0, 0, 0])
current_state = np.random.choice(states)
through_Mbit = np.array([1500, 1000, 250, 50])
through_items = []
confidence = 0.95

fig, axs = plt.subplots(2)

for i in range(Ntr):
	probs = P[current_state]
	current_state = np.random.choice(states, p=probs)
	counts[current_state] += 1
	through_items.append(through_Mbit[current_state])
	if i % 500 == 50:
		axs[0].scatter([i] * 4, counts / i, c=['r', 'b', 'k', 'g'], s=0.5)
		interval = mean_confidence_asymptotic(through_items, confidence)
		m = mean(through_items)
		axs[1].scatter([i], [m], c='k', s=0.5)
		axs[1].plot([i, i], [interval[0], interval[1]], 'b-', linewidth=0.2)

theoric_chances = np.array([0.32, 0.32, 0.20, 0.16])
theoric_mean = sum(theoric_chances * through_Mbit)
axs[1].plot([0, Ntr], [theoric_mean, theoric_mean], 'r-', linewidth=0.5)

axs[0].plot([0, Ntr], [theoric_chances[0], theoric_chances[0]], 'r-', linewidth=0.5)
axs[0].plot([0, Ntr], [theoric_chances[1], theoric_chances[1]], 'b-', linewidth=0.5)
axs[0].plot([0, Ntr], [theoric_chances[2], theoric_chances[2]], 'k-', linewidth=0.5)
axs[0].plot([0, Ntr], [theoric_chances[3], theoric_chances[3]], 'g-', linewidth=0.5)

np.set_printoptions(precision=3)
print(f"Fractions of time in each state: {counts/Ntr}")
interval = mean_confidence_asymptotic(through_items, confidence)
print(f"Mean CI at level 0.95: [{interval[0]: .0f}, {interval[1]: .0f}]")
plt.show()