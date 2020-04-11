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
Y = np.array([f(x)/2 for x in X])
plt.scatter(X, Y, s=0.1)

samples = []
Xs = []
U1s = []
Rs = []
Ntr = 1000000
for i in range(Ntr):
	U1 = np.random.rand()
	X = -1 / l * log(U1)  # choose the X value using the inverse CDF of g(x)
	if X >= 6:
		continue
	R = f(X) / (c * g(X, l))
	U2 = np.random.rand()
	if U2 <= R:
		if np.random.rand() > 0.5:
			X = -X # random sign
		samples.append(X)

plt.hist(samples, bins=200, density=True)
print(f"Efficiency: {len(samples)/Ntr}")

plt.show()