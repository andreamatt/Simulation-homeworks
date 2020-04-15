import numpy as np
from matplotlib import pyplot as plt
from numpy import mean, min, max, median, quantile
from scipy.stats import binom, norm, t as student, expon, poisson, chi2
from math import sqrt, pow, floor, ceil, exp, log, sin, pi
from statsmodels.api import qqplot

A = 1.8988


def f(x):
	if x == 0:
		return 2 / A
	return 2 / A * abs(sin(pi * x) / (pi * x))


def g(x, l):
	return l * exp(-x * l)


l = 0.5
c = 2.188

X = np.arange(0, 6, 0.01)
Y = np.array([f(x) / 2 for x in X])
plt.scatter(X, Y, s=0.1)

Ntr = 1000000
U1 = np.random.rand(Ntr)
U2 = np.random.rand(Ntr)
U3 = np.random.rand(Ntr)
# choose the X value using the inverse CDF of g(x) => quantiles
X = expon.ppf(U1, scale=1 / l)
samples = []
for i in range(Ntr):
	x = X[i]
	if x >= 6:
		continue
	R = f(x) / (c * g(x, l))
	if U2[i] <= R:
		if U3[i] > 0.5:
			x = -x  # random sign
		samples.append(x)

plt.hist(samples, bins=200, density=True)

X = np.arange(-6, 6, 0.01)
Y = np.array([f(x)/2 for x in X])
plt.plot(X, Y, 'k-')

print(f"Efficiency: {len(samples)/Ntr}")

plt.show()