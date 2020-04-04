# import and solve data
import numpy as np
from matplotlib import pyplot as plt
from numpy import mean, min, max, median, quantile
from scipy.stats import binom, norm, t as student, expon, poisson
from math import sqrt, pow, floor, ceil, exp

# define corrected functions for std and variance
def variance(values):
    return np.var(values, ddof=1)

def std(values):
    return np.std(values, ddof=1)

trials = []
for i in range(10000):
	trial = []
	for l in range(100):
		if np.random.rand()<=0.05:
			trial.append(1)
		else:
			trial.append(0)
	trials.append(trial)

trials = np.array(trials)
trial_sums = np.sum(trials, axis=1)
plt.hist(trial_sums, bins=20, density=True)

X = np.arange(0, 20, 1)
Y = binom.pmf(X, 100, 0.05)
plt.plot(X,Y, 'k-', zorder=2)

Y = poisson.pmf(X, 0.05*100)
plt.plot(X,Y, 'r-', zorder=3)

Y = poisson.pmf(X, 0.05*200)
plt.plot(X,Y, 'r-', zorder=3)
plt.show()

