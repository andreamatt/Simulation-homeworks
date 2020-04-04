# import and solve data
import numpy as np
from matplotlib import pyplot as plt
from numpy import mean, min, max, median, quantile
from scipy.stats import binom, norm, t as student, expon, poisson, chi2
from math import sqrt, pow, floor, ceil, exp, log

# define corrected functions for std and variance


def std(values):
	return np.std(values, ddof=1)


Ntr = 10000
results = -np.log(np.random.rand(Ntr)) / 2
results = expon.ppf(np.random.rand(Ntr), scale=1 / 2)
results = sorted(results)
# plt.hist(results)
# plt.show()

X = []
Y = []
n = len(results)
for i in np.arange(0, n, 1):
	X.append(expon.ppf((i + 1) / (n + 1), scale=1 / 2))
	Y.append(results[i])
X = np.array(X)
Y = np.array(Y)
m, b = np.polyfit(X, Y, 1)
plt.scatter(X, Y, s=2)
plt.plot([X[0], X[-1]], [m * X[0] + b, m * X[-1] + b], 'r--')

results = -np.log(np.random.rand(Ntr)) / 5
results = sorted(results)
X = []
Y = []
n = len(results)
for i in np.arange(0, n, 1):
	X.append(expon.ppf((i + 1) / (n + 1), scale=1 / 5))
	Y.append(results[i])
X = np.array(X)
Y = np.array(Y)
m, b = np.polyfit(X, Y, 1)
plt.scatter(X, Y, s=2)
plt.plot([X[0], X[-1]], [m * X[0] + b, m * X[-1] + b], 'r--')
plt.show()
