import numpy as np
from matplotlib import pyplot as plt
from numpy import mean, min, max, median, quantile
from scipy.stats import binom, norm, t as student, expon, poisson, chi2
from math import sqrt, pow, floor, ceil, exp, log, sin, pi
from statsmodels.api import qqplot

A = 1.8988


def f(x):
	if x == 0:
		return 1
	return abs(sin(pi * x) / (A * pi * x))


X = np.arange(-6, 6, 0.01)
Y = np.array([f(x) for x in X])
plt.scatter(X, Y, s=0.1)

samples = []
Ntr = 1000000
for i in range(Ntr):
	U1 = np.random.rand() * 12 - 6
	U2 = np.random.rand() * 1 / A
	if U2 <= f(U1):
		samples.append(U1)

plt.hist(samples, bins=200, density=True)
print(f"Efficiency: {len(samples)/Ntr}")
plt.show()