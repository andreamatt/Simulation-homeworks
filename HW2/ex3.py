# Exercise 3
# The binomial distribution describes the statistics of the number of successful events for a Bernoulli experiment
# repeated N times, where the probability of success of each experiment is p.
# 1. Execute Ntr = 10000 trials, each made of N = 100 Bernoulli experiments with probability of success p = 0.05.
# [Hint: to test whether a Bernoulli experiment is successful or not, draw a random number u ∼ U(0, 1), and check if u ≤ p.]
# 2. For each trial i, count the number of successes si, and draw the empirical probability mass function (PMF)
# of the number of successes throughout all trials. Compare against the theoretical binomial PMF.
# 3. Compare the empirical and the theoretical binomial distributions against a Poisson distribution of parameter λ = N p.
# Repeat the comparison for different values of N and p. When does the Poisson PMF accurately approximate the binomial PMF?

# import
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


Ntr = 10000
N = 100
p = 0.05
trials = []
for i in range(Ntr):
	trial = []
	for l in range(N):
		if np.random.rand() <= p:
			trial.append(1)
		else:
			trial.append(0)
	trials.append(trial)

# count successes s_i and draw pmf
trials = np.array(trials)
trial_sums = np.sum(trials, axis=1)
plt.hist(trial_sums, bins = np.arange(0, trial_sums.max() + 1.5) - 0.5, density=True)

# compare against theoretical binomial distribution
X = np.arange(0, 20, 1)
Y = binom.pmf(X, N, p)
plt.plot(X, Y, 'k-', zorder=2)

# compare against a poisson with lambda = N*p
X = np.arange(0, 20, 1)
Y = poisson.pmf(X, N * p)
plt.plot(X, Y, 'r-', zorder=3)

Y = poisson.pmf(X, 2 * p * N)
plt.plot(X, Y, 'm-', zorder=3)

Y = poisson.pmf(X, 4 * p * N)
plt.plot(X, Y, 'y-', zorder=3)
plt.show()
